using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using BarcodeLib;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace WebApp.Repository.AA
{
    public class AA0144Repository : JCLib.Mvc.BaseRepository
    {
        public AA0144Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public int ClearCfmScan(string userId) {
            string sql = @"
                update CR_DOC
                   set cfmScan = null, scanTime = null
                 where cr_status = 'I'
                   and nvl(cfmscan, 'a') = :userId
            ";
            return DBWork.Connection.Execute(sql, new { userId}, DBWork.Transaction);
        }

        public IEnumerable<AA0144> GetCrDocsList(string crdocno) {
            string sql = string.Format(@"
                select a.crdocno, a.ackmmcode, a.mmname_c, a.mmname_e, a.ackqty,
                       a.cfmqty, a.base_unit, a.wh_name, user_name(a.ackid) as ackid, 
                       twn_time(a.acktime) as acktime,
                       a.agen_no,
                       (select agen_no||' '||agen_namec 
                          from PH_VENDER where agen_no=a.agen_no) as agen_combine,
                       (select agen_namec from PH_VENDER
                         where agen_no=a.agen_no) as agen_namec,
                       a.issmall,
                       a.towh, a.inid,
                       nvl((select avg_price from MI_WHCOST
                             where mmcode=a.mmcode and data_ym=cur_setym
                               and rownum=1),0) as avg_price,
                       a.appid, twn_time(a.apptime) as apptime, twn_date(a.reqdate) as reqdate,
                       'C' as small_m_status,
                       b.tel, b.usewhen, b.usewhere,
                       '緊急醫療結驗後轉單,緊急醫療出貨單編號'||a.crdocno as memo_detail,
                       a.m_paykind, a.cr_uprice
                  from CR_DOC a
                   left join  (select crdocno,tel,usewhen,usewhere from CR_DOC_SMALL) b on (a.crdocno = b.crdocno)
                 where a.cr_status='I'  --已點收
                   and nvl(a.cfmscan, 'a') = 'a'
            ");
            if (string.IsNullOrEmpty(crdocno) == false) {
                sql += "  and a.crdocno = :crdocno";
            }
            return DBWork.PagingQuery<AA0144>(sql, new {crdocno }, DBWork.Transaction);
        }

        public IEnumerable<AA0144> GetCrDocsScan(string userId) {
            string sql = string.Format(@"
                select a.crdocno, a.ackmmcode, a.mmname_c, a.mmname_e, a.ackqty,
                       a.cfmqty, a.base_unit, a.wh_name, user_name(a.ackid) as ackid, twn_time(a.acktime) as acktime,
                       a.agen_no,
                       (select agen_no||' '||agen_namec 
                          from PH_VENDER where agen_no=a.agen_no) as agen_combine,
                       (select agen_namec from PH_VENDER
                         where agen_no=a.agen_no) as agen_namec,
                       a.issmall,
                       a.towh, a.inid,
                       nvl((select avg_price from MI_WHCOST
                             where mmcode=a.mmcode and data_ym=cur_setym
                               and rownum=1),0) as avg_price,
                       a.appid, twn_time(a.apptime) as apptime, twn_date(a.reqdate) as reqdate,
                       'C' as small_m_status,
                       b.tel, b.usewhen, b.usewhere,
                       '緊急醫療結驗後轉單,緊急醫療出貨單編號'||a.crdocno as memo_detail,
                       a.m_paykind, a.cr_uprice
                  from CR_DOC a
                   left join  (select crdocno,tel,usewhen,usewhere from CR_DOC_SMALL) b on (a.crdocno = b.crdocno)
                 where a.cr_status='I'  --已點收
                   and nvl(a.cfmscan, 'a') = :userId
            ");

            return DBWork.PagingQuery<AA0144>(sql, new { userId}, DBWork.Transaction);
        }

        public AA0144 GetCrDoc(string crdocno) {
            string sql = @"
                select a.* , user_name(cfmscan) as cfmscan_name,
                       get_param('CR_DOC', 'CR_STATUS', a.cr_status) as CR_STATUS_NAME
                  from CR_DOC a
                 where crdocno = :crdocno
            ";
            return DBWork.Connection.QueryFirstOrDefault<AA0144>(sql, new { crdocno }, DBWork.Transaction);
        }

        public int UpdateCfmScan(string crdocno, string userId) {
            string sql = @"
                update CR_DOC
                   set cfmScan = :userId, scanTime = sysdate
                 where cr_status = 'I'
                   and nvl(cfmscan, 'a') = 'a'
                   and crdocno = :crdocno
            ";
            return DBWork.Connection.Execute(sql, new { crdocno, userId }, DBWork.Transaction);
        }

        public int Remove(string crdocno) {
            string sql = @"
                update CR_DOC
                   set cfmScan = null, scanTime = null
                 where crdocno = :crdocno
            ";
            return DBWork.Connection.Execute(sql, new { crdocno }, DBWork.Transaction);
        }

        public int Reject(string crdocno, string userId) {
            string sql = @"
                update CR_DOC
                   set backqty = ackqty, backTime = sysdate, backId = :userId,
                       cfmqty = 0, cr_status = 'J'
                 where crdocno = :crdocno
                   and cr_status = 'I'
                   and cfmScan = :userId
            ";
            return DBWork.Connection.Execute(sql, new { crdocno, userId }, DBWork.Transaction);
        }

        public int UpdateCfmQty(string crdocno, string cfmqty, string userId) {
            string sql = @"
                update CR_DOC
                   set cfmqty = :cfmqty, cfmtime = sysdate, cfmid = :userId,
                       backqty = (ackqty - :cfmqty), backtime = sysdate, backId = :userId
                 where crdocno = :crdocno
                   and cr_status = 'I'
                   and nvl(cfmscan, 'a') = 'a'
            ";
            return DBWork.Connection.Execute(sql, new { crdocno, cfmqty, userId }, DBWork.Transaction);
        }


        public int Confirm(string crdocno, string userId) {
            string sql = @"
                update CR_DOC
                   set cfmTime = sysdate,
                       cfmId = :userId,
                       cr_status = 'K'
                 where crdocno = :crdocno
                   and cr_status = 'I'
                   and nvl(cfmscan, 'a') = :userId
            ";
            return DBWork.Connection.Execute(sql, new { crdocno, userId }, DBWork.Transaction);
        }
        public string GetFrwh() {
            string sql = @"
                select wh_no from MI_WHMAST
                 where wh_kind = '1' and wh_grade = '1'
                   and rownum = 1    
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }
        public string GetMedocmDocno(string towh) {
            string sql = @"
                select (:towh||twn_systime||'02') from dual
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { towh}, DBWork.Transaction);
        }
        public bool CheckDocnoExists(string docno) {
            string sql = @"
                select 1 from ME_DOCM
                 where docno = :docno
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { docno }, DBWork.Transaction) != null;
        }

        public int InsertMedocm(string docno, string inid, string frwh, string towh) {
            string sql = @"
               insert into ME_DOCM
                      (DOCNO,DOCTYPE,FLOWID,APPID,APPDEPT,APPTIME,USEID,USEDEPT,
                       FRWH,TOWH,LIST_ID,APPLY_KIND,MAT_CLASS,APPLY_NOTE,
                       CREATE_TIME,CREATE_USER,UPDATE_TIME,UPDATE_USER,
                       SENDAPVID,SENDAPVDEPT,SENDAPVTIME)
                    values
                      (:docno, 'MR4', '2', '緊急醫療出貨', :inid, sysdate, '緊急醫療出貨', :INID,
                       :frwh, :towh, 'N', '1', '02', '三聯單品項產生申請單',
                       sysdate, '緊急醫療出貨', sysdate, '緊急醫療出貨',
                       '緊急醫療出貨', :inid, sysdate)
            ";
            return DBWork.Connection.Execute(sql, new { docno, inid, frwh, towh }, DBWork.Transaction);
        }

        public string GetMedocdSeq(string docno) {
            string sql = @"
                select nvl(max(seq),0)+1 as seq 
                  from ME_DOCD 
                 where docno = :docno
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { docno }, DBWork.Transaction);
        }

        public int InsertMedocd(string docno, string seq, string ackmmcode, string cfmqty,
                                string avg_price, string crdocno)
        {
            string sql = @"
                insert into ME_DOCD
                  (docno, seq, mmcode, appqty, avg_price,
                   aplyitem_note,
                   create_time, create_user, update_time, update_user,
                   expt_distqty, apl_contime)
                values
                  (:docno, :seq, :ackmmcode, :cfmqty, :avg_price,
                   ('緊急醫療出貨所需(三聯單編號'||:crdocno||')'),
                   sysdate, '緊急醫療出貨', sysdate, '緊急醫療出貨',
                   :cfmqty, sysdate)
            ";
            return DBWork.Connection.Execute(sql, new { docno, seq, ackmmcode, cfmqty, avg_price, crdocno }, DBWork.Transaction);
        }
        public int UpdateCrDocRdocno(string crdocno, string dn, string seq) {
            string sql = @"
                update CR_DOC
                   set rdocno = :dn, rseq = :seq, cr_status = 'L'
                 where crdocno = :crdocno
                   and cr_status = 'K'
            ";
            return DBWork.Connection.Execute(sql, new { dn, seq, crdocno }, DBWork.Transaction);
        }
        public int UpdateCrDocSmallDn(string crdocno, string dn, string seq)
        {
            string sql = @"
                update CR_DOC
                   set small_dn = :dn, rseq = :seq, cr_status = 'L'
                 where crdocno = :crdocno
                   and cr_status = 'K'
            ";
            return DBWork.Connection.Execute(sql, new { dn, seq, crdocno }, DBWork.Transaction);
        }

        public string GetPhSmallDn(string inid) {
            string sql = @"
                select 'S' || :inid || TWN_SYSTIME from dual
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { inid }, DBWork.Transaction);
        }

        public int InsertPhSmallM(string dn, string agen_namec, string agen_no, string inid, string appid,
                                  string wh_name, string reqdate, string tel, string usewhen, string usewhere,
                                  string ip)
        {
            string sql = @"
                insert into PH_SMALL_M
                  (DN, AGEN_NAMEC, AGEN_NO, APP_INID, APP_USER,
                   APPTIME, DELIVERY, DUEDATE, STATUS, 
                   ACCEPT, PAYWAY,
                   TEL, USEWHEN, USEWHERE, CREATE_TIME, CREATE_USER,
                   UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                values
                  (:dn, :agen_namec, :agen_no, :inid, :appid,
                   sysdate, :wh_name, twn_todate(:reqdate), 'C',
                   '目視驗收','匯款', 
                   :tel, :usewhen, :usewhere, sysdate, '緊急醫療出貨',
                   sysdate, '緊急醫療出貨', :ip)
            ";
            return DBWork.Connection.Execute(sql, new {
                 dn,
                 agen_namec,
                 agen_no,
                 inid,
                 appid,
                 wh_name,
                 reqdate,
                 tel,
                 usewhen,
                 usewhere,
                 ip
            }, DBWork.Transaction);
        }

        public string GetPhSmallDSeq(string dn) {
            string sql = @"
                select (case when max(SEQ) is null then 1 else max(SEQ)+1 end)
                  from PH_SMALL_D where DN=:dn
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { dn }, DBWork.Transaction);
        }

        public int InsertPhSmallD(string seq, string dn, string memo_detail, string ackmmcode, string inid,
                                  string m_paykind, string mmname_c, string cr_uprice, string cfmqty, string base_unit,
                                  string ip)
        {
            string sql = @"
                insert into PH_SMALL_D
                  (SEQ, DN, MEMO, MMCODE, INID,
                   CHARGE, NMSPEC, PRICE, QTY, UNIT,
                   CREATE_USER, UPDATE_IP, CREATE_TIME)
                values
                  (:seq, :dn, :memo_detail, : ackmmcode, :inid,
                   :m_paykind, :mmname_c, :cr_uprice, :cfmqty, :base_unit,
                   '緊急醫療出貨', :ip ,sysdate)
            ";
            return DBWork.Connection.Execute(sql, new {
                 seq,
                 dn,
                 memo_detail,
                 ackmmcode,
                 inid,
                 m_paykind,
                 mmname_c,
                 cr_uprice,
                 cfmqty,
                 base_unit,
                 ip
            }, DBWork.Transaction);
        }
    }
}