using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApi.Models;

namespace WebApi.Repository
{
    public class WebApiRepository : JCLib.Mvc.BaseRepository
    {
        public WebApiRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public WhMmInvqty GetInvqty(string wh_no, string mmcode)
        {
            DynamicParameters p = new DynamicParameters();
            string sql = @"select a.wh_no, a.mmcode, 
                                  wh_name(a.wh_no) as wh_name,
                                  (select inv_qty from MI_WHINV 
                                    where wh_no = a.wh_no and mmcode = a.mmcode) as total_inv_qty,
                                  a.low_qty,
                                  a.safe_day,
                                  a.safe_qty,                                  
                                  a.high_qty,
                                  b.PARENTSTOCKCODE,
                                  b.reserveFlag,  
                                  a.NOWCONSUMEFLAG
                             from MI_WINVCTL a
                             left join HIS_STKCTDM b on (a.wh_no = b.stockcode and a.mmcode = b.skorderCode)
                            where 1=1
                              and a.wh_no = :wh_no
                              and a.mmcode = :mmcode";

            p.Add("wh_no", wh_no);
            p.Add("mmcode", mmcode);

            return DBWork.Connection.QueryFirstOrDefault<WhMmInvqty>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<LotExpInv> GetWexpInv(string wh_no, string mmcode)
        {
            DynamicParameters p = new DynamicParameters();
            string sql = @"
                select distinct c.lot_no,c.exp_date, c.inv_qty,c.agen_no, c.agen_name
                  from (
                        select a.wh_no,a.mmcode, a.lot_no, a.exp_date, a.inv_qty
                                , b.agen_no  agen_no, agen_name(b.agen_no) as agen_name
                          from MI_WEXPINV a
                          left join BC_CS_ACC_LOG b on (a.mmcode = b.mmcode and a.lot_no = b.lot_no and a.exp_date=b.exp_date and b.agen_no is not null)
                         where a.wh_no=:wh_no
                           and a.mmcode=:mmcode
                  ) c
            ";

            p.Add("wh_no", wh_no);
            p.Add("mmcode", mmcode);

            return DBWork.Connection.Query<LotExpInv>(sql, p, DBWork.Transaction);
        }

        public WhMmInvqty GetWexpInvWhenCTLNULL(string wh_no, string mmcode)
        {
            DynamicParameters p = new DynamicParameters();
            string sql = @"
                                  select a.wh_no, a.mmcode, 
                                  wh_name(:wh_no) as wh_name,
                                  inv_qty as total_inv_qty,
                                  0 as low_qty,
                                  0 as safe_day,
                                  0 as safe_qty,                                  
                                  0 as high_qty,
                                  b.PARENTSTOCKCODE,
                                  b.reserveFlag,  
                                  '' NOWCONSUMEFLAG
                             from MI_WHINV a
                             left join HIS_STKCTDM b on (a.wh_no = b.stockcode and a.mmcode = b.skorderCode)
                            where 1=1
                              and a.wh_no = :wh_no
                              and a.mmcode = :mmcode
            ";

            p.Add("wh_no", wh_no);
            p.Add("mmcode", mmcode);

            return DBWork.Connection.QueryFirstOrDefault<WhMmInvqty>(sql, p, DBWork.Transaction);
        }

        public WhMmInvqty GetWexpInvWhenINVNULL(string wh_no, string mmcode)
        {
            DynamicParameters p = new DynamicParameters();
            string sql = @"
select :wh_no as wh_no, :mmcode as mmcode, wh_name(:wh_no) as wh_name,
                                  0 as total_inv_qty,
                                  0 as low_qty,
                                  0 as safe_day,
                                  0 as safe_qty,                                  
                                  0 as high_qty,
(select PARENTSTOCKCODE from HIS_STKCTDM where stockcode = :wh_no and skorderCode = :mmcode ) as PARENTSTOCKCODE,
(select reserveFlag from HIS_STKCTDM where stockcode = :wh_no and skorderCode = :mmcode ) as reserveFlag,
'' NOWCONSUMEFLAG
from dual
            ";

            p.Add("wh_no", wh_no);
            p.Add("mmcode", mmcode);

            return DBWork.Connection.QueryFirstOrDefault<WhMmInvqty>(sql, p, DBWork.Transaction);
        }


        public IEnumerable<MmcodeInvqty> GetMmcodeInvqty(string mmcode)
        {
            string sql = @"select mmcode, wh_no, inv_qty
                             from MI_WHINV
                            where mmcode = :mmcode
                              and inv_qty <> 0";
            return DBWork.Connection.Query<MmcodeInvqty>(sql, new { mmcode = mmcode }, DBWork.Transaction);
        }

        public IEnumerable<WHTRNS> GetWhtrns(string wh_no, string mmcodes, string startDate, string endDate)
        {
            string sql = string.Format(@"
                select a.*,
                       (case when (TR_MCODE='BAKI' AND TR_DOCTYPE = 'RS') then '醫令退藥'
                             when (TR_MCODE='ADJO' AND TR_DOCTYPE = 'RR') then '補發'
                             when (TR_MCODE='USEO' AND SUBSTR(TR_DOCNO,8,4)='LOSS') then '耗損'
                             when (TR_MCODE='USEO' AND SUBSTR(TR_DOCNO,8,4)='CNS') then '撥發即消耗'
                             when (TR_MCODE='USEO' and tr_doctype in ('D', 'T')) then (select DOCTYPE_NAME from MI_DOCTYPE where DOCTYPE = a.tr_doctype)
                             else (select mcode_name from MI_MCODE where mcode = a.tr_mcode)
                         end) as TR_MCODE_NAME,
                       DOCEXP_LOT(a.tr_docno, a.tr_docseq) as LOTNO_EXPDATE_QTY
                  from MI_WHTRNS a
                 where a.wh_no = :wh_no
                   and a.mmcode in ( {0} )
                   and (
                         (TWN_DATE(TR_DATE) between :startDate and :endDate AND TR_MCODE NOT IN ('WAYI','WAYO','USEO','BAKI','ADJO')) OR
                         (TWN_DATE(TR_DATE) between :startDate and :endDate AND TR_MCODE='BAKI' AND TR_DOCTYPE<>'RS') OR
                         (TWN_DATE(TR_DATE) between :startDate and :endDate AND TR_MCODE='ADJO' AND TR_DOCTYPE<>'RR') OR
                         (SUBSTR(TR_DOCNO,1,7) between :startDate and :endDate AND TR_MCODE='USEO') OR
                         (SUBSTR(TR_DOCNO,1,7) between :startDate and :endDate AND TR_MCODE='BAKI' AND TR_DOCTYPE='RS') OR
                         (SUBSTR(TR_DOCNO,1,7) between :startDate and :endDate AND TR_MCODE='ADJO' AND TR_DOCTYPE='RR')
                        )
                   and a.tr_mcode not in ('WAYI','WAYO')
            ", mmcodes);
            return DBWork.Connection.Query<WHTRNS>(sql, new { wh_no, mmcodes, startDate, endDate }, DBWork.Transaction);
        }

        public IEnumerable<PInvqty> GetPInvqty(string pdate, string wh_no, string mmcode)
        {
            string sql = @"
                with temp_data as (
                    select twn_date(twn_todate(:pdate)+1) as data_date,
                           :wh_no as wh_no,
                           :mmcode as mmcode,
                           :pdate as pdate
                      from dual
                )
                select a.wh_no,
                       a.mmcode,
                       a.pdate,
                       day_pinvqty(a.data_date, a.wh_no, a.mmcode) as inv_qty
                  from temp_data a
                 where 1=1
            ";
            return DBWork.Connection.Query<PInvqty>(sql, new { pdate, wh_no, mmcode }, DBWork.Transaction);
        }

        public IEnumerable<TempSpecimen> GetTempSpecimens()
        {
            string sql = @"
                select * from TEMP_HIS_SPECIMEN
            ";
            return DBWork.Connection.Query<TempSpecimen>(sql, DBWork.Transaction);
        }

        public IEnumerable<TempPath> GetTempPath()
        {
            string sql = @"
                select * from TEMP_HIS_PATH
            ";
            return DBWork.Connection.Query<TempPath>(sql, DBWork.Transaction);
        }

        public IEnumerable<WhMast> GetWhMasts(string inid, string wh_kind)
        {
            string sql = @"
                select wh_no, wh_name
                  from MI_WHMAST
                 where inid = :inid
                   and wh_kind = :wh_kind
            ";
            return DBWork.Connection.Query<WhMast>(sql, new { inid, wh_kind }, DBWork.Transaction);
        }

        public List<ETagInfo> GetETagInfo(string wh_no, string mmcode)
        {
            DynamicParameters p = new DynamicParameters();
            string sql = string.Format(@"
                                        SELECT
                                            a.mmcode,
                                            a.inv_qty,
                                            b.mmname_e,
                                            b.mmname_c,
                                            b.base_unit
                                        FROM
                                            mi_whinv a,
                                            mi_mast  b
                                        WHERE
                                                1 = 1
                                            AND a.mmcode = b.mmcode
                                            AND a.wh_no = :wh_no
                                            {0}
                                    ", string.IsNullOrEmpty(mmcode) ? string.Empty
                                                                    : string.Format("AND a.mmcode IN ( {0} )", mmcode));

            p.Add("wh_no", wh_no);

            return DBWork.Connection.Query<ETagInfo>(sql, p, DBWork.Transaction).ToList();
        }

    }
}