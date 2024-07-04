using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.B
{
    public class BE0007Repository : JCLib.Mvc.BaseRepository
    {
        public BE0007Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        private string GetSql(string agen_no, string invoice, string start_date, string end_date, string data_ym, string mat_class)
        {
            //string sql = @"
            //    select '11206' data_ym,DECODE(a.invoice_type,'1','銷貨','2','退貨','3','非發票','') invoice_type,a.invoice, twn_date(a.invoice_dt) invoice_dt, a.mmcode , b.m_nhikey , b.mmname_c , c.agen_no , d.uni_no , d.agen_namec , a.disc_cprice , a.deli_qty , 
            //            nvl(a.po_amt, 0) po_amt , nvl(a.extra_disc_amount,0) extra_disc_amount, a.more_disc_amount more_disc_amount, nvl(nvl(a.po_amt, 0) - nvl(a.more_disc_amount, 0)-nvl(a.extra_disc_amount, 0), 0 )total_amt,
            //            b.po_no , a.po_price , a.transno , a.is_include_tax , twn_date(a.update_time) update_time 
            //            from PH_INVOICE a, MM_PO_D b, MM_PO_M c, PH_VENDER d 
            //            where twn_date(a.deli_dt) between '1120301' and '1190430'
            //            and a.po_no = b.po_no 
            //            and a.mmcode = b.mmcode 
            //            and b.po_no = c.po_no
            //            and c.agen_no = d.agen_no
            //            and nvl(a.status,'N') = 'N'
            //            and c.agen_no = '371'
            //            and a.invoice = 'FF58874538'
            //            and (select count(*) FROM MI_MAST WHERE MMCODE=b.MMCODE and MAT_CLASS=查詢條件_物料類別 )>0
            //            union
            //            select '11206' data_ym,DECODE(a.invoice_type,'1','銷貨','2','退貨','3','非發票','') invoice_type,a.invoice, twn_date(a.invoice_dt) invoice_dt, a.mmcode , b.m_nhikey , b.mmname_c , c.agen_no , d.uni_no , d.agen_namec , a.disc_cprice , a.deli_qty , 
            //            nvl(a.po_amt, 0) po_amt, nvl(a.extra_disc_amount,0) extra_disc_amount, a.more_disc_amount more_disc_amount, nvl(nvl(a.po_amt, 0) - nvl(a.more_disc_amount, 0)-nvl(a.extra_disc_amount, 0), 0 ) total_amt,
            //            b.po_no , a.po_price , a.transno , a.is_include_tax , twn_date(a.update_time) update_time 
            //            from PH_INVOICE a, MM_PO_D b, MM_PO_M c, PH_VENDER d 
            //            where a.data_ym = '11206'
            //            and c.agen_no = '371'
            //            and a.invoice = 'FF58874538'
            //            and a.po_no = b.po_no 
            //            and a.mmcode = b.mmcode 
            //            and b.po_no = c.po_no
            //            and c.agen_no = d.agen_no
            //            and nvl(a.status,'N') = 'N'
            //            and (select count(*) FROM MI_MAST WHERE MMCODE=b.MMCODE and MAT_CLASS=查詢條件_物料類別 )>0
            //";
            string sqlwhere = "";
            string sqlwhere2 = "";
            if (string.IsNullOrEmpty(agen_no) == false)
            {
                sqlwhere += " and c.agen_no = :agen_no";
                sqlwhere2 += " and d.agen_no = :agen_no";
            }
            if (string.IsNullOrEmpty(invoice) == false)
            {
                sqlwhere += " and a.invoice = :invoice";
                sqlwhere2 += " and a.invoice = :invoice";
            }
            if (string.IsNullOrEmpty(mat_class) == false)
            {
                if (mat_class.Contains("SUB_"))
                {
                    sqlwhere += " and (select count(*) FROM MI_MAST WHERE MMCODE=b.MMCODE and MAT_CLASS_SUB=:mat_class )>0 ";
                    sqlwhere2 += " and (select count(*) FROM MI_MAST WHERE MMCODE=b.MMCODE and MAT_CLASS_SUB=:mat_class )>0 ";
                }
                else
                {
                    sqlwhere += " and (select count(*) FROM MI_MAST WHERE MMCODE=b.MMCODE and MAT_CLASS=:mat_class )>0 ";
                    sqlwhere2 += " and (select count(*) FROM MI_MAST WHERE MMCODE=b.MMCODE and MAT_CLASS=:mat_class )>0 ";
                }
            }
            string sql = "select '" + data_ym + @"' data_ym,DECODE(a.invoice_type,'1','銷貨','2','退貨','3','非發票','') invoice_type,
                                a.invoice, twn_date(a.invoice_dt) invoice_dt, a.mmcode, e.m_nhikey, e.spmmcode, e.nhi_price,
                                b.mmname_c , c.agen_no , d.uni_no , d.agen_namec , a.disc_cprice , a.deli_qty , ";
            sql += @" round(nvl(a.po_amt, 0)) po_amt , round(nvl(a.extra_disc_amount,0)) extra_disc_amount, round(nvl(a.extra_disc_amount, 0)) as ori_extra_disc_amount,
                        round(nvl(a.more_disc_amount, 0)) more_disc_amount, 
                        round(nvl(a.po_amt, 0) - nvl(a.more_disc_amount, 0) - nvl(a.extra_disc_amount, 0)) total_amt,
                        b.po_no , a.po_price , a.transno , a.is_include_tax , twn_date(a.update_time) update_time 
                        from PH_INVOICE a, MM_PO_D b, MM_PO_M c, PH_VENDER d, MI_MAST e 
                        where twn_date(a.deli_dt) between :start_date and :end_date
                        and a.invoice_type='1'
                        and a.po_no = b.po_no 
                        and a.mmcode = b.mmcode
                        and b.mmcode = e.mmcode
                        and b.po_no = c.po_no
                        and c.agen_no = d.agen_no
                        and nvl(a.status,'N') = 'N'" + sqlwhere;
            sql += string.Format(@"
                    union select '{0}' data_ym, DECODE(a.invoice_type,'1','銷貨','2','退貨','3','非發票','') invoice_type,
                                 a.invoice, twn_date(a.invoice_dt) invoice_dt, a.mmcode, b.m_nhikey, b.spmmcode, b.nhi_price,
                                 b.mmname_c , d.agen_no , d.uni_no , d.agen_namec , b.disc_cprice , a.po_qty as deli_qty ,
                                 round(nvl(a.po_amt, 0)) po_amt, round(nvl(a.extra_disc_amount,0)) extra_disc_amount,  round(nvl(a.extra_disc_amount, 0)) as ori_extra_disc_amount,
                                 round(nvl(a.more_disc_amount, 0)) more_disc_amount, 
                                 round(nvl(a.po_amt, 0) - nvl(a.more_disc_amount, 0) - nvl(a.extra_disc_amount, 0)) total_amt,
                                 a.po_no , a.po_price , a.transno , a.is_include_tax , twn_date(a.update_time) update_time 
                            from PH_INVOICE a 
                            left join MI_MAST_HISTORY b on a.mmcode =b.mmcode
                            left join PH_VENDER d on b.m_agenno = d.agen_no
                           where 1=1
                             and twn_date(a.create_time) between :start_date and :end_date
                             and a.create_time between b.effstartdate and nvl(b.EFFENDDATE, a.create_time)
                             and a.invoice_type='2'
                             and nvl(a.status,'N') <> 'D'
                             {1}
                ", data_ym, sqlwhere2);
            sql += string.Format(@"
                    union select '{0}' data_ym, DECODE(a.invoice_type,'1','銷貨','2','退貨','3','非發票','') invoice_type,
                                 a.invoice, twn_date(a.invoice_dt) invoice_dt, a.mmcode, b.m_nhikey, b.spmmcode, b.nhi_price,
                                 b.mmname_c , d.agen_no , d.uni_no , d.agen_namec , b.disc_cprice , a.po_qty as deli_qty ,
                                 round(nvl(a.po_amt, 0)) po_amt, round(nvl(a.extra_disc_amount, 0)) extra_disc_amount,  round(nvl(a.extra_disc_amount, 0)) as ori_extra_disc_amount,
                                 round(nvl(a.more_disc_amount, 0)) more_disc_amount, 
                                 round(nvl(a.po_amt, 0) - nvl(a.more_disc_amount, 0) - nvl(a.extra_disc_amount, 0)) total_amt,
                                 a.po_no , a.po_price , a.transno , a.is_include_tax , twn_date(a.update_time) update_time 
                            from PH_INVOICE a 
                            left join MI_MAST_HISTORY b on a.mmcode =b.mmcode
                            left join PH_VENDER d on b.m_agenno = d.agen_no
                           where 1=1
                             and twn_date(a.create_time) between :start_date and :end_date
                             and a.create_time between b.effstartdate and nvl(b.EFFENDDATE, a.create_time)
                             and a.po_no='------'
                             and nvl(a.status,'N') <> 'D'
                             {1}
                ", data_ym, sqlwhere2);
            sql += "   union select '" + data_ym + @"' data_ym,DECODE(a.invoice_type,'1','銷貨','2','退貨','3','非發票','') invoice_type,
                        a.invoice, twn_date(a.invoice_dt) invoice_dt, a.mmcode, e.m_nhikey, e.spmmcode, e.nhi_price,
                        b.mmname_c , c.agen_no , d.uni_no , d.agen_namec , a.disc_cprice , a.deli_qty , ";
            sql += @"   round(nvl(a.po_amt, 0)) po_amt, round(nvl(a.extra_disc_amount, 0)) extra_disc_amount,  round(nvl(a.extra_disc_amount, 0)) as ori_extra_disc_amount,
                        round(nvl(a.more_disc_amount, 0)) more_disc_amount, 
                        round(nvl(a.po_amt, 0) - nvl(a.more_disc_amount, 0) - nvl(a.extra_disc_amount, 0)) total_amt,
                        b.po_no , a.po_price , a.transno , a.is_include_tax , twn_date(a.update_time) update_time 
                        from PH_INVOICE a, MM_PO_D b, MM_PO_M c, PH_VENDER d, MI_MAST e 
                        where a.data_ym = :data_ym 
                        and a.po_no = b.po_no 
                        and a.mmcode = b.mmcode 
                        and b.mmcode = e.mmcode
                        and b.po_no = c.po_no
                        and c.agen_no = d.agen_no
                        and nvl(a.status,'N') = 'N'" + sqlwhere;
            return sql;
        }

        public IEnumerable<BE0007> GetAll(string agen_no, string invoice, string start_date, string end_date, string data_ym, string mat_class)
        {
            string sql = GetSql(agen_no, invoice, start_date, end_date, data_ym, mat_class);

            if (mat_class.Contains("SUB_"))
                mat_class = mat_class.Replace("SUB_", "");
            return DBWork.PagingQuery<BE0007>(sql, new { agen_no, invoice, start_date, end_date, data_ym, mat_class }, DBWork.Transaction);
        }

        public IEnumerable<BE0007> Get(string po_no, string mmcode, string transno)
        {
            var sql = @"select  a.PO_NO, (select  AGEN_NO||' '||AGEN_NAMEC from PH_VENDER where agen_no=a.agen_no) as AGEN_NO,
                    b.INVOICE ,TWN_DATE(b.INVOICE_DT)INVOICE_DT,  b.MMCODE, m.MMNAME_C, m.MMNAME_E,
                    PO_PRICE, b.DELI_QTY,b.CKIN_QTY, ";

            if (po_no.IndexOf("INV") > -1 || po_no.IndexOf("GEN") > -1 || po_no.IndexOf("SML") > -1) //衛材 floor()
                sql += " to_char(floor(b.PO_PRICE* b.CKIN_QTY),'999,999,999')  as AMOUNT,";
            else
                sql += " to_char(round(b.PO_PRICE* b.CKIN_QTY),'999,999,999')  as AMOUNT,";

            sql += @" (select nvl2(min(acc_time),TWN_DATE(min(acc_time)),null) from BC_CS_ACC_LOG where po_no=a.po_no and mmcode=b.mmcode) DELI_DT,
                    b.MEMO ,b.TRANSNO,
                    (case when (a.mat_class<>'01' and a.M_CONTID='0') then '合約' when (a.mat_class<>'01' and  a.M_CONTID='2') then '非合約' when (a.mat_class<>'01' and a.M_CONTID='3') then '小採' when (a.mat_class='01' and (a.contracno='0N' or a.contracno='0Y' or a.contracno='X')) then '零購' when (a.mat_class='01' and (a.contracno='1' or a.contracno='2' or a.contracno='01' or a.contracno='02'))  then '合約' else ''  end) as M_CONTID, ";
            if (po_no.IndexOf("INV") > -1 || po_no.IndexOf("GEN") > -1 || po_no.IndexOf("SML") > -1) //衛材 floor()
                sql += @" (case when INVOICE is not null then (select to_char(sum(floor(PO_PRICE * CKIN_QTY)),'999,999,999')  from PH_INVOICE where po_no=B.PO_NO and invoice=b.invoice) else '' end) as INVOICE_TOT, ";
            else
                sql += @" (case when INVOICE is not null then (select to_char(sum(round(PO_PRICE * CKIN_QTY)),'999,999,999')  from PH_INVOICE where po_no=B.PO_NO and invoice=b.invoice) else '' end) as INVOICE_TOT, ";

            sql += @" b.M_PHCTNCO, b.PO_QTY,
                     (case when b.CKSTATUS='N' then '未驗證' when b.CKSTATUS='Y' then '已驗證' when b.CKSTATUS='T' then '驗退' end) CKSTATUS
                    FROM MM_PO_M a, PH_INVOICE b, MI_MAST m
                    where substr(a.po_no,1,3) not in ('TXT') and a.po_no=b.po_no and b.mmcode=m.mmcode  
                    AND A.PO_NO = :PO_NO AND B.MMCODE = :MMCODE AND B.TRANSNO = :TRANSNO ";
            return DBWork.Connection.Query<BE0007>(sql, new { PO_NO = po_no, MMCODE = mmcode, TRANSNO = transno }, DBWork.Transaction);
        }

        //public int UpdateInvoice(string po_no, string mmcode, string transno, string invoice,string user, string ip)
        //{
        //    var p = new DynamicParameters();

        //    var sql = @"UPDATE PH_INVOICE SET INVOICE = :INVOICE,
        //                CHK_USER = :UPDATE_USER, CHK_DT = SYSDATE,   
        //                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
        //                WHERE PO_NO = :PO_NO AND MMCODE = :MMCODE AND TRANSNO = :TRANSNO ";
        //    p.Add(":PO_NO", string.Format("{0}", po_no));
        //    p.Add(":MMCODE", string.Format("{0}", mmcode));
        //    p.Add(":TRANSNO", string.Format("{0}", transno));
        //    p.Add(":INVOICE", string.Format("{0}", invoice));
        //    p.Add(":UPDATE_USER", string.Format("{0}", user));
        //    p.Add(":UPDATE_IP", string.Format("{0}", ip));
        //    return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        //}
        public int UpdateInvoicedt(string po_no, string mmcode, string transno, string invoicedt, string invoice, string user, string ip)
        {
            var p = new DynamicParameters();

            var sql = @"UPDATE PH_INVOICE SET INVOICE_DT = TO_DATE(:INVOICE_DT,'YYYY-MM-DD'),  
                        INVOICE = :INVOICE,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                        WHERE PO_NO = :PO_NO AND MMCODE = :MMCODE AND TRANSNO = :TRANSNO";
            p.Add(":PO_NO", string.Format("{0}", po_no));
            p.Add(":MMCODE", string.Format("{0}", mmcode));
            p.Add(":TRANSNO", string.Format("{0}", transno));
            p.Add(":INVOICE_DT", string.Format("{0}", invoicedt));
            p.Add(":INVOICE", string.Format("{0}", invoice));
            p.Add(":UPDATE_USER", string.Format("{0}", user));
            p.Add(":UPDATE_IP", string.Format("{0}", ip));
            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public int Merge(string p0, string p1, string p2, string user, string ip)
        {

            var pp = new DynamicParameters();
            var sql2 = @"insert into INVOICE (INVOICE, MMCODE, INVOICE_DT, AGEN_NO, M_NHIKEY, 
                                        INVOICE_QTY, INVOICE_PRICE, INVOICE_AMOUNT,  
                                        REBATESUM, DISC_AMOUNT, IS_INCLUDE_TAX,  
                                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, 
                                        more_disc_amount, extra_disc_amount, actual_pay,
                                        acc_disc_cprice)
                                    select a.invoice, a.mmcode, a.invoice_dt, a.agen_no, a.m_nhikey,
                                        sum(a.invoice_qty) as invoice_qty, a.invoice_price, sum(a.PO_AMT) as invoice_amount, 
                                        sum(a.rebatesum) as rebatesum, sum(a.DISC_AMOUNT) as DISC_AMOUNT, a.is_include_tax, 
                                        sysdate, :tuser, sysdate, :tuser2, :userip,
                                        sum(a.extra_disc_amount) as extra_disc_amount, sum(a.more_disc_amount) as more_disc_amount, sum(a.actual_pay) as actual_pay,
                                        a.disc_cprice
                                        from 
                                        (
                                        select a.invoice, a.mmcode, a.invoice_dt, (select agen_no from MM_PO_M where po_no = a.po_no) as AGEN_NO,
                                                (select m_nhikey from MM_PO_D where po_no = a.po_no and mmcode = a.mmcode and rownum=1) as M_NHIKEY,
                                                deli_qty as invoice_qty,
                                                po_price as invoice_price,
                                                a.disc_cprice, in_amount,
                                                round(PO_AMT, 0) as PO_AMT,
                                                a.extra_disc_amount as rebatesum,
                                                a.more_disc_amount as DISC_AMOUNT,
                                                a.is_include_tax, 
                                                :p0 as act_ym,
                                                a.extra_disc_amount, a.more_disc_amount,
                                                (nvl(PO_AMT, 0) - a.more_disc_amount - a.extra_disc_amount) as actual_pay
                                            from PH_INVOICE a
                                            where twn_date(a.deli_dt) between :p1 and :p2
                                            and a.invoice_type = '1'
                                            and nvl(a.status, 'N') ='N'
                                            and a.invoice is not null
                                        ) a
                                        group by a.invoice, a.mmcode, a.invoice_dt, a.agen_no, a.m_nhikey,a.invoice_price, a.is_include_tax, a.act_ym, a.disc_cprice";
            pp.Add(":tuser", user);
            pp.Add(":tuser2", user);
            pp.Add(":userip", ip);
            pp.Add(":p0", p0);
            pp.Add(":p1", p1);
            pp.Add(":p2", p2);
            return DBWork.Connection.Execute(sql2, pp, DBWork.Transaction);
        }
        //新增資料 依據訂單號碼取得相關資料 藥材代碼$藥材名稱$單價$健保代碼$廠商代碼$廠商名稱$廠商統編
        public string GetInsertData(string po_no)
        {
            string ret = "";
            var p = new DynamicParameters();

            var sql = @"select b.mmcode ||'$'|| b.mmname_c ||'$'|| b.po_price ||'$'|| b.m_nhikey ||'$'|| a.agen_no ||'$'|| c.agen_namec ||'$'|| c.uni_no datastr
                        from MM_PO_M a, MM_PO_D b, PH_VENDER c
                        where a.po_no = :po_no and a.po_no = b.po_no and a.agen_no = c.agen_no";
            p.Add(":po_no", po_no);
            ret = DBWork.Connection.ExecuteScalar<string>(sql, p, DBWork.Transaction);
            return ret;
        }
        //同一發票不可不同日期
        public string ChkInvoice(string invoice, string invoicedt)
        {
            string ret = "Y";
            var p = new DynamicParameters();

            var sql = @"select (case when count(*) = 1 then 'N' else 'Y' end) restr
                            from (
                            select invoice_dt from PH_INVOICE where invoice = :invoice
                            union
                            select TO_DATE(:invoicedt,'YYYY-MM-DD') from dual)";
            p.Add(":invoice", invoice);
            p.Add(":invoicedt", invoicedt);
            ret = DBWork.Connection.ExecuteScalar<string>(sql, p, DBWork.Transaction);
            return ret;
        }
        //訂單號碼是否存在
        public string ChkMmPoMPONO(string po_no)
        {
            string ret = "Y";
            string temp = "";
            var p = new DynamicParameters();

            var sql = @"select count(*) from MM_PO_M where po_no = :po_no ";
            p.Add(":po_no", po_no);
            temp = DBWork.Connection.ExecuteScalar<string>(sql, p, DBWork.Transaction);
            if (temp != "0") ret = "N";
            return ret;
        }
        //同一發票需為相同廠商
        public string ChkInvoicePONO(string invoice, string po_no)
        {
            string ret = "Y";
            var p = new DynamicParameters();

            var sql = @"select (case when count(*) = 1 then 'N' else 'Y' end)
                            from (
                            select agen_no from MM_PO_M where po_no in (select po_no from PH_INVOICE where invoice = :invoice)
                            union
                            select agen_no from MM_PO_M where po_no = :po_no)";
            p.Add(":invoice", invoice);
            p.Add(":po_no", po_no);
            ret = DBWork.Connection.ExecuteScalar<string>(sql, p, DBWork.Transaction);
            return ret;
        }
        //藥材代碼是否存在於訂單號碼中
        public string ChkMmPoDdata(string po_no, string mmcode)
        {
            string ret = "Y";
            string temp = "";
            var p = new DynamicParameters();

            var sql = @"select count(*) from MM_PO_D where po_no = :po_no and mmcode = :mmcode ";
            p.Add(":po_no", po_no);
            p.Add(":mmcode", mmcode);
            temp = DBWork.Connection.ExecuteScalar<string>(sql, p, DBWork.Transaction);
            if (temp != "0") ret = "N";
            return ret;
        }
        //同一PO_NO+MMCODE不可有 1張以上發票
        public string ChkInvoiceDB(string po_no, string mmcode, string invoice)
        {
            string ret = "N";
            if (invoice != null && invoice != "")
            {
                var p = new DynamicParameters();
                var sql = @"select count(*) FROM PH_INVOICE 
                    where substr(po_no,1,3) not in ('TXT') and po_no=:po_no and mmcode=:mmcode and invoice=:invoice ";
                p.Add(":po_no", po_no);
                p.Add(":mmcode", mmcode);
                p.Add(":invoice", invoice);
                int tmp = DBWork.Connection.ExecuteScalar<int>(sql, p, DBWork.Transaction);
                if (tmp > 1) ret = "Y";
            }
            return ret;
        }
        //同一PO_NO+MMCODE不可有 1張以上發票
        public string ChkInvoiceDB_ByOLD(string po_no, string mmcode, string invoice, string invoice_old)
        {
            string ret = "N";
            if (invoice != null && invoice != "")
            {
                var p = new DynamicParameters();
                var sql = @"select count(*) FROM PH_INVOICE 
                    where substr(po_no,1,3) not in ('TXT') and po_no=:po_no and mmcode=:mmcode and invoice=:invoice ";
                p.Add(":po_no", po_no);
                p.Add(":mmcode", mmcode);
                p.Add(":invoice", invoice);
                int tmp = DBWork.Connection.ExecuteScalar<int>(sql, p, DBWork.Transaction);
                if (invoice == invoice_old)
                {
                    if (tmp > 1) ret = "Y";  //新、舊發票一樣，表示已經存在一筆
                }
                else
                {
                    if (tmp > 0) ret = "Y";
                }

            }
            return ret;
        }
        public int UpdateA(string po_no, string mmcode, string transno, string user, string ip)
        {
            var p = new DynamicParameters();

            var sql = @"UPDATE PH_INVOICE SET CKSTATUS = 'Y', FLAG='A',  
                        CHK_USER = :UPDATE_USER, CHK_DT = SYSDATE,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                        WHERE PO_NO = :PO_NO AND MMCODE = :MMCODE AND TRANSNO = :TRANSNO
                        AND CKSTATUS <>'Y' ";
            p.Add(":PO_NO", string.Format("{0}", po_no));
            p.Add(":MMCODE", string.Format("{0}", mmcode));
            p.Add(":TRANSNO", string.Format("{0}", transno));
            p.Add(":UPDATE_USER", string.Format("{0}", user));
            p.Add(":UPDATE_IP", string.Format("{0}", ip));
            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }
        public int Reject(string po_no, string mmcode, string transno, string user, string ip)
        {
            var p = new DynamicParameters();

            var sql = @"update PH_INVOICE 
                        set CKSTATUS='N', update_user=:xuser, update_time=sysdate, update_ip=:xip
                        where PO_NO = :PO_NO AND MMCODE = :MMCODE AND TRANSNO = :TRANSNO
                        and CKSTATUS ='Y' ";
            p.Add(":PO_NO", string.Format("{0}", po_no));
            p.Add(":MMCODE", string.Format("{0}", mmcode));
            p.Add(":TRANSNO", string.Format("{0}", transno));
            p.Add(":xuser", string.Format("{0}", user));
            p.Add(":xip", string.Format("{0}", ip));
            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }
        public int UpdateDEL(string po_no, string mmcode, string transno, string user, string ip)
        {   //至少保留一筆 PO_NO + MMCODE record
            var p = new DynamicParameters();

            var sql = @"update PH_INVOICE set status = 'D' 
                         where po_no = :PO_NO and mmcode = :MMCODE and transno = :TRANSNO";
            p.Add(":PO_NO", string.Format("{0}", po_no));
            p.Add(":MMCODE", string.Format("{0}", mmcode));
            p.Add(":TRANSNO", string.Format("{0}", transno));
            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }
        public int UpdateM(BE0007 be03)
        {
            var sql = @"UPDATE PH_INVOICE 
                           SET INVOICE = :INVOICE, 
                               INVOICE_DT = TO_DATE(:INVOICE_DT,'YYYY/MM/DD'), PO_AMT =:PO_AMT, INVOICE_TYPE = :INVOICE_TYPE,
                               PO_PRICE = :PO_PRICE, 
                               more_disc_amount = :MORE_DISC_AMOUNT, 
                               EXTRA_DISC_AMOUNT = :EXTRA_DISC_AMOUNT,
                               in_amount = round(nvl(DELI_QTY * :PO_PRICE, 0) - :MORE_DISC_AMOUNT - :EXTRA_DISC_AMOUNT), 
                               UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                         WHERE PO_NO = :PO_NO AND MMCODE = :MMCODE AND TRANSNO = :TRANSNO
                          ";
            return DBWork.Connection.Execute(sql, be03, DBWork.Transaction);
        }
        public int UpdateB(BE0007 be03)
        {
            var sql = @"UPDATE PH_INVOICE SET CKSTATUS = 'T', FLAG='A', MEMO = :MEMO, CHK_USER = :UPDATE_USER, CHK_DT = SYSDATE,
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE PO_NO = :PO_NO AND MMCODE = :MMCODE AND TRANSNO = :TRANSNO 
                                AND CKSTATUS <>'Y' ";
            return DBWork.Connection.Execute(sql, be03, DBWork.Transaction);
        }
        public int CreateM(BE0007 be03)
        {
            //var sql = @"insert into PH_INVOICE (
            //                    PO_NO, MMCODE, TRANSNO, CREATE_TIME, PO_QTY, PO_PRICE, M_PURUN, M_AGENLAB, 
            //                    PO_AMT, M_DISCPERC, DELI_QTY, DELI_STATUS, INVOICE, INVOICE_DT, STATUS, UPRICE, 
            //                    DISC_CPRICE, DISC_UPRICE,CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, EXTRA_DISC_AMOUNT,
            //                    MORE_DISC_AMOUNT, IS_INCLUDE_TAX, INVOICE_TYPE, DATA_YM, in_amount)
            //            select b.po_no, b.mmcode, twn_systime, sysdate, b.po_qty, b.po_price, b.m_purun,b.m_agenlab, 
            //                    :PO_AMT, m_discperc, :DELI_QTY, 'C', :INVOICE, TO_DATE(:INVOICE_DT,'YYYY/MM/DD'),'N',b.uprice, 
            //                    b.disc_cprice, b.disc_uprice, :CREATE_USER, sysdate, :CREATE_USER,:UPDATE_IP,:EXTRA_DISC_AMOUNT,
            //                    :MORE_DISC_AMOUNT,'Y',:INVOICE_TYPE, :DATA_YM,
            //                    (b.disc_cprice * :DELI_QTY - :MORE_DISC_AMOUNT - :EXTRA_DISC_AMOUNT) as in_amount
            //              from MM_PO_M a, MM_PO_D b
            //             where a.po_no = :PO_NO and a.po_no =b.po_no and b.mmcode = :MMCODE ";
            var sql = @"
                insert into PH_INVOICE (
                                PO_NO, MMCODE, TRANSNO, CREATE_TIME, PO_QTY, PO_PRICE, M_PURUN, M_AGENLAB, 
                                PO_AMT, M_DISCPERC, DELI_QTY, DELI_STATUS, INVOICE, INVOICE_DT, STATUS, UPRICE, 
                                DISC_CPRICE, DISC_UPRICE,CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, EXTRA_DISC_AMOUNT,
                                MORE_DISC_AMOUNT, IS_INCLUDE_TAX, INVOICE_TYPE, DATA_YM, in_amount)
                select nvl(:PO_NO, '------'), a.mmcode, twn_systime, sysdate, :DELI_QTY, a.M_CONTPRICE, a.m_purun, a.m_agenlab,
                       :PO_AMT, a.m_discperc, :DELI_QTY, 'C', :INVOICE, TO_DATE(:INVOICE_DT,'YYYY/MM/DD'), 'N', a.UPRICE,
                       a.disc_cprice, a.disc_uprice, :CREATE_USER, sysdate, :CREATE_USER, :UPDATE_IP, :EXTRA_DISC_AMOUNT,
                       :MORE_DISC_AMOUNT,'Y',:INVOICE_TYPE, :DATA_YM,
                       (a.M_CONTPRICE * :DELI_QTY - :MORE_DISC_AMOUNT - :EXTRA_DISC_AMOUNT) as in_amount
                 from MI_MAST a
                where a.mmcode = :MMCODE
            ";

            //INSERT INTO PH_INVOICE (
            //    PO_NO, MMCODE, TRANSNO, PO_QTY, PO_PRICE, DELI_DT,
            //    M_PURUN, M_AGENLAB, PO_AMT, 
            //    M_DISCPERC, BW_SQTY, UPRICE, 
            //    ACCOUNTDATE, DELI_STATUS, STATUS, CKSTATUS, FLAG,  
            //    PR_NO, M_PHCTNCO, DISC_CPRICE, DISC_UPRICE, UNIT_SWAP,  
            //    DELI_QTY, MEMO, INVOICE, INVOICE_DT, 
            //    CKIN_QTY, CHK_USER, CHK_DT, CREATE_USER, UPDATE_IP, CREATE_TIME) 
            //select 
            //  PO_NO, MMCODE, TO_CHAR(sysdate,'yyyymmddhh24miss'), PO_QTY, PO_PRICE, DELI_DT,
            //  M_PURUN, M_AGENLAB, PO_PRICE*:DELI_QTY, 
            //  M_DISCPERC, BW_SQTY, UPRICE,  
            //  ACCOUNTDATE, DELI_STATUS, STATUS, 'N', FLAG,  
            //  PR_NO, M_PHCTNCO, DISC_CPRICE, DISC_UPRICE, UNIT_SWAP,  
            //  :DELI_QTY, :MEMO, :INVOICE, TO_DATE(:INVOICE_DT,'YYYY/MM/DD'), 
            //  :CKIN_QTY, :CREATE_USER, SYSDATE, :CREATE_USER, :UPDATE_IP, SYSDATE
            //from PH_INVOICE 
            //where PO_NO=:PO_NO and MMCODE=:MMCODE and TRANSNO=:TRANSNO_OLD ";
            return DBWork.Connection.Execute(sql, be03, DBWork.Transaction);
        }
        public IEnumerable<COMBO_MODEL> GetMatclassCombo(string userid)
        {
            string sql = @"
                select * from 
                (
                    select mat_class as value, '全部'||mat_clsname as text 
                      from MI_MATCLASS
                     where mat_clsid in ('1', '2')
                    union
                    select 'SUB_' || data_value as value, data_desc as text from PARAM_D
                     where grp_code ='MI_MAST' 
                       and data_name = 'MAT_CLASS_SUB'
                       and data_value not in ('1','2')
                       and trim(data_desc) is not null
                )
                order by value
            ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { USERID = userid });
        }
        public IEnumerable<COMBO_MODEL> GetInvoiceExistCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MM_PO_M' AND DATA_NAME='INVOICE' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetContIdCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MM_PO_M' AND DATA_NAME='M_CONTID' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetCkStatusCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='PH_INVOICE' AND DATA_NAME='CKSTATUS' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetMemoCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='PH_INVOICE' AND DATA_NAME='MEMO' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<PH_VENDER> GetAgennoCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"select {0} AGEN_NO, AGEN_NAMEC, AGEN_NAMEE, AGEN_NO||' '||AGEN_NAMEC as EASYNAME
                            from PH_VENDER where (REC_STATUS <> 'X' or REC_STATUS is null) ";

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(AGEN_NO, :AGEN_NO_I), 1000) + NVL(INSTR(AGEN_NAMEC, :AGEN_NAMEC_I), 100) * 10 + NVL(INSTR(AGEN_NAMEE, :AGEN_NAMEE_I), 100) * 10) IDX,");
                p.Add(":AGEN_NO_I", p0);
                p.Add(":AGEN_NAMEC_I", p0);
                p.Add(":AGEN_NAMEE_I", p0);

                sql += " AND (AGEN_NO LIKE :AGEN_NO ";
                p.Add(":AGEN_NO", string.Format("{0}%", p0));

                sql += " OR AGEN_NAMEC LIKE :AGEN_NAMEC ";
                p.Add(":AGEN_NAMEC", string.Format("%{0}%", p0));

                sql += " OR AGEN_NAMEE LIKE :AGEN_NAMEE) ";
                p.Add(":AGEN_NAMEE", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY AGEN_NO ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PH_VENDER>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        #region 2022-09-26
        public DataTable GetUploadExample()
        {
            string sql = @"
                select '' as 訂單編號, '' as 院內碼, '' as 資料流水號, '' as 發票日期, '' 發票號碼
                  from dual
            ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql))
                dt.Load(rdr);

            return dt;
        }


        public DataTable GetExcel(string agen_no, string invoice, string start_date, string end_date, string data_ym, string mat_class)
        {
            string sql = @"select data_ym 年月,
                invoice 發票號碼,
                invoice_dt 發票日期,
                mmcode 院內碼,
                m_nhikey 健保碼,
                mmname_c 藥材名稱,
                agen_no 廠商編號,
                uni_no 廠商統編,
                agen_namec 廠商名稱,
                po_price 單價,
                deli_qty 數量,
                nvl(po_amt, 0) 小計,
                nvl(extra_disc_amount, 0) 折讓金額,
                round(more_disc_amount) 優惠金額,
                round(nvl(nvl(po_amt, 0) - nvl(more_disc_amount, 0) - nvl(extra_disc_amount, 0), 0 )) as 總金額,
                po_no 訂單號碼,
                po_price 合約價,
                transno 資料流水號,
                is_include_tax 含稅,
                update_time 最後異動日 
            FROM (";

            sql += GetSql(agen_no, invoice, start_date, end_date, data_ym, mat_class);

            sql += ") order by INVOICE_DT, MMCODE";

            if (mat_class.Contains("SUB_"))
                mat_class = mat_class.Replace("SUB_", "");

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, new { agen_no, invoice, start_date, end_date, data_ym, mat_class }))
                dt.Load(rdr);

            return dt;
        }

        public bool CheckPhInvoiceExists(string po_no, string mmcode, string transno)
        {
            string sql = @"
                select 1 from PH_INVOICE
                 where po_no = :po_no
                   and mmcode = :mmcode
                   and transno = :transno
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { po_no, mmcode, transno }, DBWork.Transaction) != null;
        }

        public bool CheckPhInvoiceCksStatusNotY(string po_no, string mmcode, string transno)
        {
            string sql = @"
                select 1 from PH_INVOICE
                 where po_no = :po_no
                   and mmcode = :mmcode
                   and transno = :transno
                   and ckstatus <> 'Y'
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { po_no, mmcode, transno }, DBWork.Transaction) != null;
        }

        public bool CheckInvoiceDtValid(string invoice_dt)
        {
            string sql = @"
                select (case when twn_todate(:invoice_dt) is not null then 'Y' else 'N' end) from dual
            ";

            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { invoice_dt }, DBWork.Transaction) == "Y";
        }

        public int UpdatePhInvoiceFromUpload(string po_no, string mmcode, string transno, string invoice, string invoice_dt, string userId, string update_ip)
        {
            string sql = @"
                update PH_INVOICE
                   set invoice = :invoice,
                       invoice_dt = twn_todate(:invoice_dt),
                       update_user = :userId,
                       update_ip = :update_ip
                 where po_no = :po_no
                   and mmcode = :mmcode
                   and transno = :transno
                   and ckstatus <> 'Y'
            ";
            return DBWork.Connection.Execute(sql, new { po_no, mmcode, transno, invoice, invoice_dt, userId, update_ip }, DBWork.Transaction);
        }
        #endregion

        public IEnumerable<ComboItemModel> GetDateRanges()
        {
            string sql = @"
                        select set_ym EXTRA1, twn_date(set_btime) text, twn_date(nvl(set_ctime, set_etime)) value 
                        from MI_MNSET 
                        order by set_ym desc 
            ";
            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }

        public DataTable GetDupInvoice(string p1, string p2)
        {
            string sql = @"
                with tmp_invoice as
                (
                    select a.INVOICE, a.MMCODE
                    from PH_INVOICE a
                    where twn_date(a.invoice_dt) between :p1 and :p2
                    and a.invoice_type = '1'
                    and nvl(a.status, 'N') ='N'
                    group by a.invoice, a.mmcode, a.invoice_dt, a.po_price, a.is_include_tax, a.disc_cprice
                )
                select INVOICE, MMCODE from tmp_invoice T
                where (select count(*) from tmp_invoice where INVOICE = T.INVOICE and MMCODE = T.MMCODE) > 1
            ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, new { P1 = p1, P2 = p2 }))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<INVOICE_INFO> GetDeleteInvoiceInfo(string p1, string p2)
        {
            string sql = @"
                select invoice, mmcode
                  from PH_INVOICE a
                 where twn_date(a.deli_dt) between :p1 and :p2
                   and a.invoice_type = '1'
                   and nvl(a.status, 'N') ='N'
                 group by invoice, mmcode
            ";
            return DBWork.Connection.Query<INVOICE_INFO>(sql, new { p1, p2 }, DBWork.Transaction);
        }

        public int DeleteInvoice(string invoice, string mmcode)
        {
            string sql = @"
                delete from INVOICE where invoice = :invoice and mmcode = :mmcode
            ";

            return DBWork.Connection.Execute(sql, new { invoice, mmcode }, DBWork.Transaction);
        }

        public IEnumerable<BE0007> Report(string agen_no, string invoice, string start_date, string end_date, string data_ym, string mat_class)
        {
            string sql = GetSql(agen_no, invoice, start_date, end_date, data_ym, mat_class);

            if (mat_class.Contains("SUB_"))
                mat_class = mat_class.Replace("SUB_", "");

            return DBWork.Connection.Query<BE0007>(sql, new { agen_no, invoice, start_date, end_date, data_ym, mat_class }, DBWork.Transaction);
        }

        public bool CheckHasEmptyInvoice(string start_date, string end_date)
        {
            string sql = @"
                select 1 from PH_INVOICE a
                 where twn_date(a.deli_dt) between :start_date and :end_date
                   and a.invoice_type = '1'
                   and nvl(a.status, 'N') ='N' 
                   and invoice is null
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { start_date, end_date }, DBWork.Transaction) != null;
        }

        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT, A.M_CONTPRICE, A.DISC_CPRICE,
                                A.M_AGENNO, B.AGEN_NAMEC, a.M_NHIKEY, b.UNI_NO
                        
                          FROM MI_MAST A, PH_VENDER B 
                         WHERE 1=1 
                           AND A.M_AGENNO = B.AGEN_NO
                           AND nvl(A.CANCEL_ID, 'N') <> 'Y' ";

            sql += " {1} ";

            if (p0 != "")
            {
                sql = string.Format(sql,
                     "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(MMNAME_E), UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,",
                     @"   AND (UPPER(A.MMCODE) LIKE UPPER(:MMCODE) OR UPPER(MMNAME_E) LIKE UPPER(:MMNAME_E) OR UPPER(MMNAME_C) LIKE UPPER(:MMNAME_C))");
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

        public string CheckSamePhrInsert(string mmcode, string invoice)
        {
            var p = new DynamicParameters();

            string sql = @"
                select (case when count(*) = 1 then 'N' else 'Y' end)
                            from (
                            select agen_no from MM_PO_M where po_no in (select po_no from PH_INVOICE where invoice = :invoice)
                            union
                            select m_agenno from MI_MAST where mmcode = :mmcode) 
            ";
            p.Add(":invoice", invoice);
            p.Add(":mmcode", mmcode);
            return DBWork.Connection.ExecuteScalar<string>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetInvoiceCombo(string start_date, string end_date) {
            string sql = @"
                select distinct invoice as value
                  from PH_INVOICE
                 where twn_date(deli_dt) between :start_date and :end_date
                   and invoice is not null
                 order by invoice
            ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { start_date, end_date }, DBWork.Transaction);
        }
    }
}