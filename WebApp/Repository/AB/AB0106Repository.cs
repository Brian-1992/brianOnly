using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.AB
{
    public class AB0106Item : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT { get; set; }
        public string M_TRNID { get; set; }
        public string E_SOURCECODE { get; set; }
        public string M_PAYKIND { get; set; }
        public string CANCEL_ID { get; set; }
        public string MAT_CLASS { get; set; }
        public string WEXP_ID { get; set; }
        public string INV_QTY { get; set; }
        public string LOT_NO { get; set; }
        public string EXP_DATE { get; set; }
        public string SUSE_NOTE { get; set; }
        
        public string TRATIO { get; set; }

        public string BARCODE { get; set; }

        public string ACKTIMES { get; set; }
        public string ADJQTY { get; set; }
        public string EXP_DATE_UDI { get; set; }
        public string IS_UDI { get; set; }

    }

    public class AB0106Repository : JCLib.Mvc.BaseRepository
    {
        public AB0106Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        

        public bool CheckWhnoValid(string wh_no, string userId) {
            string sql = @"
                select 1 
                  from (select A.WH_NO 
                          from MI_WHMAST A
                         WHERE WH_KIND = '1' and wh_grade > '1'
                           AND EXISTS
                                 (SELECT 'X' FROM UR_ID B
                                   WHERE (A.SUPPLY_INID=B.INID OR A.INID=B.INID) AND TUSER=:userId)
                           AND NOT EXISTS
                                 (SELECT 'X' FROM MI_WHID B
                                   WHERE TASK_ID IN ('2','3') AND WH_USERID=:userId)
                           and a.cancel_id = 'N'
                        UNION ALL 
                        SELECT A.WH_NO
                          FROM MI_WHMAST A, MI_WHID B
                         WHERE A.WH_NO=B.WH_NO AND TASK_ID IN ('2','3') AND WH_USERID=:userId
                           and a.wh_kind = '1'
                           and a.cancel_id = 'N'
                           and a.wh_grade > '1'
                       )
                 where wh_no = :wh_no
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { wh_no, userId }, DBWork.Transaction) != null;
        }

        public BC_UDI_LOG GetUdiLog(string barcode) {
            string sql = @"
                select * from BC_UDI_LOG
                 where thisbarcode = :barcode
            ";
            return DBWork.Connection.QueryFirstOrDefault<BC_UDI_LOG>(sql, new { barcode }, DBWork.Transaction);
        }

        public AB0106Item GetDataByMmcode(string mmcode, string wh_no) {
            string sql = @"
                select a.mmcode, a.mmname_c, a.mmname_e, a.base_unit, a.MAT_CLASS, a.m_paykind,
                       a.M_TRNID, a.E_SOURCECODE, a.cancel_id, trim(a.wexp_id) as wexp_id,
                       (select inv_qty from MI_WHINV
                             where wh_no = :wh_No
                               and mmcode = a.mmcode) as inv_qty
                  from MI_MAST a
                 where a.mmcode = :mmcode
            ";
            return DBWork.Connection.QueryFirstOrDefault<AB0106Item>(sql, new { mmcode, wh_no }, DBWork.Transaction);
        }

        public BC_BARCODE GetMmcodeFromBarcode(string barcode) {
            string sql = @"
                select mmcode, tratio
                  from BC_BARCODE
                 where (mmcode = :barcode or barcode = :barcode)
                   and rownum = 1
            ";
            return DBWork.Connection.QueryFirstOrDefault<BC_BARCODE>(sql, new { barcode }, DBWork.Transaction);
        }

        public string GetSUseSeq() {
            string sql = @"
                select suse_seq.nextval  from dual
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }

        public int InsertScanUseLogZ(string suse_seq, string wh_no, string mmcode, string use_type,
                                     string tratio, string base_unit, string bf_invqty, string wexp_id, 
                                     string lot_no, string exp_date, string exp_date_udi,
                                     string m_trnid, string e_sourcecode, string userId, string ip,
                                     string scan_barcode, string suse_note, string m_paykind)
        {
            string sql = @"
                insert into SCAN_USE_LOG (suse_seq, wh_no, mmcode, use_type,
                                          tratio, base_unit, bf_invqty, wexp_id, 
                                          lot_no, exp_date, exp_date_udi,
                                          m_trnid, e_sourcecode, create_time, create_user, update_ip,
                                          isuse, scan_barcode, suse_note, m_paykind)
                values (:suse_seq, :wh_no, :mmcode, :use_type,
                                          :tratio, :base_unit, nvl(:bf_invqty, 0), nvl(:wexp_id, 'N'), 
                                          :lot_no, twn_todate(:exp_date), :exp_date_udi,
                                          :m_trnid, :e_sourcecode, sysdate, :userId, :ip,
                                          'Z', :scan_barcode, :suse_note, :m_paykind)
            ";

            return DBWork.Connection.Execute(sql, new
            {
                suse_seq,wh_no,mmcode,use_type,
                tratio,base_unit, bf_invqty, wexp_id,
                lot_no, exp_date, exp_date_udi,
                m_trnid, e_sourcecode, userId, ip,
                scan_barcode, suse_note, m_paykind
            }, DBWork.Transaction);
        }

        public int InsertScanUseLog(string suse_seq, string wh_no, string mmcode, string use_type,
                                   string tratio, string acktimes, string adjqty, string use_qty,
                                   string base_unit, string bf_invqty, string wexp_id, string lot_no, string exp_date, string exp_date_udi,
                                   string m_trnid, string e_sourcecode, string userId, string ip,
                                   string scan_barcode, string suse_note)
        {
            string sql = @"
                insert into SCAN_USE_LOG (suse_seq, wh_no, mmcode, use_type,
                                          tratio, acktimes, adjqty, use_qty,
                                          base_unit, bf_invqty, wexp_id, lot_no, exp_date, exp_date_udi,
                                          m_trnid, e_sourcecode, create_time, create_user, update_ip,
                                          scan_barcode, suse_note)
                values (:suse_seq, :wh_no, :mmcode, :use_type,
                                          :tratio, :acktimes, :adjqty, :use_qty,
                                          :base_unit, nvl(:bf_invqty, 0), nvl(:wexp_id, 'N'), :lot_no, twn_todate(:exp_date), :exp_date_udi,
                                          :m_trnid, :e_sourcecode, sysdate, :userId, :ip,
                                          :scan_barcode, :suse_note)
            ";

            return DBWork.Connection.Execute(sql, 
                new { suse_seq, wh_no, mmcode, use_type,
                      tratio, acktimes, adjqty, use_qty,
                    base_unit, bf_invqty, wexp_id, lot_no, exp_date, exp_date_udi,
                    m_trnid, e_sourcecode, userId, ip,
                   scan_barcode, suse_note
                   }, 
                DBWork.Transaction);

        }

        public bool CheckWhinvExists(string wh_no, string mmcode) {
            string sql = @"
                select 1 from MI_WHINV
                 where wh_no = :wh_no and mmcode = :mmcode
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { wh_no, mmcode }, DBWork.Transaction) != null;
        }
        public int InsertWhinv(string wh_no, string mmcode) {
            string sql = @"
                insert into MI_WHINV (wh_no, mmcode) values (:wh_no, :mmcode)
            ";
            return DBWork.Connection.Execute(sql, new { wh_no, mmcode }, DBWork.Transaction);
        }
        public int UpdateWhinv(string wh_no, string mmcode, string use_qty) {
            string sql = @"
                update MI_WHINV
                   set inv_qty = (inv_qty - :use_qty),
                       use_qty = (use_qty + :use_qty)
                 where wh_no = :wh_no
                   and mmcode = :mmcode
            ";
            return DBWork.Connection.Execute(sql, new { wh_no, mmcode, use_qty }, DBWork.Transaction);
        }

        public string GetWhtrnsDocno(string suse_seq) {
            string sql = @"
                select TWN_DATE(sysdate)||'CSM'||
                       (case when length(to_char(:suse_seq))<5 
                             then lpad(to_char(:suse_seq),5,'0')
                             else substr(to_char(:suse_seq),length(to_char(:suse_seq))-4,5)
                        end)
                  from dual
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { suse_seq }, DBWork.Transaction);
        }
        public int InsertWhtrns(string wh_no, string mmcode, string tr_inv_qty,   
                                string tr_docno, string bf_tr_invqty, string use_type) {
            string sql = string.Format(@"
                insert into MI_WHTRNS (wh_no, tr_date, tr_sno, mmcode, 
                                       tr_inv_qty, tr_docno, tr_doctype,
                                       tr_io, tr_mcode, bf_tr_invqty, af_tr_invqty)
                values (:wh_no, sysdate, WHTRNS_SEQ.NEXTVAL, :mmcode,
                        :tr_inv_qty, :tr_docno, 'CSM',
                        '{0}', 'USE{0}', nvl(:bf_tr_invqty, 0), (nvl(:bf_tr_invqty, 0) {1} :tr_inv_qty)
                )
            ", use_type == "A" ? "O" : "I"
             , use_type == "A" ? "-" : "+");
            return DBWork.Connection.Execute(sql, new { wh_no, mmcode, tr_inv_qty, tr_docno, bf_tr_invqty }, DBWork.Transaction);
        }


        public bool CheckWexpinvExists(string wh_no, string mmcode, string lot_no, string exp_date) {
            string sql = @"
                select 1 from MI_WEXPINV
                 where wh_no = :wh_no
                   and mmcode = :mmcode
                   and lot_no = :lot_no
                   and twn_date(exp_date) = :exp_date
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { wh_no, mmcode, lot_no, exp_date }, DBWork.Transaction) != null;
        }
        public int InsertWexpinv(string wh_no, string mmcode, string exp_date, string lot_no, string use_qty,
                                  string use_type, string userId, string ip)
        {
            string sql = @"
                insert into MI_WEXPINV(wh_no, mmcode, exp_date, lot_no, inv_qty, 
                                       create_user, update_ip, create_time)
                 values (:wh_no, :mmcode, twn_todate(:exp_date), :lot_no, (0 - :use_qty), 
                         :userId, :ip, sysdate)
            ";
            return DBWork.Connection.Execute(sql, new { wh_no, mmcode, exp_date, lot_no, use_qty,userId, ip }, DBWork.Transaction);
        }

        public int UpdateWexpinv(string wh_no, string mmcode, string exp_date, string lot_no, string use_qty,
                                  string use_type, string userId, string ip)
        {
            string sql = @"
                update MI_WEXPINV
                   set inv_qty = inv_qty - (:use_qty),
                       update_time = sysdate,
                       update_user = :userId,
                       update_ip = :ip
                 where wh_no = :wh_no
                   and mmcode = :mmcode
                   and lot_no = :lot_no
                   and twn_date(exp_date) = :exp_date
            ";
            return DBWork.Connection.Execute(sql, new { wh_no, mmcode, exp_date, lot_no, use_qty, userId, ip }, DBWork.Transaction);
        }

        public int UpdateScanUseLogY(string seq) {
            string sql = @"
                update SCAN_USE_LOG
                   set isuse = 'Y'
                 where suse_seq = :seq
            ";
            return DBWork.Connection.Execute(sql, new { seq }, DBWork.Transaction);
        }

        #region combo

        public IEnumerable<COMBO_MODEL> GetWhnoCombo(string userId) {
            string sql = @"
                select A.WH_NO VALUE, WH_NO||' '||WH_NAME TEXT, A.WH_NO || ' ' || A.WH_NAME COMBITEM
                  from MI_WHMAST A
                 WHERE WH_KIND = '1' and wh_grade > '1'
                   AND EXISTS
                         (SELECT 'X' FROM UR_ID B
                           WHERE (A.SUPPLY_INID=B.INID OR A.INID=B.INID) AND TUSER=:userId)
                   AND NOT EXISTS
                         (SELECT 'X' FROM MI_WHID B
                           WHERE TASK_ID IN ('2','3') AND WH_USERID=:userId)
                   and a.cancel_id = 'N'
                UNION ALL 
                SELECT A.WH_NO, A.WH_NO||' '||WH_NAME, A.WH_NO || ' ' || A.WH_NAME COMBITEM
                  FROM MI_WHMAST A, MI_WHID B
                 WHERE A.WH_NO=B.WH_NO AND TASK_ID IN ('2','3') AND WH_USERID=:userId
                   and a.wh_kind = '1'
                   and a.cancel_id = 'N'
                   and a.wh_grade > '1'
                 ORDER BY 1
            ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql,new { userId}, DBWork.Transaction);
        }

        #endregion

        #region udi_log
        public int CreateUdiLog(DecodeResponse deResponse)
        {
            var sql = @"insert into BC_UDI_LOG 
            (LOG_TIME, WMMID, WMCMPY, WMWHS, WMORG, CRVMPY, CRITM, WMREFCODE, WMBOX, WMLOC, WMSRV, WMSKU, WMMIDNAME, WMMIDNAMEH, WMSKUSPEC, WMBRAND, WMMDL, WMMIDCTG, WMEFFCDATE, WMLOT, WMSENO, WMPAK, 
                                WMQY, THISBARCODE, UDIBARCODES, GTINSTRING, NHIBARCODE, NHIBARCODES, BARCODETYPE, GTININSTRING, RESULT, ERRMSG, SOURCE)  
                                values (sysdate, :WmMid, :WmCmpy, :WmWhs, :WmOrg, :CrVmpy, :CrItm, :WmRefCode, :WmBox, :WmLoc, :WmSrv, :WmSku, :WmMidName, :WmMidNameH, :WmSkuSpec, :WmBrand, :WmMdl, :WmMidCtg, :WmEffcDate, :WmLot, :WmSeno, :WmPak, 
                                :WmQy, :ThisBarcode, :UdiBarcodes, :GtinString, :NhiBarcode, :NhiBarcodes, :BarcodeType, :GtinInString, :Result, :ErrMsg, '衛星庫房條碼扣庫新增')";
            return DBWork.Connection.Execute(sql, deResponse, DBWork.Transaction);
        }

        public int UpdateUdiLog(DecodeResponse deResponse)
        {
            var sql = @" update BC_UDI_LOG set LOG_TIME = sysdate, WMCMPY = :WmCmpy, WMWHS = :WmWhs, WMORG = :WmOrg, CRVMPY = :CrVmpy, CRITM = :CrItm, WMREFCODE = :WmRefCode, WMBOX = :WmBox, WMLOC = :WmLoc, WMSRV = :WmSrv, WMSKU = :WmSku, 
                                WMMIDNAME = :WmMidName, WMMIDNAMEH = :WmMidNameH, WMSKUSPEC = :WmSkuSpec, WMBRAND = :WmBrand, WMMDL = :WmMdl, WMMIDCTG = :WmMidCtg, WMEFFCDATE = :WmEffcDate, WMLOT = :WmLot, WMSENO = :WmSeno, WMPAK = :WmPak, 
                                WMQY = :WmQy, THISBARCODE = :ThisBarcode, UDIBARCODES = :UdiBarcodes, GTINSTRING = :GtinString, NHIBARCODE = :NhiBarcode, NHIBARCODES = :NhiBarcodes, BARCODETYPE = :BarcodeType, GTININSTRING = :GtinInString, RESULT = :Result, ERRMSG = :ErrMsg
                                where WMMID=:WmMid ";

            if (deResponse.WmLot == "" || deResponse.WmLot == null)
                sql += " and (WMLOT = '' or WMLOT is null) ";
            else
                sql += " and WMLOT=:WmLot ";

            return DBWork.Connection.Execute(sql, deResponse, DBWork.Transaction);
        }
        #endregion
    }
}