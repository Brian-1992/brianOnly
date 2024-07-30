using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using Dapper;
using WebAppVen.Models;
using BarcodeLib;
using System.Drawing;
using System.Drawing.Imaging;
using System.Data;

namespace WebAppVen.Repository.B
{
    public class BH0008Repository : JCLib.Mvc.BaseRepository
    {
        public BH0008Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public DataTable GetExcel(string UserID, string PO_NO, string DNO)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"select @UserID 廠商編號, 
                            DNO 交貨批次, 
                            MMNAME_C 藥品中文名稱,
                            MMNAME_E 藥品英文名稱, 
                            convert(varchar, isnull(DELI_DT,GETDATE()), 112) 預計交貨日, 
                            INQTY 交貨數量, 
                            a.MMCODE 三總院內碼, 
                            LOT_NO 批號,
                            convert(varchar, EXP_DATE, 112) 效期, 
                            INVOICE 發票號碼, 
                            convert(varchar, isnull(INVOICE_DT,GETDATE()), 112) 發票日期, 
                            a.MEMO 備註
                            from WB_REPLY a, WB_MM_PO_D b 
                            where a.po_no=b.po_no 
                            and a.mmcode=b.mmcode  
                            and a.po_no=@PO_NO
                            and agen_no=@UserID
                            and DNO=@DNO
                            Order by a.mmcode";

            p.Add("@UserID", UserID);
            p.Add("@PO_NO", PO_NO);
            p.Add("@DNO", DNO);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public bool CheckMmcodeExists(string po_no, string mmcode)

        {
            string sql = @"select 1 from  WB_MM_PO_D where po_no =@po_no
            and  mmcode=@mmcode";
            return !(DBWork.Connection.ExecuteScalar(sql, new { po_no = po_no, mmcode = mmcode }, DBWork.Transaction) == null);
        }

        public bool CheckStatus(string po_no, string userid)
        {
            string sql = @"select wexp_id from  WB_MM_PO_D where po_no=@po_no ";
            var str = DBWork.Connection.ExecuteScalar(sql, new { po_no = po_no }, DBWork.Transaction);
            if (str != null)
            {
                if (str.ToString() == "N")
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }
        public IEnumerable<WB_MM_PO_M> GetPoCombo(string agen_no)
        {
            var p = new DynamicParameters();

            var sql = @"select po_no from WB_MM_PO_M where 1=1 and AGEN_NO=@AGEN_NO and ( po_time >= GETDATE() -90 ) ORDER BY po_no DESC";

            return DBWork.Connection.Query<WB_MM_PO_M>(sql, new { AGEN_NO = agen_no }, DBWork.Transaction);
        }

        public IEnumerable<WB_REPLY> GetMmcodeCombo(string po_no)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT MMCODE FROM WB_MM_PO_D WHERE PO_NO=@PO_NO";

            return DBWork.Connection.Query<WB_REPLY>(sql, new { PO_NO = po_no }, DBWork.Transaction);
        }

        public IEnumerable<WB_MM_PO_M> GetPoMaster(string po_no, string agen_no, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = "";
            if (po_no.Substring(1, 3).IndexOf("INV,GEN,SML") > -1) //衛材
                sql = @"SELECT a.PO_NO, MMCODE, PO_QTY, PO_PRICE, M_PURUN, M_AGENLAB, floor(PO_PRICE*PO_QTY) as PO_AMT, M_DISCPERC, UNIT_SWAP, MMNAME_C, MMNAME_E
                        from WB_MM_PO_D a, WB_MM_PO_M b
                        where a.po_no=b.po_no and b.AGEN_NO= @AGEN_NO ";
            else  //藥品
                sql = @"SELECT a.PO_NO, MMCODE, PO_QTY, PO_PRICE, M_PURUN, M_AGENLAB, round(PO_PRICE*PO_QTY,0) as PO_AMT, M_DISCPERC, UNIT_SWAP, MMNAME_C, MMNAME_E
                        from WB_MM_PO_D a, WB_MM_PO_M b
                        where a.po_no=b.po_no and b.AGEN_NO= @AGEN_NO ";

            if (po_no != "")
            {
                sql += " and b.po_no=@PO_NO";
                p.Add("@PO_NO", string.Format("{0}", po_no));
            }

            p.Add("@AGEN_NO", agen_no);
            p.Add("@OFFSET", (page_index - 1) * page_size);
            p.Add("@PAGE_SIZE", page_size);

            return DBWork.Connection.Query<WB_MM_PO_M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //public IEnumerable<WB_REPLY> GetPoDetail(string po_no, string agen_no, string dno, int page_index, int page_size, string sorters)
        public IEnumerable<WB_REPLY> GetPoDetail(string po_no, string mmcode, string agen_no, string dno, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"Select SEQ, PO_NO, MMCODE, DNO 
                        , (select WEXP_ID from WB_MM_PO_D where po_no=a.PO_NO and mmcode=a.MMCODE) as WEXP_ID
                        , REPLACE(SUBSTRING(CONVERT(NVARCHAR, DELI_DT, 120),0,11),'-','')- 19110000 DELI_DT
                        , REPLACE(SUBSTRING(CONVERT(NVARCHAR, EXP_DATE, 120),0,11),'-','')- 19110000 EXP_DATE
                        , REPLACE(SUBSTRING(CONVERT(NVARCHAR, INVOICE_DT, 120),0,11),'-','')- 19110000 INVOICE_DT
                        , LOT_NO, INQTY, INVOICE, MEMO 
                        , (CASE WHEN STATUS = 'A' THEN '處理中'   WHEN STATUS = 'B' THEN '確認回傳' WHEN STATUS = 'C' THEN '三總接收' when STATUS = 'T' THEN '三總驗退' END) STATUS 
                        from WB_REPLY a";
            if (dno != "")
            {
                sql += " where po_no=@PO_NO and agen_no=@AGEN_NO and dno =@DNO ";
                p.Add("@DNO", dno);
            }
            else
            {
                sql += " where po_no=@PO_NO and agen_no=@AGEN_NO and mmcode =@MMCODE ";
                p.Add("@MMCODE", mmcode);
            }
            p.Add("@PO_NO", po_no);
            p.Add("@AGEN_NO", agen_no);
            p.Add("@OFFSET", (page_index - 1) * page_size);
            p.Add("@PAGE_SIZE", page_size);

            return DBWork.Connection.Query<WB_REPLY>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public int CreateAll(WB_REPLY wb_reply)
        {
            var sql = @"INSERT INTO WB_REPLY (PO_NO, AGEN_NO, MMCODE, DNO, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, INQTY)  
                                    SELECT PO_NO, @AGEN_NO, MMCODE, @DNO, getDate(), @CREATE_USER, getDate(), @UPDATE_USER, @UPDATE_IP, PO_QTY  FROM WB_MM_PO_D
                                    WHERE PO_NO=@PO_NO";
            return DBWork.Connection.Execute(sql, wb_reply, DBWork.Transaction);
        }

        public int CreateOne(WB_REPLY wb_reply)
        {
            var sql = @"INSERT INTO WB_REPLY (PO_NO, AGEN_NO, MMCODE, DNO, DELI_DT, LOT_NO, EXP_DATE, INQTY, INVOICE, INVOICE_DT, MEMO, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                    VALUES (@PO_NO, @AGEN_NO, @MMCODE, @DNO, @DELI_DT, @LOT_NO, @EXP_DATE, @INQTY, @INVOICE, @INVOICE_DT, @MEMO, getDate(), @CREATE_USER, getDate(), @UPDATE_USER, @UPDATE_IP)";
            return DBWork.Connection.Execute(sql, wb_reply, DBWork.Transaction);
        }

        public int ImportCreate(WB_REPLY wb_reply)
        {
            var sql = @"INSERT INTO WB_REPLY (PO_NO, AGEN_NO, MMCODE, DNO, DELI_DT, LOT_NO, EXP_DATE, INQTY, INVOICE, INVOICE_DT, MEMO, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, BARCODE)  
                        VALUES (@PO_NO, @AGEN_NO, @MMCODE, @DNO, @DELI_DT, @LOT_NO, @EXP_DATE,　 @INQTY, @INVOICE, @INVOICE_DT, @MEMO, getDate(),　 @CREATE_USER, 　getDate(),　 @UPDATE_USER, @UPDATE_IP, @BARCODE)";
            return DBWork.Connection.Execute(sql, wb_reply, DBWork.Transaction);
        }

        public string GetMaxDno(string po_no)
        {
            var sql = @"SELECT ISNULL(MAX(DNO), 0) + 1 AS MAXDNO FROM WB_REPLY WHERE PO_NO=@PO_NO";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { PO_NO = po_no }, DBWork.Transaction);
        }

        public int DetailUpdate(WB_REPLY wb_reply)
        {
            var sql = @"UPDATE WB_REPLY SET STATUS='A', FLAG='A', LOT_NO=@LOT_NO, INQTY=@INQTY, INVOICE=@INVOICE, MEMO=@MEMO, DELI_DT=@DELI_DT, EXP_DATE=@EXP_DATE, INVOICE_DT=@INVOICE_DT,
                        UPDATE_TIME=getDate(), UPDATE_USER=@UPDATE_USER, UPDATE_IP=@UPDATE_IP 
                        WHERE SEQ=@SEQ";
            return DBWork.Connection.Execute(sql, wb_reply, DBWork.Transaction);
        }

        public int DetailDelete(string seq)
        {
            var sql = @" DELETE from WB_REPLY WHERE SEQ=@SEQ";
            return DBWork.Connection.Execute(sql, new { SEQ = seq }, DBWork.Transaction);
        }

        public int DetailSubmit(WB_REPLY wb_reply)
        {
            var sql = @"UPDATE WB_REPLY SET STATUS='B', UPDATE_TIME=getDate(), UPDATE_USER=@UPDATE_USER, UPDATE_IP=@UPDATE_IP 
                        WHERE SEQ=@SEQ and STATUS='A' ";
            return DBWork.Connection.Execute(sql, wb_reply, DBWork.Transaction);
        }

        /// <summary>
        /// 搜尋報表資料
        /// </summary>
        /// <param name="PO_NO">訂單編號</param>
        /// <param name="DNO">交貨批次</param>
        /// <param name="AGEN_NO">廠商代碼</param>
        /// <param name="IsPO_OverLenth">訂單編號是否超過11碼，是=衛材，用中文名稱；反之藥材，用英文名稱</param>
        /// <returns></returns>
        public IEnumerable<WB_REPLY> SearchReportData(string PO_NO, string DNO, string AGEN_NO, bool IsPO_OverLenth)
        {
            var p = new DynamicParameters();

            var sql = "";
            if (IsPO_OverLenth)
            {
                if (PO_NO.Substring(1, 3).IndexOf("INV,GEN,SML") > -1) //衛材
                    sql = @"SELECT a.MMCODE, MMNAME_C MMNAME_C, M_PURUN, po_price, 
                        sum(INQTY) qty, sum(floor(INQTY * po_price)) tot 
                        from WB_REPLY a, WB_MM_PO_D b
                        where a.po_no=b.po_no and a.mmcode=b.mmcode    
                        and (INQTY > 0 )
                        and a.dno=@DNO  and a.agen_no=@AGEN_NO  and a.po_no=@PO_NO 
                        group by a.MMCODE, MMNAME_C, MMNAME_C, M_PURUN, po_price";
                else
                    sql = @"SELECT a.MMCODE, MMNAME_C MMNAME_C, M_PURUN, po_price, 
                        sum(INQTY) qty, sum(round(INQTY * po_price,0)) tot 
                        from WB_REPLY a, WB_MM_PO_D b
                        where a.po_no=b.po_no and a.mmcode=b.mmcode    
                        and (INQTY > 0 )
                        and a.dno=@DNO  and a.agen_no=@AGEN_NO  and a.po_no=@PO_NO 
                        group by a.MMCODE, MMNAME_C, MMNAME_C, M_PURUN, po_price";
            }
            else
            {
                if (PO_NO.Substring(1, 3).IndexOf("INV,GEN,SML") > -1) //衛材
                    sql = @"SELECT a.MMCODE, MMNAME_E MMNAME_C, M_PURUN, po_price, 
                        sum(INQTY) qty, sum(floor(INQTY * po_price)) tot 
                        from WB_REPLY a, WB_MM_PO_D b
                        where a.po_no=b.po_no and a.mmcode=b.mmcode    
                        and (INQTY > 0  )
                        and a.dno=@DNO  and a.agen_no=@AGEN_NO  and a.po_no=@PO_NO
                        group by a.MMCODE, MMNAME_E, MMNAME_C, M_PURUN, po_price";
                else
                    sql = @"SELECT a.MMCODE, MMNAME_E MMNAME_C, M_PURUN, po_price, 
                        sum(INQTY) qty, sum(round(INQTY * po_price,0)) tot 
                        from WB_REPLY a, WB_MM_PO_D b
                        where a.po_no=b.po_no and a.mmcode=b.mmcode    
                        and (INQTY > 0  )
                        and a.dno=@DNO  and a.agen_no=@AGEN_NO  and a.po_no=@PO_NO
                        group by a.MMCODE, MMNAME_E, MMNAME_C, M_PURUN, po_price";
            }

            p.Add("@PO_NO", PO_NO);
            p.Add("@DNO", DNO);
            p.Add("@AGEN_NO", AGEN_NO);

            return DBWork.Connection.Query<WB_REPLY>(sql, p);
        }

        public string GetPO_CON_NO(string PO_NO)
        {
            string sql = @"select case when m_contid='0' then '合約' else '零購' end po_no 
                            from WB_MM_PO_M  
                            where po_no = @PO_NO";

            return DBWork.Connection.QueryFirst<string>(sql, new { PO_NO = PO_NO }, DBWork.Transaction);
        }
        //檢查[交貨數量]是否超過[訂單數量]
        public string ChkINQTY(string seq)
        {
            string ret = "N";
            var p = new DynamicParameters();

            var sql = @"select CONVERT(int,a.po_qty) po_qty, CONVERT(int,sum(b.inqty)) as sum_inqty from wb_mm_po_d a, wb_reply b
                        where a.po_no=b.po_no and a.mmcode=b.mmcode 
                        and b.po_no+b.mmcode =(select po_no+mmcode from wb_reply where seq=@seq)
                        group by a.po_qty ";
            p.Add("@seq", seq);
            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);
            if (Int32.Parse(dt.Rows[0]["PO_QTY"].ToString()) >= Int32.Parse(dt.Rows[0]["SUM_INQTY"].ToString()) )
                ret = "Y";
            return ret;
        }
    }
}