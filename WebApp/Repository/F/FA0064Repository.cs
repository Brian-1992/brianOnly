using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dapper;
using JCLib.DB;
using System.Data;
using WebApp.Models;
using WebApp.Models.F;

namespace WebApp.Repository.F
{
    public class FA0064Repository : JCLib.Mvc.BaseRepository
    {
        public FA0064Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<FA0064> GetData(string start_date, string end_date, bool isHospCode0)
        {
            string sql = string.Format(@"
                            select A.*,
                                   CNV_RATE1*DELI_QTY as CNV_DELI_QTY,
                                   CNV_RATE1*TR_INV_QTY as CNV_TR_INV_QTY,
                                   CNV_RATE1*APPQTY as CNV_APPQTY,
                                   CNV_RATE1*PINVQTY as CNV_PINVQTY,
                                   (case when DECLARE_UI is not null
                                       then DECLARE_UI
                                       else BASE_UNIT
                                    end) as DECLARE_UI1
                              from (select a.mmcode,
                                           a.mmname_e,
                                           a.mmname_c,
                                           a.E_SCIENTIFICNAME,
                                           b.med_license,
                                           a.e_restrictcode,
                                           a.cancel_id ,
                                           (select nvl(sum(DELI_QTY), 0) 
                                              from MM_PO_INREC
                                             where STATUS='Y'
                                               and TWN_DATE(ACCOUNTDATE)>=:start_date  --查詢條件日期(起)
                                               and TWN_DATE(ACCOUNTDATE)<=:end_date  --查詢條件日期(迄)
                                               and mmcode=a.mmcode
                                           ) as DELI_QTY,
                                          (select sum(TR_INV_QTY)
                                             from (select MMCODE,TR_INV_QTY, 
                                                          (select TOWH from ME_DOCM where DOCNO=TR_DOCNO) TOWH, --入庫別
                                                          (select FRWH from ME_DOCM where DOCNO=TR_DOCNO) FRWH --出庫別
                                                     from MI_WHTRNS
                                                    where 1=1
                                                      and TR_MCODE NOT IN ('WAYI','WAYO')
                                                      and WH_NO = WHNO_ME1
                                                      and TR_IO = 'O'
                                                      and (
                                                           (TWN_DATE(TR_DATE)>=:start_date and TWN_DATE(TR_DATE)<=:end_date and TR_MCODE NOT IN ('WAYI','WAYO','USEO','BAKI','ADJO')) OR  --TR_DATE：查詢條件日期(起)、查詢條件日期(迄)
                                                           (TWN_DATE(TR_DATE)>=:start_date and TWN_DATE(TR_DATE)<=:end_date and TR_MCODE='BAKI' and TR_DOCTYPE<>'RS') OR  --TR_DATE：查詢條件日期(起)、查詢條件日期(迄)
                                                           (TWN_DATE(TR_DATE)>=:start_date and TWN_DATE(TR_DATE)<=:end_date and TR_MCODE='ADJO' and TR_DOCTYPE<>'RR') OR  --TR_DATE：查詢條件日期(起)、查詢條件日期(迄)
                                                           (SUBSTR(TR_DOCNO,1,7)>=:start_date and SUBSTR(TR_DOCNO,1,7)<=:end_date and TR_MCODE='USEO') OR   --SUBSTR(TR_DOCNO,1,7)：查詢條件日期(起)、查詢條件日期(迄)
                                                           (SUBSTR(TR_DOCNO,1,7)>=:start_date and SUBSTR(TR_DOCNO,1,7)<=:end_date and TR_MCODE='BAKI' and TR_DOCTYPE='RS') OR  --SUBSTR(TR_DOCNO,1,7)：查詢條件日期(起)、查詢條件日期(迄)
                                                           (SUBSTR(TR_DOCNO,1,7)>=:start_date and SUBSTR(TR_DOCNO,1,7)<=:end_date and TR_MCODE='ADJO' and TR_DOCTYPE='RR')  --SUBSTR(TR_DOCNO,1,7)：查詢條件日期(起)、查詢條件日期(迄)
                                                          )
                                                      and TR_DOCTYPE in ('MS','MR')
                                                    ) p
                                              where FRWH=WHNO_ME1 and TOWH in ('TOPD') and p.mmcode=a.mmcode
                                            ) as TR_INV_QTY,    
                                           (select sum(p.appqty)  --appqty：table內容負值
                                              from ME_DOCD p
                                              join
                                                  (select DOCNO
                                                     from ME_DOCM
                                                    where DOCTYPE='EM'
                                                      and MAT_CLASS='01'
                                                      and FRWH=WHNO_ME1
                                                      and TWN_DATE(UPDATE_TIME)>=:start_date  --UPDATE_TIME：查詢條件日期(起)
                                                      and TWN_DATE(UPDATE_TIME)<=:end_date  --UPDATE_TIME：查詢條件日期(迄)
                                                      and FLOWID='1099'  --'換貨完成'
                                                   ) q
                                                on p.DOCNO=q.DOCNO
                                             where p.mmcode=a.mmcode) as APPQTY,
                                           DAY_PINVQTY_ALLWHNO(:end_date,a.mmcode) as PINVQTY,  --查詢條件日期(迄)                                           		
                                           (select (case when UI_CHANAME is null 
                                                        then UI_ENGNAME 
                                                     else UI_CHANAME
                                                    end) 
                                               from MI_UNITCODE
                                              where UNIT_CODE = trim(a.base_unit)) as base_unit,
                                           b.CNV_RATE ,    
                                           (case when b.CNV_RATE is not null then b.CNV_RATE
                                               else 1
                                            end) as CNV_RATE1,
                                          b.DECLARE_UI 
                                     from MI_MAST a
                                     left join CD_DCLUICNV b
                                       on (a.mmcode=b.mmcode and b.dclyr = extract( year from twn_todate(:start_date))-1911)
                                    where 1=1
                                      and a.mat_class = '01'
                                      and a.e_restrictcode between '1' and '4'  --管制藥等級1~4
                                      and a.E_PARCODE in ('0', '1')             --排除子藥(2)
                                      {0}
                                  ) A 	
                            where not(CANCEL_ID='Y' and DELI_QTY=0 and PINVQTY=0)
             ", isHospCode0 ? "and substr(a.mmcode, 1,3) in ('005', '006', '007')" : string.Empty);
            return DBWork.PagingQuery<FA0064>(sql, new { start_date, end_date });
        }

        public DataTable Excel(string start_date, string end_date, bool isHospCode0)
        {

            string sql = string.Format(@"
                            select A.MMCODE as 院內碼,
                                   A.MMNAME_E as 英文品名,
                                   A.MMNAME_C as 中文品名,
                                   A.E_SCIENTIFICNAME as 成份名稱,          
                                   A.MED_LICENSE as 許可證字號,
                                   A.E_RESTRICTCODE as 管制級別,
                                   A.CANCEL_ID as 是否全院停用,
                                   A.DELI_QTY as 進貨量,
                                   A.TR_INV_QTY as 轉讓北門,
                                   A.APPQTY as 退貨量,
                                   A.PINVQTY as 結存量,
                                   A.BASE_UNIT as 計量單位,
                                   A.CNV_RATE as 換算率,
                                   CNV_RATE1*DELI_QTY as 換算後進貨量,
                                   CNV_RATE1*TR_INV_QTY as 換算後轉讓北門,
                                   CNV_RATE1*APPQTY as 換算後退貨量,
                                   CNV_RATE1*PINVQTY as 換算後結存量,
                                   (case when DECLARE_UI is not null
                                       then DECLARE_UI
                                       else BASE_UNIT
                                    end) as 申報計量單位
                              from (select a.mmcode,
                                           a.mmname_e,
                                           a.mmname_c,
                                           a.E_SCIENTIFICNAME,
                                           b.med_license,
                                           a.e_restrictcode,
                                           a.cancel_id ,
                                           (select nvl(sum(DELI_QTY), 0) 
                                              from MM_PO_INREC
                                             where STATUS='Y'
                                               and TWN_DATE(ACCOUNTDATE)>=:start_date  --查詢條件日期(起)
                                               and TWN_DATE(ACCOUNTDATE)<=:end_date  --查詢條件日期(迄)
                                               and mmcode=a.mmcode
                                           ) as DELI_QTY,
                                          (select sum(TR_INV_QTY)
                                             from (select MMCODE,TR_INV_QTY, 
                                                          (select TOWH from ME_DOCM where DOCNO=TR_DOCNO) TOWH, --入庫別
                                                          (select FRWH from ME_DOCM where DOCNO=TR_DOCNO) FRWH --出庫別
                                                     from MI_WHTRNS
                                                    where 1=1
                                                      and TR_MCODE NOT IN ('WAYI','WAYO')
                                                      and WH_NO = WHNO_ME1
                                                      and TR_IO = 'O'
                                                      and (
                                                           (TWN_DATE(TR_DATE)>=:start_date and TWN_DATE(TR_DATE)<=:end_date and TR_MCODE NOT IN ('WAYI','WAYO','USEO','BAKI','ADJO')) OR  --TR_DATE：查詢條件日期(起)、查詢條件日期(迄)
                                                           (TWN_DATE(TR_DATE)>=:start_date and TWN_DATE(TR_DATE)<=:end_date and TR_MCODE='BAKI' and TR_DOCTYPE<>'RS') OR  --TR_DATE：查詢條件日期(起)、查詢條件日期(迄)
                                                           (TWN_DATE(TR_DATE)>=:start_date and TWN_DATE(TR_DATE)<=:end_date and TR_MCODE='ADJO' and TR_DOCTYPE<>'RR') OR  --TR_DATE：查詢條件日期(起)、查詢條件日期(迄)
                                                           (SUBSTR(TR_DOCNO,1,7)>=:start_date and SUBSTR(TR_DOCNO,1,7)<=:end_date and TR_MCODE='USEO') OR   --SUBSTR(TR_DOCNO,1,7)：查詢條件日期(起)、查詢條件日期(迄)
                                                           (SUBSTR(TR_DOCNO,1,7)>=:start_date and SUBSTR(TR_DOCNO,1,7)<=:end_date and TR_MCODE='BAKI' and TR_DOCTYPE='RS') OR  --SUBSTR(TR_DOCNO,1,7)：查詢條件日期(起)、查詢條件日期(迄)
                                                           (SUBSTR(TR_DOCNO,1,7)>=:start_date and SUBSTR(TR_DOCNO,1,7)<=:end_date and TR_MCODE='ADJO' and TR_DOCTYPE='RR')  --SUBSTR(TR_DOCNO,1,7)：查詢條件日期(起)、查詢條件日期(迄)
                                                          )
                                                      and TR_DOCTYPE in ('MS','MR')                                
                                                    ) p
                                              where FRWH=WHNO_ME1 and TOWH in ('TOPD')  and p.mmcode=a.mmcode
                                            ) as TR_INV_QTY,    
                                           (select sum(p.appqty)  --appqty：table內容負值
                                              from ME_DOCD p
                                              join
                                                  (select DOCNO
                                                     from ME_DOCM
                                                    where DOCTYPE='EM'
                                                      and MAT_CLASS='01'
                                                      and FRWH=WHNO_ME1
                                                      and TWN_DATE(UPDATE_TIME)>=:start_date  --UPDATE_TIME：查詢條件日期(起)
                                                      and TWN_DATE(UPDATE_TIME)<=:end_date  --UPDATE_TIME：查詢條件日期(迄)
                                                      and FLOWID='1099'  --'換貨完成'
                                                   ) q
                                                on p.DOCNO=q.DOCNO
                                             where p.mmcode=a.mmcode) as APPQTY,
                                           DAY_PINVQTY_ALLWHNO(:end_date,a.mmcode) as PINVQTY,  --查詢條件日期(迄)
                                           (select (case when UI_CHANAME is null 
                                                        then UI_ENGNAME 
                                                     else UI_CHANAME
                                                    end) 
                                               from MI_UNITCODE
                                              where UNIT_CODE = trim(a.base_unit)) as base_unit,
                                           b.CNV_RATE ,    
                                           (case when b.CNV_RATE is not null then b.CNV_RATE
                                               else 1
                                            end) as CNV_RATE1,
                                          b.DECLARE_UI 
                                     from MI_MAST a
                                     left join CD_DCLUICNV b
                                       on (a.mmcode=b.mmcode and b.dclyr = extract( year from twn_todate(:start_date))-1911)
                                    where 1=1
                                      and a.mat_class = '01'
                                      and a.e_restrictcode between '1' and '4'  --管制藥等級1~4
                                      and a.E_PARCODE in ('0', '1')             --排除子藥(2)
                                      {0}
                                  ) A 	
                            where not(CANCEL_ID='Y' and DELI_QTY=0 and PINVQTY=0)
                   ", isHospCode0 ? "and substr(a.mmcode, 1,3) in ('005', '006', '007')" : string.Empty);
            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, new { start_date, end_date }))
                dt.Load(rdr);

            return dt;
        }


        #region 2022-06-02 管制藥申報換算率基本檔
        private string GetDcluicnvSql(string dclyr, string mmcode, string med_license)
        {
            string sql = @"
                select DCLYR,        --申報年度
                       MMCODE,       --院內碼
                       MED_LICENSE,  --許可證字號
                       DECLARE_UI,   --申報計量單位
                       CNV_RATE,     --換算率
                       twn_time(CREATE_TIME) as CREATE_TIME,  --建立時間
                       user_name(CREATE_USER) as CREATE_USER   --建立人員
                  from CD_DCLUICNV
                 where 1=1
                   and DCLYR=:dclyr
            ";
            if (string.IsNullOrEmpty(mmcode) == false)
            {
                sql += @"  and mmcode = :mmcode";
            }
            if (string.IsNullOrEmpty(med_license) == false)
            {
                sql += @"  and med_license = :med_license";
            }
            return sql;
        }

        public IEnumerable<CD_DCLUICNV> GetDcluicnvs(string dclyr, string mmcode, string med_license)
        {
            string sql = GetDcluicnvSql(dclyr, mmcode, med_license);
            return DBWork.PagingQuery<CD_DCLUICNV>(sql, new { dclyr, mmcode, med_license }, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, int page_index, int page_size, string sorters, bool isHospCode0)
        {

            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E 
                          FROM MI_MAST A 
                         WHERE 1=1 
                           AND A.MAT_CLASS = '01'
                           and e_restrictcode between '1' and '4'  --管制藥等級1~4
                           and E_PARCODE in ('0', '1')             --排除子藥,子藥=2 
                           and cancel_id<>'Y'  --排除(是否全院停用='Y') 
                           {1} {2}";

            if (p0 != "")
            {
                sql = string.Format(sql,
                     "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,",
                     (isHospCode0 ? "and substr(mmcode, 1,3) in ('005', '006', '007')" : string.Empty),
                     @"   AND (A.MMCODE LIKE :MMCODE OR MMNAME_E LIKE UPPER(:MMNAME_E) OR MMNAME_C LIKE :MMNAME_C)");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);
                p.Add(":MMCODE", string.Format("{0}%", p0));
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "", (isHospCode0 ? "and substr(mmcode, 1,3) in ('005', '006', '007')" : string.Empty), "");
                sql += " ORDER BY MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public int Copy(string fromYear, string toYear, string userId, string updateIp)
        {
            string sql = @"
                insert into cd_dcluicnv (dclyr, mmcode, med_license, declare_ui, cnv_rate,
                                         create_time, create_user, update_ip)
                select :toYear, mmcode, med_license, declare_ui, cnv_rate,
                       sysdate, :userId, :updateIp
                  from CD_DCLUICNV
                 where dclyr = :fromYear
            ";
            return DBWork.Connection.Execute(sql, new { fromYear, toYear, userId, updateIp }, DBWork.Transaction);
        }

        public DataTable GetDcluicnvsExcel(string dclyr, string mmcode, string med_license)
        {
            string sql = string.Format(@"
                select DCLYR as 申報年度,
                       MMCODE as 院內碼,
                       MED_LICENSE as 許可證字號,
                       DECLARE_UI as 申報計量單位,
                       CNV_RATE as 換算率,
                       CREATE_TIME as 建立時間,  --建立時間
                       CREATE_USER as 建立人員   --建立人員
                  from (
                         {0}
                       )
            ", GetDcluicnvSql(dclyr, mmcode, med_license));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, new { dclyr, mmcode, med_license }))
                dt.Load(rdr);

            return dt;
        }

        public int DeleteCdDcluicnv(string dclyrs)
        {
            string sql = string.Format(@"
                delete from CD_DCLUICNV
                 where dclyr in ( {0} )
            ", dclyrs);

            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }

        public int InsertCdDcluicnv(CD_DCLUICNV item)
        {
            string sql = @"
                insert into CD_DCLUICNV (DCLYR, MMCODE, MED_LICENSE, DECLARE_UI, CNV_RATE,
                                         CREATE_TIME, CREATE_USER, UPDATE_IP)
                values (
                    :DCLYR, :MMCODE, LPAD(:MED_LICENSE, 6, '0'), :DECLARE_UI, :CNV_RATE,
                    sysdate, :CREATE_USER, :UPDATE_IP
                )
            ";
            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);
        }

        public int UpdateCdDcluicnv(CD_DCLUICNV item)
        {
            string sql = @"
                update CD_DCLUICNV
                   set MED_LICENSE = LPAD(:MED_LICENSE, 6, '0'),
                       DECLARE_UI = :DECLARE_UI, 
                       CNV_RATE = :CNV_RATE,
                       UPDATE_IP = :UPDATE_IP
                 where DCLYR = :DCLYR
                   and MMCODE = :MMCODE
            ";
            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);
        }

        public bool CheckExists(string dclyr, string mmcode)
        {
            string sql = @"
                select 1 from CD_DCLUICNV
                 where dclyr = :dclyr
                   and mmcode = :mmcode
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { dclyr, mmcode }, DBWork.Transaction) != null;
        }
        #endregion

        public bool CheckIsSameYear(string start_date, string endt_date)
        {
            string sql = @"
                with temp as (
                     select (extract( year from twn_todate(:start_date))-1911) as startYear, 
                            (extract( year from twn_todate(:endt_date))-1911) as endYear
                       from dual
                )
                select (case when startYear = endYear then 'Y' else 'N' end) as result 
                  from temp
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { start_date, endt_date }, DBWork.Transaction) == "Y";
        }

        public string GetHospCode()
        {
            string sql = @"
                select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode'
            ";

            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }
    }
}