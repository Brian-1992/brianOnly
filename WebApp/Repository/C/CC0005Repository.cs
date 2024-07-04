using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Types;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace WebApp.Repository.C
{
    public class CC0005Repository : JCLib.Mvc.BaseRepository
    {
        public CC0005Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public int Create(CC0002D cc0002d)
        {
            var sql = @" insert into BC_CS_ACC_LOG 
                                (PO_NO, MMCODE, SEQ, AGEN_NO, WEXP_ID, LOT_NO, EXP_DATE, PO_QTY, ACC_QTY, CFM_QTY, ACC_PURUN, ACC_BASEUNIT, UNIT_SWAP, STATUS, MEMO, ACC_TIME, ACC_USER, STOREID, MAT_CLASS, NEXTMON, INVOICE) 
                                select :PO_NO, :MMCODE, :SEQ, :AGEN_NO, b.WEXP_ID, trim(:LOT_NO),
                                case when to_char(:EXP_DATE, 'yyyy/mm/dd') = '0001/01/01' then null else to_date(to_char(:EXP_DATE, 'yyyy/mm/dd'), 'yyyy/mm/dd') end, 
                                a.PO_QTY, :INQTY, :INQTY, a.M_PURUN, b.BASE_UNIT, a.UNIT_SWAP, :STATUS, '', sysdate, :ACC_USER, b.M_STOREID, b.MAT_CLASS, :NEXTMON, :INVOICE 
                                from MM_PO_D a, MI_MAST b
                                where a.MMCODE=b.MMCODE and a.PO_NO=:PO_NO and a.MMCODE=:MMCODE ";
            return DBWork.Connection.Execute(sql, cc0002d, DBWork.Transaction);
        }

        public int UpdatePhReply(CC0002D cc0002d)
        {
            var sql = @" update PH_REPLY set STATUS = 'R', UPDATE_TIME = SYSDATE, UPDATE_USER = :ACC_USER
                                where PO_NO=:PO_NO and AGEN_NO=:AGEN_NO and MMCODE=:MMCODE
                                and LOT_NO = trim(:LOT_NO) and to_char(EXP_DATE, 'yyyy/mm/dd') = to_char(:EXP_DATE, 'yyyy/mm/dd')
                                and SEQ = :PH_REPLY_SEQ "; // 同PO_NO,MMCODE,LOT_NO,EXP_DATE可能會有多筆(INVOICE不同之類的),條件需再加上SEQ
            return DBWork.Connection.Execute(sql, cc0002d, DBWork.Transaction);
        }

        // 若PH_REPLY有對應到資料則取資料
        public IEnumerable<COMBO_MODEL> GetPh_ReplyCombo(string mmcode)
        {
            var p = new DynamicParameters();

            var sql = @"select PO_NO as EXTRA1, AGEN_NO, MMCODE as EXTRA2, SEQ as VALUE, 
                       (select TWN_DATE(PO_TIME) from MM_PO_M where PO_NO=PH_REPLY.PO_NO) as TEXT
                                from PH_REPLY 
                                where (MMCODE= trim(:p0)  
                                       or MMCODE= (select MMCODE from BC_BARCODE where BARCODE=trim(:p0) and status='Y' and rownum=1) ) 
                                and STATUS='B' 
                                and INQTY > 0
                                and EXISTS (SELECT 1 FROM MM_PO_INREC where MMCODE=PH_REPLY.MMCODE and STATUS in ('N','T') and PO_NO=PH_REPLY.PO_NO 
                                    and sysdate-create_time <=90 ) --針對未進貨, 避免廠商重覆傳資料
                                and LOT_NO is not null and EXP_DATE is not null
                                and sysdate-create_time <=14
                                order by create_time desc ";
            p.Add(":p0", mmcode);

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }

        // 若BC_BARCODE有對應到資料則取資料
        public IEnumerable<COMBO_MODEL> GetBc_BarcodeCombo(string mmcode)
        {
            var p = new DynamicParameters();

            var sql = @"select PO_NO as EXTRA1, AGEN_NO, MMCODE as EXTRA2, SEQ as VALUE, 
                       (select TWN_DATE(PO_TIME) from MM_PO_M where PO_NO=PH_REPLY.PO_NO) as TEXT
                                from PH_REPLY 
                                where MMCODE= trim(:p0) and STATUS='B' 
                                and INQTY > 0
                                and EXISTS (SELECT 1 FROM MM_PO_INREC where MMCODE=PH_REPLY.MMCODE and STATUS in ('N','T') and PO_NO=PH_REPLY.PO_NO 
                                and sysdate-create_time <=90 ) --針對未進貨, 避免廠商重覆傳資料
                                and LOT_NO is not null and EXP_DATE is not null
                                and sysdate-create_time <=14
                                order by create_time desc ";
            p.Add(":p0", mmcode);

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }
        // 若BC_BARCODE有對應到資料則取資料
        public IEnumerable<COMBO_MODEL> GetInrecCombo(string mmcode)
        {
            var p = new DynamicParameters();

            var sql = @" select po_no as EXTRA1, mmcode as EXTRA2, substr(po_no,1,7) as TEXT, 'INREC' as VALUE, wh_no as COMBITEM  from MM_PO_INREC where status in ('N','T') and transkind='111' and deli_qty =0 and mmcode=trim(:p0)
                                and create_time =(select max(create_time) from MM_PO_INREC where status in ('N','T') and transkind='111' and deli_qty =0 and mmcode=trim(:p0))";
            p.Add(":p0", mmcode);

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }

        // 取得資料以填入Form
        public IEnumerable<CC0002D> GetLotNoFormDataPhReply(string po_no, string mmcode, string seq)
        {
            var p = new DynamicParameters();

            var sql = @"select b.PO_NO, b.AGEN_NO, 
                                (select AGEN_NAMEC from PH_VENDER where AGEN_NO=b.AGEN_NO) as AGEN_NAME,
                                a.MMCODE, a.MMNAME_C, a.MMNAME_E, b.LOT_NO, b.EXP_DATE,
                                (select PO_QTY from MM_PO_D where PO_NO=:p0 and MMCODE=:p1) as PO_QTY, b.INQTY, a.BASE_UNIT, b.INVOICE,
                                '' as WH_NO, '' as PURDATE
                                from MI_MAST a, PH_REPLY b
                                where a.MMCODE=:p1 and a.MMCODE=b.MMCODE and b.SEQ=:p2 and b.LOT_NO is not null and b.EXP_DATE is not null";
            p.Add(":p0", po_no);
            p.Add(":p1", mmcode);
            p.Add(":p2", seq);

            return DBWork.Connection.Query<CC0002D>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<CC0002D> GetLotNoFormDataInrec(string po_no, string mmcode)
        {
            var p = new DynamicParameters();

            var sql = @"select A.PO_NO, A.AGEN_NO, A.MMCODE, 
                                (select MMNAME_E from MI_MAST where MMCODE = A.MMCODE) as MMNAME_E, '' as LOT_NO, null as EXP_DATE, 
                                (select PO_QTY from MM_PO_D where PO_NO=A.PO_NO and MMCODE=A.MMCODE) as PO_QTY, '' as INQTY,
                                A.WH_NO, A.PURDATE
                                from MM_PO_INREC A
                                where A.PO_NO=:p0 and A.MMCODE=:p1 ";
            p.Add(":p0", po_no);
            p.Add(":p1", mmcode);

            return DBWork.Connection.Query<CC0002D>(sql, p, DBWork.Transaction);
        }

        // 檢查PH_REPLY是否可對應到MMCODE
        public int ChkMmcodePhReply(string mmcode)
        {
            var p = new DynamicParameters();
            var sql = @" select count(*) from PH_REPLY 
                            where (MMCODE = trim(:p0) 
                                    or MMCODE= (select MMCODE from BC_BARCODE where BARCODE=trim(:p0) and status='Y' and rownum=1) ) 
                                and STATUS='B' 
                                and EXISTS (SELECT 1 FROM MM_PO_INREC where MMCODE=PH_REPLY.MMCODE and STATUS in ('N','T') and PO_NO=PH_REPLY.PO_NO ) --針對未進貨, 避免廠商重覆傳資料
                                and INQTY > 0";

            p.Add(":p0", mmcode);

            return DBWork.Connection.QueryFirst<int>(sql, p, DBWork.Transaction);
        }

        // 檢查BC_BARCODE是否可對應到MMCODE
        public string ChkMmcodeBarcode(string mmcode)
        {
            var p = new DynamicParameters();
            var sql = @" select case when (select count(*) from BC_BARCODE where BARCODE=trim(:p0)) > 0
                            then (select MMCODE from BC_BARCODE where (BARCODE=trim(:p0)) and rownum = 1)
                            else 'notfound' end as MMCODE
                            from dual ";

            p.Add(":p0", mmcode);

            return DBWork.Connection.QueryFirst<string>(sql, p, DBWork.Transaction);
        }

        public string getSeq()
        {
            var p = new DynamicParameters();
            var sql = @" select BC_CS_ACC_LOG_SEQ.nextval as SEQ
                            from dual ";

            return DBWork.Connection.QueryFirst<string>(sql, p, DBWork.Transaction);
        }

        public string procDocin(string PO_NO, string USER_ID, string USER_IP)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_PONO", value: PO_NO, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("I_USERID", value: USER_ID, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("I_UPDIP", value: USER_IP, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);
            p.Add("O_RETMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            DBWork.Connection.Query("INV_SET.PO_DOCIN", p, commandType: CommandType.StoredProcedure);
            string RetId = p.Get<OracleString>("O_RETID").Value;
            string RetMsg = p.Get<OracleString>("O_RETMSG").Value;

            if (RetId == "N")
                return "SP:" + RetMsg;
            else if (RetId == "Y")
                return "接收數量存檔..成功";
            else
                return "";
        }

        public string procPoInrec(string SEQ, string USER_ID, string USER_IP)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_SEQ", value: SEQ, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("I_USERID", value: USER_ID, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("I_IP", value: USER_IP, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);

            p.Add("RET_CODE", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            DBWork.Connection.Query("MM_PO_INREC_CC0005", p, commandType: CommandType.StoredProcedure);
            string RetCode = p.Get<OracleString>("RET_CODE").Value;

            return RetCode;
        }
        public int ChkReceived(string mmcode)  //檢查當天是否有接收相同院內碼
        {
            var p = new DynamicParameters();
            var sql = @" select count(*) from BC_CS_ACC_LOG
                         where MMCODE = trim(:p0)
                         and sysdate-ACC_TIME < 0.00005787 ";  // 0.00005787約 5秒
            p.Add(":p0", mmcode);
            return DBWork.Connection.QueryFirst<int>(sql, p, DBWork.Transaction);
        }
        public string ChkReplyStatus(CC0002D cc0002d)  //檢查 PH_REPLY[STATUS]='B'是否還存在, 解決重複進貨的問題
        {
            var sql = @" select STATUS from PH_REPLY
                         where seq= :PH_REPLY_SEQ and po_no=:PO_NO and mmcode=:MMCODE and agen_no=:AGEN_NO ";
            return DBWork.Connection.QueryFirst<string>(sql, cc0002d, DBWork.Transaction);
        }
        public int ChkAccLogExist(CC0002D cc0002d)  //檢查 BC_CS_ACC_LOG 是否已新增, 解決重複進貨的問題
        {
            var sql = @" select count(*) from BC_CS_ACC_LOG
                         where po_no= :PO_NO and mmcode= :MMCODE 
                         and lot_no= :LOT_NO and to_char(exp_date, 'yyyy/mm/dd') = to_char(:EXP_DATE, 'yyyy/mm/dd')
                         and sysdate - acc_time < 0.00067   
                         and acc_user= :ACC_USER ";  // 0.00067 = 1分鐘
            return DBWork.Connection.QueryFirst<int>(sql, cc0002d, DBWork.Transaction);
        }
        public string ChkDeli_Qty(CC0002D cc0002d)  //檢查 進貨數量是否超出
        {
            var repo = new CC0005Repository(DBWork);
            var ret = "N";
            if (repo.GetDELI_QTY(cc0002d) + Convert.ToInt32(cc0002d.ACC_QTY) > Convert.ToInt32(cc0002d.PO_QTY))
                ret = "FULL";
            return ret;
        }
        public int GetDELI_QTY(CC0002D cc0002d)  //檢查 table MM_PO_D[DELI_QTY] 是否會超過
        {
            var sql = @" select DELI_QTY from MM_PO_D
                         where po_no= :PO_NO and mmcode= :MMCODE ";
            return DBWork.Connection.QueryFirst<int>(sql, cc0002d, DBWork.Transaction);
        }
    }
}