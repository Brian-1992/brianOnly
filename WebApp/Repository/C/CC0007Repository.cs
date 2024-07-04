using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.C
{
    public class CC0007Repository : JCLib.Mvc.BaseRepository
    {
        public CC0007Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public bool CheckCrdocnoExist(string crdocno)
        {
            string sql = @"
                select 1 from CR_DOC
                 where crdocno = :crdocno
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { crdocno }, DBWork.Transaction) != null;
        }

        public COMBO_MODEL GetCrStatus(string crdocno)
        {
            string sql = @"
                select cr_status as value,
                       get_param('CR_DOC', 'CR_STATUS', cr_status) as text,
                       po_no as extra1
                  from CR_DOC 
                 where crdocno = :crdocno
            ";
            return DBWork.Connection.QueryFirstOrDefault<COMBO_MODEL>(sql, new { crdocno }, DBWork.Transaction);
        }
        public COMBO_MODEL GetPoStatus(string po_no)
        {
            string sql = @"
                select po_status as value,
                       get_param('MM_PO_M','PO_STATUS',PO_STATUS) as text
                  from MM_PO_M
                 where po_no = :po_no
            ";
            return DBWork.Connection.QueryFirstOrDefault<COMBO_MODEL>(sql, new { po_no }, DBWork.Transaction);
        }

        public IEnumerable<CC0007> GetCrdoc(string crdocno)
        {
            string sql = @"
                select crdocno, ackmmcode, po_no, mmname_c, mmname_e, 
                       towh, wh_name, cfmqty
                  from CR_DOC
                 where crdocno = :crdocno
            ";
            return DBWork.Connection.Query<CC0007>(sql, new { crdocno }, DBWork.Transaction);
        }


        public IEnumerable<CC0007> GetPoDs(string crdocno, string po_no, string userId, string updateIp)
        {
            string sql = @"
                select a.po_no, a.mmcode, c.agen_no,
                       a.lot_no as lot_no, to_char(a.exp_date, 'yyyy/mm/dd') as exp_date,
                       0 as bw_sqty,
                       a.inqty as inqty, a.inqty as acc_qty, a.inqty as cfm_qty,
                       (select base_unit from MI_MAST where mmcode=a.mmcode) as base_unit,
                       ('三聯單編號'||a.crdocno) as memo,
                       twn_time(sysdate) as acc_time,
                       :userId as acc_user,
                       (select m_storeid from MI_MAST where mmcode=a.mmcode) as m_storeid,
                       (select mat_class from MI_MAST where mmcode=a.mmcode) as mat_class,
                       b.po_qty, b.m_purun as acc_purun, b.unit_swap,
                       (select wexp_id from MI_MAST where mmcode=a.mmcode) as wexp_id,
                       a.inqty as tx_qty_t,
                       (select nvl(invoice,'') from PH_REPLY
                         where po_no=a.po_no and agen_no=c.agen_no and mmcode=a.mmcode
                           and status='b' and rownum=1) as invoice,
                       a.crdocno
                  from (select x.po_no, x.ackmmcode as mmcode, x.crdocno, y.lot_no, y.exp_date, y.inqty
                          from CR_DOC x, CR_DOC_D y
                         where x.crdocno=y.crdocno
                           and x.crdocno=:crdocno
                       ) a
                  join MM_PO_D b
                    on a.po_no=b.po_no and a.mmcode=b.mmcode
                  join MM_PO_M c
                    on a.po_no=c.po_no
            ";

            return DBWork.Connection.Query<CC0007>(sql, new { crdocno, po_no, userId, updateIp }, DBWork.Transaction);
        }

        public string GetAccLogSeq()
        {
            string sql = @"
                select BC_CS_ACC_LOG_SEQ.nextval from dual
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }

        public int InsertAccLog(CC0007 item)
        {
            string sql = @"
                insert into BC_CS_ACC_LOG 
                           (PO_NO, MMCODE, SEQ, AGEN_NO, LOT_NO, EXP_DATE,
                            BW_SQTY, INQTY, ACC_QTY, CFM_QTY, ACC_BASEUNIT,
                            STATUS, MEMO, ACC_TIME, ACC_USER, STOREID, MAT_CLASS, PO_QTY,
                            ACC_PURUN, UNIT_SWAP, WEXP_ID, TX_QTY_T, INVOICE)  
                         values
                           (:PO_NO, :MMCODE, :SEQ, :AGEN_NO, :LOT_NO, 
                            to_date(:EXP_DATE,'yyyy/mm/dd'), 
                            :BW_SQTY, :INQTY, :ACC_QTY*:UNIT_SWAP, :ACC_QTY*:UNIT_SWAP, :BASE_UNIT,
                            'C', trim(:MEMO),  twn_todate(:ACC_TIME), 
                            :ACC_USER, :M_STOREID, :MAT_CLASS, :PO_QTY,
                            :ACC_PURUN, :UNIT_SWAP, :WEXP_ID, :TX_QTY_T, :INVOICE)
            ";
            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);
        }

        public int UpdatePhReply(string crdocno, string po_no, string userId, string updateIp)
        {
            string sql = @"
                update PH_REPLY
                   set status = 'R', update_time  = sysdate, update_ip = :updateIp
                 where memo = ('三聯單編號'|| :crdocno)
                   and status = 'B'
            ";
            return DBWork.Connection.Execute(sql, new { crdocno, po_no, userId, updateIp }, DBWork.Transaction);
        }

        public int InsertDistLog(CC0007 item)
        {
            string sql = @"
            insert into BC_CS_DIST_LOG
                   (po_no, mmcode, seq, load_time, agen_no,
                    pr_dept, pr_qty,
                    docno, dist_baseunit, bw_sqty, lot_no, exp_date,
                    dist_status, dist_qty, dist_time, dist_user)  
            select b.po_no, b.mmcode, :SEQ, sysdate, b.agen_no,
                   b.inid, a.appqty,
                   a.docno, b.base_unit, '0',
                   :LOT_NO,
                   to_date(:EXP_DATE,'yyyy/mm/dd'),
                   'C', :CFM_QTY, sysdate, :UserId
              from PH_PO_N a
              join (select * from CR_DOC where crdocno = :CRDOCNO) b
                on a.po_no=b.po_no and a.mmcode=b.mmcode and a.docno=b.rdocno
            ";
            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);
        }

        public string procDocin(string PO_NO, string USER_ID, string USER_IP)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_PONO", value: PO_NO, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("I_USERID", value: USER_ID, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("I_UPDIP", value: USER_IP, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);
            p.Add("O_RETMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            DBWork.Connection.Query("INV_SET.PO_DOCIN", p, commandType: CommandType.StoredProcedure);
            string RetId = p.Get<OracleString>("O_RETID").Value;
            string RetMsg = p.Get<OracleString>("O_RETMSG").Value;

            if (RetId == "N")
                return "SP:" + RetMsg;
            else if (RetId == "Y")
                return "Y";
            else
                return "";
        }

        public string procDistin(string PO_NO, string MMCODE, string USER_ID, string USER_IP)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_PONO", value: PO_NO, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("I_MMCODE", value: MMCODE, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("I_USERID", value: USER_ID, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("I_UPDIP", value: USER_IP, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);
            p.Add("O_RETMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            DBWork.Connection.Query("INV_SET.DIST_IN", p, commandType: CommandType.StoredProcedure);
            string RetId = p.Get<OracleString>("O_RETID").Value;
            string RetMsg = p.Get<OracleString>("O_RETMSG").Value;

            if (RetId == "N")
                return "SP:" + RetMsg;
            else if (RetId == "Y")
                return "Y";
            else
                return "";
        }
        public string procCC0002(string PO_NO, string USER_ID, string USER_IP)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_PO_NO", value: PO_NO, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("I_USERID", value: USER_ID, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("I_IP", value: USER_IP, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);

            p.Add("RET_CODE", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            DBWork.Connection.Query("CC0002_SUMBIT", p, commandType: CommandType.StoredProcedure);
            string RetCode = p.Get<OracleString>("RET_CODE").Value;

            return RetCode;
        }

        public int UpdateCrdocStatus(string crdocno)
        {
            string sql = @"
                update CR_DOC
                   set cr_status = 'O'
                 where crdocno = :crdocno
            ";
            return DBWork.Connection.Execute(sql, new { crdocno }, DBWork.Transaction);
        }

        public int DeleteAccLog(string seqs)
        {
            string sql = string.Format(@"
                delete from BC_CS_ACC_LOG
                 where seq in ( {0} )
            ", seqs);
            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }

        public int DeleteDistLog(string seqs)
        {
            string sql = string.Format(@"
                delete from BC_CS_DIST_LOG
                 where seq in ( {0} )
            ", seqs);
            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }
    }
}