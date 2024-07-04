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
    public class BD0010Repository : JCLib.Mvc.BaseRepository
    {
        public BD0010Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MM_PO_M> GetMasterAll(string WH_NO, string YYYYMMDD, string YYYYMMDD_E, string PO_STATUS, string Agen_No, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            string sql = @"SELECT  PO_NO, substr(PO_NO,1,7) as PO_DATE, WH_NO,
                           (select AGEN_NO||' '||AGEN_NAMEC from PH_VENDER where agen_no=M.AGEN_NO) as AGEN_NO,  CONTRACNO, MEMO, SMEMO,
                           (select TWN_TIME_FORMAT(max(back_dt)) from PH_MAILBACK where AGEN_NO=M.AGEN_NO and PO_NO=M.PO_NO) as REPLY_DT,
                           (select nvl2(po_no,'已回覆','') from PH_REPLY a where AGEN_NO=M.AGEN_NO and PO_NO=M.PO_NO and rownum=1) as REPLY_DELI,
                           (select data_desc from PARAM_D where  DATA_NAME='PO_STATUS' and DATA_VALUE=m.po_status) as PO_STATUS, (select count(*) from MM_PO_D where PO_NO=M.PO_NO) as CNT
                        FROM MM_PO_M M
                        where MAT_CLASS='01'
                        and substr(PO_NO,1,7) >=:YYYYMMDD and substr(PO_NO,1,7) <=:YYYYMMDD_E
                        and WH_NO =:WH_NO
                        ";

            p.Add(":YYYYMMDD", YYYYMMDD);
            p.Add(":YYYYMMDD_E", YYYYMMDD_E);
            p.Add(":WH_NO", WH_NO);

            switch (PO_STATUS)
            {
                case "0":
                    sql += " and PO_STATUS in ('80', '84') ";
                    break;
                case "1":
                    sql += " and PO_STATUS = '82'";
                    break;
                case "D":
                    sql += " and (PO_STATUS = '87' or  po_no in (select po_no from MM_PO_D where status='D')) ";
                    break;
            }

            if (Agen_No != "")
            {
                sql += " and agen_no=:agen_no ";
                p.Add(":agen_no", Agen_No);
            }
            //sql += " order by po_no, AGEN_NO ";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MM_PO_M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public int MasterUpdate(MM_PO_M mm_po_m)
        {
            var sql = @"UPDATE MM_PO_M
                           SET MEMO = :MEMO, SMEMO = :SMEMO, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                         WHERE PO_NO = :PO_NO";
            return DBWork.Connection.Execute(sql, mm_po_m, DBWork.Transaction);
        }

        public int MasterObsolete(MM_PO_M mm_po_m)
        {
            var sql = @"UPDATE MM_PO_M
                           SET PO_STATUS = '87', UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                         WHERE PO_NO = :PO_NO";

            return DBWork.Connection.Execute(sql, mm_po_m, DBWork.Transaction);
        }

        public int DetailAllObsolete(MM_PO_D mm_po_d)
        {
            var sql = @"UPDATE MM_PO_D
                           SET STATUS = 'D', UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                         WHERE PO_NO = :PO_NO ";
            return DBWork.Connection.Execute(sql, mm_po_d, DBWork.Transaction);
        }

        public int SendEmail(MM_PO_M mm_po_m)
        {
            var sql = @"UPDATE MM_PO_M
                           SET PO_STATUS = :PO_STATUS, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                         WHERE PO_NO = :PO_NO AND PO_STATUS='80' ";
            return DBWork.Connection.Execute(sql, new { PO_NO = mm_po_m.PO_NO, PO_STATUS = mm_po_m.PO_STATUS, UPDATE_USER = mm_po_m.UPDATE_USER, UPDATE_IP = mm_po_m.UPDATE_IP }, DBWork.Transaction);
        }


        public int ReSendEmail(MM_PO_M mm_po_m)
        {
            var sql = @"UPDATE MM_PO_M
                           SET PO_STATUS = '88', UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                         WHERE PO_NO = :PO_NO AND PO_STATUS='82' ";
            return DBWork.Connection.Execute(sql, mm_po_m, DBWork.Transaction);
        }

        public IEnumerable<MM_PO_D> GetDetailAll(string po_no)
        {
            string sql = @"SELECT 
                             D.MMCODE, M.mmname_e,M.mmname_c, M.E_MANUFACT, D.M_PURUN, PO_PRICE, PO_QTY, PO_AMT, MEMO, 
                               case when STATUS='N' then '申購' when STATUS='D' then '作廢' end STATUS,
                               M.CONTRACNO
                            FROM MM_PO_D D, MI_MAST M
                            where D.mmcode=M.MMCODE and  po_no =:po_no
                            Order by mmcode
                            ";
            return DBWork.Connection.Query<MM_PO_D>(sql, new { po_no = po_no }, DBWork.Transaction);
        }

        public int DetailUpdate(MM_PO_D mm_po_d)
        {
            var sql = @"UPDATE MM_PO_D
                           SET MEMO = :MEMO,PO_QTY=:PO_QTY, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP,
                            PO_AMT= PO_PRICE*:PO_QTY
                         WHERE PO_NO = :PO_NO AND MMCODE=:MMCODE";
            return DBWork.Connection.Execute(sql, mm_po_d, DBWork.Transaction);
        }
        public int UpdateMM_PO_INREC(MM_PO_D mm_po_d)
        {
            var sql = @"UPDATE MM_PO_INREC
                           SET PO_QTY=:PO_QTY, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP,
                            PO_AMT= PO_PRICE*:PO_QTY, MEMO = nvl2(MEMO,MEMO||';修改數量','修改數量')
                         WHERE PO_NO = :PO_NO AND MMCODE=:MMCODE";
            return DBWork.Connection.Execute(sql, mm_po_d, DBWork.Transaction);
        }
        public int UpdatePH_INVOICE(MM_PO_D mm_po_d)
        {
            var sql = @"UPDATE PH_INVOICE
                           SET PO_QTY=:PO_QTY, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP,
                            PO_AMT= PO_PRICE*:PO_QTY, MEMO = nvl2(MEMO,MEMO||';修改數量','修改數量')
                         WHERE PO_NO = :PO_NO AND MMCODE=:MMCODE";
            return DBWork.Connection.Execute(sql, mm_po_d, DBWork.Transaction);
        }
        public int DetailObsolete(MM_PO_D mm_po_d)
        {
            var sql = @"UPDATE MM_PO_D
                           SET STATUS = 'D', UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                         WHERE PO_NO = :PO_NO AND MMCODE=:MMCODE";
            return DBWork.Connection.Execute(sql, mm_po_d, DBWork.Transaction);
        }

        //public IEnumerable<MI_WHMAST> GetWH_NoCombo(string p0, string userId, int page_index, int page_size, string sorters)
        //{
        //    var p = new DynamicParameters();

        //    var sql = @"SELECT {0} A.WH_NO, A.WH_NAME, A.WH_KIND, A.WH_GRADE
        //                FROM MI_WHMAST A , MI_WHID B
        //                WHERE 
        //                A.WH_NO = B.WH_NO and B.WH_USERID =:userId
        //                and A.WH_GRADE = '1' ";

        //    p.Add(":userId", userId);

        //    if (p0 != "")
        //    {
        //        sql = string.Format(sql, "(NVL(INSTR(A.WH_NO, :WH_NO_I), 1000) + NVL(INSTR(A.WH_NAME, :WH_NAME_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大                
        //        p.Add(":WH_NO_I", p0);
        //        p.Add(":WH_NAME_I", p0);

        //        sql += " AND (A.WH_NO LIKE :WH_NO ";
        //        p.Add(":WH_NO", string.Format("%{0}%", p0));

        //        sql += " OR A.WH_NAME LIKE :WH_NAME) ";
        //        p.Add(":WH_NAME", string.Format("%{0}%", p0));

        //        sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, WH_NO", sql);
        //    }
        //    else
        //    {
        //        sql = string.Format(sql, "");
        //        sql += " ORDER BY A.WH_NO ";
        //    }

        //    p.Add("OFFSET", (page_index - 1) * page_size);
        //    p.Add("PAGE_SIZE", page_size);

        //    return DBWork.Connection.Query<MI_WHMAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);

        //}
        //庫別combox
        public IEnumerable<ComboItemModel> GetWH_NO()
        {
            var sql = @"  SELECT WH_NO VALUE,
                                    WH_NO || ' ' || WH_NAME TEXT
                             FROM MI_WHMAST
                             WHERE WH_GRADE IN ('1', '5') AND WH_KIND = '0'
                             ORDER BY WH_NO";
            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }
        public IEnumerable<ComboItemModel> GetWH_NoCombo()
        {
            var p = new DynamicParameters();

            var sql = @"select AGEN_NO||'  '||AGEN_NAMEC TEXT,AGEN_NO VALUE from PH_VENDER";


            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);

        }


        public DataTable Report(string WH_NO, string YYYYMMDD, string YYYYMMDD_E, string PO_STATUS, string Agen_No)
        {

            var p = new DynamicParameters();
            //BD0009,BD0010,AA0068 報表 po_qty=0, agen_no=000(999要), 作廢不要顯示 
            string sql = @"select d.MMCODE,d.M_PURUN,d.PO_PRICE, SAFE_QTY,OPER_QTY,INV_QTY,
                            AdviseQty,d.pO_QTY,case when t.e_purtype='1' then '甲' when t.e_purtype='2' then '乙' end e_purtype,
                            d.po_no||(case when t.CONTRACNO='0Y' then '*零購*' when t.CONTRACNO='0N' then '*零購*' when t.CONTRACNO='X' then '*零購*' else '*合約*' end) as MEMO,
                            d.PO_amt,t.CONTRACNO ,
                            (select substr(mmname_e,1,25) from MI_MAST where mmcode=t.mmcode) as mmname_e,
                            (select agen_no ||'_'||easyname from ph_vender where agen_no= t.agen_no) as agen_name,d.PO_PRICE*AdviseQty PO_PRICE_X_AdviseQty, d.PO_PRICE*d.PO_QTY PO_PRICE_X_PO_QTY
                            from mm_po_t t, mm_po_d d
                            where t.po_no=d.po_no and t.mmcode=d.mmcode
                            and t.purdate >=:YYYYMMDD and t.purdate <=:YYYYMMDD_E and t.wh_no=:WH_NO 
                            and d.po_qty <> 0 and t.agen_no<>'000' and d.status<>'D' ";

            p.Add(":YYYYMMDD", YYYYMMDD);
            p.Add(":YYYYMMDD_E", YYYYMMDD_E);
            p.Add(":WH_NO", WH_NO);

            //switch (PO_STATUS)
            //{
            //    case "0":
            //        sql += " and d.status<>'D' ";
            //        break;
            //    case "D":
            //        sql += " and d.status='D' ";
            //        break;
            //}

            if (Agen_No != "")
            {
                sql += " and t.agen_no=:agen_no ";
                p.Add(":agen_no", Agen_No);
            }

            sql += " order by agen_name, d.mmcode ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        // (case when b.CONTRACNO='0Y' then '*零購*' when b.CONTRACNO='0N' then '*零購*'  when t.CONTRACNO='X' then '*零購*' else '*合約*' end) memo
        public DataTable ReportAA0068(string WH_NO, string YYYYMMDD, string YYYYMMDD_E, string CONTRACT, string Agen_No)
        {

            var p = new DynamicParameters();

            string sql = string.Empty;
            sql = @"select d.MMCODE,d.M_PURUN,d.PO_PRICE, SAFE_QTY,OPER_QTY,INV_QTY,
                            AdviseQty,d.pO_QTY,case when t.e_purtype='1' then '甲' when t.e_purtype='2' then '乙' end e_purtype, d.po_no||(case when t.CONTRACNO='0Y' then '*零購*' when t.CONTRACNO='0N' then '*零購*' when t.CONTRACNO='X' then '*零購*' else '*合約*' end) as MEMO,
                            d.PO_amt,t.CONTRACNO ,
                            (select substr(mmname_e,1,25) from MI_MAST where mmcode=t.mmcode) as mmname_e,
                            (select agen_no ||'_'||easyname from ph_vender where agen_no= t.agen_no) as agen_name,d.PO_PRICE*AdviseQty PO_PRICE_X_AdviseQty, d.PO_PRICE*d.PO_QTY PO_PRICE_X_PO_QTY
                            from mm_po_t t, mm_po_d d
                            where t.po_no=d.po_no and t.mmcode=d.mmcode
                            and t.purdate >=:YYYYMMDD and t.purdate <=:YYYYMMDD_E and t.wh_no=:WH_NO  
                            and d.po_qty <> 0 and t.agen_no<>'000' and d.status<>'D' ";
            if (Agen_No != "")
            {
                sql += " and t.agen_no=:agen_no ";
                p.Add(":agen_no", Agen_No);
            }
            switch (CONTRACT)
            {
                case "零購":
                    sql += "and t.CONTRACNO in ('0Y', '0N', 'X')";
                    break;
                case "合約":
                    sql += "and t.CONTRACNO in ('1','2','3','01','02','03')";
                    break;
            }
            sql += " order by agen_name, d.mmcode ";



            p.Add(":YYYYMMDD", YYYYMMDD);
            p.Add(":YYYYMMDD_E", YYYYMMDD_E);
            p.Add(":WH_NO", WH_NO);



            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        public int GetReportTotalCnt(string WH_NO, string YYYYMMDD, string YYYYMMDD_E, string PO_STATUS, string Agen_No)
        {
            var p = new DynamicParameters();
            string sql = @"select count(*)
                            from mm_po_t t, mm_po_d d
                            where t.po_no=d.po_no and t.mmcode=d.mmcode
                            and t.purdate >=:YYYYMMDD and t.purdate <=:YYYYMMDD_E and t.wh_no=:WH_NO
                            and d.po_qty <> 0 and t.agen_no<>'000' and d.status<>'D' 
                          ";

            p.Add(":YYYYMMDD", YYYYMMDD);
            p.Add(":YYYYMMDD_E", YYYYMMDD_E);
            p.Add(":WH_NO", WH_NO);

            switch (PO_STATUS)
            {
                case "0":
                    sql += " and d.status<>'D' ";
                    break;
                case "D":
                    sql += " and d.status='D' ";
                    break;
            }

            if (Agen_No != "")
            {
                sql += " and t.agen_no=:agen_no ";
                p.Add(":agen_no", Agen_No);
            }
            return DBWork.Connection.QueryFirst<int>(sql, p, DBWork.Transaction);
        }
        public int TotalCntAA0068(string WH_NO, string YYYYMMDD, string YYYYMMDD_E, string CONTRACT, string Agen_No)
        {

            var p = new DynamicParameters();

            string sql = @"select count(*)
                            from mm_po_t t, mm_po_d d
                            where t.po_no=d.po_no and t.mmcode=d.mmcode
                            and t.purdate >=:YYYYMMDD and t.purdate <=:YYYYMMDD_E and t.wh_no=:WH_NO 
                            and d.po_qty <> 0 and t.agen_no<>'000' and d.status<>'D' 
                           ";

            p.Add(":YYYYMMDD", YYYYMMDD);
            p.Add(":YYYYMMDD_E", YYYYMMDD_E);
            p.Add(":WH_NO", WH_NO);

            if (Agen_No != "")
            {
                sql += " and t.agen_no=:agen_no ";
                p.Add(":agen_no", Agen_No);
            }
            switch (CONTRACT)
            {
                case "零購":
                    sql += "and t.CONTRACNO in ('0Y', '0N', 'X')";
                    break;
                case "合約":
                    sql += "and t.CONTRACNO in ('1','2','01','02')";
                    break;
            }
            return DBWork.Connection.QueryFirst<int>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<MM_PO_D> GetOver100K(string po_no) {
            string sql = @"select a.po_no, a.mmcode, a.po_qty, a.po_price, a.po_amt,
                                  mmcode_name(a.mmcode) as mmname_e
                             from MM_PO_D a, MI_MAST b
                            where a.po_no = :po_no
                              and a.po_amt >= 100000
                              and b.mmcode = a.mmcode
                              and b.contracno in ('0Y', '0N', 'X')";
            return DBWork.Connection.Query<MM_PO_D>(sql, new { po_no = po_no }, DBWork.Transaction);
        }

        public DataTable GetOver100KExcel(string po_no) {
            string sql = @"
                    select a.po_no as 訂單編號,
                           a.mmcode as 院內碼,
                           mmcode_name(a.mmcode) as 藥品名稱,
                           a.po_qty as 數量,
                           a.po_price as 單價,
                           a.po_amt as 金額
                      from MM_PO_D a, MI_MAST b
                     where a.po_no = :po_no
                       and a.po_amt >= 100000      
                       and b.mmcode = a.mmcode
                       and b.contracno in ('0Y', '0N', 'X')
            ";
            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, new { po_no = po_no}, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public string GetHospCode()
        {
            string sql = @"
                select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode'
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }

        public bool IsAllStatusD(string po_no)
        {
            string sql = @"
                select 1 from MM_PO_D where po_no = :po_no and nvl(status, 'N') = 'N'
            ";

            return DBWork.Connection.ExecuteScalar(sql, new { po_no }, DBWork.Transaction) == null;
        }
    }
}