using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WebApp.Models;
using WebApp.Models.BD;
using System.Net;
using System.Net.Sockets;

namespace WebApp.Repository.B
{
    public class BD0014 : JCLib.Mvc.BaseModel
    {
        public string F01 { get; set; } //格式
        public string F02 { get; set; } //醫事服務機構代號
        public string F03 { get; set; } //申報資料年月
        public string F04 { get; set; } //藥品代碼
        public string F05 { get; set; } //藥商統一編號
        public string F06 { get; set; } //發票號碼
        public string F07 { get; set; } //發票日期
        public string F08 { get; set; } //發票購買藥品數量A
        public string F09 { get; set; } //HIS_計量單位
        public string F10 { get; set; } //HIS_規格量及單位
        public string F11 { get; set; } //HIS_成份
        public string F12 { get; set; } // HIS_藥品劑型
        public string F13 { get; set; } //NHI_規格量
        public string F14 { get; set; } //NHI_規格單位
        public string F15 { get; set; } //NHI_藥品劑型
        public string F16 { get; set; } //贈品數量附贈之藥品數量B
        public string F17 { get; set; } //贈品數量藥品耗損數量C
        public string F18 { get; set; } //退貨數量D
        public string F19 { get; set; } //實際購買數量E
        public string F20 { get; set; } //發票金額F元
        public string F21 { get; set; } //退貨金額G元
        public string F22 { get; set; } //折讓金額折讓單金額H元
        public string F23 { get; set; } //折讓金額指定捐贈I元
        public string F24 { get; set; } //折讓金額藥商提撥管理費J元
        public string F25 { get; set; } //折讓金額藥商提撥研究費K元
        public string F26 { get; set; } //折讓金額藥商提撥補助醫師出國會議L元
        public string F27 { get; set; } //折讓金額其他與本交易相關之附帶利益M元
        public string F28 { get; set; } //購藥總金額N元
        public string F29 { get; set; } //發票註記
        public string F30 { get; set; } //院內碼
        public string F31 { get; set; } //訂單號碼
        public string F32 { get; set; } //資料流水號
        public string F33 { get; set; } //廠商代碼
        public string F34 { get; set; } //進貨日期

        List<BD0014> List { get; set; }
    }

    public class BD0014Repository : JCLib.Mvc.BaseRepository
    {
        public BD0014Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MM_PO_M> GetMasterAll(string wh_no, string start_date, string end_date, string PO_STATUS, string Agen_No, string Mmcode, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            string sql = @"with temp as (
                            select a.mmcode, sum(a.po_amt)  as po_amt_sum
                            from MM_PO_D a, MM_PO_M b
                            where a.po_no = b.po_no
                            and substr(twn_yyymm(po_time), 1,3) = substr(twn_sysdate,1,3)
                            group by mmcode
                        )
                        select po_no, twn_date(po_time) as po_date, wh_no,
                                  m.agen_no,  
                                  (select mat_clsname from MI_MATCLASS where mat_class = m.mat_class) as mat_class,
                                  (select agen_namec from ph_vender where agen_no=m.agen_no) as agen_namec, memo, 
                                  (case when m.m_contid = '0' then '合約' else '非合約' end) as m_contid,
                                  (select data_desc from param_d where  data_name='PO_STATUS' and data_value=m.po_status) as po_status, 
                                  (select count(*) from mm_po_d where po_no=m.po_no) as cnt,
                                  (
                                    select listagg(a.mmcode,'、') within group (order by a.po_no) as t 
                                    from MM_PO_D a, temp b  
                                    where a.po_no = m.po_no 
                                    and a.mmcode = b.mmcode 
                                    and b.po_amt_sum > (select data_value from PARAM_D where grp_code='M_CONTID2_LIMIT' and data_name='LIMIT') 
                                    group by a.po_no
                                  ) as mmcode_over_150k,
                                 (select easyname from PH_VENDER where agen_no=m.agen_no) as easyname,
                                 (select email from PH_VENDER where agen_no=m.agen_no) as email
                             from MM_PO_M m
                            where 1=1
                              and twn_date(po_time) between :start_date and :end_date
                              and wh_no =:wh_no
                        ";

            p.Add(":start_date", start_date);
            p.Add(":end_date", end_date);
            p.Add(":wh_no", wh_no);
            p.Add(":po_status", PO_STATUS);

            if (string.IsNullOrEmpty(PO_STATUS) == false) {
                sql += @"  and po_status = :po_status";
            }

            //switch (PO_STATUS)
            //{
            //    case "0":
            //        sql += " and not (PO_STATUS = '87' or  po_no in (select po_no from MM_PO_D where status='D')) "; // 80開單, 84待傳MAIL
            //        break;
            //    case "1":
            //        sql += " and PO_STATUS = '82'"; // 已傳MAIL
            //        break;
            //    case "D":
            //        sql += " and (PO_STATUS = '87' or  po_no in (select po_no from MM_PO_D where status='D')) "; // 87作廢
            //        break;
            //}

            if (!String.IsNullOrEmpty(Agen_No))
            {
                sql += " and agen_no=:agen_no "; // 所選廠商585, 928
                p.Add(":agen_no", Agen_No);
            }
            if (!String.IsNullOrEmpty(Mmcode))
            {
                sql += " and exists(  select 1 from MM_PO_D where po_no = m.po_no and mmcode = :mmcode  ) ";  // 所選院內碼
                p.Add(":mmcode", Mmcode); // 院內碼
            }
            sql += " order by po_no, AGEN_NO ";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            String debug_sql = sql;
            foreach (var name in p.ParameterNames)
            {
                var val = p.Get<dynamic>(name);
                debug_sql = debug_sql.Replace(":" + name, "'" + val + "'");
            }

            return DBWork.Connection.Query<MM_PO_M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MM_PO_M> GetSendAll(string wh_no, string start_date, string end_date)
        {
            var p = new DynamicParameters();

            string sql = @"select PO_NO, AGEN_NO
                             from MM_PO_M
                            where PO_STATUS = '80'
                            and WH_NO = :WH_NO
                            and twn_date(PO_TIME) between :START_DATE and :END_DATE
                            order by PO_NO
                            ";

            p.Add(":WH_NO", wh_no);
            p.Add(":START_DATE", start_date);
            p.Add(":END_DATE", end_date);

            return DBWork.Connection.Query<MM_PO_M>(sql, p, DBWork.Transaction);
        }

        public int MasterUpdate(MM_PO_M mm_po_m)
        {
            var sql = @"UPDATE MM_PO_M
                           SET MEMO = :MEMO, SMEMO = :SMEMO, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                         WHERE PO_NO = :PO_NO";
            String debug_sql = sql;
            return DBWork.Connection.Execute(sql, mm_po_m, DBWork.Transaction);
        }

        public int MasterObsolete(MM_PO_M mm_po_m)
        {
            var sql = @"UPDATE MM_PO_M
                           SET PO_STATUS = '87', UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                         WHERE PO_NO = :PO_NO";
            String debug_sql = sql;
            return DBWork.Connection.Execute(sql, mm_po_m, DBWork.Transaction);
        }

        public int DetailAllObsolete(MM_PO_D mm_po_d)
        {
            var sql = @"UPDATE MM_PO_D
                           SET STATUS = 'D', UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                         WHERE PO_NO = :PO_NO ";
            String debug_sql = sql;
            return DBWork.Connection.Execute(sql, mm_po_d, DBWork.Transaction);
        }

        public int SendEmail(MM_PO_M mm_po_m)
        {
            var sql = @"UPDATE MM_PO_M
                           SET PO_STATUS = :PO_STATUS, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                         WHERE PO_NO = :PO_NO AND PO_STATUS='80' ";
            String debug_sql = sql;
            return DBWork.Connection.Execute(sql, new { PO_NO = mm_po_m.PO_NO, PO_STATUS = mm_po_m.PO_STATUS, UPDATE_USER = mm_po_m.UPDATE_USER, UPDATE_IP = mm_po_m.UPDATE_IP }, DBWork.Transaction);
        }


        public int ReSendEmail(MM_PO_M mm_po_m)
        {
            var sql = @"UPDATE MM_PO_M
                           SET PO_STATUS = '88', UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                         WHERE PO_NO = :PO_NO AND PO_STATUS='82' ";
            String debug_sql = sql;
            return DBWork.Connection.Execute(sql, mm_po_m, DBWork.Transaction);
        }

        public int SetFax(MM_PO_M mm_po_m)
        {
            var sql = @"UPDATE MM_PO_M
                           SET PO_STATUS = '85', UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                         WHERE PO_NO = :PO_NO ";
            String debug_sql = sql;
            return DBWork.Connection.Execute(sql, mm_po_m, DBWork.Transaction);
        }
        public IEnumerable<MM_PO_D> GetDetailAll(string po_no)
        {
            string sql = @"select d.mmcode, d.mmname_e,d.mmname_c, d.base_unit, d.m_purun, d.po_price, d.po_qty, d.po_amt, d.memo, 
                                  (case when status='N' then ' ' when status='D' then '作廢' end) status,
                                  d.m_discperc, d.deli_qty, d.uprice, d.disc_cprice, d.disc_uprice,
                                  d.m_nhikey, 
                                  get_param('MI_MAST','E_SOURCECODE', d.e_sourcecode) as e_sourcecode,
                                  d.unitrate, twn_date(d.e_codate) as e_codate, d.caseno,
                                  (select unitrate from MI_MAST where MMCODE=d.MMCODE) as mast_unitrate,
                                  d.chinname, d.chartno,d.partialdl_dt
                             from MM_PO_D d
                            where d.po_no =:po_no
                            order by d.mmcode
                            ";
            String debug_sql = sql;
            return DBWork.Connection.Query<MM_PO_D>(sql, new { po_no = po_no }, DBWork.Transaction);
        }

        public int DetailUpdate(MM_PO_D mm_po_d)
        {
            var sql = @"UPDATE MM_PO_D
                           SET MEMO = :MEMO,PO_QTY=:PO_QTY, UNITRATE=:UNITRATE, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP,
                            PO_AMT= PO_PRICE*:PO_QTY,PARTIALDL_DT=:PARTIALDL_DT
                         WHERE PO_NO = :PO_NO AND MMCODE=:MMCODE";
            String debug_sql = sql;
            return DBWork.Connection.Execute(sql, mm_po_d, DBWork.Transaction);
        }
        public int UpdateMM_PO_INREC(MM_PO_D mm_po_d)
        {
            var sql = @"UPDATE MM_PO_INREC
                           SET PO_QTY=:PO_QTY, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP,
                            PO_AMT= PO_PRICE*:PO_QTY, MEMO = nvl2(MEMO,MEMO||';修改數量','修改數量')
                         WHERE PO_NO = :PO_NO AND MMCODE=:MMCODE";
            String debug_sql = sql;
            return DBWork.Connection.Execute(sql, mm_po_d, DBWork.Transaction);
        }
        public int UpdatePH_INVOICE(MM_PO_D mm_po_d)
        {
            var sql = @"UPDATE PH_INVOICE
                           SET PO_QTY=:PO_QTY, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP,
                            PO_AMT= PO_PRICE*:PO_QTY, MEMO = nvl2(MEMO,MEMO||';修改數量','修改數量')
                         WHERE PO_NO = :PO_NO AND MMCODE=:MMCODE";
            String debug_sql = sql;
            return DBWork.Connection.Execute(sql, mm_po_d, DBWork.Transaction);
        }
        public int DetailObsolete(MM_PO_D mm_po_d)
        {
            var sql = @"UPDATE MM_PO_D
                           SET STATUS = 'D', UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                         WHERE PO_NO = :PO_NO AND MMCODE=:MMCODE";
            String debug_sql = sql;
            return DBWork.Connection.Execute(sql, mm_po_d, DBWork.Transaction);
        }

        //庫別combox
        public IEnumerable<ComboItemModel> GetWH_NO(string userId)
        {
            var sql = @"  select a.wh_no value, a.wh_no || ' ' || a.wh_name text
                            from MI_WHMAST a, MI_WHID b
                           where a.wh_grade in ('1') 
                             and a.wh_no = b.wh_no
                             and b.wh_userid = :userId
                           order by a.wh_no";
            String debug_sql = sql;
            return DBWork.Connection.Query<ComboItemModel>(sql,new { userId}, DBWork.Transaction);
        }
        //庫別combox
        public IEnumerable<ComboItemModel> GetMmcodeCombo()
        {
            var sql = @"
            select distinct mmcode as VALUE, mmcode || ' ' || mmname_c as TEXT, mmname_e 
            from MI_MAST
            where mat_class in ('01','02','03','04','05','06','07','08')
            and nvl(cancel_id,'N') = 'N'
            -- and rownum<3000 -- 資料載入過多，有需要全部的資料再解開
            order by TEXT
            ";
            String debug_sql = sql;
            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }
        //物料類別combox
        public IEnumerable<ComboItemModel> GetMatClassCombo(String wh_userid)
        {
            var sql = @"";
            sql += sBr + "with temp_whkinds as ( ";
            sql += sBr + "    select b.wh_no, b.wh_kind, nvl((case when a.task_id = '3' then '2' else a.task_id end), '2') as task_id ";
            sql += sBr + "    from MI_WHID a, MI_WHMAST b ";
            sql += sBr + "    where 1=1 ";
            sql += sBr + "    -- and wh_userid = '" + wh_userid + "' "; // 登入人員代碼
            if (!String.IsNullOrEmpty(wh_userid))
                sql += sBr + "    -- and wh_userid = :wh_userid "; // 登入人員代碼
            else
                sql += sBr + "    and rownum=1 "; // 暫載入1筆
            sql += sBr + "    and a.wh_no = b.wh_no ";
            sql += sBr + "    and b.wh_grade = '1' ";
            sql += sBr + "    and a.task_id in ('1','2','3') ";
            sql += sBr + ") ";
            sql += sBr + "select distinct ";
            sql += sBr + "    b.mat_class as value,b.mat_clsname as text,b.mat_class || ' ' ||  b.mat_clsname as COMBITEM  ";
            sql += sBr + "    from temp_whkinds a, MI_MATCLASS b ";
            sql += sBr + "    where (a.task_id = b.mat_clsid) ";
            sql += sBr + "union ";
            sql += sBr + "select distinct ";
            sql += sBr + "    b.mat_class as value,b.mat_clsname as text,b.mat_class || ' ' ||  b.mat_clsname as COMBITEM   ";
            sql += sBr + "    from temp_whkinds a, MI_MATCLASS b ";
            sql += sBr + "    where (a.task_id = '2') ";
            sql += sBr + "    and b.mat_clsid = '3' ";
            String debug_sql = sql;
            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }
        //物料類別的信件內容
        public IEnumerable<BD0014M> GetMatClassTextArea(
            String mat_class, // 所選物料類別
            String data_remark // 登入人員代碼
        )
        {
            var p = new DynamicParameters();
            var sql = @"";
            sql += sBr + "select  ";
            sql += sBr + "data_name, --(類別，儲存用), ";
            sql += sBr + "data_value, --(帶入至textare中)  ";
            sql += sBr + "data_remark --(使用人員，儲存用) ";
            sql += sBr + "from PARAM_D where 1=1  ";
            sql += sBr + "and grp_code = 'MM_PO_M'  ";
            if (!String.IsNullOrEmpty(mat_class))
            {
                //sql += sBr + "and data_name = ( ";
                //sql += sBr + "    case  ";
                //sql += sBr + "        when :data_name = '01' then 'MAIL_CONTENT_01'  "; // 所選物料類別
                //sql += sBr + "        else 'MAIL_CONTENT_02'  ";
                //sql += sBr + "    end ";
                //sql += sBr + ") ";
                sql += sBr + "and data_name = :data_name ";
                p.Add(":data_name", "MAIL_CONTENT_" + mat_class);
            }
            if (!String.IsNullOrEmpty(data_remark))
            {
                sql += sBr + "and data_remark = :data_remark "; // 登入人員代碼
                p.Add(":data_remark", data_remark);
            }
            String debug_sql = sql;
            foreach (var name in p.ParameterNames)
            {
                var val = p.Get<dynamic>(name);
                debug_sql = debug_sql.Replace(":" + name, "'" + val + "'");
            }
            return DBWork.Connection.Query<BD0014M>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<ComboItemModel> GetAgenCombo()
        {
            var p = new DynamicParameters();
            var sql = @"select AGEN_NO||'  '||AGEN_NAMEC TEXT,AGEN_NO VALUE from PH_VENDER";
            String debug_sql = sql;
            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);

        }
        public IEnumerable<ComboItemModel> GetMemoCombo()
        {
            var sql = @"select distinct memo as value from  MM_PO_M
                        where memo is not null";
            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);

        }

        

        public IEnumerable<ComboItemModel> GetStatusCombo(string hospCode) {
            string addSql = "";
            if (hospCode == "807")
                addSql = " and data_value in ('80','85', '87') ";
            else
                addSql = " and data_value in ('80','82','84','87','88') ";

            string sql = @"
                select data_value as value, data_desc as text
                  from PARAM_D
                 where data_name='PO_STATUS'
                   " + addSql;
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
            String debug_sql = sql;
            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            
            return dt;
        }
        // (case when b.CONTRACNO='0Y' then '*零購*' when b.CONTRACNO='0N' then '*零購*'  when t.CONTRACNO='X' then '*零購*' else '*合約*' end) memo
        public DataTable ReportAA0068(string WH_NO, string YYYYMMDD, string YYYYMMDD_E, string CONTRACT, string Agen_No)
        {

            var p = new DynamicParameters();

            string sql = @"select d.MMCODE,d.M_PURUN,d.PO_PRICE, SAFE_QTY,OPER_QTY,INV_QTY,
                            AdviseQty,d.pO_QTY,case when t.e_purtype='1' then '甲' when t.e_purtype='2' then '乙' end e_purtype, d.po_no||(case when t.CONTRACNO='0Y' then '*零購*' when t.CONTRACNO='0N' then '*零購*' when t.CONTRACNO='X' then '*零購*' else '*合約*' end) as MEMO,
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

            String debug_sql = sql;
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
            String debug_sql = sql;
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
            String debug_sql = sql;
            return DBWork.Connection.QueryFirst<int>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<MM_PO_D> GetOver100K(string po_no)
        {
            string sql = @"
                select POD.PO_NO, POD.PO_QTY, POD.PO_PRICE, POD.PO_AMT, T.*
                from (
                select a.mmcode, 
                    sum(a.po_amt) as sum_po_amt, 
                    mmcode_name(a.mmcode) as mmname_e 
                    from MM_PO_D a, MM_PO_M b 
                    where 1=1 
                    and a.po_no = b.po_no 
                    and a.mmcode in (select mmcode from MM_PO_D where po_no = :po_no )
                    and substr(twn_yyymm(b.po_time), 1,3) = substr(twn_sysdate, 1, 3) 
                    and b.m_contid='2' 
                    group by a.mmcode
                    having sum(a.po_amt) > (select data_value from PARAM_D where grp_code='M_CONTID2_LIMIT' and data_name='LIMIT')
                ) T, MM_PO_D POD
                where T.MMCODE = POD.MMCODE and POD.PO_NO = :po_no
            ";
            
            return DBWork.Connection.Query<MM_PO_D>(sql, new { po_no = po_no.Trim() }, DBWork.Transaction);
        }

        public DataTable GetOver150KExcel(string po_no)
        {
            string sql = @"
                select POD.PO_NO as 訂單編號, T.MMCODE as 院內碼, POD.PO_QTY as 數量, POD.PO_PRICE as 單價, T.sum_po_amt as 年度累計金額
                from (
                select a.mmcode, 
                    sum(a.po_amt) as sum_po_amt, 
                    mmcode_name(a.mmcode) as mmname_e 
                    from MM_PO_D a, MM_PO_M b 
                    where 1=1 
                    and a.po_no = b.po_no 
                    and a.mmcode in (select mmcode from MM_PO_D where po_no = :po_no )
                    and substr(twn_yyymm(b.po_time), 1,3) = substr(twn_sysdate, 1, 3) 
                    and b.m_contid='2' 
                    group by a.mmcode
                    having sum(a.po_amt) > (select data_value from PARAM_D where grp_code='M_CONTID2_LIMIT' and data_name='LIMIT')
                ) T, MM_PO_D POD
                where T.MMCODE = POD.MMCODE and POD.PO_NO = :po_no
            ";
            
            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, new { po_no = po_no.Trim() }, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<MM_PO_M> GetAgenAboutEmail(string wh_no, string start_date, string end_date, string po_status, bool hasEmail)
        {
            // 若hasEmail為true,則取廠商有EMAIL的訂單; 為false則取廠商沒EMAIL的訂單
            string addSql = " is null";
            if (hasEmail)
                addSql = " is not null";

            string sql = @"select A.PO_NO, A.AGEN_NO, 
                        nvl((select AGEN_NAMEC from PH_VENDER where AGEN_NO=A.AGEN_NO), '') as AGEN_NAMEC
                        from MM_PO_M A
                        where A.PO_STATUS = :PO_STATUS 
                        and WH_NO = :WH_NO
                        and twn_date(A.PO_TIME) between :START_DATE and :END_DATE
                        and (select EMAIL from PH_VENDER where AGEN_NO=A.AGEN_NO) " + addSql;

            return DBWork.Connection.Query<MM_PO_M>(sql, new { WH_NO = wh_no, START_DATE = start_date, END_DATE = end_date, PO_STATUS = po_status }, DBWork.Transaction);
        }

        const String sBr = "\r\n";
        public DataTable GetExportDetailByExcel(
            String start_date,
            String end_date,
            String wh_no,
            String po_no,
            String agen_no,
            String mmcode,
            String po_status
        )
        {
            var p = new DynamicParameters();
            string sql = @"
                    select 
                    m.po_no 訂單編號, 
                    twn_date(m.po_time) as 訂單日期, 
                    m.wh_no 庫房代碼,
                    m.agen_no 廠商代碼, 
                    (select mat_clsname from MI_MATCLASS where mat_class = m.mat_class) as 物料類別, 
                    (select agen_namec from ph_vender where agen_no=m.agen_no) as 廠商名稱,  
                    (case when m.m_contid = '0' then '合約' else '非合約' end) as 是否合約, 
                    d.mmcode 院內碼, 
                    d.mmname_e 英文品名, 
                    d.mmname_c 中文品名, 
                    d.base_unit 單位, 
                    d.m_purun 包裝, 
                    d.po_price 單價 , 
                    d.po_qty 數量, 
                    d.po_amt 小計, 
                    d.chinname as 病人姓名, 
                    d.chartno as 病歷號, 
                    d.m_discperc 折讓比,  
                    d.disc_cprice 成本價, 
                    d.m_nhikey 健保碼, 
                    get_param('MI_MAST','E_SOURCECODE', d.e_sourcecode) as 買斷寄庫,
                    d.unitrate 出貨單位, 
                    twn_date(d.e_codate) as 合約到期日, 
                    d.caseno 合約案號,
                    (select EMAIL from PH_VENDER where AGEN_NO=m.agen_no) as 廠商電子信箱,
                    d.PARTIALDL_DT 分批交貨日期

                    from MM_PO_M m, MM_PO_D d
                    where 1=1 
            ";
            sql += sBr + "and m.po_no = d.po_no ";
            sql += sBr + "-- and nvl(d.status, 'N') = 'N' -- 訂單狀態[D(作廢)|N(申購)] ";
            sql += sBr + "and m.po_status<>'87' -- 訂單狀態[80(開單)|82(已傳MAIL)|83(已傳真_待轉檔)|84(待傳MAIL)|85(已傳真)|87(作廢)|88(補寄MAIL-for 藥品)] ";
            if (!String.IsNullOrEmpty(start_date) && !String.IsNullOrEmpty(end_date))
            {
                sql += sBr + "and twn_date(m.po_time) between :start_date and :end_date -- 起始日期 and 結束日期 ";
                p.Add(":start_date", start_date);
                p.Add(":end_date", end_date);
            }    
            if (!String.IsNullOrEmpty(wh_no))
            {
                sql += sBr + "and m.wh_no = :wh_no -- 庫房代碼 ";
                p.Add(":wh_no", wh_no);
            }
            if (!String.IsNullOrEmpty(po_no))
            {
                sql += sBr + "and m.po_no = d.po_no ";
                sql += sBr + "and m.po_no = :po_no ";
                p.Add(":po_no", po_no);
            }
            if (!String.IsNullOrEmpty(agen_no))
            {
                sql += sBr + "and m.agen_no= :agen_no -- 所選廠商 ";
                p.Add(":agen_no", agen_no);
            }
            if (!String.IsNullOrEmpty(mmcode))
            {
                sql += sBr + "and d.mmcode = :mmcode -- 所選院內碼 ";
                p.Add(":mmcode", mmcode);
            }
            switch (po_status)
            {
                case "0":
                    sql += sBr + " and PO_STATUS in ('80', '84') -- 申請單狀態：未取消訂單 "; // 80開單, 84待傳MAIL
                    break;
                case "1":
                    sql += sBr + " and PO_STATUS = '82'"; // 已傳MAIL
                    break;
                case "D":
                    sql += sBr + " and (PO_STATUS = '87' or  po_no in (select po_no from MM_PO_D where status='D')) -- 申請單狀態：已刪除訂單 "; // 87作廢
                    break;
            }

            String debug_sql = sql;
            foreach (var name in p.ParameterNames)
            {
                var val = p.Get<dynamic>(name);
                debug_sql = debug_sql.Replace(":" + name, "'" + val + "'");
            }

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(
                debug_sql, 
                //new { 
                //    start_date = start_date
                //    ,end_date = end_date
                //    ,wh_no = wh_no
                //    ,po_no = po_no
                //    ,agen_no = agen_no
                //    ,mmcode = mmcode
                //    ,po_status = po_status
                //}, 
                DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        public IEnumerable<BD0014> GetAllExportDetailByZip(string start_ym, string end_ym, bool isPaging)
        {
            string sql = @"
                    select 
                    a.po_no as 訂單編號, 
                    twn_date(a.po_time) 訂購日期, 
                    a.memo as 備註, 
                    c.uni_no||' '||c.agen_namec as 廠商, 
                    c.agen_tel||c.agen_contact as 電話, 
                    c.agen_fax as 傳真, 
                    (
                    select data_value from PARAM_D 
                    where 1=1 
                    and grp_code = 'HOSP_INFO' 
                    and data_name = 'HospName'
                    ) as 送貨地點, 
                    twn_sysdate as 列印日期, 
                    b.mmcode as 藥材代碼, 
                    b.mmname_c as 藥材名稱, 
                    b.unitrate||b.base_unit||'/'||b.m_purun as 出貨單位, 
                    b.po_qty as 數量, 
                    b.base_unit as 單位, 
                    b.po_price as 單價, 
                    b.po_amt as 小計, 
                    get_param('MI_MAST','E_SOURCECODE', b.e_sourcecode) as 買斷寄庫, 
                    twn_date(b.e_codate) as 合約到期日, 
                    b.caseno 合約案號, 
                    b.chinname as 病人姓名, 
                    b.chartno as 病人病歷號, 
                    b.memo as 明細備註
                    from MM_PO_M a, MM_PO_D b, PH_VENDER c
                    where a.po_no = b.po_no
                    and a.agen_no = c.agen_no
                    -- and nvl(b.status, 'N') = 'N'
                    and a.po_no = :po_no
                    order by b.mmcode
            ";// b.status(D-作廢, N-申購)
            
            String debug_sql = sql;
            return DBWork.Connection.Query<BD0014>(sql, new { start_ym, end_ym }, DBWork.Transaction);
        }


        public IEnumerable<string> GetExportDetailByZipPoList(string start_ym, string end_ym)
        {
            string sql = @"
                with t as (
                         select twn_todate(:start_ym||'01') start_date, twn_todate(:end_ym||'01') end_date from dual
                )
                select twn_yyymm(add_months(trunc(start_date,'mm'),level - 1)) 
                  from t
               connect by trunc(end_date,'mm') >= add_months(trunc(start_date,'mm'),level - 1)
            ";
            String debug_sql = sql;
            return DBWork.Connection.Query<string>(sql, new { start_ym, end_ym }, DBWork.Transaction);
        }

        public string GetUnitRate(string po_no, string mmcode) {
            string sql = @"
                select nvl(unitrate, 1) from MM_PO_D where po_no = :po_no and mmcode = :mmcode
            ";
            String debug_sql = sql;
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { po_no, mmcode }, DBWork.Transaction);
        }

        public string GetLimitAmt()
        {
            string sql = @"
                select data_value from PARAM_D where grp_code='M_CONTID2_LIMIT' and data_name='LIMIT'
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }

        public int GetNotD(string po_no) {
            string sql = @"
                select count(*) from MM_PO_D where po_no = :po_no and status <> 'D'
            ";
            String debug_sql = sql;
            return DBWork.Connection.QueryFirstOrDefault<int>(sql, new { po_no }, DBWork.Transaction);
        }


        public IEnumerable<BD0014M> Get壓縮檔的單筆訂單(string po_no)
        {
            var p = new DynamicParameters();
            string sql = @"";
            sql += sBr + "select  ";
            sql += sBr + "a.po_no as 訂單編號,  ";
            sql += sBr + "twn_date(a.po_time) 訂購日期,  ";
            sql += sBr + "a.memo as 備註,  ";
            sql += sBr + "c.uni_no||' '||c.agen_namec as 廠商,  ";
            sql += sBr + "c.agen_tel||c.agen_contact as 電話,  ";
            sql += sBr + "c.agen_fax as 傳真,  ";
            sql += sBr + "(select data_value from PARAM_D where grp_code = 'HOSP_INFO' and data_name = 'HospName') as 送貨地點,  ";
            sql += sBr + "twn_sysdate as 列印日期,  ";
            sql += sBr + "b.mmcode as 藥材代碼,  ";
            sql += sBr + "b.mmname_c as 藥材名稱,  ";
            sql += sBr + "b.unitrate||b.base_unit||'/'||b.m_purun as 出貨單位,  ";
            sql += sBr + "b.po_qty as 數量, b.base_unit as 單位,  ";
            sql += sBr + "b.po_price as 單價,  ";
            sql += sBr + "b.po_amt as 小計,  ";
            sql += sBr + "get_param('MI_MAST','E_SOURCECODE', b.e_sourcecode) as 買斷寄庫,  ";
            sql += sBr + "twn_date(b.e_codate) as 合約到期日,  ";
            sql += sBr + "b.caseno 合約案號,  ";
            sql += sBr + "b.chinname as 病人姓名,  ";
            sql += sBr + "b.chartno as 病人病歷號,  ";
            sql += sBr + "b.memo as 明細備註, ";
            sql += sBr + "b.partialdl_dt as 分批交貨日期, ";
            sql += sBr + "C.EMAIL as EMAIL ";
            sql += sBr + "from MM_PO_M a, MM_PO_D b, PH_VENDER c ";
            sql += sBr + "where 1=1 ";
            sql += sBr + "and a.po_no = b.po_no ";
            sql += sBr + "and a.agen_no = c.agen_no ";
            sql += sBr + "-- and nvl(b.status, 'N') = 'N' -- 訂單狀態[D(作廢)|N(申購)] ";
            sql += sBr + "and a.po_status<>'87' -- 訂單狀態[80(開單)|82(已傳MAIL)|83(已傳真_待轉檔)|84(待傳MAIL)|85(已傳真)|87(作廢)|88(補寄MAIL-for 藥品)] ";
            if (!String.IsNullOrEmpty(po_no))
            {
                sql += sBr + "and a.po_no = :po_no ";
                p.Add(":po_no", po_no);
            }
            sql += sBr + "order by b.mmcode ";

            String debug_sql = sql;
            foreach (var name in p.ParameterNames)
            {
                var val = p.Get<dynamic>(name);
                debug_sql = debug_sql.Replace(":" + name, "'" + val + "'");
            }
            return DBWork.Connection.Query<BD0014M>(sql, new { po_no }, DBWork.Transaction);
        } // 


        // 儲存
        public int set信件內容維護(BD0014M o) // 原參考 public int MasterUpdate(MM_PO_M mm_po_m)
        {
            var sql = @"";
            sql += sBr + "merge into PARAM_D a ";
            sql += sBr + "    using ( ";
            sql += sBr + "      select  ";
            sql += sBr + "		'MM_PO_M' as grp_code,  ";
            sql += sBr + "		:data_name as data_name,  ";
            sql += sBr + "		:data_value as data_value, -- 內容 ";
            sql += sBr + "		:data_remark as data_remark -- 登入人員帳號 ";
            sql += sBr + "		from dual ";
            sql += sBr + "    ) b  ";
            sql += sBr + "    on ( ";
            sql += sBr + "		a.grp_code = b.grp_code  ";
            sql += sBr + "		and a.data_name = b.data_name  ";
            sql += sBr + "		and a.data_remark = b.data_remark ";
            sql += sBr + "    ) ";
            sql += sBr + "when matched then ";
            sql += sBr + "    update set data_value = to_char(:data_value) -- 內容 ";
            sql += sBr + "when not matched then ";
            sql += sBr + "    insert (grp_code, data_seq, data_name, data_value, data_remark) values ( ";
            sql += sBr + "	  'MM_PO_M',  ";
            sql += sBr + "	  ( ";
            sql += sBr + "	      SELECT NVL(MAX(DATA_SEQ), 0) + 1  ";
            sql += sBr + "	      FROM   PARAM_D WHERE 1=1  ";
            sql += sBr + "	      and GRP_CODE = 'MM_PO_M' ";
            sql += sBr + "	  ),  ";
            sql += sBr + "	  b.data_name,  ";
            sql += sBr + "	  to_char(b.data_value),  ";
            sql += sBr + "	  b.data_remark ";
            sql += sBr + " ";
            sql += sBr + ") ";


            String debug_sql = sql;
            debug_sql = debug_sql.Replace(":data_name", o.DATA_NAME);
            debug_sql = debug_sql.Replace(":data_value", o.DATA_VALUE);
            debug_sql = debug_sql.Replace(":data_remark", o.DATA_REMARK);


            return DBWork.Connection.Execute(sql, o, DBWork.Transaction);
        }


        //物料類別combox
        public IEnumerable<ComboItemModel> GetCheckAll_PH_VENDER_had_mail(String agen_nos)
        {
            var sql = @"";
            sql += sBr + "select -- 1 ";
            sql += sBr + "distinct a.agen_no, b.email, b.agen_namec, a.agen_no || ' ' || b.agen_namec TEXT, a.agen_no VALUE ";
            sql += sBr + "from MM_PO_M a, PH_VENDER b where 1=1  ";
            sql += sBr + "and a.agen_no = b.agen_no  ";
            sql += sBr + "and nvl(b.email,'N') = 'N' -- 找廠商沒email ";
            sql += sBr + "-- and nvl(b.email,'N') <> 'N'  ";
            sql += sBr + "-- and a.agen_no  in ('178','247','928','235','475','115','054') ";
            if (!String.IsNullOrEmpty(agen_nos))
                sql += sBr + "and a.agen_no in (" + agen_nos + ") ";
            sql += sBr + "order by a.agen_no ";
            sql += sBr + " ";
            sql += sBr + "-- 測試廠商沒email ";
            sql += sBr + "-- select * from PH_VENDER where rownum=1  -- 489 ";
            sql += sBr + "-- update PH_VENDER set email='chiaweili@ms.aidc.com.tw' where agen_no in ('178','247','928','235','475','115','054') ";
            sql += sBr + "-- update PH_VENDER set email='yuhsianghuang@ms.aidc.com.tw' where agen_no in ('178','247','928','235','475','115','054') ";
            sql += sBr + "-- update PH_VENDER set email=null where agen_no in ('247','928','235','475','115','054') ";
            sql += sBr + " ";

            String debug_sql = sql;
            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        } // 

        public int UPD_MM_PO_M_by_GetOver150KExcel(MM_PO_M o)
        {
            var sql = @"UPDATE MM_PO_M SET 
                            PO_STATUS = :PO_STATUS, 
                            UPDATE_TIME = SYSDATE, 
                            UPDATE_USER = :UPDATE_USER, 
                            UPDATE_IP = :UPDATE_IP 
                            WHERE PO_STATUS='80' 
                            and PO_NO = :PO_NO ";
            String debug_sql = sql;
            debug_sql = debug_sql.Replace(":PO_STATUS", o.PO_STATUS);
            debug_sql = debug_sql.Replace(":UPDATE_USER", o.UPDATE_USER);
            debug_sql = debug_sql.Replace(":UPDATE_IP", o.UPDATE_IP);
            debug_sql = debug_sql.Replace(":PO_NO", o.PO_NO);
      
            return DBWork.Connection.Execute(sql, o, DBWork.Transaction);
        } // 

        public int InsertSendMailLog(string po_no, string userId, string ip) {
            string sql = @"
                insert into SEND_MAIL_LOG(seq, send_user, send_user_email, receive_email,
                                          agen_no, mail_type, detail_value, 
                                          create_time, create_user, update_time, update_user, update_ip)
                select sendmaillog_seq.nextval, :userId, (select email from UR_ID where tuser = :userId), b.email, 
                       b.agen_no, '1', a.po_no, sysdate, :userId, sysdate, :userId, :ip
                  from MM_PO_M a, PH_VENDER b
                 where a.po_no=:po_no and a.agen_no = b.agen_no
            ";
            return DBWork.Connection.Execute(sql, new { po_no, userId, ip }, DBWork.Transaction);
        }

        public int ObsoleteSendMailLog(string po_no, string tuser, string procIp)
        {
            var sql = @"UPDATE SEND_MAIL_LOG
                           SET IS_SEND = 'Y', UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                         WHERE nvl(IS_SEND, 'N') = 'N' and DETAIL_VALUE = :PO_NO ";

            return DBWork.Connection.Execute(sql, new { PO_NO = po_no, UPDATE_USER = tuser, UPDATE_IP = procIp }, DBWork.Transaction);
        }

        public string CheckEmailExists(string userId) {
            string sql = @"
                select (case when nvl(email, 'N') = 'N' then 'N' else 'Y' end)
                  from UR_ID
                 where tuser = :userId
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { userId }, DBWork.Transaction);
        }

        public string GetAgenNo(string PO_NO)
        {
            string sql = @" SELECT AGEN_NO FROM MM_PO_M WHERE PO_NO = :PO_NO ";

            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { PO_NO }, DBWork.Transaction);
        }

        public string getHospCode()
        {
            string sql = @"
                select DATA_VALUE from PARAM_D where GRP_CODE = 'HOSP_INFO' and DATA_NAME = 'HospCode'
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }

        public string ChkAgenCnt(string PO_NOstr)
        {
            var p = new DynamicParameters();

            string[] PO_NOs = PO_NOstr.Split(',');
            p.Add(":PO_NO", PO_NOs);

            string sql = @"
                select (case when (select count(distinct AGEN_NO) from MM_PO_M where PO_NO in :PO_NO) > 1 then 'Y' else 'N' end)
                from dual
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, p, DBWork.Transaction);
        }

        public int UpdateStatusOnFax(string PO_NOstr, string tuser, string procIp)
        {
            var p = new DynamicParameters();

            string[] PO_NOs = PO_NOstr.Split(',');
            p.Add(":PO_NO", PO_NOs);
            p.Add(":UPDATE_USER", tuser);
            p.Add(":UPDATE_IP", procIp);

            string sql = @"
                UPDATE MM_PO_M
                           SET PO_STATUS = '85', UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                         WHERE PO_NO in :PO_NO AND PO_STATUS <> '87'
            ";
            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }
    }
}
