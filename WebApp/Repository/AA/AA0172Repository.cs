using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.AA
{
    public class AA0172Repository : JCLib.Mvc.BaseRepository
    {
        public AA0172Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAllM(string apptime1, string apptime2, string appdept, string[] str_FLOWID, string[] str_matclass, string strMMCode, string strDocNo, string tuser, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.* ,
                        (case when a.mat_class = '01' then (select a.flowId || ' 藥品' || flowName from ME_FLOW where doctype in ('MR', 'MS') and flowId = a.flowId)   
                         else (select DATA_VALUE || ' 衛材' || DATA_DESC from PARAM_D where grp_code = 'ME_DOCM' and data_name = 'FLOWID_MR1' and a.flowid = data_value) end) FLOWID_N,  
                        (SELECT MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS=A.MAT_CLASS) MAT_CLASS_N,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.FRWH) FRWH_N,
                        (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.TOWH) TOWH_N,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='APPLY_KIND' AND DATA_VALUE=A.APPLY_KIND) APPLY_KIND_N ,
                        TWN_DATE(A.APPTIME) APPTIME_T,
                        (case when a.docno = nvl(srcdocno, docno)then 'N' else 'Y' end) SRCDOCYN ,
                        (case when (select count(*) from ME_DOCD c, MI_MAST d where c.docno = a.docno and c.mmcode = d.mmcode and d.wexp_id = 'Y') > 0 then 'Y' else 'N' end) as wexp_yn,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='ISARMY' AND DATA_VALUE=A.ISARMY) ISARMY_N,
                       (select count(*) from ME_DOCD where DOCNO=A.DOCNO and (trim(POSTID) is null or POSTID='A')) as POSTID 
                        FROM ME_DOCM A ,MI_MATCLASS B 
                        WHERE A.MAT_CLASS=B.MAT_CLASS 
                        AND A.DOCTYPE IN ('MR','MS','MR5','MR6')
                        AND A.FRWH in (select wh_no from MI_WHID where wh_userId = :tuser) ";
            p.Add(":tuser", string.Format("{0}", tuser));

            if (apptime1 != "" & apptime2 != "")
            {
                sql += " AND TWN_DATE(A.APPTIME) BETWEEN :d0 AND :d1 ";
                p.Add(":d0", string.Format("{0}", apptime1));
                p.Add(":d1", string.Format("{0}", apptime2));
            }
            if (apptime1 != "" & apptime2 == "")
            {
                sql += " AND TWN_DATE(A.APPTIME) >= :d0 ";
                p.Add(":d0", string.Format("{0}", apptime1));
            }
            if (apptime1 == "" & apptime2 != "")
            {
                sql += " AND TWN_DATE(A.APPTIME) <= :d1 ";
                p.Add(":d1", string.Format("{0}", apptime2));
            }
            if (appdept != "")
            {
                sql += " AND A.TOWH = :p2 ";
                p.Add(":p2", string.Format("{0}", appdept));
            }
            //判斷FLOWID查詢條件是否有值，有的話用字串相加的方式串接條件(IN的方法會有問題)
            if (str_FLOWID.Length > 0)
            {
                sql += @" AND A.FLOWID in :flowids ";
                p.Add(":flowids", str_FLOWID);
            }

            //判斷MAT_CLASS查詢條件是否有值，有的話用字串相加的方式串接條件(IN的方法會有問題)
            if (str_matclass.Length > 0)
            {
                string sql_matclass = "";
                sql += @"AND (";
                foreach (string tmp_matclass in str_matclass)
                {
                    if (string.IsNullOrEmpty(sql_matclass))
                    {
                        if (tmp_matclass.Contains("SUB_"))
                            sql_matclass = @"(select count(*) from ME_DOCD C left join MI_MAST D on C.MMCODE = D.MMCODE where A.DOCNO = C.DOCNO and D.MAT_CLASS_SUB = '" + tmp_matclass.Replace("SUB_", "") + "') > 0";
                        else
                            sql_matclass = @"A.MAT_CLASS = '" + tmp_matclass + "'";
                    }
                    else
                    {
                        if (tmp_matclass.Contains("SUB_"))
                            sql_matclass += @" OR (select count(*) from ME_DOCD C left join MI_MAST D on C.MMCODE = D.MMCODE where A.DOCNO = C.DOCNO and D.MAT_CLASS_SUB = '" + tmp_matclass.Replace("SUB_", "") + "') > 0";
                        else
                            sql_matclass += @" OR A.MAT_CLASS = '" + tmp_matclass + "'";
                    }
                }
                sql += sql_matclass + ") ";
            }

            if (!String.IsNullOrEmpty(strMMCode))
            {
                sql += " AND (select count(*) from ME_DOCD where DOCNO=A.DOCNO and MMCODE= :mmcode)>0";
                p.Add(":mmcode", strMMCode);
            }

            if (!String.IsNullOrEmpty(strDocNo))
            {
                sql += " AND a.docno = :docno";
                p.Add(":docno", strDocNo);
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCD> GetAllD(string DOCNO, string MMCODE, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.DOCNO, A.SEQ, A.MMCODE , A.APPQTY , A.APLYITEM_NOTE, A.EXPT_DISTQTY, TWN_TIME(a.apl_contime) as apl_contime,A.APVTIME, 
                        B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT,B.DISC_UPRICE,
                       (case when (trim(A.POSTID) is null or A.POSTID='A') then INV_QTY(C.FRWH,A.MMCODE) else A.S_INV_QTY end) as S_INV_QTY, 
                       (case when (trim(A.POSTID) is null or A.POSTID='A') then INV_QTY(C.TOWH,A.MMCODE) else A.INV_QTY end) as INV_QTY,
                      ( SELECT MIN_ORDQTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO  IN (SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='1' AND WH_GRADE='1') AND ROWNUM=1) AS TOT_DISTUN,
                        C.FLOWID, A.SHORT_REASON,B.CASENO,TWN_DATE(B.E_CODATE) as E_CODATE,
                        (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='MI_MAST' AND DATA_NAME='M_CONTID' AND DATA_VALUE=B.M_CONTID) as M_CONTID,
                        A.APVQTY,A.ISTRANSPR, A.CHINNAME, A.CHARTNO,
                       (case when A.POSTID IS NULL then '待核可' 
                        else (select DATA_VALUE || ' ' || DATA_DESC from PARAM_D 
                                 where GRP_CODE = 'ME_DOCD' and DATA_NAME = 'POSTID' and DATA_VALUE = A.POSTID) end) as POSTID,
                        (select UNA FROM UR_ID where TUSER=A.DIS_USER) as DIS_USER, TWN_TIME(A.DIS_TIME) as DIS_TIME, A.PR_QTY,
                        (case when (trim(A.POSTID) is null or A.POSTID='A')
                          then INV_QTY(C.FRWH,A.MMCODE)-A.APVQTY-
                                                      nvl((select sum(APVQTY) from ME_DOCM X, ME_DOCD Y 
                                                                 where X.DOCNO=Y.DOCNO and X.FRWH=C.FRWH and Y.MMCODE=A.MMCODE and Y.POSTID='A' and X.DOCNO<>C.DOCNO),0)
                           else A.S_INV_QTY-A.APVQTY-
                                    nvl((select sum(APVQTY) from ME_DOCM X, ME_DOCD Y 
                                                  where X.DOCNO=Y.DOCNO and X.FRWH=C.FRWH and Y.MMCODE=A.MMCODE and Y.POSTID='A' and X.DOCNO<>C.DOCNO),0)
                           end) as REST_QTY,  A.APVQTY AS APVQTY_O,  A.PR_QTY as PRQTY_O
                        FROM ME_DOCD A
                        INNER JOIN MI_MAST B ON B.MMCODE = A.MMCODE 
                        INNER JOIN ME_DOCM C ON C.DOCNO=A.DOCNO
                        LEFT OUTER JOIN V_MM_TOTAPL2 E ON E.DATE_YM=SUBSTR(TWN_DATE((case when c.post_time is null then C.APPTIME else C.post_time end)),0,5) and E.MMCODE=A.MMCODE  and E.TOWH = C.TOWH
                        WHERE  1 = 1 ";

            if (DOCNO != "")
            {
                sql += " AND A.DOCNO LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", DOCNO));
            }
            else
            {
                sql += " AND 1=2 ";
            }
            if (MMCODE != "")
            {
                sql += " AND A.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", MMCODE));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public int UpdateMeDocd(ME_DOCD ME_DOCD)
        {
            var sql = @"UPDATE ME_DOCD SET 
                            EXPT_DISTQTY = nvl(trim(:APVQTY), '0'), 
                            APVQTY = nvl(trim(:APVQTY), '0'), 
                            PR_QTY=nvl(trim(:PR_QTY),'0'),
                            S_INV_QTY=INV_QTY((select FRWH from ME_DOCM where DOCNO=:DOCNO),MMCODE),
                            INV_QTY=INV_QTY((select TOWH from ME_DOCM where DOCNO=:DOCNO),MMCODE),
                            APLYITEM_NOTE = :APLYITEM_NOTE, 
                            UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                        WHERE DOCNO = :DOCNO AND SEQ = :SEQ";
            return DBWork.Connection.Execute(sql, ME_DOCD, DBWork.Transaction);
        }
        public int ApplyX(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCM SET FLOWID = 
                        (case   when doctype in ('MR5','MR6') and flowid='11' then '1' 
                                when doctype ='MR' and flowid = '0111' then '0101' 
                                when doctype ='MS' and flowid = '0611' then '0601' end),
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);

        }
        public int ApplyC(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCM SET FLOWID = 
                        (case   when doctype in ('MR5','MR6') and flowid='2' then '11' 
                                when doctype ='MR' and flowid = '0102' then '0111' 
                                when doctype ='MS' and flowid = '0602' then '0611' end),
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);

        }
        public int ApplyD(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCD A
                        SET EXPT_DISTQTY=(case when APPQTY>(select INV_QTY from MI_WHINV D where MMCODE=A.MMCODE and WH_NO=:FRWH)
                                          then (select INV_QTY from MI_WHINV D where MMCODE=A.MMCODE and WH_NO=:FRWH)
                                          else APVQTY end),
                            APVQTY=(case when APPQTY>(select INV_QTY from MI_WHINV D where MMCODE=A.MMCODE and WH_NO=:FRWH)
                                     then (select INV_QTY from MI_WHINV D where MMCODE=A.MMCODE and WH_NO=:FRWH)
                                     else APVQTY end),
                            ISTRANSPR=(case when APPQTY>(select INV_QTY from MI_WHINV D where MMCODE=A.MMCODE and WH_NO=:FRWH) then 'Y' else 'N' end), 
                            INV_QTY=(select INV_QTY from MI_WHINV D where MMCODE=A.MMCODE and WH_NO=:TOWH),
                            UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP,
                            POSTID='A', DIS_TIME=SYSDATE, DIS_USER=:UPDATE_USER
                      WHERE DOCNO = :DOCNO and trim(POSTID) IS NULL  ";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }
        public int ApplyM(ME_DOCM ME_DOCM)
        {
            var sql = @"UPDATE ME_DOCM SET FLOWID = 
                        (case   when doctype in ('MR5','MR6') and flowid in('11','2')  then '2' 
                                when doctype ='MR' and flowid in ( '0111','0102') then '0102' 
                                when doctype ='MS' and flowid in ( '0611','0602') then '0602' end),
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);

        }
        public bool CheckExistsWhgrade1(string id)
        {
            string sql = @"SELECT 1 FROM MI_WHMAST WHERE wh_no = ( select frwh from ME_DOCM where DOCNO=:DOCNO ) and wh_grade <> '1' ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsOrderdc(string id, string seq)
        {
            string sql = @"SELECT 1 FROM MI_MAST 
                           WHERE mmcode = ( select mmcode from ME_DOCD where DOCNO=:DOCNO and SEQ=:SEQ ) 
                             and (case when nvl(e_orderdcflag,'N')='Y' then 'Y' else 'N' end) = 'Y' ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id, SEQ = seq }, DBWork.Transaction) == null);
        }
        public bool CheckExistsParcde(string id, string seq)
        {
            string sql = @"SELECT 1 FROM MI_MAST 
                           WHERE mmcode = ( select mmcode from ME_DOCD where DOCNO=:DOCNO and SEQ=:SEQ ) 
                             and (case when nvl(e_parcode,'0')='2' then 'Y' else 'N' end) = 'Y' ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id, SEQ = seq }, DBWork.Transaction) == null);
        }
        public bool ChkIstransprExistsVender(string id, string seq)
        {
            string sql = @"select 1 from ME_DOCD A, MI_MAST B, PH_VENDER C
                            where A.MMCODE = B.MMCODE and B.M_AGENNO = C.AGEN_NO
                              and A.DOCNO = :DOCNO and A.SEQ=:SEQ 
                              and A.APPQTY>INV_QTY((select FRWH from ME_DOCM where DOCNO=A.DOCNO),A.MMCODE)
                           union
                           select 1 from ME_DOCD A where A.DOCNO = :DOCNO and A.SEQ=:SEQ 
                              and A.APPQTY<=INV_QTY((select FRWH from ME_DOCM where DOCNO=A.DOCNO),A.MMCODE)";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id, SEQ = seq }, DBWork.Transaction) == null);
        }
        public bool CheckExistsM2(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO and substr(FLOWID, -1, 1)<>'2' ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsM11(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO AND substr(FLOWID, length(FLOWID)-1, 2) <> '11' ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsM11M2(string id)
        {
            string sql = @"select 1 from ME_DOCM where DOCNO=:DOCNO and to_number(substr(FLOWID, length(FLOWID)-1, 2)) not in (11,2) ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsMSrc(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO AND nvl(SRCDOCNO, DOCNO) <> DOCNO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsPr(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO AND nvl(isTransPr,'N') = 'Y' ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }

        public int InsertMEDOCM(ME_DOCM ME_DOCM)
        {
            var sql = @"insert into ME_DOCM (
                            docno, doctype, flowid, appid, appdept, 
                            apptime, useId, useDept, frwh, towh, 
                            apply_kind, apply_note, mat_class, create_time, create_user, 
                            update_time, update_user, update_ip, isContid3, srcdocno, appuna)
                        select :ITEM_STRING as docno, a.doctype as doctype, 
                           (case when a.doctype in ('MR5','MR6') then '2' when a.doctype ='MR' then '0102' when a.doctype='MS' then '0602' end) as flowId, 
                            a.appid, a.appdept, a.apptime, a.useId, a.useDept, a.frwh, a.towh, 
                            a.apply_kind, ('轉申購產生新單據 '||a.apply_note) as apply_note, a.mat_class, sysdate as create_time, :UPDATE_USER as create_user, 
                            sysdate as update_time, :UPDATE_USER as update_user, :UPDATE_IP as update_ip, a.isContid3, a.srcdocno, a.APPUNA
                        from ME_DOCM a where a.docno = :DOCNO ";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);

        }

        public int InsertMEDOCD(ME_DOCM ME_DOCM)
        {
            var sql = @"insert into ME_DOCD (
                            docno, seq, mmcode, appqty, APVQTY, EXPT_DISTQTY, avg_price, aplyitem_note, gtapl_reson,
                            m_contprice, uprice, disc_cprice, disc_uprice, m_nhikey, M_AGENNO,
                            s_inv_qty, inv_qty, oper_qty, apl_contime, CHINNAME, CHARTNO,                           
                            create_time, create_user, update_time, update_user, update_ip, isTransPr)
                        select :ITEM_STRING as docno, a.seq, a.mmcode,
                            (a.appqty-(select INV_QTY from MI_WHINV D where MMCODE=a.MMCODE and WH_NO=b.FRWH)) as appqty, 
                            (a.appqty-(select INV_QTY from MI_WHINV D where MMCODE=a.MMCODE and WH_NO=b.FRWH)) as apvqty, 
                            (a.appqty-(select INV_QTY from MI_WHINV D where MMCODE=a.MMCODE and WH_NO=b.FRWH)) as expt_distqty,
                             a.avg_price, a.aplyitem_note, a.gtapl_reson, 
                             a.m_contprice, a.uprice, a.disc_cprice,  a.disc_uprice, a.m_nhikey, a.M_AGENNO,
                             a.s_inv_qty, a.inv_qty, a.oper_qty, a.apl_contime, a.CHINNAME, a.CHARTNO, 
                             sysdate as create_time, :UPDATE_USER as create_user, sysdate as update_time, 
                             :UPDATE_USER as update_user, :UPDATE_IP as update_ip,'N' as isTransPr 
                        from ME_DOCD a, ME_DOCM b 
                       where a.DOCNO=b.DOCNO and a.docno = :DOCNO  and nvl(a.isTransPr,'N') = 'Y' ";
            // and a.APPQTY>(select INV_QTY from MI_WHINV D where MMCODE=a.MMCODE and WH_NO=b.FRWH)";
            return DBWork.Connection.Execute(sql, ME_DOCM, DBWork.Transaction);
        }

        public int InsertMMPRM(MM_PR_M mm_pr_m, string[] str_DOCNO)
        {
            var sql = @"insert into MM_PR_M (
                            pr_no, pr_dept, pr_time, pr_user, mat_class, 
                            pr_status, create_time, create_user, update_ip, xaction, isFromDocm, memo)
                        values (
                            :PR_NO, user_inid(:UPDATE_USER), sysdate, :UPDATE_USER, :MAT_CLASS,
                            '35', sysdate, :UPDATE_USER, :UPDATE_IP, '0', 'Y', 
                            (select (listagg(APPLY_NOTE,',') within group (order by APPLY_NOTE))
                              from (select distinct APPLY_NOTE from  ME_DOCM where 1=1";
            if (str_DOCNO.Length > 0)
            {
                string sql_DOCNO = "";
                sql += @" AND (";
                foreach (string tmp_DOCNO in str_DOCNO)
                {
                    if (string.IsNullOrEmpty(sql_DOCNO))
                    {
                        sql_DOCNO = @"DOCNO = '" + tmp_DOCNO + "'";
                    }
                    else
                    {
                        sql_DOCNO += @" OR DOCNO = '" + tmp_DOCNO + "'";
                    }
                }
                sql += sql_DOCNO + "))) ";
            }
            sql += " )";
            return DBWork.Connection.Execute(sql, mm_pr_m, DBWork.Transaction);

        }

        public int InsertMMPRM01(MM_PR_M mm_pr_m, string[] str_DOCNO)
        {
            var sql = @"insert into MM_PR01_M (
                            pr_no, pr_dept, pr_time, pr_user, mat_class, 
                            create_time, create_user, update_ip, isFromDocm, memo)
                        values (
                            :PR_NO, user_inid(:UPDATE_USER), sysdate, :UPDATE_USER, '01',
                            sysdate, :UPDATE_USER, :UPDATE_IP, 'Y',
                            (select (listagg(APPLY_NOTE, ',') within group(order by APPLY_NOTE))
                               from (select distinct APPLY_NOTE from ME_DOCM where 1 = 1";
            if (str_DOCNO.Length > 0)
            {
                string sql_DOCNO = "";
                sql += @" AND (";
                foreach (string tmp_DOCNO in str_DOCNO)
                {
                    if (string.IsNullOrEmpty(sql_DOCNO))
                    {
                        sql_DOCNO = @"DOCNO = '" + tmp_DOCNO + "'";
                    }
                    else
                    {
                        sql_DOCNO += @" OR DOCNO = '" + tmp_DOCNO + "'";
                    }
                }
                sql += sql_DOCNO + "))) ";
            }
            sql += " )";
            return DBWork.Connection.Execute(sql, mm_pr_m, DBWork.Transaction);

        }
        public int InsertMMPRD(MM_PR_M mm_pr_m, string[] str_DOCNO)
        {
            var sql = @"insert into MM_PR_D (
                            pr_no, mmcode, m_contid, m_contprice, m_purun, pr_price,  pr_qty, req_qty_t, unit_swap, 
                            agen_fax, agen_name, agen_no,  disc, is_email, rec_status, m_nhikey,
                            disc_cprice, disc_uprice, uprice, m_discperc,
                            mmname_c,mmname_e,wexp_id,base_unit,orderkind,caseno,
                            e_sourcecode,unitrate,e_codate,seq,chinname,chartno,e_itemarmyno,
                            create_time, create_user, update_time, update_user, update_ip
                            )
                           select p.pr_no,p.mmcode, p.m_contid, p.m_contprice, p.m_purun, p.pr_price, p.pr_qty, p.req_qty_t, p.unit_swap,
                                      p.agen_fax, p.agen_name, p.agen_no, p.disc, p.is_email, p.rec_status, p.m_nhikey, 
                                      p.disc_cprice, p.disc_uprice, p.uprice, p.m_discperc,
                                      p.mmname_c,p.mmname_e,p.wexp_id,p.base_unit,p.orderkind,p.caseno,
                                      p.e_sourcecode,p.unitrate,p.e_codate,MM_PR_D_SEQ.nextval,p.chinname,p.chartno,p.e_itemarmyno,
                                     sysdate as create_time, :UPDATE_USER as create_user, sysdate as update_time,
                                      :UPDATE_USER as update_user, :UPDATE_IP as update_ip
                          from(
                               select  :PR_NO as pr_no, a.mmcode as mmcode, b.m_contid as m_contid, 
                                            b.m_contprice as m_contprice, b.m_purun as m_purun, 
                                           (case when   (select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode') ='805'
                                                then b.disc_cprice 
                                                else (case when b.m_contid='0' then b.m_contprice else b.disc_cprice end ) 
                                            end) as pr_price, sum(a.appqty) as pr_qty, 
                                            sum(a.appqty-a.apvqty)  as req_qty_t, 1 AS unit_swap,
                                            d.agen_fax as agen_fax, d.agen_namec as agen_name, b.m_agenno as agen_no, 
                                            b.m_discperc as disc, 'N' as is_email, '35' as rec_status, b.m_nhikey,
                                            b.disc_cprice, b.disc_uprice, b.uprice, b.m_discperc,
                                            b.mmname_c, b.mmname_e, b.wexp_id, b.base_unit, b.orderkind, b.caseno,
                                            b.e_sourcecode, b.unitrate, b.e_codate,a.chinname,a.chartno,b.e_itemarmyno
                                  from ME_DOCM e, ME_DOCD a, MI_MAST b, PH_VENDER d
                                where e.doctype in ('MR5','MR6')
                                    and e.mat_class = :MAT_CLASS
                                    and a.docno = e.docno and b.mmcode = a.mmcode  and d.agen_no = b.m_agenno  ";

            if (str_DOCNO.Length > 0)
            {
                string sql_DOCNO = "";
                sql += @" AND (";
                foreach (string tmp_DOCNO in str_DOCNO)
                {
                    if (string.IsNullOrEmpty(sql_DOCNO))
                    {
                        sql_DOCNO = @"E.DOCNO = '" + tmp_DOCNO + "'";
                    }
                    else
                    {
                        sql_DOCNO += @" OR E.DOCNO = '" + tmp_DOCNO + "'";
                    }
                }
                sql += sql_DOCNO + ") ";
            }
            sql += @"  group by a.mmcode, b.m_contid, b.m_contprice, b.m_purun, 
                                                   (case when b.m_contid='0' then b.m_contprice else b.disc_cprice end),
                                                   d.agen_fax, d.agen_namec, b.m_agenno, b.m_discperc, b.m_nhikey,
                                                   b.disc_cprice, b.disc_uprice, b.uprice, b.m_discperc,
                                                   b.mmname_c, b.mmname_e, b.wexp_id, b.base_unit, b.orderkind, b.caseno,
                                                   b.e_sourcecode, b.unitrate, b.e_codate, a.chinname, a.chartno ,b.e_itemarmyno";
            sql += " ) p";
            return DBWork.Connection.Execute(sql, mm_pr_m, DBWork.Transaction);
        }

        public int InsertMMPRD01(MM_PR_D mm_pr_d)
        {
            var sql = @"insert into MM_PR01_D (
                            pr_no, mmcode, pr_qty, 
                            pr_price, m_purun, base_unit,
                            m_contprice, m_discperc, unit_swap, 
                            req_qty_t, agen_no, m_contid, 
                            agen_name, email, agen_fax, 
                            pr_amt,  min_ordqty, disc_cprice, 
                            disc_uprice, uprice, isWilling, 
                            discount_qty, disc_cost_uprice, safe_qty, oper_qty, ship_qty, 
                            mmname_c, mmname_e, wexp_id, orderkind, caseno, e_sourcecode, unitrate,m_nhikey, e_codate,chinname,chartno,
                            create_time, create_user, update_time, update_user, update_ip
                            )
                        select
                            :PR_NO as pr_no, a.mmcode, :PR_QTY as pr_qty, 
                            (case when   (select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode') ='805'
                                then a.disc_cprice 
                                else  (case when  :M_CONTID='0' then a.m_contprice else a.disc_cprice end)  
                            end)  as pr_price, a.m_purun, a.base_unit, 
                            a.m_contprice, a.m_discperc, 1 as unit_swap,
                            :PR_QTY as rep_qty_t, a.m_agenno as agen_no, :M_CONTID as m_contid,
                            b.agen_namec as agen_name, b.email, b.agen_fax,
                            :PR_QTY * a.m_contprice as pr_amt, nvl(c.min_ordqty,1) as min_ordqty, a.disc_cprice,
                            a.disc_uprice, a.uprice,
                            (select isWilling from MILMED_JBID_LIST where substr(a.E_YRARMYNO,1,3)= JBID_STYR and a.E_ITEMARMYNO=BID_NO) as isWilling, 
                            (select discount_qty from MILMED_JBID_LIST where substr(a.E_YRARMYNO,1,3)= JBID_STYR and a.E_ITEMARMYNO=BID_NO) as discount_qty, 
                            (select disc_cost_uprice from MILMED_JBID_LIST where substr(a.E_YRARMYNO,1,3)= JBID_STYR and a.E_ITEMARMYNO=BID_NO) as disc_cost_uprice,
                            nvl(c.safe_qty,0) as safe_qty, nvl(c.oper_qty,0) as oper_qty, nvl(c.ship_qty, 0) as ship_qty, 
                            a.mmname_c, a.mmname_e, a.wexp_id, a.orderkind, a.caseno, a.e_sourcecode, a.unitrate,a.m_nhikey, a.e_codate,:CHINNAME as chinname, :CHARTNO as chartno,
                            sysdate as create_time, :UPDATE_USER as create_user, sysdate as update_time, :UPDATE_USER as update_user, :UPDATE_IP as update_ip 
                        from MI_MAST a
                        left join PH_VENDER b on (a.m_agenno = b.agen_no)
                        left join MI_WINVCTL c on (c.wh_no = WHNO_ME1 and c.mmcode = a.mmcode)
                        where a.mmcode = :MMCODE
                        and nvl(a.e_orderdcflag,'N') = 'N'
                         ";
            return DBWork.Connection.Execute(sql, mm_pr_d, DBWork.Transaction);

        }
        public int UpdateDReason(string docno, string newdocno)
        {
            var sql = @"update ME_DOCD set short_reason = '進貨待撥，新單號'|| :NEWDOCNO 
                        where docno = :DOCNO and nvl(isTransPr,'N') = 'Y' ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, NEWDOCNO = newdocno }, DBWork.Transaction);

        }
        public int UpdateDTransPr(string docno, string seq, string isTransPr)
        {
            var sql = @"update ME_DOCD set isTransPr = :ISTRANSPR 
                         where docno = :DOCNO and seq = :SEQ ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq, ISTRANSPR = isTransPr }, DBWork.Transaction);

        }
        public int ChkIsTransPr(string docno)
        {
            // 檢查所選申請單內是否需註記轉申購單品項(申請量超過庫存量)
            string sql = @"select count(*) from ME_DOCD A
                            where A.DOCNO = :DOCNO
                              and A.APPQTY>INV_QTY((select FRWH from ME_DOCM where DOCNO=A.DOCNO),A.MMCODE) ";
            return DBWork.Connection.QueryFirst<int>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }
        public bool CheckExistsD(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsM(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCM WHERE DOCNO=:DOCNO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsMMPRM(string id)
        {
            string sql = @"select 1 from MM_PR_M where pr_no = :PR_NO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { PR_NO = id }, DBWork.Transaction) == null);
        }
        public string GetIstransprDocno(string id)
        {
            string sql = @"select towh||twn_systime||mat_class from ME_DOCM WHERE DOCNO=:DOCNO ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetMRMSDocno()
        {
            var p = new OracleDynamicParameters();
            p.Add("O_DOCNO", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 12); // 務必要填上size,不然取值都會是null

            DBWork.Connection.Query("GET_DOCNO", p, commandType: CommandType.StoredProcedure);
            return p.Get<OracleString>("O_DOCNO").Value;
        }

        public string GetPrno(string id)
        {
            string sql = @"select whno_mm1||twn_systime||:MAT_CLASS from dual ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { MAT_CLASS = id }, DBWork.Transaction).ToString();
            return rtn;
        }

        public string GetPrno01()
        {
            string sql = @"select twn_systime from dual ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, null, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetDoctype(string id)
        {
            string sql = @"SELECT DOCTYPE FROM ME_DOCM where DOCNO = :DOCNO ";
            string rtn = Convert.ToString(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction));
            return rtn;
        }
        public string GetTowh(string id)
        {
            string sql = @"SELECT TOWH FROM ME_DOCM where DOCNO = :DOCNO ";
            string rtn = Convert.ToString(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction));
            return rtn;
        }
        public IEnumerable<COMBO_MODEL> GetFlowidCombo(string id)
        {
            string sql = @"
                with temp_whkinds as (
                     select b.wh_no, b.wh_kind, nvl((case when a.task_id = '3' then '2' else a.task_id end), '2') as task_id, b.wh_grade
                       from MI_WHID a, MI_WHMAST b
                      where wh_userid = :TUSER
                        and a.wh_no = b.wh_no
                        and ((b.wh_grade in ('1','2') and b.wh_kind ='0')
                                  or (b.wh_grade ='1' and b.wh_kind ='1'))
                ), temp_mat_class as (
                     select distinct b.mat_class as value,b.mat_clsname as text,b.mat_class || ' ' ||  b.mat_clsname as COMBITEM 
                       from temp_whkinds a, MI_MATCLASS b
                      where (a.task_id = b.mat_clsid)
                     union
                     select distinct b.mat_class as value,b.mat_clsname as text,b.mat_class || ' ' ||  b.mat_clsname as COMBITEM  
                       from temp_whkinds a, MI_MATCLASS b
                      where (a.task_id = '2')
                            and b.mat_clsid = '3'
                            )
                 select p.* from(
                     select b.flowid as VALUE, '藥品'||b.flowName as text,flowId || ' 藥品' || flowName as COMBITEM,
                                 (case when to_number(substr(b.flowid, length(b.flowid)-1,2)) in (11,2) then 'Y' else 'N' end) as extra1,
                                 (case when to_number(substr(b.flowid, length(b.flowid)-1,2))=11 then 1.1 
                                            when to_number(substr(b.flowid, length(b.flowid)-1,2))=51 then 5.1
                                   else to_number(substr(b.flowid, length(b.flowid)-1,2)) end) orderby
                       from temp_mat_class a, temp_whkinds c, ME_FLOW b
                     where a.value = '01' 
                         and (b.doctype in ('MR') and c.wh_grade = '1')
                         and b.flowid not in ('0100', '0104')
                     union
                     select DISTINCT DATA_VALUE as VALUE, '衛材'||DATA_DESC as TEXT,DATA_VALUE || ' 衛材' || DATA_DESC as COMBITEM,
                                 (case when to_number(substr(b.data_value, length(b.data_value)-1,2)) in (11,2)  then 'Y'  else 'N' end) as extra1,
                                 (case when to_number(substr(b.data_value, length(b.data_value)-1,2))=11 then 1.1 
                                           when to_number(substr(b.data_value, length(b.data_value)-1,2))=51 then 5.1
                                  else to_number(substr(b.data_value, length(b.data_value)-1,2)) end) orderby
                      from temp_mat_class a, PARAM_D b
                    where a.value in ('02', '03', '04', '05', '06', '07', '08') 
                        and b.DATA_VALUE not in ('5', '51')
                        and b.GRP_CODE='ME_DOCM' AND b.DATA_NAME='FLOWID_MR1'
                   )p order by substr(COMBITEM,1,2), orderby  ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id });
        }
        public IEnumerable<COMBO_MODEL> GetApplyKindCombo(string id)
        {
            string sql = @"with temp_whkinds as (
                            select b.wh_no, b.wh_kind, nvl((case when a.task_id = '3' then '2' else a.task_id end), '2') as task_id
                            from MI_WHID a, MI_WHMAST b
                            where wh_userid = :TUSER
                            and a.wh_no = b.wh_no
                            and ((b.wh_grade in ('1','2') and a.task_id ='1')
                                  or (b.wh_grade ='1' and a.task_id in ('2','3')))
                            ), temp_mat_class as (
                            select distinct b.mat_class as value,b.mat_clsname as text,b.mat_class || ' ' ||  b.mat_clsname as COMBITEM 
                            from temp_whkinds a, MI_MATCLASS b
                            where (a.task_id = b.mat_clsid)
                            union
                            select distinct b.mat_class as value,b.mat_clsname as text,b.mat_class || ' ' ||  b.mat_clsname as COMBITEM  
                            from temp_whkinds a, MI_MATCLASS b
                            where (a.task_id = '2')
                            and b.mat_clsid = '3'
                            )
                            select b.flowid as value, '藥品'||b.flowName as text,flowId || ' 藥品' || flowName as COMBITEM
                            from temp_mat_class a, ME_FLOW b
                            where a.value = '01' 
                            and b.doctype in ('MS')
                            union
                            select DISTINCT DATA_VALUE as VALUE, '衛材'||DATA_DESC as TEXT,DATA_VALUE || ' 衛材' || DATA_DESC as COMBITEM 
                            from temp_mat_class a, PARAM_D b
                            where a.value in ('02', '03', '04', '05', '06', '07', '08') 
                            and b.GRP_CODE='ME_DOCM' AND b.DATA_NAME='FLOWID_MR1'
                            ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id });
        }
        public IEnumerable<COMBO_MODEL> GetAppDeptCombo(string id)
        {
            var p = new DynamicParameters();

            string sql = @"with temp_grade1_kind0 as (
                                  select distinct wh_grade, wh_kind 
                                    from MI_WHID a, MI_WHMAST b
                                   where a.wh_userid = :TUSER
                                and a.wh_no = b.wh_no
                                and b.wh_grade in ('1', '2')
                                and b.wh_kind = '0'
                                ), temp_grade2_kind0 as (
                                select distinct wh_grade, wh_kind  
                                    from MI_WHID a, MI_WHMAST b
                                   where a.wh_userid = :TUSER
                                and a.wh_no = b.wh_no
                                and b.wh_grade = '2'
                                and b.wh_kind = '0'
                                ), temp_grade1_kind1 as (
                                select distinct wh_grade, wh_kind  
                                    from MI_WHID a, MI_WHMAST b
                                   where a.wh_userid = :TUSER
                                and a.wh_no = b.wh_no
                                and b.wh_grade = '1'
                                and b.wh_kind = '1'
                                )
                                select b.wh_no as value, b.wh_name as text, b.wh_no||' '||b.wh_name as combitem 
                                from temp_grade1_kind0 a, MI_WHMAST b
                                where a.wh_kind = b.wh_kind
                                and a.wh_grade < b.wh_grade
                                and nvl(b.cancel_id, 'N') = 'N'
                                and b.wh_grade not in ('5','M','S')
                                union
                                select b.wh_no as value, b.wh_name as text, b.wh_no||' '||b.wh_name as combitem 
                                from temp_grade2_kind0 a, MI_WHMAST b
                                where a.wh_kind = b.wh_kind
                                and a.wh_grade < b.wh_grade
                                and nvl(b.cancel_id, 'N') = 'N'
                                and b.wh_grade not in ('5','M','S')
                                union
                                select b.wh_no as value, b.wh_name as text, b.wh_no||' '||b.wh_name as combitem  
                                from temp_grade1_kind1 a, MI_WHMAST b
                                where a.wh_kind = b.wh_kind
                                and a.wh_grade < b.wh_grade
                                and nvl(b.cancel_id, 'N') = 'N'
                                and b.wh_grade not in ('5','M','S')
                              ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id });
        }
        public IEnumerable<MI_MAST> GetMMCodeDocd(string p0, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E  
                        FROM MI_MAST A, ME_DOCD B WHERE A.MMCODE=B.MMCODE AND B.DOCNO = :DOCNO  ";
            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(MMNAME_E), UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (UPPER(A.MMCODE) LIKE UPPER(:MMCODE) ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR UPPER(MMNAME_E) LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR UPPER(MMNAME_C) LIKE UPPER(:MMNAME_C)) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY MMCODE ";
            }
            p.Add(":DOCNO", p1);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<COMBO_MODEL> GetTowhCombo(string id)
        {
            string sql = @"SELECT DISTINCT A.WH_NO as VALUE, A.WH_NAME as TEXT,
                        A.WH_NO || ' ' || A.WH_NAME as COMBITEM 
                        FROM MI_WHMAST A,UR_ID B
                        WHERE A.INID=B.INID 
                        AND A.WH_KIND = '1' 
                        AND TUSER=:TUSER 
                        ORDER BY WH_NO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id });
        }
        public IEnumerable<COMBO_MODEL> GetMatclassCombo(string id)
        {
            string sql = @"  with temp_whkinds as (
                            select b.wh_no, b.wh_kind, 
                                             nvl((case when b.wh_kind = '0' then '1' else '2' end), '2') as task_id
                            from MI_WHID a, MI_WHMAST b
                            where wh_userid = :TUSER
                                       and a.wh_no = b.wh_no
                                       and ((b.wh_grade in ('1', '2') and b.wh_kind = '0')
                                            or (b.wh_grade = '1' and b.wh_kind = '1')
                                           )
                            )
                            select distinct b.mat_class as value,
                                         '全部'||b.mat_clsname as text,
                                         b.mat_class || ' ' ||  '全部'||b.mat_clsname as COMBITEM 
                                  from temp_whkinds a, MI_MATCLASS b
                                 where (a.task_id = b.mat_clsid)
                                union
                                select 'SUB_' || b.data_value as value, b.data_desc as text,
                                b.data_value || ' ' || b.data_desc as COMBITEM 
                                from temp_whkinds a, PARAM_D b
	                             where b.grp_code ='MI_MAST' 
	                               and b.data_name = 'MAT_CLASS_SUB'
	                               and b.data_value = '1'
	                               and trim(b.data_desc) is not null
                                   and (a.task_id = '1')
                                union
                                select 'SUB_' || b.data_value as value, b.data_desc as text,
                                b.data_value || ' ' || b.data_desc as COMBITEM 
                                from temp_whkinds a, PARAM_D b
	                             where b.grp_code ='MI_MAST' 
	                               and b.data_name = 'MAT_CLASS_SUB'
	                               and b.data_value <> '1'
	                               and trim(b.data_desc) is not null
                                   and (a.task_id = '2')
                            ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id });
        }
        public IEnumerable<MM_PR_M> GetMatClassList(string[] str_DOCNO)
        {
            string sql = @"SELECT DISTINCT A.MAT_CLASS FROM ME_DOCM A WHERE 1=1 ";
            if (str_DOCNO.Length > 0)
            {
                string sql_DOCNO = "";
                sql += @"AND (";
                foreach (string tmp_DOCNO in str_DOCNO)
                {
                    if (string.IsNullOrEmpty(sql_DOCNO))
                    {
                        sql_DOCNO = @"A.DOCNO = '" + tmp_DOCNO + "'";
                    }
                    else
                    {
                        sql_DOCNO += @" OR A.DOCNO = '" + tmp_DOCNO + "'";
                    }
                }
                sql += sql_DOCNO + ") ";
            }
            sql += " ORDER BY A.MAT_CLASS ";
            return DBWork.Connection.Query<MM_PR_M>(sql);
        }
        public IEnumerable<MM_PR_D> GetMmprd(string[] str_DOCNO)
        {
            string sql = @" select a.mmcode, ceil(appqty/nvl(b.min_ordqty, 1))* nvl(b.min_ordqty, 1) as pr_qty, nvl(b.min_ordqty, 1) as min_ordqty, m_contid, chinname, chartno
                            from
                            (   select a.mmcode, sum(a.appqty) as appqty, 
                                    (case when b.contracno in ('1','01','2','02','3','03') then '0' else '2' end) as m_contid, b.m_contprice, b.m_discperc, 
                                    b.disc_cprice, b.disc_uprice, b.uprice, b.base_unit, b.m_purun, b.m_agenno, c.agen_namec, c.agen_fax, a.chinname, a.chartno
                                from ME_DOCD a, MI_MAST b
                                left join PH_VENDER c on b.m_agenno = c.agen_no 
                                WHERE b.mmcode = a.mmcode ";
            if (str_DOCNO.Length > 0)
            {
                string sql_DOCNO = "";
                sql += @"AND (";
                foreach (string tmp_DOCNO in str_DOCNO)
                {
                    if (string.IsNullOrEmpty(sql_DOCNO))
                    {
                        sql_DOCNO = @"a.DOCNO = '" + tmp_DOCNO + "'";
                    }
                    else
                    {
                        sql_DOCNO += @" OR a.DOCNO = '" + tmp_DOCNO + "'";
                    }
                }
                sql += sql_DOCNO + ") ";
            }
            sql += @"       group by a.mmcode,b.contracno, b.m_contprice, b.m_discperc, 
                                b.disc_cprice, b.disc_uprice, b.uprice, b.base_unit, b.m_purun, b.m_agenno, c.agen_namec, c.agen_fax, a.chinname, a.chartno
                            ) a
                            left join MI_WINVCTL b on(a.mmcode = b.mmcode and b.wh_no = WHNO_ME1)
                            where 1 = 1 and a.m_contid = '0'
                            union
                            select a.mmcode, ceil(appqty / nvl(b.min_ordqty, 1)) * nvl(b.min_ordqty, 1) as pr_qty, nvl(b.min_ordqty, 1) as min_ordqty, m_contid, chinname, chartno
                            from
                            (
                                select a.mmcode, sum(a.appqty) as appqty,
                                (case when b.contracno in ('1', '01', '2', '02', '3', '03') then '0' else '2' end) as m_contid, b.m_contprice, b.m_discperc, 
                                b.disc_cprice, b.disc_uprice, b.uprice, b.base_unit, b.m_purun, b.m_agenno, c.agen_namec, c.agen_fax, a.chinname, a.chartno
                                from ME_DOCD a, MI_MAST b
                                left join PH_VENDER c on b.m_agenno = c.agen_no 
                                WHERE b.mmcode = a.mmcode ";
            if (str_DOCNO.Length > 0)
            {
                string sql_DOCNO = "";
                sql += @"AND (";
                foreach (string tmp_DOCNO in str_DOCNO)
                {
                    if (string.IsNullOrEmpty(sql_DOCNO))
                    {
                        sql_DOCNO = @"a.DOCNO = '" + tmp_DOCNO + "'";
                    }
                    else
                    {
                        sql_DOCNO += @" OR a.DOCNO = '" + tmp_DOCNO + "'";
                    }
                }
                sql += sql_DOCNO + ") ";
            }

            sql += @"       group by a.mmcode,b.contracno, b.m_contprice, b.m_discperc, 
                                b.disc_cprice, b.disc_uprice, b.uprice, b.base_unit, b.m_purun, b.m_agenno, c.agen_namec, c.agen_fax, a.chinname, a.chartno
                            ) a
                            left join MI_WINVCTL b on(a.mmcode = b.mmcode and b.wh_no = WHNO_ME1)
                            where 1 = 1 and a.m_contid = '2' ";
            return DBWork.Connection.Query<MM_PR_D>(sql);
        }
        public string GetDailyDocno()
        {
            string sql = @"select GET_DAILY_DOCNO from DUAL";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }
        #region 2020-05-06 檢核需對應到院內碼
        public IEnumerable<ME_DOCD> GetAllDocds(string docno)
        {
            string sql = "select * from ME_DOCD where docno = :docno";

            return DBWork.Connection.Query<ME_DOCD>(sql, new { docno = docno }, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetDocds(string docno, string seq)
        {
            string sql = "select * from ME_DOCD where docno = :docno and seq = :seq";

            return DBWork.Connection.Query<ME_DOCD>(sql, new { docno = docno, seq = seq }, DBWork.Transaction);
        }
        public bool CheckExptQtyMmcode(string docno, string mmcode)
        {
            string sql = @"select 1 from ME_DOCD A,ME_DOCM C
                            where A.DOCNO=C.DOCNO and A.DOCNO=:DOCNO and A.MMCODE = :MMCODE
                              and (case when A.APPQTY>INV_QTY(C.FRWH,A.MMCODE)  then INV_QTY(C.FRWH,A.MMCODE)
                                   else A.APPQTY end) > INV_QTY(C.FRWH,A.MMCODE) ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno, mmcode = mmcode }, DBWork.Transaction) == null);
        }
        #endregion

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"
                                    SELECT {0} 
                                           a.mmcode,
                                           a.mmname_c,
                                           a.mmname_e
                                     FROM  MI_MAST a
                                    WHERE  1 = 1
                                      AND  nvl(cancel_id, 'N') = 'N'
                                ";

            if (!string.IsNullOrWhiteSpace(p0))
            {
                sql = string.Format(sql, "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(A.MMNAME_E), UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(A.MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql = string.Format("SELECT * FROM ({0}) TMP WHERE 1=1 ", sql);

                sql += " AND (UPPER(MMCODE) LIKE UPPER(:MMCODE) ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

                sql += " OR UPPER(MMNAME_E) LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR UPPER(MMNAME_C) LIKE UPPER(:MMNAME_C)) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public DataTable GetExcel(string[] str_DOCNO)
        {
            DynamicParameters p = new DynamicParameters();

            var sql = @"SELECT a.docno 單據號碼, twn_date(apptime) 申請日期, 
                          (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.FRWH) 出庫庫房,
                          (SELECT WH_NO || ' ' || WH_NAME from MI_WHMAST where WH_NO=A.TOWH) 入庫庫房,
                          (case when a.docno = nvl(a.srcdocno, a.docno)then 'N' else 'Y' end) 轉申購單,
                          (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='ISARMY' AND DATA_VALUE=A.ISARMY) 軍民別,
                          a.appuna 申請人姓名,a.apply_note 單據備註,
                          d.mmcode 院內碼, B.MMNAME_C 中文品名, B.MMNAME_E 英文品名,
                          (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='MI_MAST' AND DATA_NAME='M_CONTID' AND DATA_VALUE=b.M_CONTID) 合約識別碼,
                          b.caseno 合約案號, TWN_DATE(B.E_CODATE) as 合約效期,
                          INV_QTY(a.FRWH,d.MMCODE) as 上級庫庫房存量,
                          INV_QTY(a.TOWH,d.MMCODE)  as 庫房存量,
                          d.DISC_UPRICE 優惠最小單價, d.appqty 申請量, d.APVQTY 核撥量, 
                        d.ISTRANSPR as  轉申購 ,d.SHORT_REASON 欠撥原因, d.APLYITEM_NOTE 明細備註, d.CHINNAME 病患姓名 
                         from ME_DOCM a, ME_DOCD d
                        INNER JOIN MI_MAST B ON B.MMCODE = d.MMCODE 
                        where a.docno = d.docno ";

            if (str_DOCNO.Length > 0)
            {
                string sql_DOCNO = "";
                sql += @"AND (";
                foreach (string tmp_DOCNO in str_DOCNO)
                {
                    if (string.IsNullOrEmpty(sql_DOCNO))
                    {
                        sql_DOCNO = @"a.DOCNO = '" + tmp_DOCNO + "'";
                    }
                    else
                    {
                        sql_DOCNO += @" OR a.DOCNO = '" + tmp_DOCNO + "'";
                    }
                }
                sql += sql_DOCNO + ") ";
            }

            sql += @" ORDER BY a.DOCNO, d.MMCODE";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        public int ChkIsTransPrDetail(string docno,string seq)
        {
            // 檢查所選申請單內是否需註記轉申購單品項(申請量超過庫存量)
            string sql = @"select count(*) from ME_DOCD A
                            where A.DOCNO = :DOCNO and A.SEQ=:SEQ
                              and A.APPQTY>INV_QTY((select FRWH from ME_DOCM where DOCNO=A.DOCNO),A.MMCODE) ";
            return DBWork.Connection.QueryFirst<int>(sql, new { DOCNO = docno, SEQ=seq }, DBWork.Transaction);
        }
        public bool CheckWhidValid(string docno, string userId)
        {
            string sql = @"
                select 1 from ME_DOCM a
                 where a.docno = :docno
                   and exists (select 1 from MI_WHID
                                where wh_userId = :userId
                                  and wh_no = a.frwh)
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { docno, userId }, DBWork.Transaction) != null;
        }
        public string GetFlowid(string docno)
        {
            string sql = @"SELECT FLOWID FROM ME_DOCM WHERE DOCNO=:DOCNO";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }
        public int UpdatePostid(ME_DOCD me_docd)
        {
            var sql = "";
            if (me_docd.POSTID != "")
            {
                sql = @"UPDATE ME_DOCD  A 
                                     SET POSTID = :POSTID, DIS_TIME=SYSDATE, DIS_USER=:UPDATE_USER, RDOCNO=:RDOCNO,
                                             UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP,
                                             S_INV_QTY= INV_QTY((SELECT FRWH FROM ME_DOCM WHERE DOCNO =A.DOCNO ), A.MMCODE),
                                              INV_QTY=INV_QTY((SELECT TOWH FROM ME_DOCM WHERE DOCNO =A.DOCNO ), A.MMCODE)
                                WHERE DOCNO=:DOCNO AND SEQ=:SEQ and trim(POSTID) is null ";
            }
            else
            {
                sql = @"UPDATE ME_DOCD A
                                     SET POSTID =null, DIS_TIME=null, DIS_USER=null, 
                                             UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP,
                                             S_INV_QTY=INV_QTY((SELECT FRWH FROM ME_DOCM WHERE DOCNO =A.DOCNO ), A.MMCODE),
                                             INV_QTY=INV_QTY((SELECT TOWH FROM ME_DOCM WHERE DOCNO =A.DOCNO ), A.MMCODE)
                                WHERE DOCNO=:DOCNO AND SEQ=:SEQ and NVL(POSTID, ' ')='A'";
            }
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }
        public bool CheckAllPostIdNotA(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO and NVL(POSTID,' ') <>'A' ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }

        public int UpdateAllPostid(ME_DOCD me_docd)
        {
            var sql = "";
            sql = @"UPDATE ME_DOCD SET POSTID =null, DIS_TIME=null, DIS_USER=null, 
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO=:DOCNO  and NVL(POSTID, ' ')='A'";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }
        public bool CheckAllPostidNotNull(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO and trim(POSTID) is not null ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool ChkprExistsVender(string docno, string seq)
        {
            string sql = @"select 1 from ME_DOCD A,  PH_VENDER B
                            where A.M_AGENNO = B.AGEN_NO
                              and A.DOCNO = :DOCNO and A.SEQ=:SEQ ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction) == null);
        }
        public bool ChkPostidNotNull(string docno, string seq)
        {
            string sql = @"select 1 from ME_DOCD
                                        where DOCNO = :DOCNO and SEQ=:SEQ and trim(POSTID) is not null";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction) == null);
        }
        public bool ChkPostIdNotA(string docno, string seq)
        {
            string sql = @"select 1 from ME_DOCD 
                                       where DOCNO = :DOCNO and SEQ=:SEQ and ((nvl(POSTID,' ') <>'A' ) or (nvl(POSTID,'')='A' and trim(RDOCNO) is not null) )";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction) == null);
        }
        public string GetMatClass(string docno)
        {
            string sql = @"SELECT MAT_CLASS FROM ME_DOCM WHERE DOCNO=:DOCNO";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }
        public int InsertMM_PR_M(MM_PR_M mm_pr_m)
        {
            var sql = @"insert into MM_PR_M (
                                         pr_no, pr_dept, pr_time, pr_user, mat_class, 
                                         pr_status, create_time, create_user, update_ip, xaction, isFromDocm)
                                      values (
                                         :PR_NO, user_inid(:UPDATE_USER), sysdate, :UPDATE_USER, :MAT_CLASS,
                                         '35', sysdate, :UPDATE_USER, :UPDATE_IP, '0', 'N')";
            return DBWork.Connection.Execute(sql, mm_pr_m, DBWork.Transaction);
        }
        public int InsertMM_PR_D(string docno, string seq, string pr_no, string update_user, string update_ip)
        {
            var sql = @"insert into MM_PR_D (
                                         pr_no, mmcode, m_contid, m_contprice, m_purun, pr_price,  pr_qty, req_qty_t, unit_swap, 
                                         agen_fax, agen_name, agen_no,  disc, is_email, rec_status, m_nhikey,
                                         disc_cprice, disc_uprice, uprice, m_discperc,
                                         mmname_c,mmname_e,wexp_id,base_unit,orderkind,caseno,
                                         e_sourcecode,unitrate,e_codate,seq,chinname,chartno,e_itemarmyno,
                                         create_time, create_user, update_time, update_user, update_ip)
                                    select  :PR_NO,A.MMCODE,A.M_CONTID,A.M_CONTPRICE, (select m_purun from MI_MAST where mmcode=a.mmcode),
                                                (case when   (select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode') ='805'
                                                     then b.disc_cprice 
                                                     else (case when A.M_CONTID='0' then A.M_CONTPRICE else A.DISC_CPRICE end )
                                                end) as pr_price,
                                                A.PR_QTY, A.PR_QTY, 1 as UNIT_SWAP,C.AGEN_FAX, C.AGEN_NAMEC, A.M_AGENNO, 
                                                B.M_DISCPERC as disc,'N' IS_EMAIL,'35' as REC_STATUS, A.M_NHIKEY,
                                                B.DISC_CPRICE, B.DISC_UPRICE, B.UPRICE, B.M_DISCPERC,
                                                B.MMNAME_C,B.MMNAME_E,B.WEXP_ID,B.BASE_UNIT,B.ORDERKIND,B.CASENO,
                                                B.E_SOURCECODE,B.UNITRATE,B.E_CODATE,MM_PR_D_SEQ.nextval,A.CHINNAME,A.CHARTNO,B.e_itemarmyno,
                                               sysdate, :UPDATE_USER, sysdate, :UPDATE_USER, :UPDATE_IP  
                                      from ME_DOCD A
                                        inner join MI_MAST B on A.MMCODE=B.MMCODE
                                        inner join PH_VENDER C on A.M_AGENNO=C.AGEN_NO
                                     where A.DOCNO=:DOCNO and SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq, PR_NO = pr_no, UPDATE_USER = update_user, UPDATE_IP = update_ip }, DBWork.Transaction); 
        }
        public bool CheckExistsMMPR01M(string id)
        {
            string sql = @"select 1 from MM_PR01_M where pr_no = :PR_NO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { PR_NO = id }, DBWork.Transaction) == null);
        }
        public int InsertMM_PR01_M(MM_PR_M mm_pr_m)
        {
            var sql = @"insert into MM_PR01_M (
                            pr_no, pr_dept, pr_time, pr_user, mat_class, 
                            create_time, create_user, update_ip, isFromDocm)
                        values (
                            :PR_NO, user_inid(:UPDATE_USER), sysdate, :UPDATE_USER, '01',
                            sysdate, :UPDATE_USER, :UPDATE_IP, 'Y')";
            return DBWork.Connection.Execute(sql, mm_pr_m, DBWork.Transaction);
        }
        public int InsertMM_PR01_D(string docno,string seq,string pr_no, string update_user, string update_ip)
        {
            var sql = @"insert into MM_PR01_D (
                            pr_no, mmcode, pr_qty,  pr_price, m_purun, base_unit, m_contprice, m_discperc, unit_swap, 
                            req_qty_t, agen_no, m_contid,  agen_name, email, agen_fax, pr_amt,  min_ordqty, disc_cprice, 
                            disc_uprice, uprice, isWilling,  discount_qty, disc_cost_uprice, safe_qty, oper_qty, ship_qty, 
                            mmname_c, mmname_e, wexp_id, orderkind, caseno, e_sourcecode, unitrate,m_nhikey, e_codate,chinname,chartno,
                            create_time, create_user, update_time, update_user, update_ip
                            )
                        select :PR_NO, A.MMCODE, A.PR_QTY,
                                    (case when   (select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode') ='805'
                                         then A.DISC_CPRICE
                                         else  (case when A.M_CONTID='0' then A.M_CONTPRICE else A.DISC_CPRICE end ) 
                                    end) as PR_PRICE,
                                    B.M_PURUN,B.BASE_UNIT,A.M_CONTPRICE,B.M_DISCPERC,1 as UNIT_SWAP,A.PR_QTY as REQ_QTY_T,
                                    A.M_AGENNO, A.M_CONTID, C.AGEN_NAMEC, C.EMAIL, C.AGEN_FAX,
                                   (A.PR_QTY*(case when A.M_CONTID='0' then A.M_CONTPRICE else A.DISC_CPRICE end )) as PR_AMT,
                                   nvl(D.MIN_ORDQTY,1) as MIN_ORDQTY, A.DISC_CPRICE,A.DISC_UPRICE,A.UPRICE,
                                   (select isWilling from MILMED_JBID_LIST where substr(B.E_YRARMYNO,1,3)= JBID_STYR and B.E_ITEMARMYNO=BID_NO) as isWilling, 
                                   (select discount_qty from MILMED_JBID_LIST where substr(B.E_YRARMYNO,1,3)= JBID_STYR and B.E_ITEMARMYNO=BID_NO) as discount_qty, 
                                   (select disc_cost_uprice from MILMED_JBID_LIST where substr(B.E_YRARMYNO,1,3)= JBID_STYR and B.E_ITEMARMYNO=BID_NO) as disc_cost_uprice,
                                   nvl(D.SAFE_QTY,0) as SAFE_QTY,nvl(D.OPER_QTY,0) as OPER_QTY, nvl(D.SHIP_QTY, 0) as SHIP_QTY, 
                                   B.MMNAME_C,B.MMNAME_E,B.WEXP_ID,B.ORDERKIND,B.CASENO,B.E_SOURCECODE,B.UNITRATE,A.M_NHIKEY,
                                   B.E_CODATE,A.CHINNAME,A.CHARTNO, sysdate, :UPDATE_USER, sysdate, :UPDATE_USER, :UPDATE_IP 
                              from ME_DOCD A
                              inner join MI_MAST B on (A.MMCODE=B.MMCODE)
                              inner join PH_VENDER C on (A.M_AGENNO=C.AGEN_NO)
                              left join MI_WINVCTL D on (D.WH_NO=WHNO_ME1 and D.MMCODE=A.MMCODE)
                         where  A.DOCNO=:DOCNO and A.SEQ=:SEQ
                         ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq, PR_NO=pr_no, UPDATE_USER = update_user, UPDATE_IP = update_ip }, DBWork.Transaction);

        }

        public IEnumerable<AB0003Model> GetLoginInfo(string id, string ip)
        {
            string sql = @"SELECT TUSER AS USERID, UNA AS USERNAME, INID, INID_NAME(INID) AS INIDNAME,
                        WHNO_MM1 CENTER_WHNO,INID_NAME(WHNO_MM1) AS CENTER_WHNAME, TO_CHAR(SYSDATE,'YYYYMMDD') AS TODAY,
                        :UPDATE_IP,
                        (select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospCode') as HOSP_CODE,
                        (case when (select count(*) from UR_UIR where RLNO in ('MAT_14') and TUSER = :TUSER) > 0 then 'Y' else 'N' end) as IS_GRADE1
                        FROM UR_ID
                        WHERE UR_ID.TUSER=:TUSER";

            return DBWork.Connection.Query<AB0003Model>(sql, new { TUSER = id, UPDATE_IP = ip });
        }
    }
}
