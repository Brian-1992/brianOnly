using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;

namespace WebApp.Repository.B
{
    public class BE0006Repository : JCLib.Mvc.BaseRepository
    {
        public BE0006Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        public IEnumerable<COMBO_MODEL> GetMatClassCombo(string str_UserID)
        {
            string sql = @"select MAT_CLASS as VALUE, MAT_CLSNAME as TEXT, 
                                  MAT_CLASS || ' ' || MAT_CLSNAME as COMBITEM
                             from MI_MATCLASS 
                            where MAT_CLASS between '01' and '02' ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { UserID = str_UserID });
        }

        public IEnumerable<COMBO_MODEL> GetAGEN_NO()
        {
            string sql = @"select AGEN_NO as VALUE,AGEN_NAMEC,AGEN_NAMEE,
                                  AGEN_NO || ' ' || TRIM(AGEN_NAMEC) as COMBITEM
                             from PH_VENDER
                            where (REC_STATUS <> 'X' or REC_STATUS is null)
                            order by AGEN_NO";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }

        public IEnumerable<BE0006> GetInvoiceEmail(string mat_class,string DELI_DT_Start, string DELI_DT_End, bool isAll)
        {
            string sql = @"select PO_NO, MMCODE, TRANSNO,AGEN_NO, MMNAME_C,
                                  MMNAME_E, PO_QTY, DELI_QTY, TWN_DATE(DELI_DT) as DELI_DT,EMAIL as agen_email,
                                  twn_date(EMAIL_DT) as EMAIL_DT, twn_date(REPLY_DT) as REPLY_DT, STATUS, UPDATE_USER, UPDATE_TIME,
                                  UPDATE_IP,MAT_CLASS,M_CONTID,PO_TIME,PO_PRICE,
                                  AMOUNT,INVOICE,INVOICE_DT,
                                  (select AGEN_NO||' '||AGEN_NAMEC from PH_VENDER where agen_no=a.agen_no) as AGEN_NAME
                             from INVOICE_EMAIL a
                            where 1=1           
                              and trunc(a.DELI_DT) >= TWN_TODATE(:DELI_DT_Start) 
                              and trunc(a.DELI_DT) <= TWN_TODATE(:DELI_DT_End) 
                          ";
            DynamicParameters p = new DynamicParameters();
            if (string.IsNullOrEmpty(mat_class) == false)
            {
                sql += @"  and mat_class = :mat_class";
                p.Add(":mat_class", mat_class);
            }
            p.Add(":DELI_DT_Start", DELI_DT_Start);
            p.Add(":DELI_DT_End", DELI_DT_End);
            sql += @" order by agen_no, po_no, invoice, mmcode";

            if (isAll)
            {
                return DBWork.Connection.Query<BE0006>(sql, p, DBWork.Transaction);
            }

            return DBWork.PagingQuery<BE0006>(sql, p, DBWork.Transaction);
        }


        public IEnumerable<BE0006> GetAll(string mat_class,string DELI_DT_Start,string DELI_DT_End,bool isAll)
        {
            string sql = @"
                           select c.*
                             from (select a.PO_NO,  
                                         (case when (a.mat_class<>'01' and a.M_CONTID='0') then '合約' 
                                               when (a.mat_class<>'01' and a.M_CONTID='2') then '非合約'
                                               when (a.mat_class<>'01' and a.M_CONTID='3') then '小採'
                                               when (a.mat_class='01' and (a.contracno='0N' or a.contracno='0Y' or a.contracno='X')) then '零購'
                                               when (a.mat_class='01' and (a.contracno='1'  or a.contracno='2'  or a.contracno='3' or
                                                     a.contracno='01' or a.contracno='02' or a.contracno='03')) then '合約'
                                          else ''
                                          end) as M_CONTID, 
                                         (select AGEN_NO||' '||AGEN_NAMEC from PH_VENDER where agen_no=a.agen_no) as AGEN_NAME,  
                                         a.agen_no,b.INVOICE,TWN_DATE(b.INVOICE_DT) as INVOICE_DT, 
                                         (select EMAIL_DT 
                                           from INVOICE_EMAIL
                                          where b.po_no=po_no and b.mmcode=mmcode
                                            and b.TRANSNO =TRANSNO) as EMAIL_DT,
                                         (select REPLY_DT 
                                            from INVOICE_EMAIL
                                           where b.po_no=po_no and b.mmcode=mmcode
                                             and b.TRANSNO =TRANSNO) as REPLY_DT,
                                          b.MMCODE,m.MMNAME_C,m.MMNAME_E,
                                          b.PO_PRICE,b.PO_QTY,b.DELI_QTY, 
                                         (case when a.mat_class='01' then to_char(round(b.PO_PRICE* b.CKIN_QTY),'999,999,999')  
                                                 else to_char(floor(b.PO_PRICE* b.CKIN_QTY),'999,999,999') end) as AMOUNT,  
                                         (case when a.mat_class='01' then to_char(round(b.PO_PRICE* b.CKIN_QTY))  
                                                 else to_char(floor(b.PO_PRICE* b.CKIN_QTY)) end) as AMOUNT_1,
                                         (case when b.DELI_DT is not null then TWN_DATE(b.DELI_DT)
                                               else (select nvl2(min(acc_time), TWN_DATE(min(acc_time)), null)
                                                       from BC_CS_ACC_LOG 
                                                      where po_no = a.po_no and mmcode = b.mmcode)
                                          end) as DELI_DT,   
                                         (case when b.DELI_DT is not null then b.DELI_DT
                                               else (select nvl2(min(acc_time),min(acc_time),null) 
                                                       from BC_CS_ACC_LOG where po_no=a.po_no and mmcode=b.mmcode)
                                         end) as DELI_DT_1, 
                                         TWN_DATE(a.PO_TIME) as TWN_PO_TIME,
                                         (select (case when EMAIL is not null then EMAIL
                                                       when EMAIL is null and EMAIL_1 is not null then EMAIL_1
                                                       else ''
                                                  end) as agen_email  
                                            from PH_VENDER 
                                           where agen_no = a.agen_no) as agen_email,  
                                          b.TRANSNO,a.mat_class                         
                                     from MM_PO_M a, PH_INVOICE b, MI_MAST m 
                                    where substr(a.po_no, 1, 3) not in ('TXT')
                                      and a.po_no = b.po_no and b.mmcode = m.mmcode                              
                                      and (b.INVOICE is null or b.INVOICE_DT is null)
                                      and b.DELI_QTY>0 
                                      and (select count(*) 
                                             from INVOICE_EMAIL 
                                            where b.po_no=po_no and b.mmcode=mmcode
                                              and b.TRANSNO =TRANSNO)=0
                                 ) c
                            where 1=1
                              and trunc(c.DELI_DT_1) >= TWN_TODATE(:DELI_DT_Start) 
                              and trunc(c.DELI_DT_1) <= TWN_TODATE(:DELI_DT_End) 
            ";
            DynamicParameters p = new DynamicParameters();
            if (string.IsNullOrEmpty(mat_class) == false)
            {
                sql += @"  and c.mat_class = :mat_class";
                p.Add(":mat_class", mat_class);
            }
            p.Add(":mat_class", mat_class);
            p.Add(":DELI_DT_Start", DELI_DT_Start);
            p.Add(":DELI_DT_End", DELI_DT_End);
            sql += @" order by c.agen_no, c.po_no, c.invoice, c.mmcode";

            if (isAll)
            {
                return DBWork.Connection.Query<BE0006>(sql, p, DBWork.Transaction);
            }

            return DBWork.PagingQuery<BE0006>(sql, p, DBWork.Transaction);
        }


        public int InsINVOICE_EMAIL(string PO_NO, string MMCODE, string TRANSNO, string AGEN_NO, string MMNAME_C,
                                    string MMNAME_E, string PO_QTY, string DELI_QTY, string DELI_DT, string agen_email,
                                    string EMAIL_DT, string REPLY_DT, string UPDATE_USER, string UPDATE_IP,string MAT_CLASS,
                                    string M_CONTID, string PO_TIME, string PO_PRICE, string AMOUNT, string INVOICE, 
                                    string INVOICE_DT)
        {
            var sql = @"insert into INVOICE_EMAIL (PO_NO, MMCODE, TRANSNO,AGEN_NO, MMNAME_C,
                                                   MMNAME_E, PO_QTY, DELI_QTY, DELI_DT, EMAIL,
                                                   EMAIL_DT, REPLY_DT, STATUS, UPDATE_USER, UPDATE_TIME,
                                                   UPDATE_IP,MAT_CLASS,M_CONTID,PO_TIME,PO_PRICE,
                                                   AMOUNT,INVOICE,INVOICE_DT)                                                  
                        values (:PO_NO,:MMCODE,:TRANSNO,:AGEN_NO,:MMNAME_C,
                                :MMNAME_E, :PO_QTY, :DELI_QTY, TWN_TODATE(:DELI_DT), :agen_email, 
                                TWN_TODATE(:EMAIL_DT),TWN_TODATE(:REPLY_DT), 'A', :UPDATE_USER, sysdate, 
                                :UPDATE_IP,:MAT_CLASS,:M_CONTID,:PO_TIME,:PO_PRICE,
                                :AMOUNT,:INVOICE,TWN_TODATE(:INVOICE_DT))
                        ";
            //return DBWork.Connection.Execute(sql, def, DBWork.Transaction);

            return DBWork.Connection.Execute(sql, new
            {
                PO_NO,MMCODE,TRANSNO,AGEN_NO,MMNAME_C,
                MMNAME_E,PO_QTY,DELI_QTY,DELI_DT,agen_email,
                EMAIL_DT,REPLY_DT,UPDATE_USER,UPDATE_IP,MAT_CLASS,
                M_CONTID,PO_TIME,PO_PRICE,AMOUNT,INVOICE,
                INVOICE_DT
            }, DBWork.Transaction);
        }

        public DataTable GetAllExcel(string mat_class, string DELI_DT_Start, string DELI_DT_End)
        {
            DynamicParameters p = new DynamicParameters();
            string sql = @"
                           select a.PO_NO as 訂單號碼,a.M_CONTID as 合約類別, 
                                  (select AGEN_NO||' '||AGEN_NAMEC from PH_VENDER where agen_no=a.agen_no) as 廠商名稱,  
                                  a.agen_no as 廠商碼,a.INVOICE as 發票號碼,TWN_DATE(a.INVOICE_DT) as 發票日期,  
                                  twn_date(a.EMAIL_DT) as 寄EMAIL日期, twn_date(a.REPLY_DT) as 收信確認日期,  
                                  a.MMCODE as 院內碼,a.MMNAME_C as中文名稱,a.MMNAME_E as 英文名稱,
                                  a.PO_PRICE as 合約價,a.PO_QTY as 訂單數量,a.DELI_QTY as 進貨數量, 
                                  a.AMOUNT  as 單筆金額,TWN_DATE(a.DELI_DT) as 進貨日期,  
                                  TWN_DATE(a.PO_TIME) as 訂單日期,a.EMAIL as 廠商email
                             from INVOICE_EMAIL a
                            where 1=1 
                              and trunc(a.DELI_DT) >= TWN_TODATE(:DELI_DT_Start) 
                              and trunc(a.DELI_DT) <= TWN_TODATE(:DELI_DT_End) 
                          ";
            if (string.IsNullOrEmpty(mat_class) == false)
            {
                sql += @"  and a.mat_class = :mat_class";
                p.Add(":mat_class", mat_class);
            }
            p.Add(":DELI_DT_Start", DELI_DT_Start);
            p.Add(":DELI_DT_End", DELI_DT_End);
            sql += @" order by a.agen_no, a.po_no, a.invoice, a.mmcode";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public int UpdateInvoiceEmail(string sUserID, string sIP)
        {
            var p = new DynamicParameters();
            var sql = @" update INVOICE_EMAIL
                            set STATUS='B',
                                EMAIL_DT=sysdate,
                                UPDATE_USER = :sUserID,
                                UPDATE_TIME =sysdate,
                                UPDATE_IP = :sIP
                          where STATUS='a' and EMAIL is not null
                        ";
            p.Add(":sUserID", sUserID);
            p.Add(":sIP", sIP);

            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public int UpInvoiceEmail_Status(string mat_class,string DELI_DT_Start,string DELI_DT_End)
        {
            var sql = @" update INVOICE_EMAIL
                            set STATUS='a'
                          where STATUS in ('A','B') and EMAIL is not null
                            and mat_class=:mat_class
                            and trunc(DELI_DT) >= TWN_TODATE(:DELI_DT_Start) 
                            and trunc(DELI_DT) <= TWN_TODATE(:DELI_DT_End) 
                       ";  
            return DBWork.Connection.Execute(sql, new { mat_class, DELI_DT_Start, DELI_DT_End }, DBWork.Transaction);
        }

        public IEnumerable<BE0006> GetAGEN_NO_forMail()
        {
            var sql = @" select distinct AGEN_NO
                           from INVOICE_EMAIL
                          where STATUS = 'a'
                        ";
            return DBWork.Connection.Query<BE0006>(sql, "", DBWork.Transaction);

        }

        public int UpInvoiceEmail_Batno(string AGEN_NO, string Batno)
        {
            var sql = @" update INVOICE_EMAIL
                            set INVOICE_BATNO=:Batno
                          where STATUS='a' and AGEN_NO=:AGEN_NO
                        ";
            return DBWork.Connection.Execute(sql, new { AGEN_NO, Batno }, DBWork.Transaction);
        }

        public IEnumerable<BE0006> GetBatno()
        {
            var sql = @" 
                         select INVOICE_BATNO_SEQ.nextval as Batno from dual
                        ";
            return DBWork.Connection.Query<BE0006>(sql, "", DBWork.Transaction);

        }

        public int DelInvoiceEmail(string mat_class)
        {
            var sql = @" Delete from (select a.*
                           from INVOICE_EMAIL a,PH_INVOICE b
                          where a.po_no=b.po_no and a.mmcode=b.mmcode and a.transno=b.transno
                            and a.MAT_CLASS = :mat_class                            
                            and not (b.INVOICE is null or b.INVOICE_DT is null))
                       ";
            return DBWork.Connection.Execute(sql, new { mat_class}, DBWork.Transaction);
        }
    }
}