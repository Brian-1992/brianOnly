using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.BE
{
    public class BE0003Repository : JCLib.Mvc.BaseRepository
    {
        public BE0003Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<BE0003> GetAll(string[] arr_p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8,
                                          string p9, string p10, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string mat_class = "";
            for (int i = 0; i < arr_p0.Length; i++)
            {
                if (i == 0)
                    mat_class = @" '" + arr_p0[i] + "'";
                else
                    mat_class += @",'" + arr_p0[i] + "'";
            }
            var sql = @"select  a.PO_NO, (select  AGEN_NO||' '||AGEN_NAMEC from PH_VENDER where agen_no=a.agen_no) as AGEN_NO,
                    b.INVOICE, b.INVOICE as INVOICE_OLD, TWN_DATE(b.INVOICE_DT) as INVOICE_DT,  b.MMCODE, m.MMNAME_C, m.MMNAME_E,
                    PO_PRICE, b.DELI_QTY,b.CKIN_QTY, ";
            if (mat_class.IndexOf("01") > -1) //藥品
                sql += " to_char(round(b.PO_PRICE* b.CKIN_QTY),'999,999,999')  as AMOUNT,";
            else
                sql += " to_char(floor(b.PO_PRICE* b.CKIN_QTY),'999,999,999')  as AMOUNT,";

            sql += @" case when b.DELI_DT is not null then TWN_DATE(b.DELI_DT)
                         else (select nvl2(min(acc_time),TWN_DATE(min(acc_time)),null) from BC_CS_ACC_LOG where po_no=a.po_no and mmcode=b.mmcode) end DELI_DT,
                    b.MEMO, b.TRANSNO, b.TRANSNO as TRANSNO_OLD,
                    (select listagg(INVOICE, ',') within group (order by mmcode) from PH_REPLY_LOG where po_no=a.PO_NO and mmcode=b.mmcode and INVOICE is not null ) CHG_LIST,
                    (select count(invoice) from PH_REPLY_LOG where po_no=a.PO_NO and mmcode=b.mmcode) CHG_CNT,
                    (case when (a.mat_class<>'01' and a.M_CONTID='0') then '合約' when (a.mat_class<>'01' and  a.M_CONTID='2') then '非合約' when (a.mat_class<>'01' and a.M_CONTID='3') then '小採' when (a.mat_class='01' and (a.contracno='0N' or a.contracno='0Y' or a.contracno='X')) then '零購' when (a.mat_class='01' and (a.contracno='1' or a.contracno='2' or a.contracno='01' or a.contracno='02'))  then '合約' else ''  end) as M_CONTID, ";
            if (mat_class.IndexOf("01") > -1) //藥品 round()
                sql += @" (case when INVOICE is not null then (select to_char(sum(round(PO_PRICE * CKIN_QTY)),'999,999,999')  from PH_INVOICE where po_no=B.PO_NO and invoice=b.invoice) else '' end) as INVOICE_TOT, ";
            else
                sql += @" (case when INVOICE is not null then (select to_char(sum(floor(PO_PRICE * CKIN_QTY)),'999,999,999')  from PH_INVOICE where po_no=B.PO_NO and invoice=b.invoice) else '' end) as INVOICE_TOT, ";

            sql += @" b.M_PHCTNCO, b.PO_QTY,
                     (case when b.CKSTATUS='N' then '未驗證' when b.CKSTATUS='Y' then '已驗證' when b.CKSTATUS='T' then '驗退' end) CKSTATUS
                    FROM MM_PO_M a, PH_INVOICE b, MI_MAST m
                    where substr(a.po_no,1,3) not in ('TXT') and a.po_no=b.po_no and b.mmcode=m.mmcode ";
            if (p8 != "")
            {
                sql += " AND  a.po_no like :p8 ";
                p.Add(":p8", string.Format("{0}", p8 + "%"));
            }
            if (arr_p0.Length > 0)
            {
                sql += @" AND a.mat_class in (" + mat_class + ") ";
            }
            if (p1 != "" & p2 != "")
            {
                sql += " AND TWN_DATE(A.po_time) BETWEEN :p1 AND :p2 ";
                p.Add(":p1", string.Format("{0}", p1));
                p.Add(":p2", string.Format("{0}", p2));
            }
            if (p1 != "" & p2 == "")
            {
                sql += " AND TWN_DATE(A.po_time) >= :p1 ";
                p.Add(":p1", string.Format("{0}", p1));
            }
            if (p1 == "" & p2 != "")
            {
                sql += " AND TWN_DATE(A.po_time) <= :p2 ";
                p.Add(":p2", string.Format("{0}", p2));
            }
            if (p3 != "")
            {
                if (p3 == "2")
                {
                    sql += " AND ( b.INVOICE is null or b.INVOICE_DT is null )";
                }
                else if (p3 == "3")
                {
                    sql += " AND ( b.INVOICE is not null AND b.INVOICE_DT is not null )";
                }
            }
            if (p4 != "")
            {
                sql += " AND a.agen_no = :p4 ";
                p.Add(":p4", string.Format("{0}", p4));
            }
            if (p5 != "")
            {
                sql += " AND b.INVOICE LIKE :p5 ";
                p.Add(":p5", string.Format("%{0}%", p5));
            }
            if (p6 != "")
            {
                if (mat_class.IndexOf("01") > -1) //藥品 
                {
                    if (p6 == "2") //零購
                    {
                        sql += " AND a.contracno in ('0Y','0N','X') ";
                    }
                    else // 合約
                    {
                        sql += " AND a.contracno in ('1','2','01','02', '3', '03') ";
                    }
                }
                else
                {
                    sql += " AND a.M_CONTID = :p6 ";
                    p.Add(":p6", string.Format("{0}", p6));
                }
            }
            if (p7 != "")
            {
                sql += " AND  b.CKSTATUS = :p7 ";
                p.Add(":p7", string.Format("{0}", p7));
            }

            sql = string.Format(@"
                select *
                  from (
                        {0}
                       )
                 where 1=1
            ", sql);
            if (string.IsNullOrEmpty(p9) == false) {
                sql += " and deli_dt >= :p9";
                p.Add(":p9", string.Format("{0}", p9));
            }
            if (string.IsNullOrEmpty(p10) == false)
            {
                sql += " and deli_dt <= :p10";
                p.Add(":p10", string.Format("{0}", p10));
            }


            //sql += " order by b.po_no, b.invoice, b.mmcode ";
            sql += " order by po_no, invoice, mmcode ";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BE0003>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<BE0003> Get(string po_no, string mmcode, string transno)
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
            return DBWork.Connection.Query<BE0003>(sql, new { PO_NO = po_no, MMCODE = mmcode, TRANSNO = transno }, DBWork.Transaction);
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
                        CHK_USER = :UPDATE_USER, CHK_DT = SYSDATE,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                        WHERE PO_NO = :PO_NO AND MMCODE = :MMCODE AND TRANSNO = :TRANSNO
                        AND CKSTATUS ='N' ";
            p.Add(":PO_NO", string.Format("{0}", po_no));
            p.Add(":MMCODE", string.Format("{0}", mmcode));
            p.Add(":TRANSNO", string.Format("{0}", transno));
            p.Add(":INVOICE_DT", string.Format("{0}", invoicedt));
            p.Add(":INVOICE", string.Format("{0}", invoice));
            p.Add(":UPDATE_USER", string.Format("{0}", user));
            p.Add(":UPDATE_IP", string.Format("{0}", ip));
            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }
        //同一發票不可以 不同發票日期
        public string ChkInvoice(string invoicedt, string invoice)
        {
            string ret = "N";
            string tmp = "";
            var p = new DynamicParameters();

            var sql = @"select to_char(b.INVOICE_DT,'yyyymmdd') 
                        FROM MM_PO_M a, PH_INVOICE b
                    where substr(a.po_no,1,3) not in ('TXT') and a.po_no=b.po_no and b.INVOICE=:invoice and a.po_time BETWEEN sysdate-30 AND sysdate";
            p.Add(":invoice", invoice);
            tmp = DBWork.Connection.ExecuteScalar<string>(sql, p, DBWork.Transaction);
            if (tmp != null && !tmp.Equals(invoicedt.Replace("-", "").Replace("/", "")))
                ret = "Y";
            return ret;
        }
        //同一發票不可以出現在1家以上廠商
        public string ChkInvoicePONO(string invoicedt, string invoice)
        {
            string ret = "N";
            var p = new DynamicParameters();

            var sql = @"select count(distinct a.po_no) FROM MM_PO_M a, PH_INVOICE b
                    where substr(a.po_no,1,3) not in ('TXT') and a.po_no=b.po_no and b.INVOICE=:invoice and a.po_time BETWEEN sysdate-30 AND sysdate";
            p.Add(":invoice", invoice);
            int tmp = DBWork.Connection.ExecuteScalar<int>(sql, p, DBWork.Transaction);
            if (tmp > 1) ret = "Y";
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

            var sql = @"delete from PH_INVOICE
                        where PO_NO = :PO_NO AND MMCODE = :MMCODE AND TRANSNO = :TRANSNO
                        and CKSTATUS <>'Y' 
                        and (select count(*) from PH_INVOICE 
                            where PO_NO = :PO_NO AND MMCODE = :MMCODE) > 1 ";
            p.Add(":PO_NO", string.Format("{0}", po_no));
            p.Add(":MMCODE", string.Format("{0}", mmcode));
            p.Add(":TRANSNO", string.Format("{0}", transno));
            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }
        public int UpdateM(BE0003 be03)
        {
            var sql = @"UPDATE PH_INVOICE SET INVOICE = :INVOICE, INVOICE_DT = TO_DATE(:INVOICE_DT,'YYYY/MM/DD'),
                                CKIN_QTY = :CKIN_QTY, MEMO = :MEMO, CHK_USER = :UPDATE_USER, CHK_DT = SYSDATE,
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE PO_NO = :PO_NO AND MMCODE = :MMCODE AND TRANSNO = :TRANSNO
                                AND CKSTATUS <>'Y' ";
            return DBWork.Connection.Execute(sql, be03, DBWork.Transaction);
        }
        public int UpdateB(BE0003 be03)
        {
            var sql = @"UPDATE PH_INVOICE SET CKSTATUS = 'T', FLAG='A', MEMO = :MEMO, CHK_USER = :UPDATE_USER, CHK_DT = SYSDATE,
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE PO_NO = :PO_NO AND MMCODE = :MMCODE AND TRANSNO = :TRANSNO 
                                AND CKSTATUS <>'Y' ";
            return DBWork.Connection.Execute(sql, be03, DBWork.Transaction);
        }
        public int CreateM(BE0003 be03)
        {
            var sql = @"INSERT INTO PH_INVOICE (
                          PO_NO, MMCODE, TRANSNO, PO_QTY, PO_PRICE, DELI_DT,
                          M_PURUN, M_AGENLAB, PO_AMT, 
                          M_DISCPERC, BW_SQTY, UPRICE, 
                          ACCOUNTDATE, DELI_STATUS, STATUS, CKSTATUS, FLAG,  
                          PR_NO, M_PHCTNCO, DISC_CPRICE, DISC_UPRICE, UNIT_SWAP,  
                          DELI_QTY, MEMO, INVOICE, INVOICE_DT, 
                          CKIN_QTY, CHK_USER, CHK_DT, CREATE_USER, UPDATE_IP, CREATE_TIME) 
                      select 
                        PO_NO, MMCODE, TO_CHAR(sysdate,'yyyymmddhh24miss'), PO_QTY, PO_PRICE, DELI_DT,
                        M_PURUN, M_AGENLAB, PO_PRICE*:DELI_QTY, 
                        M_DISCPERC, BW_SQTY, UPRICE,  
                        ACCOUNTDATE, DELI_STATUS, STATUS, 'N', FLAG,  
                        PR_NO, M_PHCTNCO, DISC_CPRICE, DISC_UPRICE, UNIT_SWAP,  
                        :DELI_QTY, :MEMO, :INVOICE, TO_DATE(:INVOICE_DT,'YYYY/MM/DD'), 
                        :CKIN_QTY, :CREATE_USER, SYSDATE, :CREATE_USER, :UPDATE_IP, SYSDATE
                      from PH_INVOICE 
                      where PO_NO=:PO_NO and MMCODE=:MMCODE and TRANSNO=:TRANSNO_OLD ";
            return DBWork.Connection.Execute(sql, be03, DBWork.Transaction);
        }
        public IEnumerable<COMBO_MODEL> GetMatclassCombo(string userid)
        {
            string sql = @"SELECT count(*)
                        FROM MI_MATCLASS WHERE mat_clsid in (select task_id from mi_whid where wh_userid =:USERID)";
            int tmp = DBWork.Connection.ExecuteScalar<int>(sql, new { USERID = userid }, DBWork.Transaction);
            if (tmp == 0) //user 沒有建 mi_whid 資料, 給衛材LIST
                sql = @" SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                             MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM
                        FROM MI_MATCLASS WHERE  mat_clsid in ('2','3')
                        ORDER BY MAT_CLASS";
            else
                sql = @" SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                             MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM
                        FROM MI_MATCLASS WHERE  mat_clsid in (select task_id from mi_whid where wh_userid=:USERID)
                        ORDER BY MAT_CLASS";

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

        public DataTable GetExcel(string mat_classes, string start_dt, string end_dt, string agen_no, string deli_dt_s, string deli_dt_e)
        {
            string sql = string.Format(@"
                SELECT b.PO_NO as 訂單號碼,
                       b.transno as 資料流水號,
                       b.MMCODE as 院內碼, 
                       b.INVOICE as 發票號碼,
                       TWN_DATE (b.INVOICE_DT)  as 發票日期,
                       a.agen_no 廠商代碼,
                       (SELECT AGEN_NAMEC FROM PH_VENDER
                         WHERE agen_no = a.agen_no) AS 廠商名稱,
                       (SELECT uni_no FROM PH_VENDER
                         WHERE agen_no = a.agen_no) AS 廠商統一編號,
                       m.MMNAME_C as 中文名稱,
                       m.MMNAME_E as 英文名稱,
                       b.PO_PRICE as 合約價,
                       b.DELI_QTY as 發票購買藥品數量,
                       (b.PO_PRICE * b.DELI_QTY) as 發票金額,
                       (CASE
                          WHEN b.DELI_DT IS NOT NULL
                          THEN
                             TWN_DATE (b.DELI_DT)
                          ELSE
                             (SELECT NVL2 (MIN (acc_time), TWN_DATE (MIN (acc_time)), NULL)
                                FROM BC_CS_ACC_LOG
                               WHERE po_no = a.po_no AND mmcode = b.mmcode)
                       END) as 進貨日期
                  FROM MM_PO_M a, PH_INVOICE b, MI_MAST m
                 WHERE     SUBSTR (a.po_no, 1, 3) NOT IN ('TXT')
                   AND a.po_no = b.po_no
                   AND b.mmcode = m.mmcode
                   {1}
                   AND TWN_DATE (A.po_time) BETWEEN :start_dt AND :end_dt
                   {0}
                 ORDER BY b.po_no, b.mmcode, b.invoice
            ", string.IsNullOrEmpty(agen_no) ? string.Empty : "AND a.agen_no = :agen_no"
             , string.IsNullOrEmpty(mat_classes) ? string.Empty : string.Format("AND a.mat_class IN ( {0} )", mat_classes));

            sql = string.Format(@"
                select * 
                  from (
                        {0}
                       )
                 where 1=1
            ", sql);
            if (string.IsNullOrEmpty(deli_dt_s) == false)
            {
                sql += " and 進貨日期 >= :deli_dt_s";
            }
            if (string.IsNullOrEmpty(deli_dt_e) == false)
            {
                sql += " and 進貨日期 <= :deli_dt_e";
            }


            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, new { mat_classes, start_dt, end_dt, agen_no, deli_dt_s, deli_dt_e }))
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
    }
}