using System;
using System.Data;
using JCLib.DB;
using Dapper;
using System.Collections.Generic;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using WebApp.Models.AA;
using System.Text;
using WebApp.Models;

namespace WebApp.Repository.AA
{
    public class AA0153Repository : JCLib.Mvc.BaseRepository
    {
        public AA0153Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0153> GetAllM(AA0153 query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select A.PO_NO, A.MAT_CLASS, A.AGEN_NO, (select AGEN_NAMEC from PH_VENDER where AGEN_NO = A.AGEN_NO) as AGEN_NAMEC,
                (select MAT_CLSNAME from MI_MATCLASS where MAT_CLASS = A.MAT_CLASS) as MAT_CLSNAME,
                TWN_DATE(A.PO_TIME) as PO_TIME
                from MM_PO_M A
                where (select count(*) from MM_PO_D where PO_NO = A.PO_NO and DELI_STATUS <> 'Y') > 0 ";

            if (query.MAT_CLASS != "")
            {
                if (query.MAT_CLASS.Contains("SUB_"))
                {
                    sql += " and (select count(*) from MM_PO_D C left join MI_MAST D on C.MMCODE = D.MMCODE where A.PO_NO = C.PO_NO and D.MAT_CLASS_SUB = :MAT_CLASS) > 0";
                    p.Add(":MAT_CLASS", string.Format("{0}", query.MAT_CLASS.Replace("SUB_", "")));
                }
                else
                {
                    sql += " and MAT_CLASS = :MAT_CLASS ";
                    p.Add(":MAT_CLASS", string.Format("{0}", query.MAT_CLASS));
                }  
            }
            if (query.PO_TIME_S != "")
            {
                sql += " and TWN_DATE(PO_TIME)>=:PO_TIME_S ";
                p.Add(":PO_TIME_S", string.Format("{0}", query.PO_TIME_S));
            }
            if (query.PO_TIME_E != "")
            {
                sql += " and TWN_DATE(PO_TIME)<=:PO_TIME_E ";
                p.Add(":PO_TIME_E", string.Format("{0}", query.PO_TIME_E));
            }
            if (query.PO_NO != "")
            {
                sql += " and A.PO_NO like :PO_NO ";
                p.Add(":PO_NO", string.Format("%{0}%", query.PO_NO));
            }

            if (query.AGEN_NO != "")
            {
                sql += " and A.AGEN_NO = :AGEN_NO ";
                p.Add(":AGEN_NO", string.Format("{0}", query.AGEN_NO));
            }

            if (query.MMCODE != "")
            {
                sql += " and (select count(*) from MM_PO_D where PO_NO = A.PO_NO and MMCODE like :MMCODE and PO_QTY <> DELI_QTY) > 0 ";
                p.Add(":MMCODE", string.Format("%{0}%", query.MMCODE));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0153>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA0153> GetAllD(AA0153 query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select A.PO_NO, A.MAT_CLASS, 
                (select MAT_CLSNAME from MI_MATCLASS where MAT_CLASS = A.MAT_CLASS) as MAT_CLSNAME,
                TWN_DATE(A.PO_TIME) as PO_TIME ,B.MMCODE, 
                (select STORE_LOC from MI_WLOCINV where WH_NO = A.WH_NO and MMCODE = B.MMCODE and rownum = 1) as STORE_LOC,
                B.PO_QTY, B.DELI_QTY, B.M_PURUN, B.UNIT_SWAP, 
                B.PO_PRICE, (SELECT M_CONTPRICE FROM MI_MAST WHERE MMCODE = B.MMCODE) as M_CONTPRICE, 
                B.M_DISCPERC, 
                B.UPRICE, (SELECT UPRICE FROM MI_MAST WHERE MMCODE = B.MMCODE) UPRICE_M, 
                B.DISC_CPRICE, (SELECT DISC_CPRICE FROM MI_MAST WHERE MMCODE = B.MMCODE) as DISC_CPRICE_M, 
                B.DISC_UPRICE, (SELECT DISC_UPRICE FROM MI_MAST WHERE MMCODE = B.MMCODE) as DISC_UPRICE_M,
                B.CHINNAME, B.CHARTNO, B.SEQ, B.MMNAME_C, B.MMNAME_E,
                (SELECT M_CONTPRICE*B.DELI_QTY FROM MI_MAST WHERE MMCODE = B.MMCODE) as DELI_AMT,
                (B.PO_QTY-B.DELI_QTY) AS INQTY,
                (SELECT M_CONTPRICE*(B.PO_QTY-B.DELI_QTY) FROM MI_MAST WHERE MMCODE = B.MMCODE) as IN_CONT_AMT,
                (B.PO_PRICE*(B.PO_QTY-B.DELI_QTY)) as IN_PO_AMT,
                (B.DISC_CPRICE*(B.PO_QTY-B.DELI_QTY)) as IN_DISC_AMT
                from MM_PO_M A, MM_PO_D B
                where A.PO_NO = B.PO_NO and PO_QTY <> DELI_QTY ";

            if (query.PO_NO != "")
            {
                sql += " and A.PO_NO = :PO_NO ";
                p.Add(":PO_NO", string.Format("{0}", query.PO_NO));
            }
            if (query.MMCODE != "")
            {
                sql += " and B.MMCODE like :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", query.MMCODE));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0153>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA0153> GetAllAccLogM(AA0153 query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select distinct A.PO_NO, A.AGEN_NO, (select AGEN_NAMEC from PH_VENDER where AGEN_NO = A.AGEN_NO) as AGEN_NAMEC, TWN_DATE(B.PO_TIME) as PO_TIME
                from BC_CS_ACC_LOG A, MM_PO_M B
                where A.TX_QTY_T <> 0 and A.PO_NO=B.PO_NO ";

            if (query.MAT_CLASS != "")
            {
                if (query.MAT_CLASS.Contains("SUB_"))
                {
                    sql += " and (select count(*) from MI_MAST B where A.MMCODE = B.MMCODE and B.MAT_CLASS_SUB = :MAT_CLASS) > 0";
                    p.Add(":MAT_CLASS", string.Format("{0}", query.MAT_CLASS.Replace("SUB_", "")));
                }
                else
                {
                    sql += " and A.MAT_CLASS = :MAT_CLASS ";
                    p.Add(":MAT_CLASS", string.Format("{0}", query.MAT_CLASS));
                } 
            }
            if (query.PO_NO != "")
            {
                sql += " and A.PO_NO like :PO_NO ";
                p.Add(":PO_NO", string.Format("%{0}%", query.PO_NO));
            }

            if (query.AGEN_NO != "")
            {
                sql += " and A.AGEN_NO = :AGEN_NO ";
                p.Add(":AGEN_NO", string.Format("{0}", query.AGEN_NO));
            }
            if (query.PO_TIME_S != "")
            {
                sql += " and TWN_DATE(B.PO_TIME)>=:PO_TIME_S ";
                p.Add(":PO_TIME_S", string.Format("{0}", query.PO_TIME_S));
            }
            if (query.PO_TIME_E != "")
            {
                sql += " and TWN_DATE(B.PO_TIME)<=:PO_TIME_E ";
                p.Add(":PO_TIME_E", string.Format("{0}", query.PO_TIME_E));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0153>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA0153> GetAllAccLogD(AA0153 query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select PO_NO, MMCODE,
                (select MMNAME_C from MI_MAST where MMCODE = A.MMCODE) as MMNAME_C, 
                (select MMNAME_E from MI_MAST where MMCODE = A.MMCODE) as MMNAME_E, 
                SEQ, AGEN_NO, LOT_NO, TWN_DATE(EXP_DATE) as EXP_DATE, BW_SQTY, ACC_QTY, CFM_QTY, 
                ACC_BASEUNIT, STATUS, MEMO, TWN_DATE(ACC_TIME) as ACC_TIME, ACC_USER, STOREID, MAT_CLASS, PO_QTY, ACC_PURUN, 
                UNIT_SWAP, WEXP_ID, TX_QTY_T, INVOICE, to_char(INVOICE_DT, 'YYYY/MM/DD') as INVOICE_DT, EXTRA_DISC_AMOUNT, SRC_SEQ,
                (select 1 from BC_CS_ACC_LOG where SRC_SEQ = A.SEQ) as REF_CNT, A.CHINNAME, A.CHARTNO,
                (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='BC_CS_ACC_LOG' AND DATA_NAME='STATUS' AND DATA_VALUE=A.STATUS) STATUS_N
                from BC_CS_ACC_LOG A
                where TX_QTY_T <> 0 ";

            if (query.PO_NO != "")
            {
                sql += " and PO_NO = :PO_NO ";
                p.Add(":PO_NO", string.Format("{0}", query.PO_NO));
            }
            if (query.MMCODE != "")
            {
                sql += " and MMCODE like :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", query.MMCODE));
            }
            if (query.ACC_TIME_S != "")
            {
                sql += " and TWN_DATE(ACC_TIME)>=:ACC_TIME_S ";
                p.Add(":ACC_TIME_S", string.Format("{0}", query.ACC_TIME_S));
            }
            if (query.ACC_TIME_E != "")
            {
                sql += " and TWN_DATE(ACC_TIME)<=:ACC_TIME_E ";
                p.Add(":ACC_TIME_E", string.Format("{0}", query.ACC_TIME_E));
            }

            if (query.STATUS == "N")
            {
                sql += " and TX_QTY_T > 0 ";
            }
            if (query.STATUS2 == "N")
            {
                sql += " and (select count(*) from BC_CS_ACC_LOG where PO_NO = A.PO_NO and SRC_SEQ = A.SEQ) = 0 ";
            }

            if (query.LOT_NO != "")
            {
                sql += " and LOT_NO like :LOT_NO ";
                p.Add(":LOT_NO", string.Format("%{0}%", query.LOT_NO));
            }
            if (query.EXP_DATE_S != "")
            {
                sql += " and TWN_DATE(EXP_DATE)>=:EXP_DATE_S ";
                p.Add(":EXP_DATE_S", string.Format("{0}", query.EXP_DATE_S));
            }
            if (query.EXP_DATE_E != "")
            {
                sql += " and TWN_DATE(EXP_DATE)<=:EXP_DATE_E ";
                p.Add(":EXP_DATE_E", string.Format("{0}", query.EXP_DATE_E));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0153>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public AA0153 GetAccLog(string po_no, string mmcode, string seq)
        {
            var sql = string.Format(@"select PO_NO, MMCODE, SEQ, AGEN_NO, LOT_NO, TWN_DATE(EXP_DATE) as EXP_DATE, BW_SQTY, ACC_QTY, CFM_QTY, 
                ACC_BASEUNIT, STATUS, MEMO, TWN_DATE(ACC_TIME) as ACC_TIME, ACC_USER, STOREID, MAT_CLASS, PO_QTY, ACC_PURUN, 
                UNIT_SWAP, WEXP_ID, TX_QTY_T, INVOICE, TWN_DATE(INVOICE_DT) as INVOICE_DT, EXTRA_DISC_AMOUNT, CHINNAME, CHARTNO, SRC_SEQ, POACC_SEQ, PO_D_SEQ  
                from BC_CS_ACC_LOG 
                where PO_NO like '{0}%' 
                and MMCODE = :MMCODE and SEQ = :SEQ ", po_no);
            return DBWork.Connection.QueryFirstOrDefault<AA0153>(sql, new { MMCODE = mmcode, SEQ = seq }, DBWork.Transaction);
        }

        // SetQty
        public string GetAccSeq()
        {
            var sql = @"select BC_CS_ACC_LOG_SEQ.nextval from dual";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, null, DBWork.Transaction);
        }

        public int UpdateAccDeliQty(AA0153 aa0153)
        {
            var sql = @" update MM_PO_ACC
                set DELI_QTY=DELI_QTY + :INQTY, UPDATE_TIME=sysdate, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP
                WHERE PO_NO = :PO_NO and SEQ=:POACC_SEQ ";
            return DBWork.Connection.Execute(sql, aa0153, DBWork.Transaction);
        }

        public int InsertAccLog(AA0153 aa0153, bool addSrcSeq = false)
        {
            string addSqlCol = "";
            string addSqlVal = "";
            if (addSrcSeq)
            {
                addSqlCol = ", SRC_SEQ";
                addSqlVal = ", :SRC_SEQ as SRC_SEQ";
            }

            var sql = string.Format(@" insert into BC_CS_ACC_LOG (PO_NO, MMCODE, SEQ, AGEN_NO, LOT_NO, EXP_DATE, BW_SQTY, ACC_QTY, CFM_QTY, 
                ACC_BASEUNIT, STATUS, MEMO, ACC_TIME, ACC_USER, STOREID, MAT_CLASS, PO_QTY, ACC_PURUN, UNIT_SWAP, WEXP_ID, WH_NO,
                TX_QTY_T, INVOICE, INVOICE_DT, EXTRA_DISC_AMOUNT,
                ACC_PO_PRICE, ACC_M_DISCPERC, ACC_UPRICE, ACC_DISC_CPRICE, ACC_DISC_UPRICE, ACC_ISWILLING, ACC_DISCOUNT_QTY, ACC_DISC_COST_UPRICE, CHINNAME, CHARTNO, PO_D_SEQ" + addSqlCol + @")
                select b.PO_NO, b.MMCODE, :SEQ, a.AGEN_NO, trim(:LOT_NO) as LOT_NO, twn_todate(:EXP_DATE) as EXP_DATE, 0 as BW_SQTY, :INQTY as ACC_QTY, :INQTY as CFM_QTY, 
                b.BASE_UNIT as ACC_BASEUNIT, 'C' as STATUS, :MEMO as MEMO, sysdate as ACC_TIME, :UPDATE_USER as ACC_USER, b.STOREID, a.MAT_CLASS, b.PO_QTY, b.M_PURUN, nvl(b.UNIT_SWAP, 1) as UNIT_SWAP, (select WEXP_ID from MI_MAST where MMCODE = b.MMCODE) as WEXP_ID, a.WH_NO,
                :INQTY as TX_QTY_T, :INVOICE as INVOICE, twn_todate(:INVOICE_DT) as INVOICE_DT, nvl(:EXTRA_DISC_AMOUNT, 0) as EXTRA_DISC_AMOUNT,
                b.PO_PRICE as ACC_PO_PRICE, b.M_DISCPERC as ACC_M_DISCPERC, b.UPRICE as ACC_UPRICE, b.DISC_CPRICE as ACC_DISC_CPRICE, b.DISC_UPRICE as ACC_DISC_UPRICE,
                b.ISWILLING as ACC_ISWILLING, b.DISCOUNT_QTY as ACC_DISCOUNT_QTY, b.DISC_COST_UPRICE as ACC_DISC_COST_UPRICE, b.CHINNAME, b.CHARTNO, b.SEQ
                " + addSqlVal + @"
                from MM_PO_M a, MM_PO_D b
                where a.PO_NO = b.PO_NO and a.PO_NO like '{0}%' and b.MMCODE = :MMCODE and b.SEQ = :PO_D_SEQ
                ", aa0153.PO_NO);
            return DBWork.Connection.Execute(sql, aa0153, DBWork.Transaction);
        }

        public int UpdateInvoice(AA0153 aa0153)
        {

            var sql = string.Format(@" update PH_INVOICE 
                set CKIN_QTY = CKIN_QTY + :INQTY, DELI_QTY= DELI_QTY + :INQTY, IN_AMOUNT = nvl(:DISC_CPRICE, 0) * :INQTY - nvl(:EXTRA_DISC_AMOUNT, 0), UPDATE_TIME=sysdate, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP, 
                DELI_STATUS='C', INVOICE=:INVOICE, INVOICE_DT = twn_todate(:INVOICE_DT), DELI_DT=sysdate, EXTRA_DISC_AMOUNT=nvl(:EXTRA_DISC_AMOUNT, 0),
                PO_PRICE = :PO_PRICE, DISC_CPRICE = :DISC_CPRICE, DISC_UPRICE = :DISC_UPRICE,
                ACC_LOG_SEQ = :SEQ
                WHERE PO_NO like '{0}%' and MMCODE=:MMCODE and INVOICE is null and CKSTATUS<>'Y' and DELI_STATUS <> 'C' "
                , aa0153.PO_NO);
            return DBWork.Connection.Execute(sql, aa0153, DBWork.Transaction);
        }

        public int UpdateInvoiceByTransBack(AA0153 aa0153)
        {
            var sql = string.Format(@" update PH_INVOICE 
                set CKIN_QTY = 0, DELI_QTY= 0, IN_AMOUNT = 0, UPDATE_TIME=sysdate, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP, 
                DELI_STATUS='N', INVOICE=null, INVOICE_DT = null, DELI_DT=sysdate, EXTRA_DISC_AMOUNT=0
                WHERE PO_NO like '{0}%' and MMCODE=:MMCODE and ACC_LOG_SEQ = :SEQ "
                , aa0153.PO_NO);
            return DBWork.Connection.Execute(sql, aa0153, DBWork.Transaction);
        }

        public AA0153 ChkPrice(string po_no, string mmcode)
        {
            string sql = string.Format(@" select ACC_PO_PRICE, ACC_DISC_CPRICE, ACC_DISC_UPRICE
                from BC_CS_ACC_LOG 
                where PO_NO like '{0}%' and MMCODE=:MMCODE "
                , po_no);
            return DBWork.Connection.QueryFirst<AA0153>(sql, new { PO_NO = po_no, MMCODE = mmcode }, DBWork.Transaction);
        }

        public AA0153 ChkInvoice(string po_no, string mmcode)
        {
            string sql = string.Format(@" select PO_NO, MMCODE, PO_QTY, sum(DELI_QTY) as DELI_QTY_SUM 
                from PH_INVOICE 
                where PO_NO like '{0}%' and MMCODE=:MMCODE and DELI_STATUS = 'C'
                group by PO_NO, MMCODE, PO_QTY "
                , po_no);
            return DBWork.Connection.QueryFirst<AA0153>(sql, new { PO_NO = po_no, MMCODE = mmcode }, DBWork.Transaction);
        }

        public int InsertInvoice(AA0153 aa0153)
        {
            var sql = string.Format(@" 
                insert into PH_INVOICE (PO_NO, MMCODE, TRANSNO, PO_QTY, PO_PRICE, M_PURUN,  M_AGENLAB, PO_AMT, 
                        M_DISCPERC, PR_NO ,UNIT_SWAP, UPRICE, DISC_CPRICE, DISC_UPRICE, CREATE_TIME, CREATE_USER, UPDATE_IP, 
                    M_PHCTNCO, DELI_QTY, CKIN_QTY, BW_SQTY, ACC_LOG_SEQ)
                select PO_NO, MMCODE, TO_CHAR(sysdate,'yyyymmddhh24miss') as TRANSNO, PO_QTY, PO_PRICE, M_PURUN, M_AGENLAB, PO_AMT, 
                         M_DISCPERC, PR_NO ,nvl(UNIT_SWAP, 1) as UNIT_SWAP, UPRICE, DISC_CPRICE, DISC_UPRICE, sysdate as CREATE_TIME, :UPDATE_USER as CREATE_USER, :UPDATE_IP as UPDATE_IP, 
                        M_PHCTNCO, 0 as DELI_QTY, 0 as CKIN_QTY, 0 as BW_QTY, :SEQ as ACC_LOG_SEQ
                from PH_INVOICE
                where PO_NO like '{0}%' and MMCODE=:MMCODE and ROWNUM = 1 "
                , aa0153.PO_NO);
            return DBWork.Connection.Execute(sql, aa0153, DBWork.Transaction);
        }

        public int DelInvoice(string po_no, string mmcode, string seq)
        {
            var sql = string.Format(@" 
                delete from PH_INVOICE
                where PO_NO like '{0}%' and MMCODE=:MMCODE and DELI_STATUS = 'N' "
                , po_no);
            return DBWork.Connection.Execute(sql, new { MMCODE = mmcode, SEQ = seq }, DBWork.Transaction);
        }

        public int UpdateDeliQty(AA0153 aa0153)
        {
            var sql = string.Format(@" update MM_PO_D 
                set DELI_QTY=DELI_QTY + :INQTY, DELI_STATUS='C', UPDATE_TIME=sysdate, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP
                WHERE PO_NO like '{0}%' and MMCODE=:MMCODE and DELI_STATUS<>'Y' 
                and SEQ = :PO_D_SEQ "
            , aa0153.PO_NO);
            return DBWork.Connection.Execute(sql, aa0153, DBWork.Transaction);
        }

        public int UpdateDeliStatus(AA0153 aa0153)
        {
            var sql = string.Format(@" update MM_PO_D 
                set DELI_STATUS='Y', UPDATE_TIME=sysdate, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP
                WHERE PO_NO like '{0}%' and MMCODE=:MMCODE and DELI_STATUS='C' 
                and PO_QTY <= DELI_QTY and SEQ = :PO_D_SEQ "
            , aa0153.PO_NO);
            return DBWork.Connection.Execute(sql, aa0153, DBWork.Transaction);
        }

        public int UpdateDeliStatusByTransBack(AA0153 aa0153)
        {
            var sql = string.Format(@" update MM_PO_D 
                set DELI_STATUS='C', UPDATE_TIME=sysdate, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP
                WHERE PO_NO like '{0}%' and MMCODE=:MMCODE and DELI_STATUS='Y' 
                and SEQ = :PO_D_SEQ "
            , aa0153.PO_NO);
            return DBWork.Connection.Execute(sql, aa0153, DBWork.Transaction);
        }

        public int InsertPhLotno(AA0153 aa0153)
        {
            var sql = string.Format(@" insert into PH_LOTNO( SEQ, PO_NO, MMCODE, LOT_NO, EXP_DATE, QTY, 
                SOURCE, STATUS, ACC_LOG_SEQ, CREATE_TIME, CREATE_USER, UPDATE_IP)
                select PH_LOTNO_SEQ.NEXTVAL as SEQ, PO_NO, MMCODE, :LOT_NO as LOT_NO, twn_todate(:EXP_DATE) as EXP_DATE, :INQTY as QTY, 
                'U' as SOURCE, 'N' as STATUS, :SEQ as ACC_LOG_SEQ, sysdate as CREATE_TIME, :UPDATE_USER as CREATE_USER, :UPDATE_IP as UPDATE_IP
                from MM_PO_D
                where PO_NO like '{0}%' and MMCODE=:MMCODE and rownum = 1 "
            , aa0153.PO_NO);
            return DBWork.Connection.Execute(sql, aa0153, DBWork.Transaction);
        }

        public int DelPhLotno(AA0153 aa0153)
        {
            var sql = string.Format(@" delete from PH_LOTNO
                where PO_NO like '{0}%' and MMCODE=:MMCODE and ACC_LOG_SEQ = :SEQ "
            , aa0153.PO_NO);
            return DBWork.Connection.Execute(sql, aa0153, DBWork.Transaction);
        }

        public int UpdateAccLog(AA0153 aa0153)
        {
            var sql = string.Format(@" update BC_CS_ACC_LOG 
                set INVOICE = :INVOICE, INVOICE_DT = twn_todate(:INVOICE_DT), MEMO = :MEMO
                WHERE PO_NO like '{0}%' and MMCODE=:MMCODE and SEQ = :SEQ "
            , aa0153.PO_NO);

            return DBWork.Connection.Execute(sql, aa0153, DBWork.Transaction);
        }

        public int UpdateInvoiceByAccLog(AA0153 aa0153)
        {

            var sql = string.Format(@" update PH_INVOICE 
                set INVOICE=:INVOICE, INVOICE_DT = twn_todate(:INVOICE_DT),
                UPDATE_TIME=sysdate, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP
                WHERE PO_NO like '{0}%' and MMCODE=:MMCODE and ACC_LOG_SEQ = :SEQ and DELI_STATUS = 'C' "
                , aa0153.PO_NO);
            return DBWork.Connection.Execute(sql, aa0153, DBWork.Transaction);
        }

        public string GetInrecSeq(AA0153 aa0153)
        {
            var sql = string.Format(@"select SEQ from MM_PO_INREC where PO_NO like '{0}%' and MMCODE=:MMCODE and status = 'N'", aa0153.PO_NO);
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, aa0153, DBWork.Transaction);
        }

        public string GetInrecSeqForTransBack(AA0153 aa0153)
        {
            var sql = string.Format(@"select SEQ from MM_PO_INREC where PO_NO like '{0}%' and MMCODE=:MMCODE 
                and LOT_NO = trim(:LOT_NO) and EXP_DATE = twn_todate(:EXP_DATE) 
                and DELI_QTY = :INQTY and status = 'Y' ", aa0153.PO_NO);
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, aa0153, DBWork.Transaction);
        }

        public int UpdateInrec(AA0153 aa0153)
        {
            var sql = string.Format(@" update MM_PO_INREC 
                set DELI_QTY = :INQTY, PO_AMT = PO_PRICE * :INQTY, STATUS = 'Y', LOT_NO = trim(:LOT_NO), 
                EXP_DATE = twn_todate(:EXP_DATE), ACCOUNTDATE = sysdate, UPDATE_TIME = sysdate, UPDATE_USER = :UPDATE_USER, 
                UPDATE_IP = :UPDATE_IP, EXTRA_DISC_AMOUNT=nvl(:EXTRA_DISC_AMOUNT, 0)
                where PO_NO like '{0}%' and MMCODE=:MMCODE and SEQ = :SEQ "
            , aa0153.PO_NO);
            return DBWork.Connection.Execute(sql, aa0153, DBWork.Transaction);
        }

        public int UpdateInrecStatus(AA0153 aa0153)
        {
            var sql = string.Format(@" UPDATE MM_PO_INREC
                        SET STATUS = 'E',
                            UPDATE_IP = :UPDATE_IP,
                            UPDATE_USER = :UPDATE_USER,
                            UPDATE_TIME = SYSDATE
                        WHERE PO_NO like '{0}%'
                        AND MMCODE = :MMCODE
                        AND SEQ = :FROMSEQ "
            , aa0153.PO_NO);
            return DBWork.Connection.Execute(sql, aa0153, DBWork.Transaction);
        }

        public int InsertInrec(AA0153 aa0153)
        {
            var sql = string.Format(@" insert into MM_PO_INREC(PURDATE, PO_NO, MMCODE, E_PURTYPE, CONTRACNO, AGEN_NO, PO_QTY, PO_PRICE, 
                M_PURUN, PO_AMT, WH_NO, M_DISCPERC, UNIT_SWAP, UPRICE, DISC_CPRICE, DISC_UPRICE, 
                CREATE_TIME, CREATE_USER, UPDATE_IP, TRANSKIND, STATUS, IFLAG, SEQ, ISWILLING, DISCOUNT_QTY, DISC_COST_UPRICE)
                select substr(B.PO_NO,1,7), B.PO_NO, B.MMCODE, B.E_PURTYPE, A.CONTRACNO, A.AGEN_NO, B.PO_QTY, B.PO_PRICE, 
                B.M_PURUN, 0, A.WH_NO, B.M_DISCPERC, nvl(B.UNIT_SWAP, 1) as UNIT_SWAP, B.UPRICE, B.DISC_CPRICE, B.DISC_UPRICE, 
                sysdate, :UPDATE_USER, :UPDATE_IP, '111', 'N', 'Y', MM_PO_INREC_SEQ.NEXTVAL, c.ISWILLING, c.DISCOUNT_QTY, c.DISC_COST_UPRICE 
                from MM_PO_M A, MM_PO_D B, MM_PO_INREC c
                where A.PO_NO=B.PO_NO and B.PO_NO like '{0}%' and B.MMCODE=:MMCODE
                and c.PO_NO = b.PO_NO and c.MMCODE = b.MMCODE and c.SEQ = :SEQ
                and b.PO_QTY > b.DELI_QTY and b.SEQ = :PO_D_SEQ "
            , aa0153.PO_NO);
            return DBWork.Connection.Execute(sql, aa0153, DBWork.Transaction);
        }

        public int InsertInrecByTransBack(AA0153 aa0153)
        {
            var sql = string.Format(@" insert into MM_PO_INREC(PURDATE, PO_NO, MMCODE, E_PURTYPE, CONTRACNO, AGEN_NO, PO_QTY, PO_PRICE, 
                M_PURUN, PO_AMT, WH_NO, M_DISCPERC, UNIT_SWAP, UPRICE, DISC_CPRICE, DISC_UPRICE, 
                CREATE_TIME, CREATE_USER, UPDATE_IP, TRANSKIND, STATUS, IFLAG, SEQ, ISWILLING, DISCOUNT_QTY, DISC_COST_UPRICE, FROMSEQ)
                select substr(B.PO_NO,1,7), B.PO_NO, B.MMCODE, B.E_PURTYPE, A.CONTRACNO, A.AGEN_NO, B.PO_QTY, B.PO_PRICE, 
                B.M_PURUN, 0, A.WH_NO, B.M_DISCPERC, nvl(B.UNIT_SWAP, 1) as UNIT_SWAP, B.UPRICE, B.DISC_CPRICE, B.DISC_UPRICE, 
                sysdate, :UPDATE_USER, :UPDATE_IP, '120', 'Y', 'Y', MM_PO_INREC_SEQ.NEXTVAL, c.ISWILLING, c.DISCOUNT_QTY, c.DISC_COST_UPRICE, c.SEQ 
                from MM_PO_M A, MM_PO_D B, MM_PO_INREC c
                where A.PO_NO=B.PO_NO and B.PO_NO like '{0}%' and B.MMCODE=:MMCODE
                and c.PO_NO = b.PO_NO and c.MMCODE = b.MMCODE and c.SEQ = :FROMSEQ and B.SEQ = :PO_D_SEQ "
            , aa0153.PO_NO);
            return DBWork.Connection.Execute(sql, aa0153, DBWork.Transaction);
        }

        public bool CheckStatusNExists(string po_no, string mmcode)
        {
            string sql = @"
                select 1 from MM_PO_INREC
                 where po_no = :po_no
                   and mmcode = :mmcode
                   and status = 'N'
            ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { po_no, mmcode }, DBWork.Transaction) == null);
        }

        public string INV_SET_PO_DOCIN(string I_PONO, string I_USERID, string I_UPDIP)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_PONO", value: I_PONO, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 15);
            p.Add("I_USERID", value: I_USERID, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 10);
            p.Add("I_UPDIP", value: I_UPDIP, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 10);
            p.Add("O_RETMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            DBWork.Connection.Query("INV_SET.PO_DOCIN", p, commandType: CommandType.StoredProcedure);
            string O_RETMSG = p.Get<OracleString>("O_RETMSG").Value;
            string O_RETID = p.Get<OracleString>("O_RETID").Value;
            return O_RETID + "-" + O_RETMSG;
        }

        // SetPrice
        public int InsertPriceLog(string po_no, string mmcode, string userName, string userIp)
        {

            var sql = string.Format(@" insert into MM_PO_D_PRICE_LOG (CREATE_TIME, CREATE_USER, PO_NO, MMCODE, PO_QTY, PO_PRICE, 
                M_PURUN, PO_AMT, M_DISCPERC, DELI_QTY, UPDATE_IP, MEMO, PR_NO, UNIT_SWAP, UPRICE, DISC_CPRICE, DISC_UPRICE, 
                ISWILLING, DISCOUNT_QTY, DISC_COST_UPRICE)
                select sysdate as CREATE_TIME, :CREATE_USER AS CREATE_USER, PO_NO, MMCODE, PO_QTY, PO_PRICE, 
                M_PURUN, PO_AMT, M_DISCPERC, DELI_QTY, :UPDATE_IP AS UPDATE_IP, MEMO, PR_NO, nvl(UNIT_SWAP, 1) as UNIT_SWAP, UPRICE, DISC_CPRICE, DISC_UPRICE, 
                ISWILLING, DISCOUNT_QTY, DISC_COST_UPRICE
                from MM_PO_D
                where PO_NO like '{0}%' and MMCODE = :MMCODE and rownum = 1 "
            , po_no);
            return DBWork.Connection.Execute(sql, new { PO_NO = po_no, MMCODE = mmcode, CREATE_USER = userName, UPDATE_IP = userIp }, DBWork.Transaction);
        }

        public int UpdatePoPrice(string po_no, string mmcode, string userName, string userIp)
        {

            var sql = string.Format(@" update MM_PO_D A 
                set PO_PRICE = (select M_CONTPRICE from MI_MAST where MMCODE = A.MMCODE),
                M_DISCPERC = (select M_DISCPERC from MI_MAST where MMCODE = A.MMCODE),
                UPRICE = (select UPRICE from MI_MAST where MMCODE = A.MMCODE),
                DISC_CPRICE = (select DISC_CPRICE from MI_MAST where MMCODE = A.MMCODE),
                DISC_UPRICE = (select DISC_UPRICE from MI_MAST where MMCODE = A.MMCODE),
                ISWILLING = (select C.ISWILLING from MI_MAST B, MILMED_JBID_LIST C where A.MMCODE=B.MMCODE and SUBSTR(B.E_YRARMYNO,1,3)=C.JBID_STYR and B.E_ITEMARMYNO=C.BID_NO ),
                DISCOUNT_QTY = (select C.DISCOUNT_QTY from MI_MAST B, MILMED_JBID_LIST C where A.MMCODE=B.MMCODE and SUBSTR(B.E_YRARMYNO,1,3)=C.JBID_STYR and B.E_ITEMARMYNO=C.BID_NO ),
                DISC_COST_UPRICE = (select C.DISC_COST_UPRICE from MI_MAST B, MILMED_JBID_LIST C where A.MMCODE=B.MMCODE and SUBSTR(B.E_YRARMYNO,1,3)=C.JBID_STYR and B.E_ITEMARMYNO=C.BID_NO),
                UPDATE_TIME = sysdate, UPDATE_USER = :UPDATE_USER, UPDATE_IP=:UPDATE_IP
                where A.PO_NO like '{0}%' and A.MMCODE = :MMCODE "
            , po_no);
            return DBWork.Connection.Execute(sql, new { PO_NO = po_no, MMCODE = mmcode, UPDATE_USER = userName, UPDATE_IP = userIp }, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetMatClassCombo(string userid)
        {
            var sql = @"with temp_whkinds as (
                            select b.wh_no, b.wh_kind, 
                            nvl((case when a.task_id = '3' then '2' else a.task_id end), '2') as task_id
                            from MI_WHID a, MI_WHMAST b
                            where wh_userid = :userId
                            and a.wh_no = b.wh_no
                            and b.wh_grade = '1'
                            and a.task_id in ('1','2','3')
                        )
                        select distinct b.mat_class as VALUE, '全部'||b.mat_clsname as TEXT, b.mat_class || ' ' ||  '全部' ||b.mat_clsname as COMBITEM 
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

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { userId = userid });
        }

        public IEnumerable<PH_VENDER> GetAgennoCombo(string p0, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"SELECT {0} AGEN_NO, AGEN_NAMEC, AGEN_NAMEE
                        FROM PH_VENDER WHERE (REC_STATUS<>'X' OR REC_STATUS is null)";
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

        public string getPO_WHNO(string po_no)
        {
            string sql = string.Format(@" select WH_NO
                from MM_PO_M 
                where PO_NO =:PO_NO "
                , po_no);
            return DBWork.Connection.QueryFirst<string>(sql, new { PO_NO = po_no }, DBWork.Transaction);
        }

        public bool CheckStlocExists(string wh_no, string store_loc)
        {
            string sql = @"SELECT 1 FROM BC_STLOC WHERE WH_NO=:WH_NO AND STORE_LOC=:STORE_LOC";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = wh_no, STORE_LOC = store_loc }, DBWork.Transaction) == null);
        }

        public bool CheckWlocExists(string wh_no, string mmcode, string store_loc)
        {
            string sql = @"SELECT 1 FROM MI_WLOCINV WHERE WH_NO=:WH_NO AND MMCODE = :MMCODE AND STORE_LOC=:STORE_LOC";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = wh_no, MMCODE = mmcode, STORE_LOC = store_loc }, DBWork.Transaction) == null);
        }

        public int InsertStloc(string wh_no, string store_loc, string tuser, string userIp)
        {
            string sql = string.Format(@" insert into BC_STLOC (WH_NO, STORE_LOC, BARCODE, XCATEGORY, CREATE_USER, CREATE_TIME, UPDATE_IP, FLAG)
                values(:WH_NO, :STORE_LOC, :STORE_LOC, (select XCATEGORY from BC_CATEGORY where rownum = 1), :TUSER, sysdate, :USERIP, 'N')
                ");
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, STORE_LOC = store_loc, TUSER = tuser, USERIP = userIp }, DBWork.Transaction);
        }

        public int InsertWloc(string wh_no, string mmcode, string store_loc, string invqty, string tuser, string userIp)
        {
            string sql = string.Format(@" insert into MI_WLOCINV (WH_NO, MMCODE, STORE_LOC, INV_QTY, CREATE_USER, CREATE_TIME, UPDATE_IP)
                values(:WH_NO, :MMCODE, :STORE_LOC, :INVQTY, :TUSER, sysdate, :USERIP)
                ");
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = mmcode, STORE_LOC = store_loc, INVQTY = invqty, TUSER = tuser, USERIP = userIp }, DBWork.Transaction);
        }

        public int UpdateWloc(string wh_no, string mmcode, string store_loc, string invqty, string tuser, string userIp)
        {
            string sql = @" update MI_WLOCINV
                set INV_QTY = INV_QTY + :INVQTY, UPDATE_USER = :TUSER, UPDATE_TIME = sysdate, UPDATE_IP = :USERIP
                where WH_NO = :WH_NO and MMCODE = :MMCODE and STORE_LOC = :STORE_LOC
                ";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = mmcode, STORE_LOC = store_loc, INVQTY = invqty, TUSER = tuser, USERIP = userIp }, DBWork.Transaction);
        }

        public string GetStoreLoc(string wh_no, string mmcode)
        {
            string sql = "SELECT STORE_LOC FROM MI_WLOCINV WHERE WH_NO=:WH_NO AND MMCODE=:MMCODE AND rownum = 1 ";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction);
        }

        public bool CheckWexpExists(string wh_no, string mmcode, string lot_no, string exp_date)
        {
            string sql = @"SELECT 1 FROM MI_WEXPINV WHERE WH_NO=:WH_NO AND MMCODE = :MMCODE and LOT_NO = LOT_NO and TWN_DATE(EXP_DATE) = :EXP_DATE ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = wh_no, MMCODE = mmcode, LOT_NO = lot_no, EXP_DATE = exp_date }, DBWork.Transaction) == null);
        }

        public int InsertWexp(string wh_no, string mmcode, string lot_no, string exp_date, string invqty, string tuser, string userIp)
        {
            string sql = string.Format(@" insert into MI_WEXPINV (WH_NO, MMCODE, LOT_NO, EXP_DATE, INV_QTY, CREATE_USER, CREATE_TIME, UPDATE_IP)
                values(:WH_NO, :MMCODE, :LOT_NO, twn_todate(:EXP_DATE), :INVQTY, :TUSER, sysdate, :USERIP)
                ");
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = mmcode, LOT_NO = lot_no, EXP_DATE = exp_date, INVQTY = invqty, TUSER = tuser, USERIP = userIp }, DBWork.Transaction);
        }

        public int UpdateWexp(string wh_no, string mmcode, string lot_no, string exp_date, string invqty, string tuser, string userIp)
        {
            string sql = @" update MI_WEXPINV
                set INV_QTY = INV_QTY + :INVQTY, UPDATE_USER = :TUSER, UPDATE_TIME = sysdate, UPDATE_IP = :USERIP
                where WH_NO = :WH_NO and MMCODE = :MMCODE and LOT_NO = :LOT_NO and EXP_DATE = twn_todate(:EXP_DATE)
                ";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = mmcode, LOT_NO = lot_no, EXP_DATE = exp_date, INVQTY = invqty, TUSER = tuser, USERIP = userIp }, DBWork.Transaction);
        }

        public int getPO_INVITEM(string po_no)
        {
            string sql = string.Format(@" select count(*)
                from MM_PO_D
                where PO_NO =:PO_NO and DELI_STATUS <> 'Y' "
                , po_no);
            return DBWork.Connection.QueryFirst<int>(sql, new { PO_NO = po_no }, DBWork.Transaction);
        }
    }
}