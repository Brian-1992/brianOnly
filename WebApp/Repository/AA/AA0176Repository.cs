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
    public class AA0176Repository : JCLib.Mvc.BaseRepository
    {
        public AA0176Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0176> GetAllM(AA0176 query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select A.PO_NO, A.MAT_CLASS, A.AGEN_NO, B.AGEN_NAMEC, B.EASYNAME,
                (select MAT_CLSNAME from MI_MATCLASS where MAT_CLASS = A.MAT_CLASS) as MAT_CLSNAME,
                TWN_DATE(A.PO_TIME) as PO_TIME,
                (case when (select count(*) from MM_PO_ACC where PO_NO = A.PO_NO and CREATE_TIME <> UPDATE_TIME) > 0 then 'Y' else 'N' end) as isEdit
                from MM_PO_M A, PH_VENDER B
                where A.AGEN_NO = B.AGEN_NO
                and a.po_status <> '87'  --作廢不顯示
                and (select count(*) from MM_PO_D where PO_NO = A.PO_NO and DELI_STATUS <> 'Y' and nvl(STATUS,'N') <> 'D') > 0 
                and ((select count(*) from MM_PO_ACC where PO_NO = A.PO_NO) = 0
                    or (select count(*) from MM_PO_ACC where PO_NO = A.PO_NO and PO_D_SEQ in (select SEQ from MM_PO_D where PO_NO = A.PO_NO and DELI_STATUS <> 'Y' and nvl(STATUS,'N') <> 'D')) > 0) ";
                // 已進貨完的單子不再顯示, 考慮可能有轉入資料後才刪除同院內碼的部分項目, 需再判斷MM_PO_ACC尚無資料或有項目但對應的MM_PO_D尚未進完

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

            if (query.EASYNAME != "")
            {
                sql += " and B.EASYNAME like :EASYNAME ";
                p.Add(":EASYNAME", string.Format("%{0}%", query.EASYNAME));
            }

            if (query.isEdit == "Y")
            {
                sql += " and (case when (select count(*) from MM_PO_ACC where PO_NO = A.PO_NO and CREATE_TIME <> UPDATE_TIME) > 0 then 'Y' else 'N' end) = 'Y' ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0176>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA0176> GetAllD(AA0176 query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select A.PO_NO, A.MAT_CLASS, 
                (select MAT_CLSNAME from MI_MATCLASS where MAT_CLASS = A.MAT_CLASS) as MAT_CLSNAME,
                TWN_DATE(A.PO_TIME) as PO_TIME, B.SEQ as POACC_SEQ, B.MMCODE, 
                (select MMNAME_C from MI_MAST where MMCODE = B.MMCODE) as MMNAME_C, 
                (select MMNAME_E from MI_MAST where MMCODE = B.MMCODE) as MMNAME_E, STORE_LOC,
                B.PO_QTY, B.DELI_QTY, 
                (select sum(DELI_QTY) from MM_PO_ACC where PO_NO = B.PO_NO and MMCODE = B.MMCODE and PO_D_SEQ = B.PO_D_SEQ) as DELI_QTY_SUM,
                C.M_PURUN, C.UNIT_SWAP, B.ACC_QTY as INQTY,
                B.ACC_AMT, B.LOT_NO, TWN_DATE(B.EXP_DATE) as EXP_DATE, B.INVOICE, TWN_DATE(B.INVOICE_DT) as INVOICE_DT, 
                B.EXTRA_DISC_AMOUNT,
                B.PO_PRICE, 
                --(SELECT M_CONTPRICE FROM MI_MAST WHERE MMCODE = B.MMCODE) as M_CONTPRICE, 
                nvl((case when a.mat_class='01'
                        then ( case when b.po_qty >= (select discount_qty from MI_MAST where mmcode=b.mmcode) then (select disc_cost_uprice from MI_MAST where mmcode=b.mmcode) 
                    else (select disc_cprice from MI_MAST where mmcode=b.mmcode) end)
                    when a.mat_class='02' then
                    (
                        case when nvl((select (select JBID_RCRATE from RCRATE where CASENO = e.CASENO and DATA_YM = (select SET_YM from MI_MNSET where SET_STATUS = 'N') and rownum = 1) from MI_MAST e where MMCODE = b.MMCODE), 0) > 0 then
                            round(c.PO_PRICE * ((100 - (select (select JBID_RCRATE from RCRATE where CASENO = e.CASENO and DATA_YM = (select SET_YM from MI_MNSET where SET_STATUS = 'N') and rownum = 1) from MI_MAST e where MMCODE = b.MMCODE)) / 100), 3)
                        else c.DISC_CPRICE end
                    )
                    end
                ), B.PO_PRICE) as M_CONTPRICE,
                C.M_DISCPERC, (SELECT M_DISCPERC FROM MI_MAST WHERE MMCODE = B.MMCODE) as M_DISCPERC_M, 
                C.UPRICE, (SELECT UPRICE FROM MI_MAST WHERE MMCODE = B.MMCODE) UPRICE_M, 
                B.DISC_CPRICE, (SELECT DISC_CPRICE FROM MI_MAST WHERE MMCODE = B.MMCODE) as DISC_CPRICE_M, 
                B.DISC_UPRICE, (SELECT DISC_UPRICE FROM MI_MAST WHERE MMCODE = B.MMCODE) as DISC_UPRICE_M,
                C.DELI_STATUS, B.MEMO, B.CHINNAME, B.CHARTNO, B.PO_D_SEQ
                from MM_PO_M A, MM_PO_ACC B left join MM_PO_D C on B.PO_NO = C.PO_NO and B.MMCODE = C.MMCODE
                and B.PO_D_SEQ = C.SEQ
                where A.PO_NO = B.PO_NO
                and nvl(C.STATUS,'N') <> 'D' "; // and B.PO_QTY <> B.DELI_QTY ";

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

            return DBWork.Connection.Query<AA0176>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA0176> GetAllAccLogM(AA0176 query, int page_index, int page_size, string sorters)
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

            return DBWork.Connection.Query<AA0176>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA0176> GetAllAccLogD(AA0176 query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select PO_NO, MMCODE, 
                (select MMNAME_C from MI_MAST where MMCODE = A.MMCODE) as MMNAME_C, 
                (select MMNAME_E from MI_MAST where MMCODE = A.MMCODE) as MMNAME_E, 
                SEQ, AGEN_NO, LOT_NO, TWN_DATE(EXP_DATE) as EXP_DATE, BW_SQTY, ACC_QTY, ACC_PO_PRICE, ACC_QTY * ACC_PO_PRICE as PO_PRICE_AMT, ACC_DISC_CPRICE, ACC_AMT, CFM_QTY, 
                ACC_BASEUNIT, STATUS, MEMO, TWN_DATE(ACC_TIME) as ACC_TIME, ACC_USER, STOREID, MAT_CLASS, PO_QTY, ACC_PURUN, 
                UNIT_SWAP, WEXP_ID, TX_QTY_T, INVOICE, to_char(INVOICE_DT, 'YYYY/MM/DD') as INVOICE_DT, EXTRA_DISC_AMOUNT, SRC_SEQ, CHINNAME, CHARTNO, POACC_SEQ,
                (select 1 from BC_CS_ACC_LOG where SRC_SEQ = A.SEQ) as REF_CNT
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

            return DBWork.Connection.Query<AA0176>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public AA0176 GetAccLog(string po_no, string mmcode, string seq)
        {
            var sql = string.Format(@"select PO_NO, MMCODE, SEQ, AGEN_NO, LOT_NO, TWN_DATE(EXP_DATE) as EXP_DATE, BW_SQTY, ACC_QTY, CFM_QTY, 
                ACC_BASEUNIT, STATUS, MEMO, TWN_DATE(ACC_TIME) as ACC_TIME, ACC_USER, STOREID, MAT_CLASS, PO_QTY, ACC_PURUN, 
                UNIT_SWAP, WEXP_ID, TX_QTY_T, INVOICE, TWN_DATE(INVOICE_DT) as INVOICE_DT, EXTRA_DISC_AMOUNT, SRC_SEQ, POACC_SEQ, PO_D_SEQ    
                from BC_CS_ACC_LOG 
                where PO_NO like '{0}%' 
                and MMCODE = :MMCODE and SEQ = :SEQ ", po_no);
            return DBWork.Connection.QueryFirstOrDefault<AA0176>(sql, new { MMCODE = mmcode, SEQ = seq }, DBWork.Transaction);
        }

        public AA0176 GetPoAcc(string po_no, string seq)
        {
            var sql = @"select PO_NO, MMCODE, SEQ as POACC_SEQ, PO_QTY, DELI_QTY, ACC_QTY as INQTY, ACC_AMT, LOT_NO, TWN_DATE(EXP_DATE) as EXP_DATE, INVOICE, TWN_DATE(INVOICE_DT) as INVOICE_DT,
                EXTRA_DISC_AMOUNT, MEMO, ACC_STATUS, PO_PRICE, DISC_CPRICE, DISC_UPRICE, STORE_LOC, 
                (select MAT_CLASS from MM_PO_M where PO_NO = A.PO_NO) as MAT_CLASS, PO_D_SEQ
                from MM_PO_ACC A
                where PO_NO = :PO_NO and SEQ = :SEQ ";
            return DBWork.Connection.QueryFirstOrDefault<AA0176>(sql, new { PO_NO = po_no, SEQ = seq }, DBWork.Transaction);
        }

        public int DetailUpdate(AA0176 aa0176)
        {
            var sql = @"update MM_PO_ACC
                         SET ACC_QTY = :INQTY, ACC_AMT = :ACC_AMT, LOT_NO = :LOT_NO, EXP_DATE = twn_todate(:EXP_DATE), INVOICE = upper(:INVOICE), INVOICE_DT = twn_todate(:INVOICE_DT), 
                        PO_PRICE = :PO_PRICE, DISC_CPRICE = :DISC_CPRICE, EXTRA_DISC_AMOUNT = :EXTRA_DISC_AMOUNT, MEMO=:MEMO, STORE_LOC = :STORE_LOC, UPDATE_TIME = sysdate, UPDATE_USER=:UPDATE_USER,UPDATE_IP=:UPDATE_IP 
                        where PO_NO=:PO_NO and SEQ = :POACC_SEQ ";
            return DBWork.Connection.Execute(sql, aa0176, DBWork.Transaction);
        }

        public int DetailDelete(string po_no, string poacc_seq)
        {
            var sql = @"delete from MM_PO_ACC
                        where PO_NO=:PO_NO and SEQ = :POACC_SEQ ";
            return DBWork.Connection.Execute(sql, new { PO_NO = po_no, POACC_SEQ = poacc_seq }, DBWork.Transaction);
        }

        public int UpdateCompleteMemo(string po_no, string poacc_seq, string update_user, string update_ip)
        {
            var sql = @" update MM_PO_ACC 
                set MEMO = MEMO || '(不再進貨)', UPDATE_TIME=sysdate, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP
                where PO_NO=:PO_NO and SEQ = :POACC_SEQ ";

            return DBWork.Connection.Execute(sql, new { PO_NO = po_no, POACC_SEQ = poacc_seq, UPDATE_USER = update_user, UPDATE_IP = update_ip }, DBWork.Transaction);
        }

        public int DetailComplete(AA0176 aa0176)
        {
            var sql = string.Format(@" update MM_PO_D 
                set DELI_STATUS='Y', UPDATE_TIME=sysdate, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP
                WHERE PO_NO like '{0}%' and MMCODE=:MMCODE and SEQ = :PO_D_SEQ "
            , aa0176.PO_NO);

            return DBWork.Connection.Execute(sql, aa0176, DBWork.Transaction);
        }

        public bool CheckPoAccExists(string po_no)
        {
            string sql = @"SELECT 1 FROM MM_PO_ACC WHERE PO_NO=:PO_NO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { PO_NO = po_no }, DBWork.Transaction) == null);
        }

        public int ImportPoAcc(string po_no, string update_user, string update_ip)
        {
            // DISC_CPRICE(優惠金額)需依二次折讓金額(藥品-於轉訂單時判斷)或管理費(衛材)計算
            string sql_DISC_CPRICE = @"(
                    case when a.MAT_CLASS = '01' then
                        b.PO_PRICE
                    when a.MAT_CLASS = '02' then
                    (
                        case when nvl((select (select JBID_RCRATE from RCRATE where CASENO = c.CASENO and DATA_YM = (select SET_YM from MI_MNSET where SET_STATUS = 'N') and rownum = 1) from MI_MAST c where MMCODE = b.MMCODE), 0) > 0 then
                           round(b.PO_PRICE * ((100 - (select (select JBID_RCRATE from RCRATE where CASENO = c.CASENO and DATA_YM = (select SET_YM from MI_MNSET where SET_STATUS = 'N') and rownum = 1) from MI_MAST c where MMCODE = b.MMCODE)) / 100), 3)
                        else b.DISC_CPRICE end
                    )
                    else b.DISC_CPRICE end
                )";

            var sql = @" insert into MM_PO_ACC (PO_NO, MMCODE, SEQ, PO_QTY, DELI_QTY, ACC_QTY, PO_PRICE, DISC_CPRICE, DISC_UPRICE, ACC_AMT, MEMO, STORE_LOC, CHINNAME, CHARTNO, PO_D_SEQ, CREATE_USER, CREATE_TIME, UPDATE_TIME, UPDATE_IP)
                select a.PO_NO, b.MMCODE, rownum as SEQ, b.PO_QTY, b.DELI_QTY, b.PO_QTY - b.DELI_QTY as ACC_QTY, 
                        (case when nvl(b.M_CONTPRICE, 0) = 0 then " + sql_DISC_CPRICE + @" else b.M_CONTPRICE end) as PO_PRICE, 
                " + sql_DISC_CPRICE + @" as DISC_CPRICE, 
                b.DISC_UPRICE, (b.PO_QTY - b.DELI_QTY) * " + sql_DISC_CPRICE + @" as ACC_AMT,b.MEMO, 
                (select STORE_LOC from MI_WLOCINV where WH_NO = A.WH_NO and MMCODE = B.MMCODE and rownum = 1) as STORE_LOC,
                b.CHINNAME, b.CHARTNO, b.SEQ,
                :UPDATE_USER as CREATE_USER, sysdate as CREATE_TIME, sysdate as UPDATE_TIME, :UPDATE_IP as UPDATE_IP     
                from MM_PO_M a, MM_PO_D b, MI_MAST c
                where a.PO_NO = b.PO_NO and a.PO_NO = :PO_NO and nvl(b.STATUS,'N') <> 'D'
                    and c.mmcode=b.mmcode
                ";
            return DBWork.Connection.Execute(sql, new { PO_NO = po_no, UPDATE_USER = update_user, UPDATE_IP = update_ip }, DBWork.Transaction);
        }

        public int ImportPoAccByExcel(AA0176 aa0176)
        {
            var sql = @" insert into MM_PO_ACC (PO_NO, MMCODE, SEQ, PO_QTY, DELI_QTY, PO_PRICE, DISC_CPRICE, DISC_UPRICE, ACC_QTY, ACC_AMT, 
                LOT_NO, EXP_DATE, INVOICE, INVOICE_DT, 
                MEMO, STORE_LOC, CHINNAME, CHARTNO, PO_D_SEQ, CREATE_USER, CREATE_TIME, UPDATE_IP)
                select a.PO_NO, b.MMCODE, :SEQ as SEQ, b.PO_QTY, b.DELI_QTY, b.PO_PRICE, b.DISC_CPRICE, b.DISC_UPRICE, :INQTY as ACC_QTY, round(:INQTY * b.DISC_CPRICE) as ACC_AMT,
                :LOT_NO as LOT_NO, twn_todate(:EXP_DATE) as EXP_DATE, :INVOICE as INVOICE, twn_todate(:INVOICE_DT) as INVOICE_DT,
                :MEMO as MEMO, :STORE_LOC as STORE_LOC, b.CHINNAME, b.CHARTNO, b.SEQ, :UPDATE_USER as CREATE_USER, sysdate as CREATE_TIME, :UPDATE_IP as UPDATE_IP     
                from MM_PO_M a, MM_PO_D b
                where a.PO_NO = b.PO_NO and a.PO_NO = :PO_NO and b.MMCODE = :MMCODE and rownum = 1
                ";

            return DBWork.Connection.Execute(sql, aa0176, DBWork.Transaction);
        }

        public int InsertPoAcc(string po_no, string poacc_seq, string split_qty, string memo, string update_user, string update_ip)
        {
            // 新的項目進貨接收量為 (訂單數量-已進貨量)/分項數
            var sql = @" insert into MM_PO_ACC (PO_NO, MMCODE, SEQ, PO_QTY, DELI_QTY, ACC_QTY, ACC_AMT, PO_PRICE, DISC_CPRICE, DISC_UPRICE, EXTRA_DISC_AMOUNT, MEMO, STORE_LOC, CHINNAME, CHARTNO, PO_D_SEQ, CREATE_USER, CREATE_TIME, UPDATE_IP)
                select PO_NO, MMCODE, 
                    (select max(SEQ) from MM_PO_ACC where PO_NO = :PO_NO) + 1 as SEQ, 
                    PO_QTY, 0 as DELI_QTY, 
                    floor((PO_QTY - DELI_QTY) / :SPLIT_QTY) as ACC_QTY, 
                    round(floor((PO_QTY - DELI_QTY) / :SPLIT_QTY) * DISC_CPRICE - EXTRA_DISC_AMOUNT) as ACC_AMT, 
                    PO_PRICE, DISC_CPRICE, DISC_UPRICE, EXTRA_DISC_AMOUNT, :MEMO as MEMO, STORE_LOC, CHINNAME, CHARTNO, PO_D_SEQ, :UPDATE_USER as CREATE_USER, sysdate as CREATE_TIME, :UPDATE_IP as UPDATE_IP     
                from MM_PO_ACC
                where PO_NO = :PO_NO and SEQ = :POACC_SEQ
                ";
            return DBWork.Connection.Execute(sql, new { PO_NO = po_no, POACC_SEQ = poacc_seq, SPLIT_QTY = split_qty, MEMO = memo, UPDATE_USER = update_user, UPDATE_IP = update_ip }, DBWork.Transaction);
        }

        public int UpdatePoAcc(string po_no, string poacc_seq, string split_qty, string update_user, string update_ip)
        {
            // 原始項的本次進貨量更新為 (訂單數量-已進貨量)/分項數 加上餘數的部分
            var sql = @" update MM_PO_ACC 
                set ACC_QTY = floor((PO_QTY - DELI_QTY) / :SPLIT_QTY) + mod((PO_QTY - DELI_QTY), :SPLIT_QTY), 
                ACC_AMT = round((floor((PO_QTY - DELI_QTY) / :SPLIT_QTY) + mod((PO_QTY - DELI_QTY), :SPLIT_QTY)) * DISC_CPRICE - EXTRA_DISC_AMOUNT), 
                UPDATE_USER = :UPDATE_USER, UPDATE_TIME = sysdate, UPDATE_IP = :UPDATE_IP
                WHERE PO_NO = :PO_NO and SEQ=:POACC_SEQ ";
            return DBWork.Connection.Execute(sql, new { PO_NO = po_no, POACC_SEQ = poacc_seq, SPLIT_QTY = split_qty, UPDATE_USER = update_user, UPDATE_IP = update_ip }, DBWork.Transaction);
        }

        // SetQty
        public string GetAccSeq()
        {
            var sql = @"select BC_CS_ACC_LOG_SEQ.nextval from dual";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, null, DBWork.Transaction);
        }

        public int InsertAccLog(AA0176 AA0176, bool addSrcSeq = false)
        {
            string addSqlCol = "";
            string addSqlVal = "";
            if (addSrcSeq)
            {
                addSqlCol = ", SRC_SEQ";
                addSqlVal = ", :SRC_SEQ as SRC_SEQ";
            }

            var sql = string.Format(@" insert into BC_CS_ACC_LOG (PO_NO, MMCODE, SEQ, AGEN_NO, LOT_NO, EXP_DATE, BW_SQTY, ACC_QTY, ACC_AMT, CFM_QTY, 
                ACC_BASEUNIT, STATUS, MEMO, ACC_TIME, ACC_USER, STOREID, MAT_CLASS, PO_QTY, ACC_PURUN, UNIT_SWAP, WEXP_ID, WH_NO,
                TX_QTY_T, INVOICE, INVOICE_DT, EXTRA_DISC_AMOUNT,
                ACC_PO_PRICE, ACC_M_DISCPERC, ACC_UPRICE, ACC_DISC_CPRICE, ACC_DISC_UPRICE, ACC_ISWILLING, ACC_DISCOUNT_QTY, ACC_DISC_COST_UPRICE, CHINNAME, CHARTNO, PO_D_SEQ, POACC_SEQ" + addSqlCol + @")
                select b.PO_NO, b.MMCODE, :SEQ, a.AGEN_NO, trim(:LOT_NO) as LOT_NO, twn_todate(:EXP_DATE) as EXP_DATE, 0 as BW_SQTY, :INQTY as ACC_QTY, :ACC_AMT as ACC_AMT, :INQTY as CFM_QTY, 
                c.BASE_UNIT as ACC_BASEUNIT, 'C' as STATUS, :MEMO as MEMO, sysdate as ACC_TIME, :UPDATE_USER as ACC_USER, c.STOREID, a.MAT_CLASS, b.PO_QTY, c.M_PURUN, nvl(c.UNIT_SWAP, 1) as UNIT_SWAP, (select WEXP_ID from MI_MAST where MMCODE = b.MMCODE) as WEXP_ID, a.WH_NO,
                :INQTY as TX_QTY_T, :INVOICE as INVOICE, twn_todate(:INVOICE_DT) as INVOICE_DT, nvl(:EXTRA_DISC_AMOUNT, 0) as EXTRA_DISC_AMOUNT,
                c.PO_PRICE as ACC_PO_PRICE, c.M_DISCPERC as ACC_M_DISCPERC, c.UPRICE as ACC_UPRICE, b.DISC_CPRICE as ACC_DISC_CPRICE, c.DISC_UPRICE as ACC_DISC_UPRICE,
                c.ISWILLING as ACC_ISWILLING, c.DISCOUNT_QTY as ACC_DISCOUNT_QTY, c.DISC_COST_UPRICE as ACC_DISC_COST_UPRICE, b.CHINNAME, b.CHARTNO, c.SEQ, :POACC_SEQ as POACC_SEQ
                " + addSqlVal + @"
                from MM_PO_M a, MM_PO_ACC b left join MM_PO_D c on b.PO_NO = c.PO_NO and b.MMCODE = c.MMCODE and b.PO_D_SEQ = c.SEQ
                where a.PO_NO = b.PO_NO and a.PO_NO like '{0}%' and b.MMCODE = :MMCODE and b.SEQ = :POACC_SEQ
                ", AA0176.PO_NO);
            return DBWork.Connection.Execute(sql, AA0176, DBWork.Transaction);
        }

        public int UpdateInvoice(AA0176 AA0176)
        {

            var sql = string.Format(@" update PH_INVOICE A
                set CKIN_QTY = CKIN_QTY + :INQTY, DELI_QTY= DELI_QTY + :INQTY, PO_AMT = nvl(:PO_PRICE, 0) * :INQTY, IN_AMOUNT = nvl(:DISC_CPRICE, 0) * :INQTY - nvl(:EXTRA_DISC_AMOUNT, 0), UPDATE_TIME=sysdate, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP, 
                DELI_STATUS='C', INVOICE=:INVOICE, INVOICE_DT = twn_todate(:INVOICE_DT), DELI_DT=sysdate, EXTRA_DISC_AMOUNT=nvl(:EXTRA_DISC_AMOUNT, 0),
                PO_PRICE = :PO_PRICE, DISC_CPRICE = :DISC_CPRICE, DISC_UPRICE = :DISC_UPRICE, 
                MORE_DISC_AMOUNT = nvl(
                (
                    case when (select MAT_CLASS from MI_MAST where MMCODE = A.MMCODE) = '01'
                        then (nvl(:PO_PRICE, 0) * :INQTY) - (nvl(:DISC_CPRICE, 0) * :INQTY)
                    else
                    (
                        case when nvl((select (select JBID_RCRATE from RCRATE where CASENO = B.CASENO and DATA_YM = (select SET_YM from MI_MNSET where SET_STATUS = 'N') and rownum = 1) from MI_MAST B where MMCODE = A.MMCODE), 0) > 0 then
                            (:INQTY * :PO_PRICE * ((select (select JBID_RCRATE from RCRATE where CASENO = B.CASENO and DATA_YM = (select SET_YM from MI_MNSET where SET_STATUS = 'N') and rownum = 1) from MI_MAST B where MMCODE = A.MMCODE) / 100))
                        else 
                            (nvl(:PO_PRICE, 0) * :INQTY) - (nvl(:DISC_CPRICE, 0) * :INQTY)
                        end
                    ) end
                ), 0),
                ACC_LOG_SEQ = :SEQ
                WHERE PO_NO like '{0}%' and MMCODE=:MMCODE and INVOICE is null and CKSTATUS<>'Y' and DELI_STATUS <> 'C' "
                , AA0176.PO_NO);
            return DBWork.Connection.Execute(sql, AA0176, DBWork.Transaction);
        }

        public int UpdateInvoiceByTransBack(AA0176 AA0176)
        {
            var sql = string.Format(@" update PH_INVOICE 
                set CKIN_QTY = 0, DELI_QTY= 0, IN_AMOUNT = 0, UPDATE_TIME=sysdate, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP, 
                DELI_STATUS='N', INVOICE=null, INVOICE_DT = null, DELI_DT = null, EXTRA_DISC_AMOUNT=0
                WHERE PO_NO like '{0}%' and MMCODE=:MMCODE and ACC_LOG_SEQ = :SEQ "
                , AA0176.PO_NO);
            return DBWork.Connection.Execute(sql, AA0176, DBWork.Transaction);
        }

        public AA0176 ChkInvoice(string po_no, string mmcode)
        {
            string sql = string.Format(@" select PO_NO, MMCODE, PO_QTY, sum(DELI_QTY) as DELI_QTY_SUM 
                from PH_INVOICE 
                where PO_NO like '{0}%' and MMCODE=:MMCODE and DELI_STATUS = 'C'
                group by PO_NO, MMCODE, PO_QTY "
                , po_no);
            return DBWork.Connection.QueryFirst<AA0176>(sql, new { PO_NO = po_no, MMCODE = mmcode }, DBWork.Transaction);
        }

        public int InsertInvoice(AA0176 AA0176)
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
                , AA0176.PO_NO);
            return DBWork.Connection.Execute(sql, AA0176, DBWork.Transaction);
        }

        public int DelInvoice(string po_no, string mmcode, string seq)
        {
            var sql = string.Format(@" 
                delete from PH_INVOICE
                where PO_NO like '{0}%' and MMCODE=:MMCODE and DELI_STATUS = 'N' "
                , po_no);
            return DBWork.Connection.Execute(sql, new { MMCODE = mmcode, SEQ = seq }, DBWork.Transaction);
        }

        public int UpdateDeliQty(AA0176 AA0176)
        {
            var sql = string.Format(@" update MM_PO_D 
                set DELI_QTY=DELI_QTY + :INQTY, DELI_STATUS='C', UPDATE_TIME=sysdate, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP
                WHERE PO_NO like '{0}%' and MMCODE=:MMCODE and DELI_STATUS<>'Y' 
                and SEQ = :PO_D_SEQ "
            , AA0176.PO_NO);
            return DBWork.Connection.Execute(sql, AA0176, DBWork.Transaction);
        }

        public int UpdateAccDeliQty(AA0176 AA0176)
        {
            var sql = @" update MM_PO_ACC
                set DELI_QTY=DELI_QTY + :INQTY, UPDATE_TIME=sysdate, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP
                WHERE PO_NO = :PO_NO and SEQ=:POACC_SEQ ";
            return DBWork.Connection.Execute(sql, AA0176, DBWork.Transaction);
        }

        public int UpdateAccAccQty(AA0176 AA0176)
        {
            var sql = @" update MM_PO_ACC
                set ACC_QTY = 0, ACC_AMT = 0, EXTRA_DISC_AMOUNT = 0, INVOICE = null, INVOICE_DT = null, UPDATE_TIME=sysdate, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP
                WHERE PO_NO = :PO_NO and SEQ = :POACC_SEQ ";
            return DBWork.Connection.Execute(sql, AA0176, DBWork.Transaction);
        }

        public int UpdateDeliStatus(AA0176 AA0176)
        {
            var sql = string.Format(@" update MM_PO_D 
                set DELI_STATUS='Y', UPDATE_TIME=sysdate, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP
                WHERE PO_NO like '{0}%' and MMCODE=:MMCODE and DELI_STATUS='C' 
                and PO_QTY <= DELI_QTY and SEQ = :PO_D_SEQ "
            , AA0176.PO_NO);
            return DBWork.Connection.Execute(sql, AA0176, DBWork.Transaction);
        }

        public int UpdateDeliStatusByTransBack(AA0176 AA0176)
        {
            var sql = string.Format(@" update MM_PO_D 
                set DELI_STATUS='C', UPDATE_TIME=sysdate, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP
                WHERE PO_NO like '{0}%' and MMCODE=:MMCODE and DELI_STATUS='Y' 
                and SEQ = :PO_D_SEQ "
            , AA0176.PO_NO);
            return DBWork.Connection.Execute(sql, AA0176, DBWork.Transaction);
        }

        public int InsertPhLotno(AA0176 AA0176)
        {
            var sql = string.Format(@" insert into PH_LOTNO( SEQ, PO_NO, MMCODE, LOT_NO, EXP_DATE, QTY, 
                SOURCE, STATUS, ACC_LOG_SEQ, CREATE_TIME, CREATE_USER, UPDATE_IP)
                select PH_LOTNO_SEQ.NEXTVAL as SEQ, PO_NO, MMCODE, :LOT_NO as LOT_NO, twn_todate(:EXP_DATE) as EXP_DATE, :INQTY as QTY, 
                'U' as SOURCE, 'N' as STATUS, :SEQ as ACC_LOG_SEQ, sysdate as CREATE_TIME, :UPDATE_USER as CREATE_USER, :UPDATE_IP as UPDATE_IP
                from MM_PO_D
                where PO_NO like '{0}%' and MMCODE=:MMCODE "
            , AA0176.PO_NO);
            return DBWork.Connection.Execute(sql, AA0176, DBWork.Transaction);
        }

        public int DelPhLotno(AA0176 AA0176)
        {
            var sql = string.Format(@" delete from PH_LOTNO
                where PO_NO like '{0}%' and MMCODE=:MMCODE and ACC_LOG_SEQ = :SEQ "
            , AA0176.PO_NO);
            return DBWork.Connection.Execute(sql, AA0176, DBWork.Transaction);
        }

        public int UpdateAccLog(AA0176 AA0176)
        {
            var sql = string.Format(@" update BC_CS_ACC_LOG 
                set ACC_AMT = :ACC_AMT, INVOICE = :INVOICE, INVOICE_DT = twn_todate(:INVOICE_DT), MEMO = :MEMO
                WHERE PO_NO like '{0}%' and MMCODE=:MMCODE and SEQ = :SEQ "
            , AA0176.PO_NO);

            return DBWork.Connection.Execute(sql, AA0176, DBWork.Transaction);
        }

        public int UpdateInvoiceByAccLog(AA0176 AA0176)
        {

            var sql = string.Format(@" update PH_INVOICE 
                set INVOICE=:INVOICE, INVOICE_DT = twn_todate(:INVOICE_DT),
                UPDATE_TIME=sysdate, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP
                WHERE PO_NO like '{0}%' and MMCODE=:MMCODE and ACC_LOG_SEQ = :SEQ and DELI_STATUS = 'C' "
                , AA0176.PO_NO);
            return DBWork.Connection.Execute(sql, AA0176, DBWork.Transaction);
        }

        public string GetInrecSeq(AA0176 AA0176)
        {
            var sql = string.Format(@"select SEQ from MM_PO_INREC where PO_NO like '{0}%' and MMCODE=:MMCODE and status = 'N'", AA0176.PO_NO);
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, AA0176, DBWork.Transaction);
        }

        public string GetInrecSeqForTransBack(AA0176 AA0176)
        {
            var sql = string.Format(@"select SEQ from MM_PO_INREC where PO_NO like '{0}%' and MMCODE=:MMCODE 
                and LOT_NO = trim(:LOT_NO) and EXP_DATE = twn_todate(:EXP_DATE) 
                and DELI_QTY = :INQTY and status = 'Y' ", AA0176.PO_NO);
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, AA0176, DBWork.Transaction);
        }

        public int UpdateInrec(AA0176 AA0176)
        {
            var sql = string.Format(@" update MM_PO_INREC 
                set DELI_QTY = :INQTY, PO_AMT = PO_PRICE * :INQTY, STATUS = 'Y', LOT_NO = trim(:LOT_NO), 
                EXP_DATE = twn_todate(:EXP_DATE), ACCOUNTDATE = sysdate, UPDATE_TIME = sysdate, UPDATE_USER = :UPDATE_USER, 
                UPDATE_IP = :UPDATE_IP, EXTRA_DISC_AMOUNT=nvl(:EXTRA_DISC_AMOUNT, 0)
                where PO_NO like '{0}%' and MMCODE=:MMCODE and SEQ = :SEQ "
            , AA0176.PO_NO);
            return DBWork.Connection.Execute(sql, AA0176, DBWork.Transaction);
        }

        public int UpdateInrecStatus(AA0176 AA0176)
        {
            var sql = string.Format(@" UPDATE MM_PO_INREC
                        SET STATUS = 'E',
                            UPDATE_IP = :UPDATE_IP,
                            UPDATE_USER = :UPDATE_USER,
                            UPDATE_TIME = SYSDATE
                        WHERE PO_NO like '{0}%'
                        AND MMCODE = :MMCODE
                        AND SEQ = :FROMSEQ "
            , AA0176.PO_NO);
            return DBWork.Connection.Execute(sql, AA0176, DBWork.Transaction);
        }

        public int InsertInrec(AA0176 AA0176)
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
                and b.PO_QTY > b.DELI_QTY "
            , AA0176.PO_NO);
            return DBWork.Connection.Execute(sql, AA0176, DBWork.Transaction);
        }

        public int InsertInrecByTransBack(AA0176 AA0176)
        {
            var sql = string.Format(@" insert into MM_PO_INREC(PURDATE, PO_NO, MMCODE, E_PURTYPE, CONTRACNO, AGEN_NO, PO_QTY, PO_PRICE, 
                M_PURUN, PO_AMT, WH_NO, M_DISCPERC, UNIT_SWAP, UPRICE, DISC_CPRICE, DISC_UPRICE, 
                CREATE_TIME, CREATE_USER, UPDATE_IP, TRANSKIND, STATUS, IFLAG, SEQ, ISWILLING, DISCOUNT_QTY, DISC_COST_UPRICE, FROMSEQ)
                select substr(B.PO_NO,1,7), B.PO_NO, B.MMCODE, B.E_PURTYPE, A.CONTRACNO, A.AGEN_NO, B.PO_QTY, B.PO_PRICE, 
                B.M_PURUN, 0, A.WH_NO, B.M_DISCPERC, nvl(B.UNIT_SWAP, 1) as UNIT_SWAP, B.UPRICE, B.DISC_CPRICE, B.DISC_UPRICE, 
                sysdate, :UPDATE_USER, :UPDATE_IP, '120', 'Y', 'Y', MM_PO_INREC_SEQ.NEXTVAL, c.ISWILLING, c.DISCOUNT_QTY, c.DISC_COST_UPRICE, c.SEQ 
                from MM_PO_M A, MM_PO_D B, MM_PO_INREC c
                where A.PO_NO=B.PO_NO and B.PO_NO like '{0}%' and B.MMCODE=:MMCODE
                and c.PO_NO = b.PO_NO and c.MMCODE = b.MMCODE and c.SEQ = :FROMSEQ "
            , AA0176.PO_NO);
            return DBWork.Connection.Execute(sql, AA0176, DBWork.Transaction);
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
            //1130122 修改訂單單價邏輯非805=>非合約用disc_cprice成本價,合約用m_contprice合約價
            var sql = string.Format(@" update MM_PO_D A 
                set 
                --PO_PRICE = (select M_CONTPRICE from MI_MAST where MMCODE = A.MMCODE),
                    PO_PRICE=
                          (case when (select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode') ='805'
                               then (case when (select mat_class from MM_PO_M where po_no=a.po_no)='01'
                                                   then ( case when a.po_qty >= (select discount_qty from MI_MAST where mmcode=a.mmcode) then (select disc_cost_uprice from MI_MAST where mmcode=a.mmcode) 
                                                    else (select disc_cprice from MI_MAST where mmcode=a.mmcode) end)
                                          when (select mat_class from MM_PO_M where po_no=a.po_no)='02'
                                                    then ( case when nvl((select (select JBID_RCRATE from RCRATE where CASENO = e.CASENO and DATA_YM = (select SET_YM from MI_MNSET where SET_STATUS = 'N') and rownum = 1)  from MI_MAST e where MMCODE = a.MMCODE), 0) > 0 
                                                                 then round((select DISC_CPRICE from MI_MAST where MMCODE = A.MMCODE) * ((100 - (select (select JBID_RCRATE from RCRATE where CASENO = e.CASENO and DATA_YM = (select SET_YM from MI_MNSET where SET_STATUS = 'N') and rownum = 1) from MI_MAST e where MMCODE = a.MMCODE)) / 100), 3)
                                                                  else (select disc_cprice from MI_MAST where mmcode=a.mmcode) end)
                                            end)
                               else (select (case when m_contid ='0'  then m_contprice else disc_cprice end) from MI_MAST where mmcode=a.mmcode)
                           end),
                UPRICE = (select UPRICE from MI_MAST where MMCODE = A.MMCODE),
                DISC_CPRICE = (select DISC_CPRICE from MI_MAST where MMCODE = A.MMCODE),
                DISC_UPRICE = (select DISC_UPRICE from MI_MAST where MMCODE = A.MMCODE),
                UPDATE_TIME = sysdate, UPDATE_USER = :UPDATE_USER, UPDATE_IP=:UPDATE_IP
                where A.PO_NO like '{0}%' and A.MMCODE = :MMCODE "
            , po_no);
            return DBWork.Connection.Execute(sql, new { PO_NO = po_no, MMCODE = mmcode, UPDATE_USER = userName, UPDATE_IP = userIp }, DBWork.Transaction);
        }

        public string getDiscCprice(string po_no, string mmcode)
        {
            string sql = @" select (case when a.mat_class='01'
                                then ( case when b.po_qty >= (select discount_qty from MI_MAST where mmcode=b.mmcode) then (select disc_cost_uprice from MI_MAST where mmcode=b.mmcode) 
                                            else (select disc_cprice from MI_MAST where mmcode=b.mmcode) end)
                              when a.mat_class='02' then
                                    (
                        case when nvl((select (select JBID_RCRATE from RCRATE where CASENO = e.CASENO and DATA_YM = (select SET_YM from MI_MNSET where SET_STATUS = 'N') and rownum = 1) from MI_MAST e where MMCODE = b.MMCODE), 0) > 0 then
                           round(b.PO_PRICE * ((100 - (select (select JBID_RCRATE from RCRATE where CASENO = e.CASENO and DATA_YM = (select SET_YM from MI_MNSET where SET_STATUS = 'N') and rownum = 1) from MI_MAST e where MMCODE = b.MMCODE)) / 100), 3)
                        else (select disc_cprice from MI_MAST where mmcode=b.mmcode) end
                    )
                            
                    else b.DISC_CPRICE end) as DISC_CPRICE
                from MM_PO_M a, MM_PO_D b
                where a.PO_NO = b.PO_NO and a.PO_NO = :PO_NO and MMCODE = :MMCODE";
            return DBWork.Connection.QueryFirst<string>(sql, new { PO_NO = po_no, MMCODE = mmcode }, DBWork.Transaction);
        }


        public int UpdateAccPoPrice(string po_no, string mmcode, string new_disc_cprice, string userName, string userIp)
        {
            var sql = string.Format(@" update MM_PO_ACC A 
                set 
                PO_PRICE = (select (case when m_contid='0' then m_contprice else disc_cprice end) from MI_MAST where MMCODE = A.MMCODE),
                DISC_CPRICE = nvl(:DISC_CPRICE, (select (case when m_contid='0' then m_contprice else disc_cprice end) from MI_MAST where MMCODE = A.MMCODE)),
                DISC_UPRICE = (select DISC_UPRICE from MI_MAST where MMCODE = A.MMCODE),
                ACC_AMT = ACC_QTY * nvl(:DISC_CPRICE, (select (case when m_contid='0' then m_contprice else disc_cprice end) from MI_MAST where MMCODE = A.MMCODE)) - EXTRA_DISC_AMOUNT,
                UPDATE_TIME = sysdate, UPDATE_USER = :UPDATE_USER, UPDATE_IP=:UPDATE_IP
                where A.PO_NO like '{0}%' and A.MMCODE = :MMCODE "
            , po_no);
            return DBWork.Connection.Execute(sql, new { PO_NO = po_no, MMCODE = mmcode, DISC_CPRICE = new_disc_cprice, UPDATE_USER = userName, UPDATE_IP = userIp }, DBWork.Transaction);
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

        public int CheckAccCnt(string po_no, string poacc_seq)
        {
            string sql = @"SELECT count(*) FROM MM_PO_ACC WHERE PO_NO=:PO_NO 
                AND MMCODE=(select MMCODE from MM_PO_ACC where PO_NO=:PO_NO AND SEQ=:POACC_SEQ AND rownum = 1)";
            return Convert.ToInt32(DBWork.Connection.ExecuteScalar(sql, new { PO_NO = po_no, POACC_SEQ = poacc_seq }, DBWork.Transaction));
        }

        public bool CheckBcCsAccExists(string po_no, string poacc_seq)
        {
            string sql = @"SELECT 1 FROM BC_CS_ACC_LOG WHERE PO_NO=:PO_NO AND POACC_SEQ = :POACC_SEQ ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { PO_NO = po_no, POACC_SEQ = poacc_seq }, DBWork.Transaction) == null);
        }

        public int UpdateAccInvoice(string po_no, string poacc_seq, string lot_no, string exp_date, string invoice, string invoice_dt, string extra_disc_amount, string memo, string tuser, string procIp)
        {
            var p = new DynamicParameters();

            string addSql = "";

            // 若欄位有填寫才更新
            if (!string.IsNullOrEmpty(lot_no))
            {
                addSql += " LOT_NO = :LOT_NO, ";
                p.Add(":LOT_NO", string.Format("{0}", lot_no));
            }
            if (!string.IsNullOrEmpty(exp_date))
            {
                addSql += " EXP_DATE = twn_todate(:EXP_DATE), ";
                p.Add(":EXP_DATE", string.Format("{0}", exp_date));
            }
            if (!string.IsNullOrEmpty(invoice))
            {
                addSql += " INVOICE = upper(:INVOICE), ";
                p.Add(":INVOICE", string.Format("{0}", invoice));
            }
            if (!string.IsNullOrEmpty(invoice_dt))
            {
                addSql += " INVOICE_DT = twn_todate(:INVOICE_DT), ";
                p.Add(":INVOICE_DT", string.Format("{0}", invoice_dt));
            }
            if (!string.IsNullOrEmpty(extra_disc_amount))
            {
                addSql += " EXTRA_DISC_AMOUNT = :EXTRA_DISC_AMOUNT, ";
                p.Add(":EXTRA_DISC_AMOUNT", string.Format("{0}", extra_disc_amount));
            }
            if (!string.IsNullOrEmpty(memo))
            {
                addSql += " MEMO = :MEMO, ";
                p.Add(":MEMO", string.Format("{0}", memo));
            }

            var sql = @" update MM_PO_ACC 
                set " + addSql + @" UPDATE_USER=:TUSER, UPDATE_TIME = sysdate, UPDATE_IP=:PROCIP 
                WHERE PO_NO = :PO_NO and SEQ = :SEQ ";

            p.Add(":PO_NO", string.Format("{0}", po_no));
            p.Add(":SEQ", string.Format("{0}", poacc_seq));
            p.Add(":TUSER", string.Format("{0}", tuser));
            p.Add(":PROCIP", string.Format("{0}", procIp));

            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public DataTable GetExcel()
        {
            var p = new DynamicParameters();

            var sql = @" SELECT '' 訂單號碼,'' 院內碼, '' 儲位, '' 本次進貨量, '' 批號, '' 效期, '' 發票號碼, '' 發票日期, '' 備註 FROM DUAL ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public bool CheckExistsMMCODE(string id)
        {
            string sql = @" SELECT 1 FROM MI_MAST WHERE 1=1 
                          AND MMCODE = :MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = id }, DBWork.Transaction) == null);
        }

        public bool CheckExistsPo(string po_no)
        {
            string sql = @" SELECT 1 FROM MM_PO_M WHERE PO_NO = :PO_NO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { PO_NO = po_no }, DBWork.Transaction) == null);
        }

        public bool CheckPoExistsMMCODE(string po_no, string mmcode)
        {
            string sql = @" SELECT 1 FROM MM_PO_M A, MM_PO_D B WHERE A.PO_NO = B.PO_NO
                          AND A.PO_NO = :PO_NO and B.MMCODE = :MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { PO_NO = po_no, MMCODE = mmcode }, DBWork.Transaction) == null);
        }

        public bool CheckExistsD(string id)
        {
            string sql = @"SELECT 1 FROM MM_PO_ACC WHERE PO_NO=:PO_NO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { PO_NO = id }, DBWork.Transaction) == null);
        }

        public string GetPoAccSeq(string id)
        {
            string sql = @"SELECT MAX(SEQ)+1 as SEQ FROM MM_PO_ACC WHERE PO_NO=:PO_NO ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { PO_NO = id }, DBWork.Transaction).ToString();
            return rtn;
        }

        public int getPO_INVITEM(string po_no)
        {
            string sql = @" select count(*)
                from MM_PO_D
                where PO_NO =:PO_NO and DELI_STATUS <> 'Y' and nvl(STATUS,'N') <> 'D' ";
            return DBWork.Connection.QueryFirst<int>(sql, new { PO_NO = po_no }, DBWork.Transaction);
        }

        public DataTable calcAmtMsg(string po_no)
        {
            var p = new DynamicParameters();

            var sql = @"  
                select nvl((select round(sum(PO_PRICE*PO_QTY)) from MM_PO_D where PO_NO = :PO_NO and nvl(STATUS,'N') <> 'D'), 0) as SUM_PO_PRICE,
                (select round(sum(ACC_AMT)) from MM_PO_ACC where PO_NO = :PO_NO) as SUM_IN_DISC_CPRICE,
                round((SUM_INV_PO_PRICE - SUM_INV_DISC_CPRICE_1) + SUM_EXTRA_DISC) as SUM_DISC,
                round(SUM_INV_DISC_CPRICE_1 - SUM_EXTRA_DISC) as SUM_INV_DISC_CPRICE_2,
                T.*
                from (
                select
                    nvl(round(sum(TX_QTY_T*ACC_PO_PRICE)), 0) as SUM_INV_PO_PRICE, 
                    nvl(round(sum(TX_QTY_T*ACC_DISC_CPRICE)), 0) as SUM_INV_DISC_CPRICE_1, 
                    round(nvl(sum(EXTRA_DISC_AMOUNT), 0)) as SUM_EXTRA_DISC 
                    from BC_CS_ACC_LOG A where A.PO_NO = :PO_NO
                    and A.TX_QTY_T > 0
                    and (select count(*) from BC_CS_ACC_LOG where PO_NO = A.PO_NO and SRC_SEQ = A.SEQ) = 0
                ) T ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, new { PO_NO = po_no }, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        //同一發票不可不同日期(同月份)
        public string ChkInvoiceDup(string invoice, string invoicedt, string accLogSeq = "")
        {
            string ret = "Y";
            var p = new DynamicParameters();

            var sql = @"select (case when count(*) = 1 then 'N' else 'Y' end) restr
                            from (
                            select invoice_dt from PH_INVOICE where INVOICE = :invoice and twn_date(INVOICE_DT) like substr(:invoicedt,0,5) || '%'";

            if (accLogSeq != "")
            {
                sql += @" and ACC_LOG_SEQ <> :SEQ ";
                p.Add(":SEQ", accLogSeq);
            }

            sql += @"   union
                            select twn_todate(:invoicedt) from dual)";

            
            p.Add(":invoice", invoice);
            p.Add(":invoicedt", invoicedt);
            ret = DBWork.Connection.ExecuteScalar<string>(sql, p, DBWork.Transaction);
            return ret;
        }

        public int RecaltAccAmt(string po_no, string poacc_seq)
        {
            string sql = @" update MM_PO_ACC
                set ACC_AMT = ACC_QTY * DISC_CPRICE - EXTRA_DISC_AMOUNT 
                WHERE PO_NO = :PO_NO and SEQ = :SEQ
                ";
            return DBWork.Connection.Execute(sql, new { PO_NO = po_no, SEQ = poacc_seq }, DBWork.Transaction);
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