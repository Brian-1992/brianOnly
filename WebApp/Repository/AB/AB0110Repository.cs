using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Data;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using WebApp.Models.AB;
namespace WebApp.Repository.AB
{
    public class AB0110Repository : JCLib.Mvc.BaseRepository
    {
        public AB0110Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public bool CheckCrDocNoExists(string crdocno) {
            string sql = @"
                select crdocno from CR_DOC where crdocno = :crdocno
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { crdocno }, DBWork.Transaction) != null;
        }
        public string CheckCrDocStatuInvalid(string crdocno) {
            string sql = @"
                select nvl(get_param('CR_DOC', 'CR_STATUS', a.cr_status) , '')
                  from CR_DOC  a
                 where a.crdocno = :crdocno
                   and a.cr_status not in ('B','E','F','G','H')
            ";

            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { crdocno }, DBWork.Transaction);
        }

        public string CheckCrDocWhNoInvalid(string crdocno, string userId) {
            string sql = @"
                select wh_name(a.towh)
                  from CR_DOC a
                 where a.crdocno = :crdocno
                   and towh not in (select a.WH_NO
                                      from MI_WHMAST a
                                     where WH_KIND='1' and wh_grade>'1'
                                       and exists
                                             (select 'X' from UR_ID b 
                                               where (a.SUPPLY_INID=b.INID OR a.INID=b.INID)
                                                 and TUSER=:userId)
                                       and not exists
                                             (select 'X' from MI_WHID b
                                               where TASK_ID in ('2','3') and WH_USERID=:userId)
                                       and NVL(a.cancel_id,'N')='N'
                                    union all 
                                     select a.WH_NO
                                       from MI_WHMAST a,MI_WHID b
                                      where a.WH_NO=b.WH_NO and TASK_ID in ('2','3')
                                        and WH_USERID=:userId
                                        and NVL(a.cancel_id,'N')='N'
                                        and a.wh_grade>'1'
                                   )

            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { crdocno, userId}, DBWork.Transaction);
        }

        public IEnumerable<AB0110> GetMaster(string crdocno) {
            string sql = @"
                select a.crdocno, a.ackmmcode, a.mmname_c, a.mmname_e, a.appqty,
                       a.base_unit, a.inqty, a.wh_name,
                       get_param('CR_DOC','CR_STATUS', a.CR_STATUS) as cr_status_name,
                       a.lot_no, twn_date(a.exp_date) as exp_date,
                       a.isSmall, b.usewhen, b.usewhere, b.tel
                  from CR_DOC a
                  left join CR_DOC_SMALL b on (a.crdocno = b.crdocno)
                 where a.crdocno = :crdocno
            ";
            return DBWork.Connection.Query<AB0110>(sql, new { crdocno }, DBWork.Transaction);
        }

        public IEnumerable<AB0110> GetDetails(string crdocno) {
            string sql = @"
                select b.crdocno,
                       b.cr_d_seq, b.lot_no, twn_date(b.exp_date) as exp_date,
                       b.inqty, b.isudi
                  from  CR_DOC_D b
                 where b.crdocno = :crdocno
            ";
            return DBWork.PagingQuery<AB0110>(sql, new { crdocno }, DBWork.Transaction);
        }

        public int Insert(string crdocno, string lot_no, string exp_date, string inqty, string isUdi, string userId, string updateIp) {
            string sql = @"
                insert into CR_DOC_D
                           (crdocno,cr_d_seq,lot_no,exp_date,inqty,
                            isudi,create_time,create_user, update_time, update_user, update_ip)
                values (
                    :crdocno, crdoc_d_seq.nextval, :lot_no, twn_todate(:exp_date), :inqty,
                    :isUdi, sysdate, :userId, sysdate, :userId, :updateIp
                )
            ";
            return DBWork.Connection.Execute(sql, new { crdocno, lot_no, exp_date, inqty, isUdi, userId, updateIp }, DBWork.Transaction);
        }
        public int Update(string crdocno, string cr_d_seq, string lot_no, string exp_date, string inqty, string userId, string updateIp) {
            string sql = @"
                update CR_DOC_D
                   set lot_no = :lot_no,
                       exp_date = twn_todate(:exp_date),
                       inqty = :inqty,
                       update_time = sysdate,
                       update_user = :userId,
                       update_ip = :updateIp
                 where crdocno = :crdocno
                   and cr_d_seq = :cr_d_seq
            ";
            return DBWork.Connection.Execute(sql, new { crdocno, cr_d_seq, lot_no, exp_date, inqty, userId, updateIp }, DBWork.Transaction);
        }

        public int Delete(string crdocno, string cr_d_seq) {
            string sql = @"
                delete from CR_DOC_D
                 where crdocno = :crdocno
                   and cr_d_seq = :cr_d_seq
            ";
            return DBWork.Connection.Execute(sql, new { crdocno, cr_d_seq }, DBWork.Transaction);
        }

        public int CreateUdiLog(DecodeResponse deResponse)
        {
            var sql = @"insert into BC_UDI_LOG 
            (LOG_TIME, WMMID, WMCMPY, WMWHS, WMORG, CRVMPY, CRITM, WMREFCODE, WMBOX, WMLOC, WMSRV, WMSKU, WMMIDNAME, WMMIDNAMEH, WMSKUSPEC, WMBRAND, WMMDL, WMMIDCTG, WMEFFCDATE, WMLOT, WMSENO, WMPAK, 
                                WMQY, THISBARCODE, UDIBARCODES, GTINSTRING, NHIBARCODE, NHIBARCODES, BARCODETYPE, GTININSTRING, RESULT, ERRMSG)  
                                values (sysdate, :WmMid, :WmCmpy, :WmWhs, :WmOrg, :CrVmpy, :CrItm, :WmRefCode, :WmBox, :WmLoc, :WmSrv, :WmSku, :WmMidName, :WmMidNameH, :WmSkuSpec, :WmBrand, :WmMdl, :WmMidCtg, :WmEffcDate, :WmLot, :WmSeno, :WmPak, 
                                :WmQy, :ThisBarcode, :UdiBarcodes, :GtinString, :NhiBarcode, :NhiBarcodes, :BarcodeType, :GtinInString, :Result, :ErrMsg)";
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

        public int getUdiMmcodeCnt(string mmcode, string lot_no)
        {
            var p = new DynamicParameters();
            var sql = @" select count(*) from BC_UDI_LOG where WMMID = :p0 ";

            if (lot_no == "" || lot_no == null)
                sql += " and (WMLOT = '' or WMLOT is null) ";
            else
                sql += " and WMLOT=:p1 ";

            p.Add(":p0", mmcode);
            p.Add(":p1", lot_no);

            return DBWork.Connection.QueryFirst<int>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<AB0110> GetUdiInfo(string barcode)
        {
            string sql = @"
                select a.wmmid as ackmmcode,
                       twn_date(to_date(a.wmeffcdate, 'YYYYMMDD')) as exp_date,
                       a.wmlot as lot_no,
                       b.mmname_c, b.mmname_e,
                       '1' as inqty,
                       'Y' as isUdi
                  from BC_UDI_LOG a, MI_MAST b
                 where a.thisBarcode = :barcode
                   and a.wmmid = b.mmcode
            ";
            return DBWork.Connection.Query<AB0110>(sql, new { barcode }, DBWork.Transaction);
        }

        public IEnumerable<AB0110> GetBcBarcode(string barcode) {
            string sql = @"
                select a.mmcode as ackmmcode,
                       a.tratio as inqty,
                       b.mmname_c, b.mmname_e,
                       'N' as isUdi
                  from BC_BARCODE a, MI_MAST b
                 where a.barcode =  :barcode
                   and a.mmcode = b.mmcode
            ";
            return DBWork.Connection.Query<AB0110>(sql, new { barcode }, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"SELECT {0} MMCODE, MMNAME_C, MMNAME_E, MAT_CLASS, BASE_UNIT,
                               (case when (m_applyid='E' and m_contid='3') then 'Y' else 'N' end) as isSmall
                          FROM MI_MAST  
                         WHERE 1=1 
                           and mat_class = '02'
                           and ( (m_applyid<>'E')  --申請申購識別碼
                                  or (m_applyid='E' and m_contid='3') )  --鎖E但是如果是3零購,就可以申請
                           and m_storeid<>'1'      --庫備識別碼
                           and nvl(cancel_id,'N')='N'      --是否作廢
                           and (M_APPLYID<>'P')  --申請申購識別碼鎖P
                           {1}";

            if (p0 != "")
            {
                sql = string.Format(sql,
                     "(NVL(INSTR(MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,",
                     @"   AND (MMCODE LIKE :MMCODE OR MMNAME_E LIKE UPPER(:MMNAME_E) OR MMNAME_C LIKE :MMNAME_C)");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);
                p.Add(":MMCODE", string.Format("{0}%", p0));
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "", "");
                sql += " ORDER BY MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public int UpdateAckmmcode(string crdocno, string ackmmcode, string userId, string updateIp) {
            string sql = @"
                update CR_DOC a
                   set ackmmcode= :ackmmcode,
                       update_time = sysdate,
                       update_user = :userId,
                       update_ip = :updateIp,
                       (mmname_c, mmname_e, base_unit, cr_uprice, m_paykind, agen_no,
                        email, wexp_id, m_contprice, uprice, issmall)
                       =(select mmname_c, mmname_e, base_unit,
                          (case when (m_applyid='e' and m_contid='3') then uprice
                                else (case when (m_contprice is null or m_contprice=0)
                                           then uprice
                                           else m_contprice
                                      end)
                           end) as cr_uprice,
                           m_paykind, m_agenno,
                           (select email from PH_VENDER where agen_no=b.m_agenno) as email,
                           (case when wexp_id='Y' then 'Y'
                                 else 'N'
                            end) as wexp_id,
                           m_contprice, uprice,
                           (case when (m_applyid='e' and m_contid='3')
                                 then 'Y' else 'N' end)
                          from MI_MAST b
                         where mmcode = :ackmmcode
                        )
                   where crdocno = :crdocno
                   and exists (
                         select 1 from MI_MAST
                          where mmcode = :ackmmcode
                       )
            ";
            return DBWork.Connection.Execute(sql, new { crdocno, ackmmcode, userId, updateIp }, DBWork.Transaction);
        }

        public int DeleteAllD(string crdocno) {
            string sql = @"
                delete from CR_DOC_D
                 where crdocno = :crdocno
            ";
            return DBWork.Connection.Execute(sql, new { crdocno }, DBWork.Transaction);
        }

        public bool CheckDExists(string crdocno) {
            string sql = @"
                select 1 from CR_DOC_D
                 where crdocno = :crdocno
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { crdocno }, DBWork.Transaction) != null;
        }

        public string CheckQty(string crdocno) {
            string sql = @"
                with ackqty as (
                    select  crdocno, sum(inqty) as ackqty
                      from CR_DOC_D
                     where crdocno = :crdocno
                     group by crdocno
                )
                select (case 
                            when b.ackqty = 0 then '0'
                            when a.appqty > b.ackqty then '>'
                            when a.appqty = b.ackqty then '='
                            when a.appqty < b.ackqty then '<'
                            else ''
                         end)
                  from CR_DOC a, ackqty b
                 where a.crdocno = :crdocno
                   and b.crdocno = a.crdocno
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { crdocno }, DBWork.Transaction);
        }

        public int Confirm(string crdocno, string userId, string updateIp) 
        {
            string sql = @"
                update CR_DOC a
                   set ackqty =
                         (select sum(inqty) as ackqty
                            from CR_DOC_D
                           where crdocno=a.crdocno  --上畫面的緊急醫療出貨單編號
                           group by crdocno),
                       cfmqty =  --為結驗帶預設值
                         (select sum(inqty) as ackqty
                            from CR_DOC_D
                           where crdocno=a.crdocno  --上畫面的緊急醫療出貨單編號
                           group by crdocno),
                       acktime = sysdate,
                       ackid = :userId,
                       cr_status = 'I',  --已點收
                       update_user = :userId,
                       update_time = sysdate,
                       update_ip = :updateIp
                 where crdocno = :crdocno
            ";
            return DBWork.Connection.Execute(sql, new { crdocno, userId, updateIp }, DBWork.Transaction);
        }

        public int Reject(string crdocno, string userId, string updateIp)
        {
            string sql = @"
                update CR_DOC
                   set backqty = (case when nvl(inqty,0)=0
                                  then appqty else inqty end),
                       backtime = sysdate,
                       backid = :userId,
                       cr_status = 'J',  --已退回
                       update_user = :userId,
                       update_time = sysdate,
                       update_ip = :updateIp
                 where crdocno = :crdocno
            ";
            return DBWork.Connection.Execute(sql, new { crdocno, userId, updateIp }, DBWork.Transaction);

        }

        public int Import(string crdocno, string userId, string updateIp)
        {
            string sql = @"
                insert into CR_DOC_D 
                       (crdocno, cr_d_seq, lot_no, exp_date, inqty,
                        create_time, create_user, 
                        update_time, update_user, update_ip)
                select crdocno, crdoc_d_seq.nextval as cr_d_seq, lot_no, exp_date, inqty,
                       sysdate, :userId,
                       sysdate, :userId, :updateIp
                  from CR_DOC
                 where crdocno = :crdocno

            ";
            return DBWork.Connection.Execute(sql, new { crdocno, userId, updateIp }, DBWork.Transaction);

        }

        public bool CheckIsAppkykind3(string mmcode) {
            string sql = @"
                select 1 from MI_MAST
                 where mmcode = :mmcode
                   and (m_applyid='E' and m_contid='3')
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { mmcode }, DBWork.Transaction) != null;
        }

        public int MergeCrdoc(string crdocno, string usewhen, string usewhere, string tel) {
            string sql = @"
                merge into CR_DOC_SMALL a
                using (select crdocno from CR_DOC
                        where crdocno=:crdocno) b
                on (a.crdocno=b.crdocno)
                when matched then
                    update set USEWHERE=:usewhere, USEWHEN=:usewhen, TEL=:tel
                when not matched then
                    insert(a.CRDOCNO,a.USEWHERE,a.USEWHEN,a.TEL)
                    values(:crdocno, :usewhere, :usewhen, :tel)
            ";
            return DBWork.Connection.Execute(sql, new { crdocno, usewhen, usewhere, tel }, DBWork.Transaction);
        }
        public int DeleteDocSmall(string crdocno) {
            string sql = @"
                delete from CR_DOC_SMALL
                 where crdocno = :crdocno
            ";
            return DBWork.Connection.Execute(sql, new { crdocno }, DBWork.Transaction);
        }

        public bool CheckCrDocLotNoExists(string crdocno) {
            string sql = @"
                select 1 from CR_DOC
                 where crdocno = :crdocno
                   and (lot_no is not null or exp_date is not null)
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { crdocno }, DBWork.Transaction) != null;
        }
    }
}