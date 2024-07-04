using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.F
{
    public class FA0063 : JCLib.Mvc.BaseModel
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

        List<FA0063> List { get; set; }
    }
    public class FA0063Repository : JCLib.Mvc.BaseRepository
    {

        public FA0063Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        private string GetSql() {
            string sql = @"
                select F01, F02, F03, F30, F04,F05,F06,F07,F09,F10, F11,F12,HIS_英文品名, 
       F13, F14, F15, F16, F17, F18, sum(F08) as F08,
        sum(F19) as F19, sum(F20) as F20,
       F21, sum(F22) as F22, F23, F24, F25, F26, F27, sum(F28) as F28,
       F29, F31, F32, F33, F34
from (
select '14' as F01, --格式
                       (select data_value from PARAM_D 
                         where grp_code = 'FA0063'
                           and data_name = 'HOSP_ID'
                           and rownum = 1) as F02,  --醫事服務機構代號
                       twn_yyymm(e.accountdate) as F03, --申報資料年月
                       a.mmcode as F30, --院內碼
                       b.M_NHIKEY as F04,   -- 藥品代碼
                       (select f.UNI_NO from PH_VENDER f,MM_PO_M g where f.agen_no = g.agen_no and g.po_no=e.po_no) as F05, --藥商統一編號 
                       a.INVOICE as F06,    --發票號碼
                       twn_date(a.INVOICE_DT) as F07, --發票日期  
                       e.DELI_QTY * c.set_ratio as F08,   --發票購買藥品數量A
                       b.BASE_UNIT as F09,  -- HIS_計量單位
                       b.e_specnunit as F10,  --HIS_規格量及單位
                       b.e_compunit as F11, --HIS_成份
                       b.E_DRUGFORM as F12, --HIS_藥品劑型
                       b.mmname_e as HIS_英文品名,
                       c.E_SPEC as F13, -- NHI_規格量
                       c.E_UNIT as F14, --NHI_規格單位
                       c.E_DRUGFORM as F15,   -- NHI_藥品劑型    
                       0 as F16, --贈品數量附贈之藥品數量B
                       0 as F17, --贈品數量藥品耗損數量C
                       0 as F18, --退貨數量D,
                       e.DELI_QTY * c.set_ratio as F19,
                       -- e.DELI_QTY as F19,  --實際購買數量E,
                       round(e.PO_PRICE * e.DELI_QTY) as F20, --發票金額F元,
                       0 as F21, --退貨金額G元,   
                       e.PO_PRICE ,e.DISC_CPRICE,d.DISC_COST_UPRICE,
                       (case when d.ISWILLING='是' and e.PO_QTY>=d.DISCOUNT_QTY
                               then (round(e.PO_PRICE * e.DELI_QTY) - round(d.DISC_COST_UPRICE * e.DELI_QTY))
                               else (round(e.PO_PRICE * e.DELI_QTY) - round(e.DISC_CPRICE * e.DELI_QTY))
                        end) as F22, --折讓金額折讓單金額H元,                     
                       0 as F23, --折讓金額指定捐贈I元,
                       0 as F24, --折讓金額藥商提撥管理費J元,
                       0 as F25, --折讓金額藥商提撥研究費K元,
                       0 as F26, --折讓金額藥商提撥補助醫師出國會議L元,
                       0 as F27, --折讓金額其他與本交易相關之附帶利益M元,
                       (round(e.PO_PRICE * e.DELI_QTY) - 0 -
                       (case when d.ISWILLING='是' and e.PO_QTY>=d.DISCOUNT_QTY
                             then (round(e.PO_PRICE * e.DELI_QTY) - round(d.DISC_COST_UPRICE * e.DELI_QTY))
                             else (round(e.PO_PRICE * e.DELI_QTY) - round(e.DISC_CPRICE * e.DELI_QTY))
                        end) - 0 - 0 - 0 - 0 - 0) as F28, --購藥總金額N元,  --N=F-G-H-I-J-K-L-M                  
                       (case when nvl(a.INVOICE, 'N') = 'N' then '' 
                             when substr(a.INVOICE,1,1)>'9'
                             then '0' else '2'
                         end) as F29, --發票註記   
                       a.po_no as  F31,     --訂單編號
                       a.transno as F32,    --資料流水號
                       (select agen_no from MM_PO_M where po_no = a.po_no) as F33, -- 廠商代碼
                       twn_date(e.accountdate) as F34
                  from PH_INVOICE a  --MM_PO_INREC a
                  join (select mmcode, M_NHIKEY, mmname_e, mmname_c, e_specnunit,
                               e_compunit, BASE_UNIT,E_DRUGFORM, m_agenno
                         from MI_MAST where M_NHIKEY is not null
                       ) b
                    on a.mmcode = b.mmcode
                  join NHI_PSURV_LIST c
                    on b.M_NHIKEY=c.M_NHIKEY
                  join (select po_no, mmcode, iswilling, discount_qty, disc_cost_uprice
                         from MM_PO_D
                       ) d
                    on a.po_no=d.po_no and a.mmcode=d.mmcode   
                  join(select po_no,mmcode,accountdate,INVOICE,deli_qty,PO_PRICE,DISC_CPRICE,PO_QTY from mm_po_inrec
                        where wh_no = WHNO_ME1 and status='Y' and transkind='111'
                      ) e  
                    on a.po_no=e.po_no and a.mmcode=e.mmcode 
                    --AND a.INVOICE=e.INVOICE                
                 where 1=1
                   and exists(select 1 from mm_po_inrec where po_no=a.po_no and mmcode=a.mmcode
                   -- AND INVOICE=a.INVOICE   
                                 and wh_no = WHNO_ME1 and status='Y' and transkind='111')
                   and a.deli_qty <> 0
                   and twn_yyymm(e.accountdate) between :start_ym and :end_ym  --進貨日期 
                   and a.deli_status='C'  --已交貨
                   and a.status='N'  --申購
                 -- and a.mmcode='005PAN08'
                  --'005CAL04'
                  --'005ALP04' 
                   --and a.mmcode='005BAY02'
          )
          group by F01, F02, F03, F30, F04,F05,F06,F07,F09,F10, F11,F12,HIS_英文品名, 
                   F13, F14, F15, F16, F17, F18, F21,F23, F24, F25, F26, F27,
                   F29, F31, F32, F33, F34

            ";
            return sql;
        }

        public IEnumerable<FA0063> GetAll(string start_ym, string end_ym, bool isPaging) {
            string sql = GetSql();
            if (isPaging) {
                return DBWork.PagingQuery<FA0063>(sql, new { start_ym, end_ym }, DBWork.Transaction);
            }
            return DBWork.Connection.Query<FA0063>(sql, new { start_ym, end_ym }, DBWork.Transaction);
        }

        public string GetLastUploadTime() {
            string sql = @"
                select twn_time(max(create_time))
                  from NHI_PSURV_LIST
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }

        public int DeleteNhiPsurvList() {
            string sql = @"
                delete from NHI_PSURV_LIST
            ";
            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }

        public int InsertNhiPsurvList(NHI_PSURV_LIST item) {
            string sql = @"
                insert into NHI_PSURV_LIST(ps_seq, m_nhikey, mmname_c, mmname_e, e_spec,
                                           e_unit, e_drugform, agen_name,
                                           create_time, create_user, update_ip)
                values (:ps_seq, :m_nhikey, :mmname_c, :mmname_e, :e_spec,
                                           :e_unit, :e_drugform, :agen_name,
                                           sysdate, :create_user, :update_ip)
            ";
            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);
        }

        public IEnumerable<NHI_PSURV_LIST> GetNhipsurvlist(bool starOnly, bool isPaging) {
            string sql = @"
                select a.ps_seq, a.m_nhikey, a.mmname_c, a.mmname_e, 
                       a.e_spec, a.e_unit, a.e_drugform,  
                       nvl(a.rcm_ratio, '') as rcm_ratio, nvl(a.set_ratio, '') as set_ratio,
                       b.mmcode, b.e_specnunit as his_e_specnunit, b.e_compunit as his_e_compunit, 
                       b.base_unit as his_base_unit,b.e_drugform as his_e_drugform
                  from NHI_PSURV_LIST a
                  join (select mmcode, M_NHIKEY, mmname_e, mmname_c, e_specnunit,
                               e_compunit, BASE_UNIT,E_DRUGFORM
                          from MI_MAST where M_NHIKEY is not null) b
                          -- from MI_MAST_20220118 where M_NHIKEY is not null) b  --AIDC test table
                    on a.M_NHIKEY = b.M_NHIKEY
                 where 1=1
            ";
            if (starOnly) {
                sql += @"  and a.rcm_ratio = '*'";
            }
            sql += " order by a.ps_seq";
            if (isPaging) {
                return DBWork.PagingQuery<NHI_PSURV_LIST>(sql, DBWork.Transaction);
            }
            return DBWork.Connection.Query<NHI_PSURV_LIST>(sql, DBWork.Transaction);
        }

        public int UpdateNhipsurvlistRatios(string ps_seq, string rcm_ratio, string set_ratio)
        {
            string sql = @"
                update NHI_PSURV_LIST
                   set rcm_ratio = :rcm_ratio,
                       set_ratio = :set_ratio
                 where ps_seq = :ps_seq
            ";
            return DBWork.Connection.Execute(sql, new { ps_seq, rcm_ratio, set_ratio }, DBWork.Transaction);

        }

        public int ReCal() {
            string sql = @"
               update NHI_PSURV_LIST
                   set rcm_ratio = null,
                       set_ratio = null
            ";
            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }
        public bool CheckSetRatioEmpty() {
            string sql = @"
                select 1 from NHI_PSURV_LIST
                 where set_ratio is null
                   and rcm_ratio is not null
            ";
            return DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction) != null;
        }

        public string GetHospId() {
            string sql = @"
                select data_value from PARAM_D
                 where grp_code = 'FA0063'
                   and data_name = 'HOSP_ID'
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }

        public int UpdateHospId1(string hospId) {
            string sql = @"
                merge into PARAM_M a
                using (select 'FA0063' as GRP_CODE,'醫事服務機構代號' as GRP_USE from dual) b
                   on (a.GRP_CODE=b.GRP_CODE and a.GRP_USE=b.GRP_USE)
                 when matched then
                     update set GRP_DESC='價量調查報表'
                 when not matched then
                     insert(GRP_CODE,GRP_DESC,GRP_USE)
                      values('FA0063','價量調查報表','醫事服務機構代號')
            ";
            return DBWork.Connection.Execute(sql, new { hospId }, DBWork.Transaction);
        }
        public int UpdateHospId2(string hospId) {
            string sql = @"
                 merge into PARAM_D a
                using (select 'FA0063' as GRP_CODE, 1 as DATA_SEQ, 'HOSP_ID' as DATA_NAME from dual) b
                   on (a.GRP_CODE=b.GRP_CODE and a.DATA_SEQ=b.DATA_SEQ and a.DATA_NAME=b.DATA_NAME)
                 when matched then
                  update set DATA_VALUE=:hospId
                when not matched then
                  insert(GRP_CODE,DATA_SEQ,DATA_NAME,DATA_VALUE,DATA_DESC)
                    values('FA0063',1,'HOSP_ID',:hospId,'醫事服務機構代號')
            ";
            return DBWork.Connection.Execute(sql, new { hospId }, DBWork.Transaction);
        }

        public IEnumerable<string> GetUnitCnvs(string ui_from, string value, string his_base_unit) {
            string sql = @"
                 select (case when mod(coeff_to, coeff_from) <> 0 
                              then to_char(nvl(trim(:value), 0) * (coeff_to / coeff_from), 'FM9999999990.999') || ' ' || ui_to || '/' || :his_base_unit
                              else to_char(nvl(trim(:value), 0) * (coeff_to / coeff_from)) || ' ' || ui_to || '/' || :his_base_unit
                         end)
                  from BASEUNITCNV
                 where ui_from  = :ui_from
                union
                 select (case when mod(coeff_to, coeff_from) <> 0 
                              then to_char(nvl(trim(:value), 0) * (coeff_to / coeff_from), 'FM9999999990.999') || ' ' || ui_to
                              else to_char(nvl(trim(:value), 0) * (coeff_to / coeff_from)) || ' ' || ui_to
                         end)
                  from BASEUNITCNV
                 where ui_from  = :ui_from
                union 
                 select :value || ' ' || :ui_from from dual
            ";
            return DBWork.Connection.Query<string>(sql, new { ui_from, value, his_base_unit }, DBWork.Transaction);
        }

        #region GetExcel
        public DataTable GetExcel(string start_ym, string end_ym) {
            DynamicParameters p = new DynamicParameters();

            string sql = string.Format(@"
                select F01, F02, F03,
                       F04, F05, F06,
                       F07, F08, F16,
                       F17, F18, F19,
                       F20, F21, F22,
                       F23, F24, 
                       F25, F26, 
                       F27, F28, 
                       F29
                  from (
                        {0}
                       )
            ", GetSql());

            p.Add(":start_ym", string.Format("{0}", start_ym));
            p.Add(":end_ym", string.Format("{0}", end_ym));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        #endregion

        #region txt
        public IEnumerable<string> GetTxtYmList(string start_ym, string end_ym) {
            string sql = @"
                with t as (
                         select twn_todate(:start_ym||'01') start_date, twn_todate(:end_ym||'01') end_date from dual
                )
                select twn_yyymm(add_months(trunc(start_date,'mm'),level - 1)) 
                  from t
               connect by trunc(end_date,'mm') >= add_months(trunc(start_date,'mm'),level - 1)
            ";
            return DBWork.Connection.Query<string>(sql, new { start_ym, end_ym }, DBWork.Transaction);
        }
        #endregion

        #region combo
        public IEnumerable<COMBO_MODEL> GetYMCombo() {
            string sql = @"
                with t as (
                         select date '2020-03-01' start_date, add_months( sysdate, -1) end_date from dual
                )
                select twn_yyymm(add_months(trunc(start_date,'mm'),level - 1))  as value,
                       twn_yyymm(add_months(trunc(start_date,'mm'),level - 1))  as text,
                       level
                  from t
               connect by trunc(end_date,'mm') >= add_months(trunc(start_date,'mm'),level - 1)
                 order by value desc
            ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }
        #endregion

        #region 單位設定
        public IEnumerable<BASEUNITCNV> GetBaseUnitCnvs(string ui_from) {
            string sql = @"
                select * from BASEUNITCNV
                 where 1=1
            ";
            if (string.IsNullOrEmpty(ui_from) == false) {
                sql += "  and ui_from = :ui_from";
            }
            return DBWork.PagingQuery<BASEUNITCNV>(sql, new { ui_from }, DBWork.Transaction);
        }
        public bool CheckBaseunitcnvExists(string ui_from, string ui_to) {
            string sql = @"
                select 1 from BASEUNITCNV
                 where ui_from  = :ui_from
                   and ui_to = :ui_to
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { ui_from, ui_to }, DBWork.Transaction)  != null;
        }
        public int CreateBaseunitcnv(BASEUNITCNV item) {
            string sql = @"
                insert into BASEUNITCNV(ui_from, ui_to, coeff_from, coeff_to, cnvnote,
                                         create_time, create_user, update_time, update_user, update_ip)
                values(:ui_from, :ui_to, :coeff_from, :coeff_to, :cnvnote,
                       sysdate, :create_user, sysdate, :update_user, :update_ip)
            ";
            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);
        }
        public int UpdateBaseunitcnv(BASEUNITCNV item) {
            string sql = @"
                update BASEUNITCNV
                   set coeff_from = :coeff_from,
                       coeff_to = :coeff_to,
                       cnvnote = :cnvnote
                 where ui_from = :ui_from
                   and ui_to = :ui_to
            ";
            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);
        }
        public int DeleteBaseunitcnv(BASEUNITCNV item) {
            string sql = @"
                delete from BASEUNITCNV
                 where ui_from = :ui_from
                   and ui_to = :ui_to
            ";
            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);
        }
        #endregion
    }
}