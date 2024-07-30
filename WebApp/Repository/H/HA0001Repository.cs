using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models.H;
using System.Collections.Generic;
using WebApp.Models;

namespace WebApp.Repository.H
{
    public class HA0001Repository : JCLib.Mvc.BaseRepository
    {
        public HA0001Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public bool CheckExistsData(string data_ym)
        {
            string sql = @"SELECT 1 FROM ACC_BANK_XFR WHERE DATA_YM=:DATA_YM ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DATA_YM = data_ym }, DBWork.Transaction) == null);
        }

        public int ImportData(string data_ym, string hospCode, string tuser, string userIp)
        {
            var sql = @"insert into ACC_BANK_XFR (
                            REMITNO, DATA_YM, REMITDATE, AGEN_NO, AGEN_BANK_14, 
                            AGEN_ACC, PO_AMT, ADDORSUB_AMT, AMTPAYABLE, DISC_AMT, 
                            REBATE_AMT, AMTPAID, PROCFEE, MGTFEE, REMIT, ISREMIT, XFRMEMO,
                            CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                        select T2.CHK_YM|| lpad(rownum, 5, '0') as REMITNO, T2.*
                        from
                        (
                            select T1.CHK_YM, '' as REMITDATE, T1.AGEN_NO, T1.AGEN_BANK_14,
                            T1.AGEN_ACC, round(sum(T1.PAYMASS)) as PO_AMT, 0 as ADDORSUB_AMT, round(sum(T1.PAYMASS)) as AMTPAYABLE, round(sum(T1.DISC_AMT)) as DISC_AMT,
                            sum(T1.EXTRA_DISC_AMOUNT) as REBATE_AMT, round(sum(T1.AMTPAID)) as AMTPAID, T1.PROCFEE, 0 as MGTFEE,  round(sum(T1.AMTPAID)) as REMIT,  'A' as ISREMIT, 
                            (case when (select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospCode') = '803' 
                                then '扣合約折扣' || round(sum(T1.DISC_AMT)) 
                                else '' end
                            ) as XFRMEMO,
                            sysdate as CREATE_TIME, :TUSER as CREATE_USER, sysdate as UPDATE_TIME, :TUSER as UPDATE_USER, :USERIP as UPDATE_IP
                            from
                            (
                                select  A.CHK_YM, A.AGEN_NO,
                                    nvl(B.AGEN_BANK_14, B.AGEN_BANK || B.AGEN_SUB) as AGEN_BANK_14,
                                    B.AGEN_ACC, A.PAYMASS as PAYMASS,
                                    (case when nvl(A.RCMOD, 0)<0 then round(nvl(A.DISC_AMT, 0))-(nvl(A.RCMOD, 0)*-1) else round(nvl(A.DISC_AMT, 0))+nvl(A.RCMOD, 0) end) as DISC_AMT, 
                                    A.EXTRA_DISC_AMOUNT, 
                                    (case when nvl(A.RCMOD, 0)<0 then (A.PAYMASS - round(nvl(A.DISC_AMT, 0))+(nvl(A.RCMOD, 0)*-1)) 
                                                    else (A.PAYMASS - round(nvl(A.DISC_AMT, 0))-nvl(A.RCMOD, 0)) end) as AMTPAID, 
                                    nvl(A.PROCFEE, 0) as PROCFEE
                                from INVCHK A, PH_VENDER B
                                where A.AGEN_NO=B.AGEN_NO and A.CHK_YM = :DATA_YM
                                and (select count(*) from ACC_BANK_XFR where DATA_YM = A.CHK_YM and AGEN_NO = A.AGEN_NO) = 0
                            ) T1
                            group by T1.CHK_YM, T1.AGEN_NO, T1.AGEN_BANK_14, T1.AGEN_ACC, T1.PROCFEE
                        ) T2 ";

            // 1121107改在BG0010將資料寫入INVCHK, HA0001直接抓INVCHK
            /*
            string sql_procfee = @"(case when (substr(T.AGEN_BANK_14, 1, 3) = (select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospBankCode'))
                        then 0
                        else
                            (select FEE from PH_BANK_FEE where (PO_AMT + ADDORSUB_AMT - DISC_AMT) >= CASHFROM and (PO_AMT + ADDORSUB_AMT - DISC_AMT) <= CASHTO and rownum = 1) 
                        end) as PROCFEE, ";
            // 部分醫院匯費預設為0
            if (hospCode == "805")
                sql_procfee = " 0 as PROCFEE, ";

            var sql = @"insert into ACC_BANK_XFR (
                        REMITNO, DATA_YM, REMITDATE, AGEN_NO, AGEN_BANK_14, 
                        AGEN_ACC, PO_AMT, ADDORSUB_AMT, AMTPAYABLE, DISC_AMT, 
                        REBATE_AMT, AMTPAID, PROCFEE, MGTFEE, REMIT, ISREMIT, XFRMEMO,
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      select :DATA_YM || lpad(rownum, 5, '0') as REMITNO, T.DATA_YM, T.REMITDATE, T.AGEN_NO, trim(T.AGEN_BANK_14) as AGEN_BANK_14, T.AGEN_ACC, T.PO_AMT, T.ADDORSUB_AMT,
                        PO_AMT + ADDORSUB_AMT as AMTPAYABLE, T.DISC_AMT, T.REBATE_AMT, 
                        PO_AMT + ADDORSUB_AMT - DISC_AMT as AMTPAID, 
                        " + sql_procfee + @"
                        0 as MGTFEE, 
                        0 as REMIT, 
                        T.ISREMIT, T.XFRMEMO, T.CREATE_TIME, T.CREATE_USER, T.UPDATE_TIME, T.UPDATE_USER, T.UPDATE_IP
                      from (
                            select :DATA_YM as DATA_YM,
                            '' as REMITDATE, A.AGEN_NO,
                            nvl(C.AGEN_BANK_14, C.AGEN_BANK || C.AGEN_SUB) as AGEN_BANK_14,
                            C.AGEN_ACC, sum(B.DELI_QTY * B.PO_PRICE) as PO_AMT, 
                            0 as ADDORSUB_AMT, 
                            sum((B.PO_PRICE - B.DISC_CPRICE) * B.DELI_QTY) + nvl(sum(B.EXTRA_DISC_AMOUNT), 0) as DISC_AMT,
                            nvl(sum(B.EXTRA_DISC_AMOUNT), 0) as REBATE_AMT,
                            'A' as ISREMIT, '' as XFRMEMO,
                            sysdate as CREATE_TIME, :TUSER as CREATE_USER, sysdate as UPDATE_TIME, :TUSER as UPDATE_USER, :USERIP as UPDATE_IP
                            from MM_PO_M A, PH_INVOICE B, PH_VENDER C, INVOICE D
                            where A.PO_NO = B.PO_NO and A.AGEN_NO = C.AGEN_NO and B.MMCODE = D.MMCODE and B.INVOICE = D.INVOICE and B.INVOICE_DT = D.INVOICE_DT
                            and D.INVMARK = '1'
                            and D.ACT_YM = :DATA_YM
                            and B.DELI_STATUS = 'C'
                            group by A.AGEN_NO, C.AGEN_BANK_14, C.AGEN_BANK, C.AGEN_SUB, C.AGEN_ACC
                            order by A.AGEN_NO
                      ) T
                        ";*/
            return DBWork.Connection.Execute(sql, new { DATA_YM = data_ym, TUSER = tuser, USERIP = userIp }, DBWork.Transaction);
        }

        public int updateRemit(string data_ym)
        {
            var sql = @"update ACC_BANK_XFR
                        set REMIT = AMTPAID - PROCFEE - MGTFEE
                        where DATA_YM = :DATA_YM
                        ";
            return DBWork.Connection.Execute(sql, new { DATA_YM = data_ym }, DBWork.Transaction);
        }

        public int updateRemitdate(string data_ym, string remitdate, string remitnoFrom, string remitnoTo)
        {
            var sql = @"update ACC_BANK_XFR
                        set REMITDATE = :REMITDATE, ISREMIT = 'B'
                        where DATA_YM = :DATA_YM and REMITNO >= :REMITNOFROM and REMITNO <= :REMITNOTO
                        and AGEN_BANK_14 is not null and AGEN_ACC is not null
                        ";
            return DBWork.Connection.Execute(sql, new { DATA_YM = data_ym, REMITDATE = remitdate, REMITNOFROM = remitnoFrom, REMITNOTO = remitnoTo }, DBWork.Transaction);
        }

        public int LoadAgenBank(string remitno, string agen_no)
        {
            var sql = @"update ACC_BANK_XFR
                        set AGEN_BANK_14 = (select nvl(AGEN_BANK_14, AGEN_BANK || AGEN_SUB) from PH_VENDER where AGEN_NO = :AGEN_NO),
                        AGEN_ACC = (select AGEN_ACC from PH_VENDER where AGEN_NO = :AGEN_NO)
                        where REMITNO = :REMITNO
                        ";
            return DBWork.Connection.Execute(sql, new { REMITNO = remitno, AGEN_NO = agen_no }, DBWork.Transaction);
        }

        public IEnumerable<HA0001> GetPreChkResult(string data_ym)
        {
            var sql = @"select A.CHK_YM as DATA_YM, A.AGEN_NO,'沒有廠商銀行代碼或帳戶資料' as CHKMSG
                                     from INVCHK A
                                   where A.CHK_YM = :DATA_YM
                                       and ((select nvl(AGEN_BANK_14, AGEN_BANK || AGEN_SUB) from PH_VENDER where AGEN_NO = A.AGEN_NO) is null
                                                   or (select AGEN_ACC from PH_VENDER where AGEN_NO = A.AGEN_NO) is null)
                                ";
            /*
            var sql = @"
                        select D.ACT_YM as DATA_YM, D.AGEN_NO, '沒有廠商銀行代碼或帳戶資料' as CHKMSG
                        from MM_PO_M A, PH_INVOICE B, PH_VENDER C, INVOICE D
                        where A.PO_NO = B.PO_NO and A.AGEN_NO = C.AGEN_NO and B.MMCODE = D.MMCODE and B.INVOICE = D.INVOICE and B.INVOICE_DT = D.INVOICE_DT
                        and D.INVMARK = '1'
                        and D.ACT_YM = :DATA_YM
                        and B.DELI_STATUS = 'C'
                        and ((select nvl(AGEN_BANK_14, AGEN_BANK || AGEN_SUB) from PH_VENDER where AGEN_NO = D.AGEN_NO) is null
                            or (select AGEN_ACC from PH_VENDER where AGEN_NO = D.AGEN_NO) is null)
                        group by D.ACT_YM, D.AGEN_NO
                        "; */
            return DBWork.Connection.Query<HA0001>(sql, new { DATA_YM = data_ym }, DBWork.Transaction);
        }

        public IEnumerable<HA0001> GetChkResult(string data_ym)
        {
            var sql = @"
                        select A.DATA_YM, A.AGEN_NO, '沒有廠商銀行代碼或帳戶資料, 請在轉電匯資料前將帳戶資料補入!' as CHKMSG
                            from ACC_BANK_XFR A
                        where A.DATA_YM = :DATA_YM
                           and ((select nvl(AGEN_BANK_14, AGEN_BANK || AGEN_SUB) from PH_VENDER where AGEN_NO = A.AGEN_NO) is null
                            or (select AGEN_ACC from PH_VENDER where AGEN_NO = A.AGEN_NO) is null)
                        union     
                        select DATA_YM, AGEN_NO, '匯款金額不足付匯費及手續費, 應付金額為:' || AMTPAYABLE as CHKMSG from ACC_BANK_XFR where DATA_YM = :DATA_YM and AMTPAYABLE - PROCFEE - MGTFEE <= 0
                        ";

            return DBWork.Connection.Query<HA0001>(sql, new { DATA_YM = data_ym }, DBWork.Transaction);
        }

        public IEnumerable<HA0001> GetAll(string data_ym, string agen, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select A.REMITNO, A.DATA_YM, A.REMITDATE, A.AGEN_NO, 
                B.AGEN_NAMEC as AGEN_NAME,
                (B.AGEN_TEL || ' ' || B.AGEN_CONTACT) as AGEN_TEL, 
                ((case when B.AGEN_ZIP is null then '' else '(' || B.AGEN_ZIP || ')' end) || B.AGEN_ADD) as AGEN_ADD,
                (select BANKNAME from PH_BANK_AF where AGEN_BANK_14 = A.AGEN_BANK_14) as BANKNAME,
                A.AGEN_BANK_14, A.AGEN_ACC, A.PO_AMT, A.ADDORSUB_AMT, A.AMTPAYABLE, A.DISC_AMT,
                A.REBATE_AMT, A.AMTPAID, A.PROCFEE, A.MGTFEE, A.REMIT, A.ISREMIT, A.XFRMEMO
                from ACC_BANK_XFR A left join PH_VENDER B on A.AGEN_NO = B.AGEN_NO
                where 1=1 ";

            sql += " and DATA_YM = :DATA_YM ";
            p.Add(":DATA_YM", string.Format("{0}", data_ym));

            if (agen != "")
            {
                sql += " and (upper(A.AGEN_NO) like upper(:AGEN) or B.AGEN_NAMEC like :AGEN) ";
                p.Add(":AGEN", string.Format("%{0}%", agen));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<HA0001>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public string getBANKNAME(string agen_bank_14)
        {
            string sql = @" select BANKNAME
                from PH_BANK_AF 
                where AGEN_BANK_14 =:AGEN_BANK_14 ";
            return Convert.ToString(DBWork.Connection.ExecuteScalar(sql, new { AGEN_BANK_14 = agen_bank_14 }, DBWork.Transaction));
        }

        public string getHospBankAcc()
        {
            string sql = @" select DATA_VALUE
                from PARAM_D 
                where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospBankAcc' ";
            return Convert.ToString(DBWork.Connection.ExecuteScalar(sql, null, DBWork.Transaction));
        }

        public string ChkIsremit(string data_ym, string remitnoFrom, string remitnoTo)
        {
            string sql = @" select (case when (select count(*) from ACC_BANK_XFR 
                                        where DATA_YM = :DATA_YM and REMITNO >= :REMITNOFROM and REMITNO <= :REMITNOTO and ISREMIT = 'B') = 0 then 'P' else 'N' end)
                        from dual ";
            return Convert.ToString(DBWork.Connection.ExecuteScalar(sql, new { DATA_YM = data_ym, REMITNOFROM = remitnoFrom, REMITNOTO = remitnoTo }, DBWork.Transaction));
        }

        public string ChkBank(string data_ym, string remitnoFrom, string remitnoTo)
        {
            string sql = @" select (case when (select count(*) from ACC_BANK_XFR A
                                        where A.DATA_YM = :DATA_YM and A.REMITNO >= :REMITNOFROM and A.REMITNO <= :REMITNOTO
                                        and (A.AGEN_BANK_14 is null or A.AGEN_ACC is null)
                                    ) = 0 then 'P' else 'N' end)
                        from dual ";
            return Convert.ToString(DBWork.Connection.ExecuteScalar(sql, new { DATA_YM = data_ym, REMITNOFROM = remitnoFrom, REMITNOTO = remitnoTo }, DBWork.Transaction));
        }

        public string getHospName()
        {
            string sql = @" select DATA_VALUE
                from PARAM_D 
                where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospName' ";
            return Convert.ToString(DBWork.Connection.ExecuteScalar(sql, null, DBWork.Transaction));
        }

        public string getAgenNoByRemitno(string remitno)
        {
            string sql = @" select A.AGEN_NO
                from ACC_BANK_XFR A
                where A.REMITNO = :REMITNO ";
            return Convert.ToString(DBWork.Connection.ExecuteScalar(sql, new { REMITNO = remitno }, DBWork.Transaction));
        }

        public DataTable getDataByRemitno(string remitno)
        {
            var p = new DynamicParameters();

            var sql = @"select A.AGEN_NO,
                A.AGEN_NO || (select AGEN_NAMEC from PH_VENDER where AGEN_NO = A.AGEN_NO) as AGEN_NAME,
                (select AGEN_NAMEC from PH_VENDER where AGEN_NO = A.AGEN_NO) as AGEN_NAMEC,
                (select EMAIL from PH_VENDER where AGEN_NO = A.AGEN_NO) as EMAIL, 
                A.AGEN_BANK_14 || ' ' || (select BANKNAME from PH_BANK_AF where AGEN_BANK_14 = A.AGEN_BANK_14) as BANK, 
                A.AGEN_ACC, A.REMIT, A.REMITDATE, A.XFRMEMO,
                (select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospAccContact') as ACC_CONTACT,
                (select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospAccTel') as ACC_TEL,
                (select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospRecAddr') as HOSP_ADDR,
                (select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospAccRoom') as ACC_ROOM
			from ACC_BANK_XFR A
            where REMITNO = :REMITNO
            ";

            p.Add(":REMITNO", remitno);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public DataTable getInvoiceByRemitno(string remitno)
        {
            var p = new DynamicParameters();

            var sql = @"select B.INVOICE, B.INVOICE_AMOUNT, B.REBATESUM
                    from ACC_BANK_XFR A, INVOICE B
                     where A.DATA_YM = B.ACT_YM
                     and A.AGEN_NO = B.AGEN_NO
                     and B.INVMARK = '1'
                     and REMITNO = :REMITNO
                     ";

            p.Add(":REMITNO", remitno);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public string getMailByRemitno(string remitno)
        {
            string sql = @" select (select EMAIL from PH_VENDER where AGEN_NO = A.AGEN_NO) as EMAIL
                from ACC_BANK_XFR A
                where A.REMITNO = :REMITNO ";
            return Convert.ToString(DBWork.Connection.ExecuteScalar(sql, new { REMITNO = remitno }, DBWork.Transaction));
        }

        public string getMailByUser(string tuser)
        {
            string sql = @" select EMAIL
                from UR_ID
                where TUSER = :TUSER ";
            return Convert.ToString(DBWork.Connection.ExecuteScalar(sql, new { TUSER = tuser }, DBWork.Transaction));
        }

        public int Update(HA0001 ha0001)
        {
            var sql = @"UPDATE ACC_BANK_XFR 
                            SET AGEN_BANK_14 = :AGEN_BANK_14,
                            AGEN_ACC = :AGEN_ACC,
                            PO_AMT =:PO_AMT,
                            ADDORSUB_AMT = :ADDORSUB_AMT,
                            DISC_AMT = :DISC_AMT,
                            PROCFEE =:PROCFEE,
                            MGTFEE = :MGTFEE,
                            AMTPAYABLE = :AMTPAYABLE,
                            AMTPAID = :AMTPAID,
                            REMIT = :REMIT,
                            XFRMEMO = :XFRMEMO,
                            UPDATE_TIME=sysdate,
                            UPDATE_USER=:UPDATE_USER,
                            UPDATE_IP=:UPDATE_IP
                            WHERE REMITNO=:REMITNO";

            return DBWork.Connection.Execute(sql, ha0001, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetRemitnoCombo(string data_ym)
        {
            var sql = @"select REMITNO as TEXT, REMITNO as VALUE from ACC_BANK_XFR
                    where DATA_YM = :DATA_YM
                    order by REMITNO
                    ";

            return DBWork.Connection.Query<ComboItemModel>(sql, new { DATA_YM = data_ym });
        }

        public DataTable GetTxtData(string data_ym, string remitnoFrom, string remitnoTo)
        {
            var p = new DynamicParameters();

            var sql = @"select A.AGEN_ACC as 銀行帳號, A.REMIT as 匯款金額, A.AGEN_BANK_14 as 銀行代碼, 
                (select AGEN_NAMEC from PH_VENDER where AGEN_NO = A.AGEN_NO) as 廠商名稱,
                (select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospName') as 醫院名稱,
                (case when (select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospCode') = '805' 
                    then '國花醫' || ' ' || substr(:DATA_YM, 4, 2) || '月 藥衛材貨款'
                    when  (select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospCode') = '803' 
                    then A.XFRMEMO
                    else 
                    (select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospName') || ' ' || substr(:DATA_YM, 4, 2) || '月 藥衛材貨款' || ' ' || A.XFRMEMO
                end) as 附言,
                (select UNI_NO from PH_VENDER where AGEN_NO = A.AGEN_NO) as 統一編號
			from ACC_BANK_XFR A
            where DATA_YM = :DATA_YM and REMITNO >= :REMITNOFROM and REMITNO <= :REMITNOTO
            and AGEN_BANK_14 is not null and AGEN_ACC is not null
			order by A.REMITNO
            ";

            p.Add(":DATA_YM", data_ym);
            p.Add(":REMITNOFROM", remitnoFrom);
            p.Add(":REMITNOTO", remitnoTo);

            sql = string.Format(@"select * 
                                    from ( {0} )
                                   where 匯款金額 <> 0", sql);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public int MailLog(string tuser, string mailFrom, string mailTo, string agenno, string remitno, string userip)
        {
            var sql = @"insert into SEND_MAIL_LOG (SEQ, SEND_USER, SEND_USER_EMAIL, RECEIVE_EMAIL, AGEN_NO, MAIL_TYPE, DETAIL_VALUE, 
                IS_SEND, SEND_TIME, CREATE_TIME, CREATE_USER, UPDATE_IP)
                values ((select max(SEQ) + 1 from SEND_MAIL_LOG), :TUSER, :MAIL_FROM, :MAIL_TO, :AGENNO, '2', 'REMITNO=' || :REMITNO, 
                'Y', sysdate, sysdate, :TUSER, :USER_IP)
                ";
            return DBWork.Connection.Execute(sql, new { TUSER = tuser, MAIL_FROM = mailFrom, MAIL_TO = mailTo, AGENNO = agenno, REMITNO = remitno, USER_IP = userip }, DBWork.Transaction);
        }

        public string GetHospCode()
        {
            var sql = @"select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospCode'";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }

        public string GetUserRole(string tuser)
        {
            var sql = @"select (case when (select count(*) from UR_UIR where TUSER = :TUSER and RLNO = 'MACC_14') > 0
                            and (select count(*) from UR_UIR where TUSER = :TUSER and RLNO = 'MMSpl_14') > 0 then 'ALL'
                            when (select count(*) from UR_UIR where TUSER = :TUSER and RLNO = 'MACC_14') > 0 then 'MACC'
                            when (select count(*) from UR_UIR where TUSER = :TUSER and RLNO = 'MMSpl_14') > 0 then 'MMSPL'
                            else 'ALL' end) as CHK from dual ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { TUSER = tuser }, DBWork.Transaction);
        }

        public IEnumerable<HA0001> GetPrintData(string PrintType, string RemitnoFrom, string RemitnoTo)
        {
            string addSql = "";
            if (PrintType == "1")
                addSql = " and A.ISREMIT = 'B' ";
            else if (PrintType == "2")
                addSql = " and A.ISREMIT = 'A' ";

            var sql = @"select A.REMITNO, A.AGEN_NO, B.AGEN_NAMEC as AGEN_NAME, A.AGEN_BANK_14, A.AGEN_ACC, 
                nvl(A.AMTPAYABLE, 0) as AMTPAYABLE, nvl(A.PROCFEE, 0) as PROCFEE, nvl(A.MGTFEE, 0) as MGTFEE, nvl(A.REMIT, 0) as REMIT, 
                ((case when nvl(A.DISC_AMT, 0) > 0 then '扣合約優惠款 ' || nvl(A.DISC_AMT, 0) || '元 ' else '' end) || A.XFRMEMO) as XFRMEMO
                from ACC_BANK_XFR A left join PH_VENDER B on A.AGEN_NO = B.AGEN_NO
                where A.REMITNO >= :REMITNO_F and REMITNO <= :REMITNO_T " + addSql;

            return DBWork.Connection.Query<HA0001>(sql, new { REMITNO_F = RemitnoFrom, REMITNO_T = RemitnoTo }, DBWork.Transaction);
        }

        public DataTable calcAmtMsg(string data_ym, string agen)
        {
            var p = new DynamicParameters();

            var sql = @"  
                select round(sum(nvl(A.REMIT, 0))) as SUM_REMIT from ACC_BANK_XFR A where A.DATA_YM = :DATA_YM ";
            p.Add(":DATA_YM", string.Format("{0}", data_ym));

            if (agen != "")
            {
                sql += " and (upper(A.AGEN_NO) like upper(:AGEN) or (select AGEN_NAMEC from PH_VENDER where AGEN_NO = A.AGEN_NO) like :AGEN) ";
                p.Add(":AGEN", string.Format("%{0}%", agen));
            }

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
    }
}