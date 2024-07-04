using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;

namespace WebApp.Repository.AA
{
    public class AA0139Repository : JCLib.Mvc.BaseRepository
    {
        public AA0139Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        // 主畫面_查詢
        public IEnumerable<AA0139> GetAll(string styr, string edyr, string bidno)
        {
            var p = new DynamicParameters();

            string sql = @" select * from MILMED_JBID_LIST
                            where 1=1 ";

            if (styr != string.Empty)
            {
                sql += " and JBID_STYR >= :styr ";
                p.Add(":styr", string.Format("{0}", styr));
            }
            if (edyr != string.Empty)
            {
                sql += " and JBID_EDYR <= :edyr ";
                p.Add(":edyr", string.Format("{0}", edyr));
            }
            if (bidno != string.Empty)
            {
                sql += " and BID_NO = :bidno ";
                p.Add(":bidno", string.Format("{0}", bidno));
            }

            return DBWork.PagingQuery<AA0139>(sql, p, DBWork.Transaction);
        }

        public int DeleteWK()
        {
            var sql = @" DELETE from MILMED_JBID_LISTWK ";
            return DBWork.Connection.Execute(sql, null, DBWork.Transaction);
        }

        public int TransM(string styr, string edyr)
        {
            var sql = @" insert into MILMED_JBID_LISTWK
                       select * from MILMED_JBID_LIST
                        where JBID_STYR=:JBID_STYR and JBID_EDYR=:JBID_EDYR ";
            return DBWork.Connection.Execute(sql, new { JBID_STYR = styr, JBID_EDYR = edyr }, DBWork.Transaction);
        }

        public int DeleteM(string styr, string edyr)
        {
            var sql = @" DELETE from MILMED_JBID_LIST
                       where JBID_STYR=:JBID_STYR and JBID_EDYR=:JBID_EDYR ";
            return DBWork.Connection.Execute(sql, new { JBID_STYR = styr, JBID_EDYR = edyr }, DBWork.Transaction);
        }

        public int CreateM(AA0139 aa0139)
        {
            var sql = @"INSERT INTO MILMED_JBID_LIST (
                        JBID_STYR, JBID_EDYR, BID_NO, INGR, INGR_CONTENT, SPEC,
                            DOSAGE_FORM, MMNAME_E, MMNAME_C, PACKQTY, ORIG_BRAND,
                            LICENSE_NO, ISWILLING, DISCOUNT_QTY, INSU_CODE,
                            INSU_RATIO, K_UPRICE, COST_UPRICE, DISC_COST_UPRICE,
                            UNIFORM_NO, AGEN_NAME, UPDATE_YMD, UPADTEUSER, UPDATETIME, UPDATEIP)  
                      VALUES (
                        :JBID_STYR, :JBID_EDYR, :BID_NO, :INGR, :INGR_CONTENT, :SPEC,
                            :DOSAGE_FORM, :MMNAME_E, :MMNAME_C, :PACKQTY, :ORIG_BRAND,
                            :LICENSE_NO, :ISWILLING, :DISCOUNT_QTY, :INSU_CODE,
                            :INSU_RATIO, :K_UPRICE, :COST_UPRICE, :DISC_COST_UPRICE,
                            :UNIFORM_NO, :AGEN_NAME, :UPDATE_YMD, :UPADTEUSER, sysdate, :UPDATEIP)";
            return DBWork.Connection.Execute(sql, aa0139, DBWork.Transaction);
        }

        public int DeleteLog(string twmn)
        {
            var sql = @" DELETE from MILMED_JBID_LOG
                       where UPDATE_YMD<:UPDATE_YMD ";
            return DBWork.Connection.Execute(sql, new { UPDATE_YMD = twmn }, DBWork.Transaction);
        }

        public int CreateLogD()
        {
            var sql = @" insert into MILMED_JBID_LOG
                            select JBID_SEQ.nextval as JBIDSEQ, 'd' as TRANSCODE,
                            a.*,
                            sysdate as CREATETIME
                            from MILMED_JBID_LISTWK a,
                            MILMED_JBID_LIST b
                            where a.JBID_STYR=b.JBID_STYR
                            and a.JBID_EDYR=b.JBID_EDYR
                            and a.BID_NO=b.BID_NO
                            and (a.PACKQTY<>b.PACKQTY or
                            a.ORIG_BRAND<>b.ORIG_BRAND or
                            a.INSU_CODE<>b.INSU_CODE or
                            a.INSU_RATIO<>b.INSU_RATIO or
                            a.K_UPRICE<>b.K_UPRICE or
                            a.COST_UPRICE<>b.COST_UPRICE or
                            a.DISC_COST_UPRICE<>b.DISC_COST_UPRICE
                            )
                            ";
            return DBWork.Connection.Execute(sql, null, DBWork.Transaction);
        }

        public int CreateLogI()
        {
            var sql = @" insert into MILMED_JBID_LOG
                            select JBID_SEQ.nextval as JBIDSEQ, 'i' as TRANSCODE,
                            b.*,
                            sysdate as CREATETIME
                            from MILMED_JBID_LISTWK a,
                            MILMED_JBID_LIST b
                            where a.JBID_STYR=b.JBID_STYR
                            and a.JBID_EDYR=b.JBID_EDYR
                            and a.BID_NO=b.BID_NO
                            and (a.PACKQTY<>b.PACKQTY or
                            a.ORIG_BRAND<>b.ORIG_BRAND or
                            a.INSU_CODE<>b.INSU_CODE or
                            a.INSU_RATIO<>b.INSU_RATIO or
                            a.K_UPRICE<>b.K_UPRICE or
                            a.COST_UPRICE<>b.COST_UPRICE or
                            a.DISC_COST_UPRICE<>b.DISC_COST_UPRICE
                            )
                            ";
            return DBWork.Connection.Execute(sql, null, DBWork.Transaction);
        }

        // 顯示異動_查詢
        public IEnumerable<AA0139> GetWin2All_1(string updateYdm)
        {
            var p = new DynamicParameters();

            string extraSql1 = "";
            string extraSql2 = "";
            if (updateYdm != string.Empty)
            {
                extraSql1 += " and substr(y.UPDATE_YMD,1,5)=:updateYdm ";
                extraSql2 += " and substr(A.UPDATE_YMD,1,5)=:updateYdm ";
                p.Add(":updateYdm", string.Format("{0}", updateYdm));
            }

            string sql = @" select A.*,B.MMCODELIST
                                from MILMED_JBID_LOG A, 
                                (
                                select substr(x.E_YRARMYNO,1,3) as E_YRARMYNO,x.E_ITEMARMYNO,
                                listagg(x.MMCODE, ',') within group (order by x.MMCODE) as MMCODELIST
                                from MI_MAST x,MILMED_JBID_LOG y
                                where 1=1
                                and substr(x.E_YRARMYNO,1,3)=y.JBID_STYR
                                and x.E_ITEMARMYNO=y.BID_NO 
                                and y.TRANSCODE = 'i'
                                " + extraSql1 + @"
                                group by substr(x.E_YRARMYNO,1,3),x.E_ITEMARMYNO
                                ) B
                                where 1=1
                                and A.JBID_STYR=B.E_YRARMYNO
                                and A.BID_NO=B.E_ITEMARMYNO
                                " + extraSql2 + @"
                                order by A.BID_NO,A.JBIDSEQ ";

            return DBWork.PagingQuery<AA0139>(sql, p, DBWork.Transaction);
        }

        //匯出
        public DataTable GetExcelT1(string p0)
        {
            var p = new DynamicParameters();

            string extraSql1 = "";
            string extraSql2 = "";
            if (p0 != string.Empty)
            {
                extraSql1 += " and substr(y.UPDATE_YMD,1,5)=:updateYdm ";
                extraSql2 += " and substr(A.UPDATE_YMD,1,5)=:updateYdm ";
                p.Add(":updateYdm", string.Format("{0}", p0));
            }

            string sql = @" select case when A.TRANSCODE='d' then 'd 修改前' when A.TRANSCODE='i' then 'i 修改後' end as 異動代碼,
                                A.JBID_STYR as 聯標生效起年, A.JBID_EDYR as 聯標生效迄年, A.BID_NO as 投標項次, A.INGR as 招標成分, A.INGR_CONTENT as 成分含量, A.SPEC as 規格量,
                                A.DOSAGE_FORM as 劑型, A.MMNAME_E as 英文品名, A.MMNAME_C as 中文品名, A.PACKQTY as 包裝, A.ORIG_BRAND as 原廠牌,
                                A.LICENSE_NO as 許可證字號, A.ISWILLING as 單次訂購達優惠數量折讓意願, A.DISCOUNT_QTY as 單次採購優惠數量, A.INSU_CODE as 健保代碼,
                                A.INSU_RATIO as 健保價_健保品項_上月預算單價_非健保品項, A.K_UPRICE as 決標契約單價, A.COST_UPRICE as 決標成本單價, A.DISC_COST_UPRICE as 單次訂購達優惠數量成本價,
                                A.UNIFORM_NO as 廠商統編, A.AGEN_NAME as 廠商名稱, B.MMCODELIST as 院內碼
                                from MILMED_JBID_LOG A, 
                                (
                                select substr(x.E_YRARMYNO,1,3) as E_YRARMYNO,x.E_ITEMARMYNO,
                                listagg(x.MMCODE, ',') within group (order by x.MMCODE) as MMCODELIST
                                from MI_MAST x,MILMED_JBID_LOG y
                                where 1=1
                                and substr(x.E_YRARMYNO,1,3)=y.JBID_STYR
                                and x.E_ITEMARMYNO=y.BID_NO 
                                and y.TRANSCODE = 'i'
                                " + extraSql1 + @"
                                group by substr(x.E_YRARMYNO,1,3),x.E_ITEMARMYNO
                                ) B
                                where 1=1
                                and A.JBID_STYR=B.E_YRARMYNO
                                and A.BID_NO=B.E_ITEMARMYNO
                                " + extraSql2 + @"
                                order by A.BID_NO,A.JBIDSEQ ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        //匯出(多筆)
        public DataTable GetExcelT2(string p0)
        {
            var p = new DynamicParameters();

            string extraSql = "";
            if (p0 != string.Empty)
            {
                extraSql += " and substr(B.UPDATE_YMD, 1,5)=:updateYdm ";
                p.Add(":updateYdm", string.Format("{0}", p0));
            }

            string sql = @" select A.MMCODE as 院內碼, 
                                case when B.TRANSCODE='d' then 'd 修改前' when B.TRANSCODE='i' then 'i 修改後' end as 異動代碼,
                                B.JBID_STYR as 聯標生效起年, B.JBID_EDYR as 聯標生效迄年, B.BID_NO as 投標項次, B.INGR as 招標成分, B.INGR_CONTENT as 成分含量, B.SPEC as 規格量,
                                B.DOSAGE_FORM as 劑型, B.MMNAME_E as 英文品名, B.MMNAME_C as 中文品名, B.PACKQTY as 包裝, B.ORIG_BRAND as 原廠牌,
                                B.LICENSE_NO as 許可證字號, B.ISWILLING as 單次訂購達優惠數量折讓意願, B.DISCOUNT_QTY as 單次採購優惠數量, B.INSU_CODE as 健保代碼,
                                B.INSU_RATIO as 健保價_健保品項_上月預算單價_非健保品項, B.K_UPRICE as 決標契約單價, B.COST_UPRICE as 決標成本單價, B.DISC_COST_UPRICE as 單次訂購達優惠數量成本價,
                                B.UNIFORM_NO as 廠商統編, B.AGEN_NAME as 廠商名稱
                                from MI_MAST A,MILMED_JBID_LOG B
                                where 1=1
                                and substr(A.E_YRARMYNO,1,3)=B.JBID_STYR
                                and A.E_ITEMARMYNO=B.BID_NO
                                " + extraSql + @"
                                order by A.MMCODE,B.BID_NO, B.JBIDSEQ ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
    }
}