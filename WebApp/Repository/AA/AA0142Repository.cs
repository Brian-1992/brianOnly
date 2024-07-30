using Dapper;
using JCLib.DB;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.AA
{
    public class AA0142Repository : JCLib.Mvc.BaseRepository
    {
        public AA0142Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetMasterAll(string startDate, string endDate)
        {
            string sql = @"SELECT A.* ,
                                  (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='FLOWID_AJ' AND DATA_VALUE=A.FLOWID) FLOWID_N,
                                  (SELECT MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS=A.MAT_CLASS) MAT_CLASS_N,
                                  (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.FRWH) FRWH_N,
                                  (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.TOWH) TOWH_N,
                                  (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='APPLY_KIND' AND DATA_VALUE=A.APPLY_KIND) APPLY_KIND_N ,
                                  (SELECT UNA FROM UR_ID WHERE TUSER=A.APPID) APP_NAME,
                                  (SELECT INID_NAME FROM UR_INID WHERE INID=A.APPDEPT) APPDEPT_NAME,
                                  TWN_DATE(A.APPTIME) APPTIME_T
                             from ME_DOCM a
                            where doctype = 'CAJ'";
            if (string.IsNullOrEmpty(startDate) == false)
            {
                sql += " and twn_date(apptime) >= :startDate";
            }
            if (string.IsNullOrEmpty(endDate) == false)
            {
                sql += " and twn_date(apptime) <= :endDate";
            }

            return DBWork.PagingQuery<ME_DOCM>(sql, new { startDate, endDate }, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetDetailAll(string docno)
        {
            string sql = @"
                select a.docno, a.seq, a.mmcode , a.appqty , a.aplyitem_note, 
                        a.gtapl_reson,a.stat,a.expt_distqty,a.apvqty,a.ackqty,
                        a.bw_mqty,a.bw_sqty,a.apvtime,a.onway_qty,a.rv_mqty,
                        b.mmname_c, b.mmname_e, b.base_unit,b.m_contprice,
                        ( select avg_price from mi_whcost where mmcode = a.mmcode and rownum=1 ) as avg_price,
                        ( select inv_qty from mi_whinv where mmcode = a.mmcode and wh_no=c.frwh ) as inv_qty,
                        ( select avg_aplqty from v_mm_avgapl where mmcode = a.mmcode and wh_no=c.frwh ) as avg_aplqty,
                        ( select nvl(tot_apvqty,0) from v_mm_totapl where date_ym=substr(twn_sysdate,0,5) and mmcode=a.mmcode ) as tot_apvqty,
                        ( select sum(tot_bwqty) from v_mm_totapl where date_ym=substr(twn_sysdate,0,5) and mmcode=a.mmcode ) as tot_bwqty,
                        ( select min_ordqty from mi_winvctl where mmcode = a.mmcode and wh_no ='560000' and rownum =1) as tot_distun,
                        c.flowid,
                        ( select safe_qty from mi_winvctl where mmcode = a.mmcode and wh_no=c.towh ) as safe_qty,
                        (select data_desc from param_d where grp_code='me_docd' and data_name='stat' and data_value=a.stat) stat_n,
                        ( a.appqty * b.m_contprice ) tot_price 
                   from ME_DOCD a, MI_MAST b, ME_DOCM c 
                  where c.docno=a.docno and a.mmcode = b.mmcode
                    and a.docno = :docno
            ";
            return DBWork.PagingQuery<ME_DOCD>(sql, new { docno }, DBWork.Transaction);
        }

        public int CreateM(ME_DOCM ME_DOCM)
        {
            var sql = @"INSERT INTO ME_DOCM (
                        DOCNO, DOCTYPE, FLOWID , APPID , APPDEPT , 
                        APPTIME , USEID , USEDEPT , FRWH , TOWH , 
                        APPLY_KIND ,APPLY_NOTE ,MAT_CLASS,
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, 'CAJ', '1' , :APPID , :APPDEPT , 
                        SYSDATE , :USEID , :USEDEPT , :FRWH , :TOWH , 
                        :APPLY_KIND , :APPLY_NOTE ,:MAT_CLASS,
                        SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }

        public int CreateD(ME_DOCD ME_DOCD)
        {
            var sql = @"INSERT INTO ME_DOCD (
                        DOCNO, SEQ, MMCODE , APPQTY , APLYITEM_NOTE, STAT, 
                        CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :SEQ, :MMCODE , :APPQTY , :APLYITEM_NOTE, :STAT,
                        :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, ME_DOCD, DBWork.Transaction);
        }
        public int DeleteM(string docno)
        {
            string sql = @"
                update ME_DOCM
                   set flowId = 'X'
                 where docno = :docno
            ";
            return DBWork.Connection.Execute(sql, new { docno }, DBWork.Transaction);
        }

        public bool ChceckFlowId01(string docno)
        {
            string sql = @"
                select 1 from ME_DOCM
                 where docno = :docno
                   and (substr(flowId, length(flowId)-1 , 2) = '01'
                       or substr(flowId, length(flowId)-1 , 2) = '00'
                       or substr(flowId, length(flowId)-1 , 2) = '1')
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { docno }, DBWork.Transaction) != null;
        }
        
        public ME_DOCM GetFrwh(string docno)
        {
            string sql = @"
                select frwh as wh_no ,
                       (select wh_kind from MI_WHMAST where wh_no = a.frwh) as wh_kind,
                        mat_class
                  from ME_DOCM a
                 where docno = :docno
            ";
            return DBWork.Connection.QueryFirstOrDefault<ME_DOCM>(sql, new { docno }, DBWork.Transaction);
        }

        public IEnumerable<MI_WINVMON> GetInvs(string wh_no, string mat_class)
        {
            string sql = string.Format(@"
                select wh_no, mmcode, inv_qty
                  from MI_WHINV a
                 where a.wh_no = :wh_no
                   and exists (select 1 from MI_MAST where mmcode = a.mmcode and mat_class = :mat_class)
                   and inv_qty <> 0
            "); 

            return DBWork.Connection.Query<MI_WINVMON>(sql, new { wh_no, mat_class }, DBWork.Transaction);
        }

        public string GetDocDSeq(string id)
        {
            string sql = @"SELECT nvl(MAX(SEQ), 0)+1 as SEQ FROM ME_DOCD WHERE DOCNO=:DOCNO ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public bool CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public string GetDocno()
        {
            var p = new OracleDynamicParameters();
            p.Add("O_DOCNO", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 12);

            DBWork.Connection.Query("GET_DOCNO", p, commandType: CommandType.StoredProcedure);
            return p.Get<OracleString>("O_DOCNO").Value;
        }
        public string GetWhKind(string wh_no)
        {
            string sql = @"
                select wh_kind from MI_WHMAST where wh_no = :wh_no
            ";
            return DBWork.Connection.QueryFirst<string>(sql, new { wh_no }, DBWork.Transaction);
        }

        public int UpdateFlowId(string docno, string userId, string ip)
        {
            string sql = @"
                update ME_DOCM
                   set flowId = '3',
                       sendapvtime = sysdate,
                       sendapvid = :userId,
                       sendapvdept = (select inid from UR_ID where tuser = :userId),
                       update_time = sysdate,
                       update_user = :userId,
                       update_ip = :ip
                 where docno = :docno
            ";
            return DBWork.Connection.Execute(sql, new { docno, userId, ip }, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetDs(string docno)
        {
            string sql = @"
                select a.* , b.frwh as wh_no
                  from ME_DOCD a, ME_DOCM b
                 where a.docno = :docno
                   and b.docno = a.docno
            ";
            return DBWork.Connection.Query<ME_DOCD>(sql, new { docno }, DBWork.Transaction);
        }

        public int UpdateDocd(string docno, string userId, string ip)
        {
            string sql = @"
                update ME_DOCD
                   set APVQTY = APPQTY,
                       APVTIME = sysdate,
                       APVID = :userId,
                       update_user = :userId,
                       update_time = sysdate,
                       update_ip = :ip
                 where docno = :docno
            ";
            return DBWork.Connection.Execute(sql, new { docno, userId, ip }, DBWork.Transaction);
        }


        public int UpdateWhinv(string wh_no, string mmcode, string appqty)
        {
            string sql = string.Format(@"
                update MI_WHINV
                   set inv_qty = inv_qty - (:appqty),
                       adj_outqty = adj_outqty + (:appqty)
                 where wh_no = :wh_no
                   and mmcode = :mmcode
            ", double.Parse(appqty) >= 0 ? " adj_outqty = adj_outqty + (:appqty)" : " adj_inqty = adj_inqty + (:appqty)");
            return DBWork.Connection.Execute(sql, new { wh_no, mmcode, appqty }, DBWork.Transaction);
        }

        public int InsertWhtrns(MI_WHTRNS trns)
        {
            string sql = @"
                insert into MI_WHTRNS (
                    WH_NO, TR_DATE, TR_SNO, MMCODE,
                    TR_INV_QTY, TR_ONWAY_QTY, TR_DOCNO, TR_DOCSEQ,
                    TR_FLOWID, TR_DOCTYPE, TR_IO, TR_MCODE,
                    BF_TR_INVQTY, AF_TR_INVQTY
                ) values (
                    :WH_NO, sysdate, WHTRNS_SEQ.NEXTVAL, :MMCODE,
                    :TR_INV_QTY, :TR_ONWAY_QTY, :TR_DOCNO, :TR_DOCSEQ,
                    :TR_FLOWID, :TR_DOCTYPE, :TR_IO, :TR_MCODE,
                    :BF_TR_INVQTY, :AF_TR_INVQTY
                )
            ";
            return DBWork.Connection.Execute(sql, trns, DBWork.Transaction);
        }
        public ME_DOCM GetM(string docno)
        {
            string sql = @"
                select * from ME_DOCM
                 where docno = :docno
            ";
            return DBWork.Connection.QueryFirstOrDefault<ME_DOCM>(sql, new { docno }, DBWork.Transaction);
        }
        public MI_WINVMON GetD(string wh_no, string mmcode)
        {
            string sql = @"
                select * 
                  from MI_WHINV
                 where wh_no = :wh_no
                   and mmcode = :mmcode
            ";
            return DBWork.Connection.QueryFirstOrDefault<MI_WINVMON>(sql, new { wh_no, mmcode }, DBWork.Transaction);
        }

        #region combo
        public IEnumerable<COMBO_MODEL> GetMatClassCombo() {
            string sql = @"
                select mat_class as value,
                       mat_class || ' ' || mat_clsname as text
                  from MI_MATCLASS
                 order by mat_class
            ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetWhnoCombo()
        {
            string sql = @"
                select wh_no as value,
                       wh_no||' '|| wh_name as text,
                       wh_kind as EXTRA1
                  from MI_WHMAST
                 where 1=1
                   and cancel_id = 'Y'
                 order by wh_no  
            ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }
        #endregion
    }
}