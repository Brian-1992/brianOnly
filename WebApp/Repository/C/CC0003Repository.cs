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
    public class CC0003Repository : JCLib.Mvc.BaseRepository
    {
        public CC0003Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //查詢前(呼叫STORE PROCEDURE: MM_PO_INREC_CHK) ---取消
        public string MM_PO_INREC_CHK(string i_wh_no, string i_purdate, string i_userid, string i_ip)
        {
            var p = new OracleDynamicParameters();
            p.Add("i_wh_no", value: i_wh_no, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 8);
            p.Add("i_purdate", value: i_purdate, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 7);
            p.Add("i_userid", value: i_userid, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 10);
            p.Add("i_ip", value: i_ip, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);

            p.Add("ret_code", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 10);

            DBWork.Connection.Query("MM_PO_INREC_CHK", p, commandType: CommandType.StoredProcedure);
            string ret_code = p.Get<OracleString>("ret_code").Value;
            return ret_code;
        }

        //查詢TAB_1
        public IEnumerable<CC0003> GetAll_1(string WH_NO, string AGEN_NO, string PURDATE, string PURDATE_1, string KIND, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"  select (case when a.E_VACCINE = 'Y' then 'Y' else '' end) VACCINE,
                                 b.CONTRACNO,
                                 (case when b.E_PURTYPE='1' then '甲'
                                       when b.E_PURTYPE='2' then '乙'
                                       else ''
                                  end) as E_PURTYPE,
                                 b.PURDATE, b.PO_NO, b.PO_NO PO_NO_REF, b.AGEN_NO,
                                 (select c.AGEN_NO || ' ' || c.AGEN_NAMEC
                                    from PH_VENDER c
                                   where c.AGEN_NO = b.AGEN_NO) AGEN_NO_NAME,
                                 b.MMCODE,
                                 a.MMNAME_C, a.MMNAME_E,
                                 to_char(b.ACCOUNTDATE, 'YYYY/MM/DD') ACCOUNTDATE,
                                 to_char(b.ACCOUNTDATE, 'YYYYMMDD') - 19110000 ACCOUNTDATE_REF,
                                 b.PO_QTY,
                                 (b.DELI_QTY / b.UNIT_SWAP) DELI_QTY,
                                 (b.DELI_QTY / b.UNIT_SWAP) DELI_QTY_REF,
                                 (case when b.STATUS = 'Y' OR b.STATUS = 'E' then '進貨'
                                       else ''
                                  end) INFLAG,
                                 (case when b.STATUS = 'E' then '退貨'
                                       else ''
                                  end) OUTFLAG,
                                 b.PO_PRICE,
                                 (case when b.STATUS = 'Y' OR b.STATUS = 'E' then b.DELI_QTY*b.PO_PRICE
                                       when b.STATUS = 'E' then -1*b.DELI_QTY*b.PO_PRICE
                                       else 0
                                  end ) PO_AMT,
                                 b.M_PURUN, b.LOT_NO, b.LOT_NO LOT_NO_REF,
                                 to_char(b.EXP_DATE, 'YYYY/MM/DD') EXP_DATE,
                                 to_char(b.EXP_DATE, 'YYYYMMDD') - 19110000 EXP_DATE_REF,
                                 b.MEMO, b.MEMO MEMO_REF, b.WH_NO, b.STATUS, b.DELI_QTY ORI_QTY,
                                 b.SEQ, b.M_DISCPERC, b.UNIT_SWAP, b.UPRICE, b.DISC_CPRICE,
                                 b.DISC_UPRICE, b.TRANSKIND, b.IFLAG
                            from MI_MAST a, MM_PO_INREC b
                           where a.MMCODE = b.MMCODE
                             and b.TRANSKIND = '111'
                             and not (NEXTMON = 'Y' AND STATUS = 'N')  --排除下月進貨
                             and b.WH_NO = :WH_NO
                             and b.PURDATE between :PURDATE and :PURDATE_1
                           ";

            p.Add(":WH_NO", string.Format("{0}", WH_NO));
            p.Add(":PURDATE", string.Format("{0}", PURDATE));
            p.Add(":PURDATE_1", string.Format("{0}", PURDATE_1));

            if (AGEN_NO == "0")
            {
                sql += @" and b.AGEN_NO between '001' and '100'";
            }
            else if (AGEN_NO == "1")
            {
                sql += @" and b.AGEN_NO between '101' and '200'";
            }
            else if (AGEN_NO == "2")
            {
                sql += @" and b.AGEN_NO between '201' and '300'";
            }
            else if (AGEN_NO == "3")
            {
                sql += @" and b.AGEN_NO between '301' and '400'";
            }
            else if (AGEN_NO == "4")
            {
                sql += @" and b.AGEN_NO between '401' and '500'";
            }
            else if (AGEN_NO == "5")
            {
                sql += @" and b.AGEN_NO between '501' and '600'";
            }
            else if (AGEN_NO == "6")
            {
                sql += @" and b.AGEN_NO between '601' and '700'";
            }
            else if (AGEN_NO == "7")
            {
                sql += @" and b.AGEN_NO between '701' and '800'";
            }
            else if (AGEN_NO == "8")
            {
                sql += @" and b.AGEN_NO between '801' and '900'";
            }
            else if (AGEN_NO == "9")
            {
                sql += @" and b.AGEN_NO between '901' and '999'";
            }

            if (KIND == "")
            {
                sql += @"  and b.STATUS IN ('N', 'T', 'Y', 'E')";
            }
            else if (KIND == "N")
            {
                sql += @" and b.STATUS IN ('N', 'T')";
            }
            else if (KIND == "Y")
            {
                sql += @" and b.STATUS IN ('Y' ,'E')";
            }

            sql += @" order by b.AGEN_NO, a.MMNAME_E, b.MMCODE";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CC0003>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //查詢TAB_2
        public IEnumerable<CC0003> GetAll_2(string WH_NO, string AGEN_NO, string PURDATE, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"  SELECT (CASE WHEN MMT.E_VACCINE = 'Y' THEN 'Y' ELSE '' END) VACCINE,
                                 MPC.CONTRACNO,
                                 (case when MPC.E_PURTYPE='1' then '甲' when MPC.E_PURTYPE='2' then '乙' else '' end) as E_PURTYPE,
                                 MPC.PURDATE,
                                 MPC.PO_NO,
                                 MPC.PO_NO PO_NO_REF,
                                 MPC.AGEN_NO,
                                 (SELECT PVR.AGEN_NO || ' ' || PVR.AGEN_NAMEC
                                  FROM PH_VENDER PVR
                                  WHERE PVR.AGEN_NO = MPC.AGEN_NO) AGEN_NO_NAME,
                                 MPC.MMCODE,
                                 MMT.MMNAME_C,
                                 MMT.MMNAME_E,
                                 TO_CHAR (MPC.ACCOUNTDATE, 'YYYY/MM/DD') ACCOUNTDATE,
                                 TO_CHAR (MPC.ACCOUNTDATE, 'YYYYMMDD') - 19110000 ACCOUNTDATE_REF,
                                 MPC.PO_QTY,
                                 (MPC.DELI_QTY / MPC.UNIT_SWAP) DELI_QTY,
                                 (MPC.DELI_QTY / MPC.UNIT_SWAP) DELI_QTY_REF,
                                 (CASE
                                      WHEN MPC.STATUS = 'Y' OR MPC.STATUS = 'E' THEN '進貨'
                                      ELSE ''
                                  END) INFLAG,
                                 (CASE
                                      WHEN MPC.STATUS = 'E' THEN '退貨'
                                      ELSE ''
                                  END) OUTFLAG,
                                 MPC.PO_PRICE,
                                 (CASE WHEN MPC.STATUS = 'Y' OR MPC.STATUS = 'E' THEN MPC.DELI_QTY*MPC.PO_PRICE
                                  WHEN MPC.STATUS = 'E' THEN -1*MPC.DELI_QTY*MPC.PO_PRICE else 0 END ) PO_AMT,
                                 MPC.M_PURUN,
                                 MPC.LOT_NO,
                                 MPC.LOT_NO LOT_NO_REF,
                                 TO_CHAR ( MPC.EXP_DATE, 'YYYY/MM/DD') EXP_DATE,
                                 TO_CHAR ( MPC.EXP_DATE, 'YYYYMMDD')  - 19110000 EXP_DATE_REF,
                                 MPC.MEMO,
                                 MPC.MEMO MEMO_REF,
                                 MPC.WH_NO,
                                 MPC.STATUS,
                                 MPC.DELI_QTY ORI_QTY,
                                 MPC.SEQ,
                                 MPC.M_DISCPERC,
                                 MPC.UNIT_SWAP,
                                 MPC.UPRICE,
                                 MPC.DISC_CPRICE,
                                 MPC.DISC_UPRICE,
                                 MPC.TRANSKIND,
                                 MPC.IFLAG
                          FROM MI_MAST MMT, MM_PO_INREC MPC
                          WHERE     MMT.MMCODE = MPC.MMCODE
                          AND MPC.PURDATE = :PURDATE
                          AND MPC.WH_NO = :WH_NO
                          AND NEXTMON = 'Y'
                          AND STATUS = 'N'";


            p.Add(":WH_NO", string.Format("{0}", WH_NO));
            p.Add(":PURDATE", string.Format("{0}", PURDATE));

            if (AGEN_NO == "0")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '001' AND '100'";
            }
            else if (AGEN_NO == "1")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '101' AND '200'";
            }
            else if (AGEN_NO == "2")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '201' AND '300'";
            }
            else if (AGEN_NO == "3")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '301' AND '400'";
            }
            else if (AGEN_NO == "4")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '401' AND '500'";
            }
            else if (AGEN_NO == "5")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '501' AND '600'";
            }
            else if (AGEN_NO == "6")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '601' AND '700'";
            }
            else if (AGEN_NO == "7")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '701' AND '800'";
            }
            else if (AGEN_NO == "8")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '801' AND '900'";
            }
            else if (AGEN_NO == "9")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '901' AND '999'";
            }

            sql += @" ORDER BY MPC.AGEN_NO, MMT.MMNAME_E, MPC.MMCODE";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CC0003>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //庫別combox
        public IEnumerable<ComboItemModel> GetWH_NO()
        {
            string sql = @"  SELECT WH_NO VALUE,
                                    WH_NO || ' ' || WH_NAME TEXT
                             FROM MI_WHMAST
                             WHERE WH_GRADE IN ('1', '5') AND WH_KIND = '0'
                             ORDER BY WH_NO";
            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }

        //廠商代碼combox
        public IEnumerable<ComboItemModel> GetAGEN_NO()
        {
            string sql = @"SELECT AGEN_NO VALUE,
                                  AGEN_NO || ' ' || TRIM(AGEN_NAMEC) TEXT
                           FROM PH_VENDER
                           ORDER BY AGEN_NO";
            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }

        //儲存
        public int Commit(string ACCOUNTDATE, string DELI_QTY, string MEMO, string LOT_NO, string EXP_DATE, string PO_AMT
            , string UPDATE_IP, string UPDATE_USER, string PO_QTY, string PO_NO, string MMCODE, string SEQ, bool isTempSave = false)
        {
            var p = new DynamicParameters();
            if ((ACCOUNTDATE.Length == 7) && (ACCOUNTDATE.Length != 0))
            {
                ACCOUNTDATE = (int.Parse(ACCOUNTDATE) + 19110000).ToString();
            }
            if ((EXP_DATE.Length == 7) && (EXP_DATE.Length != 0))
            {
                EXP_DATE = (int.Parse(EXP_DATE) + 19110000).ToString();
            }

            var sql = string.Format(
                      @"UPDATE MM_PO_INREC
                           SET {0}
                               DELI_QTY = :DELI_QTY,
                               MEMO = :MEMO,
                               LOT_NO = :LOT_NO,
                               EXP_DATE = TO_DATE ( :EXP_DATE, 'YYYY/MM/DD'),
                               PO_AMT = :PO_AMT,
                               UPDATE_IP = :UPDATE_IP,
                               UPDATE_USER = :UPDATE_USER,
                               UPDATE_TIME = SYSDATE,
                               STATUS = 'N'"
                    , isTempSave ? string.Empty : " ACCOUNTDATE = SYSDATE, ");

            sql += @" WHERE PO_NO = :PO_NO
                        AND MMCODE = :MMCODE
                        AND SEQ = :SEQ";

            p.Add(":ACCOUNTDATE", string.Format("{0}", ACCOUNTDATE));
            p.Add(":DELI_QTY", string.Format("{0}", DELI_QTY));
            p.Add(":MEMO", string.Format("{0}", MEMO));
            p.Add(":LOT_NO", string.Format("{0}", LOT_NO));
            p.Add(":EXP_DATE", string.Format("{0}", EXP_DATE));
            p.Add(":PO_AMT", string.Format("{0}", PO_AMT));
            p.Add(":UPDATE_IP", string.Format("{0}", UPDATE_IP));
            p.Add(":UPDATE_USER", string.Format("{0}", UPDATE_USER));
            p.Add(":PO_NO", string.Format("{0}", PO_NO));
            p.Add(":MMCODE", string.Format("{0}", MMCODE));
            p.Add(":SEQ", string.Format("{0}", SEQ));

            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        //取得SEQ
        public string GetSEQ()
        {
            var sql = @"SELECT MM_PO_INREC_SEQ.NEXTVAL
                        FROM DUAL";

            return DBWork.Connection.QueryFirst<string>(sql, DBWork.Transaction);
        }

        //新增
        public int Insert(string SEQ, string PURDATE, string PO_NO, string MMCODE, string E_PURTYPE, string CONTRACNO, string AGEN_NO
        , string PO_QTY, string PO_PRICE, string STATUS, string M_PURUN, string PO_AMT, string WH_NO, string M_DISCPERC,
            string UNIT_SWAP, string UPRICE, string DISC_CPRICE, string DISC_UPRICE, string CREATE_USER, string UPDATE_USER,
            string UPDATE_IP, string TRANSKIND, string IFLAG, string ISWILLING, string DISCOUNT_QTY, string DISC_COST_UPRICE)
        {
            var sql = @"INSERT INTO MM_PO_INREC 
                                    (PURDATE,
                                     PO_NO,
                                     MMCODE,
                                     SEQ,
                                     E_PURTYPE,
                                     CONTRACNO,
                                     AGEN_NO,            
                                     PO_QTY,            
                                     PO_PRICE,
                                     STATUS,
                                     M_PURUN,
                                     PO_AMT,
                                     WH_NO,
                                     M_DISCPERC,
                                     UNIT_SWAP,
                                     UPRICE,
                                     DISC_CPRICE,
                                     DISC_UPRICE,
                                     CREATE_TIME,
                                     CREATE_USER,
                                     UPDATE_TIME,
                                     UPDATE_USER,
                                     UPDATE_IP,
                                     TRANSKIND,
                                     IFLAG,
                                     ISWILLING,
                                     DISCOUNT_QTY,
                                     DISC_COST_UPRICE)
                              VALUES (:PURDATE,
                                      :PO_NO,
                                      :MMCODE,
                                      :SEQ,
                                      :E_PURTYPE,
                                      :CONTRACNO,
                                      :AGEN_NO,
                                      :PO_QTY,
                                      :PO_PRICE,
                                      'N',
                                      :M_PURUN,
                                      :PO_AMT,
                                      :WH_NO,
                                      :M_DISCPERC,
                                      :UNIT_SWAP,
                                      :UPRICE,
                                      :DISC_CPRICE,
                                      :DISC_UPRICE,
                                      SYSDATE,
                                      :CREATE_USER,
                                      SYSDATE,
                                      :UPDATE_USER,
                                      :UPDATE_IP,
                                      :TRANSKIND,
                                      :IFLAG,
                                      :ISWILLING,
                                      :DISCOUNT_QTY,
                                      :DISC_COST_UPRICE)";

            return DBWork.Connection.Execute(sql, new
            {
                SEQ = SEQ,
                PURDATE = PURDATE,
                PO_NO = PO_NO,
                MMCODE = MMCODE,
                E_PURTYPE = E_PURTYPE,
                CONTRACNO = CONTRACNO,
                AGEN_NO = AGEN_NO,
                PO_QTY = PO_QTY,
                PO_PRICE = PO_PRICE,
                STATUS = STATUS,
                M_PURUN = M_PURUN,
                PO_AMT = PO_AMT,
                WH_NO = WH_NO,
                M_DISCPERC = M_DISCPERC,
                UNIT_SWAP = UNIT_SWAP,
                UPRICE = UPRICE,
                DISC_CPRICE = DISC_CPRICE,
                DISC_UPRICE = DISC_UPRICE,
                CREATE_USER = CREATE_USER,
                UPDATE_USER = UPDATE_USER,
                UPDATE_IP = UPDATE_IP,
                TRANSKIND = TRANSKIND,
                IFLAG = IFLAG,
                ISWILLING,
                DISCOUNT_QTY,
                DISC_COST_UPRICE
            }, DBWork.Transaction);
        }

        //退貨_1
        public int Back_1(string PO_NO, string MMCODE, string SEQ, string UPDATE_NAME, string UPDATE_IP)
        {
            var p = new DynamicParameters();

            var sql = @"INSERT INTO MM_PO_INREC
                                    (PURDATE,
                                    PO_NO,
                                    MMCODE,
                                    SEQ,
                                    E_PURTYPE,
                                    CONTRACNO,
                                    AGEN_NO,
                                    PO_QTY,
                                    PO_PRICE,
                                    M_PURUN,
                                    PO_AMT,
                                    WH_NO,
                                    M_DISCPERC,
                                    UNIT_SWAP,
                                    UPRICE,
                                    DISC_CPRICE,
                                    DISC_UPRICE,
                                    ACCOUNTDATE,
                                    LOT_NO,
                                    EXP_DATE,
                                    CREATE_TIME,
                                    CREATE_USER,
                                    UPDATE_TIME,
                                    UPDATE_USER,
                                    UPDATE_IP,
                                    TRANSKIND,
                                    DELI_QTY,
                                    STATUS,
                                    IFLAG,
                                    FROMSEQ,
                                    ISWILLING,
                                    DISCOUNT_QTY,
                                    DISC_COST_UPRICE)
                        SELECT MPC.PURDATE,
                               MPC.PO_NO,
                               MPC.MMCODE,
                               MM_PO_INREC_SEQ.NEXTVAL,
                               MPC.E_PURTYPE,
                               MPC.CONTRACNO,
                               MPC.AGEN_NO,
                               MPC.PO_QTY,
                               MPC.PO_PRICE,
                               MPC.M_PURUN,
                               MPC.PO_AMT,
                               MPC.WH_NO,
                               MPC.M_DISCPERC,
                               MPC.UNIT_SWAP,
                               MPC.UPRICE,
                               MPC.DISC_CPRICE,
                               MPC.DISC_UPRICE,
                               MPC.ACCOUNTDATE,
                               MPC.LOT_NO,
                               MPC.EXP_DATE,
                               SYSDATE,
                               :UPDATE_NAME,
                               SYSDATE,
                               :UPDATE_NAME,
                               :UPDATE_IP,
                               '120',
                               MPC.DELI_QTY,
                               'Y',
                               'Y',
                               MPC.SEQ,
                               MPC.ISWILLING,
                               MPC.DISCOUNT_QTY,
                               MPC.DISC_COST_UPRICE
                        FROM MM_PO_INREC MPC
                        WHERE MPC.PO_NO = :PO_NO
                        AND MPC.MMCODE = :MMCODE
                        AND MPC.SEQ = :SEQ";

            p.Add(":PO_NO", string.Format("{0}", PO_NO));
            p.Add(":MMCODE", string.Format("{0}", MMCODE));
            p.Add(":SEQ", string.Format("{0}", SEQ));
            p.Add(":UPDATE_NAME", string.Format("{0}", UPDATE_NAME));
            p.Add(":UPDATE_IP", string.Format("{0}", UPDATE_IP));

            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        //退貨_2
        public int Back_2(string PO_NO, string MMCODE, string SEQ, string UPDATE_NAME, string UPDATE_IP)
        {
            var p = new DynamicParameters();

            var sql = @"INSERT INTO MM_PO_INREC
                                    (PURDATE,
                                    PO_NO,
                                    MMCODE,
                                    SEQ,
                                    E_PURTYPE,
                                    CONTRACNO,
                                    AGEN_NO,
                                    PO_QTY,
                                    PO_PRICE,
                                    M_PURUN,
                                    PO_AMT,
                                    WH_NO,
                                    M_DISCPERC,
                                    UNIT_SWAP,
                                    UPRICE,
                                    DISC_CPRICE,
                                    DISC_UPRICE,
                                    -- ACCOUNTDATE,
                                    LOT_NO,
                                    EXP_DATE,
                                    CREATE_TIME,
                                    CREATE_USER,
                                    UPDATE_TIME,
                                    UPDATE_USER,
                                    UPDATE_IP,
                                    TRANSKIND,
                                    DELI_QTY,
                                    STATUS,
                                    IFLAG,
                                    FROMSEQ,
                                    ISWILLING,
                                    DISCOUNT_QTY,
                                    DISC_COST_UPRICE)
                        SELECT MPC.PURDATE,
                               MPC.PO_NO,
                               MPC.MMCODE,
                               MM_PO_INREC_SEQ.NEXTVAL,
                               MPC.E_PURTYPE,
                               MPC.CONTRACNO,
                               MPC.AGEN_NO,
                               MPC.PO_QTY,
                               MPC.PO_PRICE,
                               MPC.M_PURUN,
                               MPC.PO_AMT,
                               MPC.WH_NO,
                               MPC.M_DISCPERC,
                               MPC.UNIT_SWAP,
                               MPC.UPRICE,
                               MPC.DISC_CPRICE,
                               MPC.DISC_UPRICE,
                              -- MPC.ACCOUNTDATE,
                               MPC.LOT_NO,
                               MPC.EXP_DATE,
                               SYSDATE,
                               :UPDATE_NAME,
                               SYSDATE,
                               :UPDATE_NAME,
                               :UPDATE_IP,
                               '111',
                               0,
                               'N',
                               'Y',
                               MPC.SEQ,
                               MPC.ISWILLING,
                               MPC.DISCOUNT_QTY,
                               MPC.DISC_COST_UPRICE
                        FROM MM_PO_INREC MPC
                        WHERE MPC.PO_NO = :PO_NO
                        AND MPC.MMCODE = :MMCODE
                        AND MPC.SEQ = :SEQ";

            p.Add(":PO_NO", string.Format("{0}", PO_NO));
            p.Add(":MMCODE", string.Format("{0}", MMCODE));
            p.Add(":SEQ", string.Format("{0}", SEQ));
            p.Add(":UPDATE_NAME", string.Format("{0}", UPDATE_NAME));
            p.Add(":UPDATE_IP", string.Format("{0}", UPDATE_IP));

            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        //退貨_3
        public int Back_3(string SEQ, string USER_NAME)
        {
            var p = new DynamicParameters();

            var sql = @"INSERT INTO BC_CS_ACC_LOG 
                                    (PO_NO,
                                     MMCODE,
                                     SEQ,
                                     AGEN_NO,
                                     LOT_NO,
                                     EXP_DATE,
                                     ACC_QTY,
                                     ACC_PURUN,
                                     CFM_QTY,
                                     STATUS,
                                     MEMO,
                                     ACC_TIME,
                                     ACC_USER,
                                     ACC_BASEUNIT,
                                     STOREID,
                                     PO_QTY,
                                     MAT_CLASS,
                                     UNIT_SWAP,
                                     WEXP_ID,
                                     FLAG)
                        SELECT MPC.PO_NO,
                               MPC.MMCODE,
                               BC_CS_ACC_LOG_SEQ.NEXTVAL,
                               MPC.AGEN_NO,
                               MPC.LOT_NO,
                               MPC.EXP_DATE,
                               - (MPC.DELI_QTY / MPC.UNIT_SWAP),
                               MPC.M_PURUN,
                               - (MPC.DELI_QTY / MPC.UNIT_SWAP),
                               'C',
                               '退貨',
                               SYSDATE,
                               :USER_NAME,
                               MMT.BASE_UNIT,
                               MMT.M_STOREID,
                               MPC.PO_QTY,
                               MMT.MAT_CLASS,
                               MPC.UNIT_SWAP,
                               MMT.WEXP_ID,
                               'B'
                        FROM MM_PO_INREC MPC, MI_MAST MMT
                        WHERE MPC.MMCODE = MMT.MMCODE
                        AND MPC.SEQ = :SEQ";

            p.Add(":SEQ", string.Format("{0}", SEQ));
            p.Add(":USER_NAME", string.Format("{0}", USER_NAME));

            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        //退貨_4
        public int Back_4(string PO_NO, string MMCODE, string SEQ, string UPDATE_USER, string UPDATE_IP)
        {
            var p = new DynamicParameters();

            var sql = @"UPDATE MM_PO_INREC
                        SET STATUS = 'E',
                            UPDATE_IP = :UPDATE_IP,
                            UPDATE_USER = :UPDATE_USER,
                            UPDATE_TIME = SYSDATE
                        WHERE PO_NO = :PO_NO
                        AND MMCODE = :MMCODE
                        AND SEQ = :SEQ";

            p.Add(":PO_NO", string.Format("{0}", PO_NO));
            p.Add(":MMCODE", string.Format("{0}", MMCODE));
            p.Add(":SEQ", string.Format("{0}", SEQ));
            p.Add(":UPDATE_IP", string.Format("{0}", UPDATE_IP));
            p.Add(":UPDATE_USER", string.Format("{0}", UPDATE_USER));

            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        //退貨_5/T1T2進貨_3(呼叫STORE PROCEDURE: INV_SET.PO_DOCIN )
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

        //T1進貨_1
        public int T1in_pur_1(string SEQ, string USER_ID)
        {
            var p = new DynamicParameters();

            var sql = @"INSERT INTO BC_CS_ACC_LOG
                                    (PO_NO,
                                     MMCODE,
                                     SEQ,
                                     AGEN_NO,
                                     LOT_NO,
                                     EXP_DATE,
                                     ACC_QTY,
                                     ACC_PURUN,
                                     CFM_QTY,
                                     STATUS,
                                     MEMO,
                                     ACC_TIME,
                                     ACC_USER,
                                     ACC_BASEUNIT,
                                     STOREID,
                                     PO_QTY,
                                     MAT_CLASS,
                                     UNIT_SWAP,
                                     WEXP_ID,
                                     FLAG)
                        SELECT MPI.PO_NO,
                               MPI.MMCODE,
                               BC_CS_ACC_LOG_SEQ.NEXTVAL,
                               MPI.AGEN_NO,
                               MPI.LOT_NO,
                               MPI.EXP_DATE,
                               (MPI.DELI_QTY / MPI.UNIT_SWAP),
                               MPI.M_PURUN,
                               (MPI.DELI_QTY / MPI.UNIT_SWAP),
                               'C',
                               '進貨',
                               SYSDATE,
                               :USER_ID,
                               MMT.BASE_UNIT,
                               MMT.M_STOREID,
                               MPI.PO_QTY,
                               MMT.MAT_CLASS,
                               MPI.UNIT_SWAP,
                               MMT.WEXP_ID,
                               'B'
                        FROM MM_PO_INREC MPI, MI_MAST MMT
                        WHERE MPI.MMCODE = MMT.MMCODE
                        AND MPI.SEQ = :SEQ";

            p.Add(":SEQ", string.Format("{0}", SEQ));
            p.Add(":USER_ID", string.Format("{0}", USER_ID));

            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        //T1進貨_2
        public int T1in_pur_2(string PO_NO, string MMCODE, string SEQ)
        {
            var p = new DynamicParameters();

            var sql = @"UPDATE MM_PO_INREC
                        SET   STATUS = 'Y',
                              ACCOUNTDATE = SYSDATE
                        WHERE PO_NO = :PO_NO
                        AND   MMCODE = :MMCODE
                        AND   SEQ = :SEQ";

            p.Add(":PO_NO", string.Format("{0}", PO_NO));
            p.Add(":MMCODE", string.Format("{0}", MMCODE));
            p.Add(":SEQ", string.Format("{0}", SEQ));

            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        //T2進貨_1
        public int T2in_pur_1(string PO_NO, string MMCODE, string SEQ)
        {
            var p = new DynamicParameters();

            var sql = @"UPDATE MM_PO_INREC
                        SET STATUS = 'Y',
                            ACCOUNTDATE = SYSDATE
                        WHERE PO_NO = :PO_NO
                        AND MMCODE = :MMCODE
                        AND SEQ = :SEQ";

            p.Add(":PO_NO", string.Format("{0}", PO_NO));
            p.Add(":MMCODE", string.Format("{0}", MMCODE));
            p.Add(":SEQ", string.Format("{0}", SEQ));

            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        //T2進貨_2
        public int T2in_pur_2(string SEQ, string USER_ID)
        {
            var p = new DynamicParameters();

            var sql = @"INSERT INTO BC_CS_ACC_LOG
                                    (PO_NO,
                                     MMCODE,
                                     SEQ,
                                     AGEN_NO,
                                     LOT_NO,
                                     EXP_DATE,
                                     ACC_QTY,
                                     ACC_PURUN,
                                     CFM_QTY,
                                     STATUS,
                                     MEMO,
                                     ACC_TIME,
                                     ACC_USER,
                                     ACC_BASEUNIT,
                                     STOREID,
                                     PO_QTY,
                                     MAT_CLASS,
                                     UNIT_SWAP,
                                     WEXP_ID,
                                     FLAG)
                        SELECT MPI.PO_NO,
                               MPI.MMCODE,
                               BC_CS_ACC_LOG_SEQ.NEXTVAL,
                               MPI.AGEN_NO,
                               MPI.LOT_NO,
                               MPI.EXP_DATE,
                               (MPI.DELI_QTY / MPI.UNIT_SWAP),
                               MPI.M_PURUN,
                               (MPI.DELI_QTY / MPI.UNIT_SWAP),
                               'C',
                               '下個月入帳',
                               SYSDATE,
                               :USER_ID,
                               MMT.BASE_UNIT,
                               MMT.M_STOREID,
                               MPI.PO_QTY,
                               MMT.MAT_CLASS,
                               MPI.UNIT_SWAP,
                               MMT.WEXP_ID,
                               'B'
                        FROM MM_PO_INREC MPI, MI_MAST MMT
                        WHERE MPI.MMCODE = MMT.MMCODE
                        AND MPI.SEQ = :SEQ";

            p.Add(":SEQ", string.Format("{0}", SEQ));
            p.Add(":USER_ID", string.Format("{0}", USER_ID));

            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }
        //先檢查[進貨量]是否超過[訂單量], 多人操作畫面沒有即時更新,可能造成進貨重覆
        public string ChkDELI_QTY(string seq)
        {
            string ret = "Y";
            var p = new DynamicParameters();
            var sql = @"select  a.po_qty, a.deli_qty, nvl(sum_qty,0) sum_qty from 
                        (select po_no, mmcode, po_qty, deli_qty from MM_PO_INREC where seq=:seq) a, 
                        (select po_no, mmcode, sum(nvl(deli_qty,0)) sum_qty from MM_PO_INREC where po_no||mmcode =(select po_no||mmcode from mm_po_inrec where seq=:seq)
                          and transkind='111' and status='Y'   group by po_no, mmcode ) b
                        where a.po_no =b.po_no(+) and a.mmcode=b.mmcode(+)  ";
            p.Add(":seq", seq);
            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);
            if (Int32.Parse(dt.Rows[0]["PO_QTY"].ToString()) < Int32.Parse(dt.Rows[0]["DELI_QTY"].ToString()) + Int32.Parse(dt.Rows[0]["SUM_QTY"].ToString()))
                ret = "N";
            return ret;
        }

        public string MM_PO_INREC_CHK_SUBMIT(string i_po_no, string i_mmcode, string i_seq, string i_userid, string i_ip)
        {
            var p = new OracleDynamicParameters();
            p.Add("i_po_no", value: i_po_no, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 21);
            p.Add("i_mmcode", value: i_mmcode, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);
            p.Add("i_seq", value: i_seq, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 10);
            p.Add("i_userid", value: i_userid, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 10);
            p.Add("i_ip", value: i_ip, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);

            p.Add("ret_code", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 10);

            DBWork.Connection.Query("MM_PO_INREC_CHK_SUBMIT", p, commandType: CommandType.StoredProcedure);
            string ret_code = p.Get<OracleString>("ret_code").Value;
            return ret_code;
        }
        public DataTable GetExcel(string WH_NO, string AGEN_NO, string PURDATE, string PURDATE_1, string KIND)
        {
            var p = new DynamicParameters();

            var sql = @"  SELECT (CASE WHEN MMT.E_VACCINE = 'Y' THEN 'Y' ELSE '' END) as 疫苗,
                                 MPC.CONTRACNO as 合約碼,
                                 (case when MPC.E_PURTYPE='1' then '甲' when MPC.E_PURTYPE='2' then '乙' else '' end) as 案別,
                                 MPC.PURDATE as 採購日期,
                                 MPC.PO_NO as 採購單號,
                                 (SELECT PVR.AGEN_NO || ' ' || PVR.AGEN_NAMEC
                                  FROM PH_VENDER PVR
                                  WHERE PVR.AGEN_NO = MPC.AGEN_NO) as 廠商,
                                 MPC.MMCODE as 院內碼,
                                 MMT.MMNAME_E as 英文品名,
                                 TO_CHAR (MPC.ACCOUNTDATE, 'YYYYMMDD') - 19110000 as 進貨日期,
                                 MPC.PO_QTY as 採購量,
                                 MPC.DELI_QTY as 進貨量,
                                 (CASE
                                      WHEN MPC.STATUS = 'Y' OR MPC.STATUS = 'E' THEN 'Y'
                                      ELSE ''
                                  END) 進貨,
                                 (CASE
                                      WHEN MPC.STATUS = 'E' THEN 'Y'
                                      ELSE ''
                                  END) as 退貨,
                                 MPC.PO_PRICE as 價格,
                                 round(MPC.PO_AMT) as 總金額,
                                 MPC.M_PURUN as 進貨單位,
                                 MPC.LOT_NO as 批號,
                                 TO_CHAR ( MPC.EXP_DATE, 'YYYY/MM/DD') as 效期,
                                 MPC.MEMO as 備註
                          FROM MI_MAST MMT, MM_PO_INREC MPC
                          WHERE     MMT.MMCODE = MPC.MMCODE
                          AND MPC.TRANSKIND = '111'
                          AND MPC.WH_NO = :WH_NO
                          AND MPC.PURDATE BETWEEN :PURDATE AND :PURDATE_1 ";

            p.Add(":WH_NO", string.Format("{0}", WH_NO));
            p.Add(":PURDATE", string.Format("{0}", PURDATE));
            p.Add(":PURDATE_1", string.Format("{0}", PURDATE_1));
            if (AGEN_NO == "0")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '001' AND '100'";
            }
            else if (AGEN_NO == "1")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '101' AND '200'";
            }
            else if (AGEN_NO == "2")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '201' AND '300'";
            }
            else if (AGEN_NO == "3")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '301' AND '400'";
            }
            else if (AGEN_NO == "4")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '401' AND '500'";
            }
            else if (AGEN_NO == "5")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '501' AND '600'";
            }
            else if (AGEN_NO == "6")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '601' AND '700'";
            }
            else if (AGEN_NO == "7")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '701' AND '800'";
            }
            else if (AGEN_NO == "8")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '801' AND '900'";
            }
            else if (AGEN_NO == "9")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '901' AND '999'";
            }

            if (KIND == "")
            {
                sql += @" AND MPC.STATUS IN ('N', 'T', 'Y', 'E')";
            }
            else if (KIND == "N")
            {
                sql += @" AND MPC.STATUS IN ('N', 'T')";
            }
            else if (KIND == "Y")
            {
                sql += @" AND MPC.STATUS IN ('Y' ,'E')";
            }

            sql += @" ORDER BY MPC.AGEN_NO, MMT.MMNAME_E, MPC.MMCODE";
            
            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        public DataTable Report(string WH_NO, string AGEN_NO, string PURDATE, string PURDATE_1, string KIND)
        {
            var p = new DynamicParameters();

            var sql = @"  SELECT (CASE WHEN MMT.E_VACCINE = 'Y' THEN 'Y' ELSE '' END) E_VACCINE,
                                 MPC.CONTRACNO,
                                 (case when MPC.E_PURTYPE='1' then '甲' when MPC.E_PURTYPE='2' then '乙' else '' end) as E_PURTYPE,
                                 MPC.PURDATE,
                                 MPC.PO_NO,
                                 (SELECT PVR.AGEN_NO || ' ' || PVR.EASYNAME
                                  FROM PH_VENDER PVR
                                  WHERE PVR.AGEN_NO = MPC.AGEN_NO) AGEN_NO_NAME,
                                 MPC.MMCODE,
                                 MMT.MMNAME_E,
                                 MPC.PO_QTY,
                                 MPC.DELI_QTY,
                                 (CASE
                                      WHEN MPC.STATUS = 'Y' OR MPC.STATUS = 'E' THEN 'Y'
                                      ELSE ''
                                  END) INFLAG,
                                 (CASE
                                      WHEN MPC.STATUS = 'E' THEN 'Y'
                                      ELSE ''
                                  END) as OUTFLAG,
                                 MPC.PO_PRICE,
                                 (CASE WHEN MPC.STATUS = 'Y' OR MPC.STATUS = 'E' THEN MPC.DELI_QTY*MPC.PO_PRICE
                                  WHEN MPC.STATUS = 'E' THEN -1*MPC.DELI_QTY*MPC.PO_PRICE else 0 END ) PO_AMT,
                                 MPC.M_PURUN,
                                 MPC.LOT_NO,
                                 TO_CHAR ( MPC.EXP_DATE, 'YYYY/MM/DD') EXP_DATE,
                                 MPC.MEMO
                          FROM MI_MAST MMT, MM_PO_INREC MPC
                          WHERE     MMT.MMCODE = MPC.MMCODE
                          AND MPC.TRANSKIND = '111'
                          AND MPC.WH_NO = :WH_NO
                          AND MPC.PURDATE BETWEEN :PURDATE AND :PURDATE_1 ";

            p.Add(":WH_NO", string.Format("{0}", WH_NO));
            p.Add(":PURDATE", string.Format("{0}", PURDATE));
            p.Add(":PURDATE_1", string.Format("{0}", PURDATE_1));

            if (AGEN_NO == "0")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '001' AND '100'";
            }
            else if (AGEN_NO == "1")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '101' AND '200'";
            }
            else if (AGEN_NO == "2")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '201' AND '300'";
            }
            else if (AGEN_NO == "3")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '301' AND '400'";
            }
            else if (AGEN_NO == "4")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '401' AND '500'";
            }
            else if (AGEN_NO == "5")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '501' AND '600'";
            }
            else if (AGEN_NO == "6")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '601' AND '700'";
            }
            else if (AGEN_NO == "7")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '701' AND '800'";
            }
            else if (AGEN_NO == "8")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '801' AND '900'";
            }
            else if (AGEN_NO == "9")
            {
                sql += @" AND MPC.AGEN_NO BETWEEN '901' AND '999'";
            }

            if (KIND == "")
            {
                sql += @" AND MPC.STATUS IN ('N', 'T', 'Y', 'E')";
            }
            else if (KIND == "N")
            {
                sql += @" AND MPC.STATUS IN ('N', 'T')";
            }
            else if (KIND == "Y")
            {
                sql += @" AND MPC.STATUS IN ('Y' ,'E')";
            }
            sql += @" ORDER BY MPC.AGEN_NO, MMT.MMNAME_E, MPC.MMCODE";
            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public MILMED_JBID_LIST GetMILMED_JBID_LIST(string mmcode) {
            string sql = @"
               select a.MMCODE, b.ISWILLING, b.DISCOUNT_QTY, b.DISC_COST_UPRICE
                 from MI_MAST a, MILMED_JBID_LIST b
                where substr(a.E_YRARMYNO,1,3)=b.JBID_STYR
                  and a.E_ITEMARMYNO=b.BID_NO
                  and b.ISWILLING='是' and a.MAT_CLASS='01'
                  and a.mmcode = :mmcode
            ";
            return DBWork.Connection.QueryFirstOrDefault<MILMED_JBID_LIST>(sql, new { mmcode }, DBWork.Transaction);
        }

        public bool CheckStatusNExists(string po_no, string mmcode) {
            string sql = @"
                select 1 from MM_PO_INREC
                 where po_no = :po_no
                   and mmcode = :mmcode
                   and status = 'N'
            ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { po_no, mmcode }, DBWork.Transaction) == null);
        }


        #region 2022-01-22 新增 訂單進貨資料 頁面：查詢MM_PO_INREC訂單進貨資料檔 所有資料
        public IEnumerable<CC0003> GetAll_3(string WH_NO, string AGEN_NO, string PURDATE, string PURDATE_1, string KIND, string mmcode) {
            var p = new DynamicParameters();
            string sql = @"
                select (case when (select E_VACCINE from MI_MAST
                                     where MMCODE=b.MMCODE) = 'Y'
                              then 'Y' else ''
                        end) VACCINE,  --疫苗
                        b.CONTRACNO,  --合約碼
                        (case when b.E_PURTYPE='1' then '甲'
                              when b.E_PURTYPE='2' then '乙'
                              else ''
                         end) as E_PURTYPE,  --案別
                        b.PURDATE, b.PO_NO, --採購日期,採購單號
                        (select c.AGEN_NO || ' ' || c.AGEN_NAMEC
                           from PH_VENDER c
                          where c.AGEN_NO = b.AGEN_NO) AGEN_NO_NAME,  --廠商代碼
                        b.MMCODE,  --院內碼
                        (select MMNAME_C from MI_MAST where MMCODE=b.MMCODE) as MMNAME_C,  --中文品名
                        (select MMNAME_E from MI_MAST where MMCODE=b.MMCODE) as MMNAME_E,  --英文品名
                        to_char(b.ACCOUNTDATE, 'YYYYMMDD') - 19110000 ACCOUNTDATE_REF,   --進貨日期
                        b.PO_QTY,  --採購量
                        (b.DELI_QTY / b.UNIT_SWAP) DELI_QTY,  --進貨量
                        (case when b.STATUS = 'Y' OR b.STATUS = 'E' then '進貨'
                              else ''
                         end) INFLAG,   --進貨
                        (case when b.STATUS = 'E' then '退貨'
                              else ''
                         end) OUTFLAG,  --退貨
                        b.M_PURUN, b.LOT_NO,  --進貨單位,批號
                        to_char(b.EXP_DATE, 'YYYYMMDD') - 19110000 EXP_DATE_REF,  --效期
                        b.MEMO,   --備註
                        b.WH_NO,  --庫房別 
                        (case when b.STATUS='D' then 'D 作廢'
                              when b.STATUS='N' then 'N 未確認'
                              when b.STATUS='T' then 'T 已傳送'
                              when b.STATUS='Y' then 'Y 已確認'
                              when b.STATUS='E' then 'E 退貨'
                              else b.STATUS
                         end) STATUS,  --狀態
                        b.SEQ,         --流水號
                        (case when b.TRANSKIND='111'
                              then '111 進貨' else '120 退貨回廠商'
                        end) as TRANSKIND  --異動類別
                   from MM_PO_INREC b
                  where 1=1
                  --庫別
                    and b.transkind = '111'
                    and b.WH_NO = :WH_NO
                    and b.PURDATE between :PURDATE and :PURDATE_1
            ";
            if (string.IsNullOrEmpty(AGEN_NO) == false) {
                sql += string.Format(" and b.agen_no between '{0}01' and '{0}99'", AGEN_NO);
            }

            if (string.IsNullOrEmpty(mmcode) == false) {
                sql += " and b.mmcode = :mmcode";
            }

            if (KIND == "")
            {
                sql += @"  and b.STATUS IN ('N', 'T', 'Y', 'E')";
            }
            else if (KIND == "N")
            {
                sql += @" and b.STATUS IN ('N', 'T')";
            }
            else if (KIND == "Y")
            {
                sql += @" and b.STATUS IN ('Y' ,'E')";
            }

            p.Add(":WH_NO", string.Format("{0}", WH_NO));
            p.Add(":PURDATE", string.Format("{0}", PURDATE));
            p.Add(":PURDATE_1", string.Format("{0}", PURDATE_1));
            p.Add(":mmcode", string.Format("{0}", mmcode));

            return DBWork.PagingQuery<CC0003>(sql, p, DBWork.Transaction);
        }
        #endregion
    }
}
