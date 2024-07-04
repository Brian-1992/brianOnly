using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using TSGH.Models;

namespace WebApp.Repository.AB
{
    public class AB0068_TIME : JCLib.Mvc.BaseModel
    {
        public string SET_BTIME { get; set; }
        public string SET_CTIME { get; set; }

    }
    public class AB0068Repository : JCLib.Mvc.BaseRepository
    {
        public AB0068Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AB0068> SearchReportData_Date(string WHNO, string MedicineClass, string tmp_Date, string IsHighPrice, string IsCDC, bool isGrid, string e_orderdcflag, string ctdmdccode)
        {
            var p = new DynamicParameters();
            string Filter_Medicine = "";
            switch (MedicineClass)
            {
                case "1":
                    Filter_Medicine = @" and (c.E_RESTRICTCODE='1' or c.E_RESTRICTCODE='2' or c.E_RESTRICTCODE='3') ";
                    break;
                case "4":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='4' ";
                    break;
                case "0":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='0' ";
                    break;
                case "N":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='N' ";
                    break;
                default:
                    break;
            }
            // --本日結存(H)=上日結存(A)+入帳(B)-出帳(C)-醫令消耗(D)+醫令退藥(E)+盤點(F)+調帳(G)-耗損(I)
            var sql = string.Format(@"select wh_no,mmcode,
                                             wh_name(wh_no) as wh_name,
                                             mmcode_name(mmcode) mmname_e,
                                             (select e_orderdcflag from MI_MAST
                                               where mmcode = a.mmcode) as e_orderdcflag,
                                             (select ctdmdccode from MI_WINVCTL
                                               where mmcode = a.mmcode and wh_no = a.wh_no) as ctdmdccode,
                                             PQTY as BF_QTY,      --上日結存(A)
                                             INQTY as IN_SUM,      --入帳(B)
                                             OUTQTY as OUT_SUM,     --出帳C)
                                             USEQTY as USEO_qty,    --醫令消耗(D)
                                             RSQTY as RS_qty,      --醫令退藥(E)
                                             CHQTY as CHIO_qty,    --盤點(F)
                                             ADJQTY as adj_qty, --調帳(G)
                                             LOSSQTY as loss_qty,   --耗損(I)
                                             PQTY + INQTY - OUTQTY - USEQTY + RSQTY + CHQTY + ADJQTY - LOSSQTY as AF_QTY --本日結存(H), 當日結存(日報), 結存(月報)
                                       from (
                                             SELECT WH_NO,MMCODE,
                                                    DAY_PINVQTY(:tmp_Date, WH_NO,MMCODE) PQTY,
                                                    INQTY,OUTQTY,USEQTY,RSQTY,CHQTY,ADJQTY, LOSSQTY
                                               FROM (
                                                     SELECT WH_NO,MMCODE,INQTY,OUTQTY,USEQTY,RSQTY,CHQTY,ADJQTY, LOSSQTY
                                                       FROM (
                                                             SELECT WH_NO,MMCODE,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE IN ('ADJI','ADJO','MILI','MILO')) THEN
                                                                          DECODE(TR_IO,'I',TR_INV_QTY,TR_INV_QTY*-1)
                                                                         ELSE 0
                                                                        END) ADJQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='CHIO') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) CHQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='BAKI' AND TR_DOCTYPE = 'RS') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) RSQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='USEO' AND SUBSTR(TR_DOCNO,8,4)<>'LOSS' AND SUBSTR(TR_DOCNO,8,3)<>'CNS') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) USEQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='USEO' AND (SUBSTR(TR_DOCNO,8,4)='LOSS' OR SUBSTR(TR_DOCNO,8,3)='CNS')) THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) LOSSQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE NOT IN ('ADJI','ADJO','CHIO','MILI','MILO','USEO')
                                                                              AND TR_DOCTYPE<>'RS' AND TR_IO='I') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) INQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE NOT IN ('ADJI','ADJO','CHIO','MILI','MILO','USEO')
                                                                              AND TR_DOCTYPE<>'RS' AND TR_IO='O') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) OUTQTY
                                                               FROM MI_WHTRNS
                                                              WHERE WH_NO = :WHNO 
                                                               AND (
                                                                    (TWN_DATE(TR_DATE)=:tmp_Date AND TR_MCODE NOT IN ('WAYI','WAYO','USEO','BAKI','ADJO')) OR
                                                                    (TWN_DATE(TR_DATE)=:tmp_Date AND TR_MCODE='BAKI' AND TR_DOCTYPE<>'RS') OR
                                                                    (TWN_DATE(TR_DATE)=:tmp_Date AND TR_MCODE='ADJO' AND TR_DOCTYPE<>'RR') OR
                                                                    (SUBSTR(TR_DOCNO,1,7)=:tmp_Date AND TR_MCODE='USEO') OR
                                                                    (SUBSTR(TR_DOCNO,1,7)=:tmp_Date AND TR_MCODE='BAKI' AND TR_DOCTYPE='RS') OR
                                                                    (SUBSTR(TR_DOCNO,1,7)=:tmp_Date AND TR_MCODE='ADJO' AND TR_DOCTYPE='RR')
                                                                   )
                                                              GROUP BY WH_NO,MMCODE
                                                              )
                                                     UNION ALL
                                                     SELECT WH_NO,MMCODE,0 INQTY,0 OUTQTY,0 USEQTY,0 LOSSQTY,0 RSQTY,0 CHQTY,0 ADJQTY
                                                       FROM MI_WHINV A
                                                      WHERE WH_NO = :WHNO 
                                                            AND NOT EXISTS
                                                            (SELECT 1 FROM MI_WHTRNS 
                                                              WHERE (
                                                                     (TWN_DATE(TR_DATE)=:tmp_Date AND TR_MCODE NOT IN ('WAYI','WAYO','USEO','BAKI','ADJO')) OR
                                                                     (TWN_DATE(TR_DATE)=:tmp_Date AND TR_MCODE='BAKI' AND TR_DOCTYPE<>'RS') OR
                                                                     (TWN_DATE(TR_DATE)=:tmp_Date AND TR_MCODE='ADJO' AND TR_DOCTYPE<>'RR') OR
                                                                     (SUBSTR(TR_DOCNO,1,7)=:tmp_Date AND TR_MCODE='USEO') OR
                                                                     (SUBSTR(TR_DOCNO,1,7)=:tmp_Date AND TR_MCODE='BAKI' AND TR_DOCTYPE='RS') OR
                                                                     (SUBSTR(TR_DOCNO,1,7)=:tmp_Date AND TR_MCODE='ADJO' AND TR_DOCTYPE='RR')
                                                                    ) 
                                                                AND WH_NO=A.WH_NO AND MMCODE=A.MMCODE
                                                            ) 
                                                   )
                                             ORDER BY WH_NO,MMCODE
                                          )   a
                                      where exists (select 1 from MI_MAST c 
                                                     where 1=1 
                                                       and c.mmcode = a.mmcode
                                                           {0} 
                                                           {1}
                                                           {2} 
                                                           {3} 
                                                    )
                                        {4}
                                     order by a.mmcode
                ", Filter_Medicine
                 , IsHighPrice != string.Empty ? " and c.e_highpriceflag = :IsHighPrice" : string.Empty
                 , IsCDC != string.Empty ? " and c.e_returndrugflag = :IsCDC" : string.Empty
                 , e_orderdcflag != string.Empty ? " and c.e_orderdcflag = :e_orderdcflag" : string.Empty
                 , ctdmdccode != string.Empty ? @"  and exists (select 1 from MI_WINVCTL
                                                                 where wh_no = a.wh_no 
                                                                   and mmcode = a.mmcode 
                                                                   and ctdmdccode = :ctdmdccode)" : string.Empty );

            p.Add(":WHNO", string.Format("{0}", WHNO));
            p.Add(":tmp_Date", string.Format("{0}", tmp_Date));
            p.Add(":IsHighPrice", string.Format("{0}", IsHighPrice));
            p.Add(":IsCDC", string.Format("{0}", IsCDC));
            p.Add(":e_orderdcflag", string.Format("{0}", e_orderdcflag));
            p.Add(":ctdmdccode", string.Format("{0}", ctdmdccode));

            //return DBWork.Connection.Query<AB0068>(sql, p);
            if (isGrid) {
                return DBWork.PagingQuery<AB0068>(sql, p, DBWork.Transaction);
            }

            return DBWork.Connection.Query<AB0068>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<AB0068> SearchReportData_Date_notCombo(string wh_type, string MedicineClass, string tmp_Date, string IsHighPrice, string IsCDC, bool isGrid, string e_orderdcflag, string ctdmdccode)
        {
            var p = new DynamicParameters();
            string Filter_Medicine = "";

            string wh_type_string = string.Empty;
            if (wh_type == "isPhd") {
                wh_type_string = " (select wh_no from MI_WHMAST where wh_kind = '0' and wh_grade in ('2'))";
            }
            if (wh_type == "all")
            {
                wh_type_string = " (select wh_no from MI_WHMAST where wh_kind = '0' and wh_grade in ('1','2','3', '4'))";
            }

            switch (MedicineClass)
            {
                case "1":
                    Filter_Medicine = @" and (c.E_RESTRICTCODE='1' or c.E_RESTRICTCODE='2' or c.E_RESTRICTCODE='3') ";
                    break;
                case "4":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='4' ";
                    break;
                case "0":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='0' ";
                    break;
                case "N":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='N' ";
                    break;
                default:
                    break;
            }
            var sql = string.Format(@"select wh_no,mmcode,
                                             wh_name(wh_no) as wh_name,
                                             mmcode_name(mmcode) mmname_e,
                                             PQTY as BF_QTY,      --上日結存(A)
                                             INQTY as IN_SUM,      --入帳(B)
                                             OUTQTY as OUT_SUM,     --出帳C)
                                             USEQTY as USEO_qty,    --醫令消耗(D)
                                             RSQTY as RS_qty,      --醫令退藥(E)
                                             CHQTY as CHIO_qty,    --盤點(F)
                                             ADJQTY as adj_qty, --調帳(G)
                                             LOSSQTY as loss_qty,   --耗損(I)
                                             PQTY + INQTY - OUTQTY - USEQTY + RSQTY + CHQTY + ADJQTY - LOSSQTY as AF_QTY --本日結存(H), 當日結存(日報), 結存(月報)
                                       from (
                                             SELECT WH_NO,MMCODE,
                                                    DAY_PINVQTY(:tmp_Date, WH_NO,MMCODE) PQTY,
                                                    INQTY,OUTQTY,USEQTY,RSQTY,CHQTY,ADJQTY, LOSSQTY
                                               FROM (
                                                     SELECT WH_NO,MMCODE,INQTY,OUTQTY,USEQTY,RSQTY,CHQTY,ADJQTY, LOSSQTY
                                                       FROM (
                                                             SELECT WH_NO,MMCODE,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE IN ('ADJI','ADJO','MILI','MILO')) THEN
                                                                          DECODE(TR_IO,'I',TR_INV_QTY,TR_INV_QTY*-1)
                                                                         ELSE 0
                                                                        END) ADJQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='CHIO') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) CHQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='BAKI' AND TR_DOCTYPE = 'RS') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) RSQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='USEO' AND SUBSTR(TR_DOCNO,8,4)<>'LOSS' AND SUBSTR(TR_DOCNO,8,3)<>'CNS') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) USEQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='USEO' AND (SUBSTR(TR_DOCNO,8,4)='LOSS' OR SUBSTR(TR_DOCNO,8,3)='CNS')) THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) LOSSQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE NOT IN ('ADJI','ADJO','CHIO','MILI','MILO','USEO')
                                                                              AND TR_DOCTYPE<>'RS' AND TR_IO='I') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) INQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE NOT IN ('ADJI','ADJO','CHIO','MILI','MILO','USEO')
                                                                              AND TR_DOCTYPE<>'RS' AND TR_IO='O') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) OUTQTY
                                                               FROM MI_WHTRNS
                                                              WHERE WH_NO in {1} 
                                                               AND (
                                                                    (TWN_DATE(TR_DATE)=:tmp_Date AND TR_MCODE NOT IN ('WAYI','WAYO','USEO','BAKI','ADJO')) OR
                                                                    (TWN_DATE(TR_DATE)=:tmp_Date AND TR_MCODE='BAKI' AND TR_DOCTYPE<>'RS') OR
                                                                    (TWN_DATE(TR_DATE)=:tmp_Date AND TR_MCODE='ADJO' AND TR_DOCTYPE<>'RR') OR
                                                                    (SUBSTR(TR_DOCNO,1,7)=:tmp_Date AND TR_MCODE='USEO') OR
                                                                    (SUBSTR(TR_DOCNO,1,7)=:tmp_Date AND TR_MCODE='BAKI' AND TR_DOCTYPE='RS') OR
                                                                    (SUBSTR(TR_DOCNO,1,7)=:tmp_Date AND TR_MCODE='ADJO' AND TR_DOCTYPE='RR')
                                                                   )
                                                              GROUP BY WH_NO,MMCODE
                                                              )
                                                     UNION ALL
                                                     SELECT WH_NO,MMCODE,0 INQTY,0 OUTQTY,0 USEQTY,0 LOSSQTY,0 RSQTY,0 CHQTY,0 ADJQTY
                                                       FROM MI_WHINV A
                                                      WHERE WH_NO in {1} 
                                                            AND NOT EXISTS
                                                            (SELECT 1 FROM MI_WHTRNS 
                                                              WHERE (
                                                                     (TWN_DATE(TR_DATE)=:tmp_Date AND TR_MCODE NOT IN ('WAYI','WAYO','USEO','BAKI','ADJO')) OR
                                                                     (TWN_DATE(TR_DATE)=:tmp_Date AND TR_MCODE='BAKI' AND TR_DOCTYPE<>'RS') OR
                                                                     (TWN_DATE(TR_DATE)=:tmp_Date AND TR_MCODE='ADJO' AND TR_DOCTYPE<>'RR') OR
                                                                     (SUBSTR(TR_DOCNO,1,7)=:tmp_Date AND TR_MCODE='USEO') OR
                                                                     (SUBSTR(TR_DOCNO,1,7)=:tmp_Date AND TR_MCODE='BAKI' AND TR_DOCTYPE='RS') OR
                                                                     (SUBSTR(TR_DOCNO,1,7)=:tmp_Date AND TR_MCODE='ADJO' AND TR_DOCTYPE='RR')
                                                                    )
                                                                AND WH_NO=A.WH_NO AND MMCODE=A.MMCODE
                                                            )
                                                   )
                                             ORDER BY WH_NO,MMCODE
                                          )   a
                                      where exists (select 1 from MI_MAST c 
                                                     where 1=1 
                                                       and c.mmcode = a.mmcode
                                                           {0} 
                                                       {2}
                                                       {3}  
                                                       {4} 
                                                    )
                                        {5}
                                     order by a.mmcode
                ", Filter_Medicine
                 , wh_type_string
                 , IsHighPrice != string.Empty ? " and c.e_highpriceflag = :IsHighPrice" : string.Empty
                 , IsCDC != string.Empty ? " and c.e_returndrugflag = :IsCDC" : string.Empty
                 , e_orderdcflag != string.Empty ? " and c.e_orderdcflag = :e_orderdcflag" : string.Empty
                 , ctdmdccode != string.Empty ? @"  and exists (select 1 from MI_WINVCTL
                                                                 where wh_no = a.wh_no 
                                                                   and mmcode = a.mmcode 
                                                                   and ctdmdccode = :ctdmdccode)" : string.Empty );

            p.Add(":tmp_Date", string.Format("{0}", tmp_Date));
            p.Add(":IsHighPrice", string.Format("{0}", IsHighPrice));
            p.Add(":IsCDC", string.Format("{0}", IsCDC));
            p.Add(":e_orderdcflag", string.Format("{0}", e_orderdcflag));
            p.Add(":ctdmdccode", string.Format("{0}", ctdmdccode));

            if (isGrid)
            {
                return DBWork.PagingQuery<AB0068>(sql, p, DBWork.Transaction);
            }

            return DBWork.Connection.Query<AB0068>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<AB0068> SearchReportData_Month(string WHNO, string MedicineClass, string tmp_Date, string IsHighPrice, string IsCDC, bool isGrid, AB0068_TIME time_range, string e_orderdcflag, string ctdmdccode)
        {
            var p = new DynamicParameters();
            string Filter_Medicine = "";
            switch (MedicineClass)
            {
                case "1":
                    Filter_Medicine = @" and (c.E_RESTRICTCODE='1' or c.E_RESTRICTCODE='2' or c.E_RESTRICTCODE='3') ";
                    break;
                case "4":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='4' ";
                    break;
                case "0":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='0' ";
                    break;
                case "N":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='N' ";
                    break;
                default:
                    break;
            }
            // --本日結存(H)=上日結存(A)+入帳(B)-出帳(C)-醫令消耗(D)+醫令退藥(E)+盤點(F)+調帳(G)
            var sql = string.Format(@"select wh_no,mmcode,
                                             wh_name(wh_no) as wh_name,
                                             mmcode_name(mmcode) mmname_e,
                                             PMN_INVQTY as BF_QTY,      --上月結存(A)
                                             INQTY as IN_SUM,      --入帳(B)
                                             OUTQTY as OUT_SUM,     --出帳C)
                                             USEQTY as USEO_qty,    --醫令消耗(D)
                                             RSQTY as RS_qty,      --醫令退藥(E)
                                             CHQTY as CHIO_qty,    --盤點(F)
                                             ADJQTY as adj_qty, --調帳(G)
                                             LOSSQTY as loss_qty, --調帳(G)
                                             PMN_INVQTY + INQTY - OUTQTY - USEQTY + RSQTY + CHQTY + ADJQTY - LOSSQTY as AF_QTY --本日結存(H), 當日結存(日報), 結存(月報)
                                       from (
                                             SELECT A.WH_NO, A.MMCODE,
                                                    {5} PMN_INVQTY,
                                                    (APL_INQTY + TRN_INQTY + BAK_INQTY + EXG_INQTY - NVL(B.RSQTY,0)) INQTY,
                                                    (APL_OUTQTY + TRN_OUTQTY + BAK_OUTQTY + REJ_OUTQTY + DIS_OUTQTY + EXG_OUTQTY) OUTQTY,
                                                    NVL(B.USEQTY,0) USEQTY,
                                                    NVL(B.RSQTY,0) RSQTY,
                                                    NVL(B.CHQTY,0) CHQTY,
                                                    NVL(B.LOSSQTY,0) LOSSQTY,
                                                    (ADJ_INQTY - ADJ_OUTQTY + MIL_INQTY - MIL_OUTQTY) ADJQTY
                                               FROM (
                                                     SELECT WH_NO,MMCODE,APL_INQTY,APL_OUTQTY,TRN_INQTY,TRN_OUTQTY,ADJ_INQTY,
                                                            ADJ_OUTQTY,BAK_INQTY,BAK_OUTQTY,REJ_OUTQTY,DIS_OUTQTY,EXG_INQTY,EXG_OUTQTY,
                                                            MIL_INQTY,MIL_OUTQTY
                                                       FROM MI_WINVMON
                                                      WHERE DATA_YM=:tmp_Date 
                                                        AND WH_NO = :WHNO 
                                                     ) A,
                                                     (
                                                     SELECT WH_NO,MMCODE,
                                                             SUM(CASE
                                                                  WHEN (TR_MCODE='CHIO') THEN
                                                                   TR_INV_QTY
                                                                  ELSE 0
                                                                 END) CHQTY,
                                                             SUM(CASE
                                                                  WHEN (TR_MCODE='BAKI' AND TR_DOCTYPE = 'RS') THEN
                                                                   TR_INV_QTY
                                                                  ELSE 0
                                                                 END) RSQTY,
                                                             SUM(CASE
                                                                  WHEN (TR_MCODE='USEO' AND SUBSTR(TR_DOCNO,8,4)<>'LOSS' AND SUBSTR(TR_DOCNO,8,3)<>'CNS') THEN
                                                                   TR_INV_QTY
                                                                  ELSE 0
                                                                 END) USEQTY,
                                                             SUM(CASE
                                                                  WHEN (TR_MCODE='USEO' AND (SUBSTR(TR_DOCNO,8,4)='LOSS' OR SUBSTR(TR_DOCNO,8,3)='CNS')) THEN
                                                                   TR_INV_QTY
                                                                  ELSE 0
                                                                 END) LOSSQTY
                                                       FROM MI_WHTRNS
                                                      WHERE WH_NO = :WHNO 
                                                        AND (
                                                             ((TWN_DATE(TR_DATE) BETWEEN :start_date AND :end_date) AND TR_MCODE='CHIO') OR
                                                             ((SUBSTR(TR_DOCNO,1,7) BETWEEN :start_date AND :end_date) AND TR_MCODE='USEO') OR
                                                             ((SUBSTR(TR_DOCNO,1,7) BETWEEN :start_date AND :end_date) AND TR_MCODE='BAKI' AND TR_DOCTYPE='RS')
                                                            )
                                                      GROUP BY WH_NO,MMCODE
                                                     ) B
                                              WHERE A.WH_NO=B.WH_NO(+) AND A.MMCODE=B.MMCODE(+)
                                            ) a
                                      where exists (select 1 from MI_MAST c 
                                                     where 1=1 
                                                       and c.mmcode = a.mmcode
                                                           {0} 
                                                           {1}
                                                           {2} 
                                                           {3} 
                                                    )
                                        {4}
                                     order by a.mmcode
                ", Filter_Medicine
                 , IsHighPrice != string.Empty ? " and c.e_highpriceflag = :IsHighPrice" : string.Empty
                 , IsCDC != string.Empty ? " and c.e_returndrugflag = :IsCDC" : string.Empty
                 , e_orderdcflag != string.Empty ? " and c.e_orderdcflag = :e_orderdcflag" : string.Empty
                 , ctdmdccode != string.Empty ? @"  and exists (select 1 from MI_WINVCTL
                                                                 where wh_no = a.wh_no 
                                                                   and mmcode = a.mmcode 
                                                                   and ctdmdccode = :ctdmdccode)" : string.Empty
                 , tmp_Date == "10908" ? "DAY_PINVQTY('1090801', A.WH_NO,A.MMCODE)" : "PMN_INVQTY(:tmp_Date, A.WH_NO, A.MMCODE)");

            p.Add(":WHNO", string.Format("{0}", WHNO));
            p.Add(":tmp_Date", string.Format("{0}", tmp_Date));
            p.Add(":IsHighPrice", string.Format("{0}", IsHighPrice));
            p.Add(":IsCDC", string.Format("{0}", IsCDC));
            p.Add(":start_date", string.Format("{0}", time_range.SET_BTIME.Substring(0,7)));
            p.Add(":end_date", string.Format("{0}", time_range.SET_CTIME.Substring(0, 7)));
            p.Add(":e_orderdcflag", string.Format("{0}", e_orderdcflag));
            p.Add(":ctdmdccode", string.Format("{0}", ctdmdccode));

            if (isGrid)
            {
                return DBWork.PagingQuery<AB0068>(sql, p, DBWork.Transaction);
            }

            return DBWork.Connection.Query<AB0068>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<AB0068> SearchReportData_Month_notCombo(string wh_type, string MedicineClass, string tmp_Date, string IsHighPrice, string IsCDC, bool isGrid, AB0068_TIME time_range, string e_orderdcflag, string ctdmdccode)
        {
            var p = new DynamicParameters();
            string Filter_Medicine = "";

            string wh_type_string = string.Empty;
            if (wh_type == "isPhd")
            {
                wh_type_string = " (select wh_no from MI_WHMAST where wh_kind = '0' and wh_grade in ('2'))";
            }
            if (wh_type == "all")
            {
                wh_type_string = " (select wh_no from MI_WHMAST where wh_kind = '0' and wh_grade in ('1','2','3', '4'))";
            }

            switch (MedicineClass)
            {
                case "1":
                    Filter_Medicine = @" and (c.E_RESTRICTCODE='1' or c.E_RESTRICTCODE='2' or c.E_RESTRICTCODE='3') ";
                    break;
                case "4":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='4' ";
                    break;
                case "0":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='0' ";
                    break;
                case "N":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='N' ";
                    break;
                default:
                    break;
            }
            // --本日結存(H)=上日結存(A)+入帳(B)-出帳(C)-醫令消耗(D)+醫令退藥(E)+盤點(F)+調帳(G)
            var sql = string.Format(@"select wh_no,mmcode,
                                             wh_name(wh_no) as wh_name,
                                             mmcode_name(mmcode) mmname_e,
                                             PMN_INVQTY as BF_QTY,      --上月結存(A)
                                             INQTY as IN_SUM,      --入帳(B)
                                             OUTQTY as OUT_SUM,     --出帳C)
                                             USEQTY as USEO_qty,    --醫令消耗(D)
                                             RSQTY as RS_qty,      --醫令退藥(E)
                                             CHQTY as CHIO_qty,    --盤點(F)
                                             ADJQTY as adj_qty, --調帳(G)
                                             LOSSQTY as loss_qty, --耗損(I)
                                             PMN_INVQTY + INQTY - OUTQTY - USEQTY + RSQTY + CHQTY + ADJQTY - LOSSQTY as AF_QTY --本日結存(H), 當日結存(日報), 結存(月報)
                                       from (
                                             SELECT A.WH_NO, A.MMCODE,
                                                    {6} PMN_INVQTY,
                                                    (APL_INQTY + TRN_INQTY + BAK_INQTY + EXG_INQTY - NVL(B.RSQTY,0)) INQTY,
                                                    (APL_OUTQTY + TRN_OUTQTY + BAK_OUTQTY + REJ_OUTQTY + DIS_OUTQTY + EXG_OUTQTY) OUTQTY,
                                                    NVL(B.USEQTY,0) USEQTY,
                                                    NVL(B.RSQTY,0) RSQTY,
                                                    NVL(B.CHQTY,0) CHQTY,
                                                    NVL(B.LOSSQTY,0) LOSSQTY,
                                                    (ADJ_INQTY - ADJ_OUTQTY + MIL_INQTY - MIL_OUTQTY) ADJQTY
                                               FROM (
                                                     SELECT WH_NO,MMCODE,APL_INQTY,APL_OUTQTY,TRN_INQTY,TRN_OUTQTY,ADJ_INQTY,
                                                            ADJ_OUTQTY,BAK_INQTY,BAK_OUTQTY,REJ_OUTQTY,DIS_OUTQTY,EXG_INQTY,EXG_OUTQTY,
                                                            MIL_INQTY,MIL_OUTQTY
                                                       FROM MI_WINVMON
                                                      WHERE DATA_YM=:tmp_Date 
                                                        AND WH_NO in {1}  
                                                     ) A,
                                                     (
                                                     SELECT WH_NO,MMCODE,
                                                             SUM(CASE
                                                                  WHEN (TR_MCODE='CHIO') THEN
                                                                   TR_INV_QTY
                                                                  ELSE 0
                                                                 END) CHQTY,
                                                             SUM(CASE
                                                                  WHEN (TR_MCODE='BAKI' AND TR_DOCTYPE = 'RS') THEN
                                                                   TR_INV_QTY
                                                                  ELSE 0
                                                                 END) RSQTY,
                                                             SUM(CASE
                                                                  WHEN (TR_MCODE='USEO' AND SUBSTR(TR_DOCNO,8,4)<>'LOSS' and SUBSTR(TR_DOCNO,8,3)<>'CNS') THEN
                                                                   TR_INV_QTY
                                                                  ELSE 0
                                                                 END) USEQTY,
                                                             SUM(CASE
                                                                  WHEN (TR_MCODE='USEO' AND (SUBSTR(TR_DOCNO,8,4)='LOSS' or SUBSTR(TR_DOCNO,8,3)='CNS')) THEN
                                                                   TR_INV_QTY
                                                                  ELSE 0
                                                                 END) LOSSQTY
                                                       FROM MI_WHTRNS
                                                      WHERE WH_NO in {1} 
                                                        AND (
                                                             ((TWN_DATE(TR_DATE) BETWEEN :start_date AND :end_date) AND TR_MCODE='CHIO') OR
                                                             ((SUBSTR(TR_DOCNO,1,7) BETWEEN :start_date AND :end_date) AND TR_MCODE='USEO') OR
                                                             ((SUBSTR(TR_DOCNO,1,7) BETWEEN :start_date AND :end_date) AND TR_MCODE='BAKI' AND TR_DOCTYPE='RS')
                                                            )
                                                       GROUP BY WH_NO,MMCODE
                                                     ) B
                                              WHERE A.WH_NO=B.WH_NO(+) AND A.MMCODE=B.MMCODE(+)
                                            ) a
                                      where exists (select 1 from MI_MAST c 
                                                     where 1=1 
                                                       and c.mmcode = a.mmcode
                                                           {0} 
                                                       {2}
                                                       {3}  
                                                       {4} 
                                                    )
                                        {5}
                                     order by a.mmcode
                ", Filter_Medicine
                 , wh_type_string
                 , IsHighPrice != string.Empty ? " and c.e_highpriceflag = :IsHighPrice" : string.Empty
                 , IsCDC != string.Empty ? " and c.e_returndrugflag = :IsCDC" : string.Empty
                 , e_orderdcflag != string.Empty ? " and c.e_orderdcflag = :e_orderdcflag" : string.Empty
                 , ctdmdccode != string.Empty ? @"  and exists (select 1 from MI_WINVCTL
                                                                 where wh_no = a.wh_no 
                                                                   and mmcode = a.mmcode 
                                                                   and ctdmdccode = :ctdmdccode)" : string.Empty
                 , tmp_Date == "10908" ? "DAY_PINVQTY('1090801', A.WH_NO, A.MMCODE)" : "PMN_INVQTY(:tmp_Date, A.WH_NO, A.MMCODE)");

            p.Add(":tmp_Date", string.Format("{0}", tmp_Date));
            p.Add(":IsHighPrice", string.Format("{0}", IsHighPrice));
            p.Add(":IsCDC", string.Format("{0}", IsCDC));
            p.Add(":start_date", string.Format("{0}", time_range.SET_BTIME.Substring(0, 7)));
            p.Add(":end_date", string.Format("{0}", time_range.SET_CTIME.Substring(0, 7)));
            p.Add(":e_orderdcflag", string.Format("{0}", e_orderdcflag));
            p.Add(":ctdmdccode", string.Format("{0}", ctdmdccode));

            if (isGrid)
            {
                return DBWork.PagingQuery<AB0068>(sql, p, DBWork.Transaction);
            }

            return DBWork.Connection.Query<AB0068>(sql, p, DBWork.Transaction);
        }

        #region excel

        public DataTable SearchReportData_Date_excel(string WHNO, string MedicineClass, string tmp_Date, string IsHighPrice, string IsCDC, string e_orderdcflag, string ctdmdccode)
        {
            var p = new DynamicParameters();
            string Filter_Medicine = "";
            switch (MedicineClass)
            {
                case "1":
                    Filter_Medicine = @" and (c.E_RESTRICTCODE='1' or c.E_RESTRICTCODE='2' or c.E_RESTRICTCODE='3') ";
                    break;
                case "4":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='4' ";
                    break;
                case "0":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='0' ";
                    break;
                case "N":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='N' ";
                    break;
                default:
                    break;
            }
            // --本日結存(H)=上日結存(A)+入帳(B)-出帳(C)-醫令消耗(D)+醫令退藥(E)+盤點(F)+調帳(G)
            var sql = string.Format(@"select wh_no as 庫房代碼,
                                             mmcode as 院內碼,
                                             wh_name(wh_no) as 庫房代碼,
                                             mmcode_name(mmcode) as 品項名稱,
                                             PQTY as 前日結存,      --上日結存(A)
                                             INQTY as 入帳,      --入帳(B)
                                             OUTQTY as 出帳,     --出帳C)
                                             USEQTY as 醫令消耗,    --醫令消耗(D)
                                             RSQTY as 醫令退藥,      --醫令退藥(E)
                                             CHQTY as 盤點差,    --盤點(F)
                                             ADJQTY as 調帳, --調帳(G)
                                             LOSSQTY as 耗損, --耗損(I)
                                             PQTY + INQTY - OUTQTY - USEQTY + RSQTY + CHQTY + ADJQTY - LOSSQTY as 結存 --本日結存(H), 當日結存(日報), 結存(月報)
                                       from (
                                             SELECT WH_NO,MMCODE,
                                                    DAY_PINVQTY(:tmp_Date, WH_NO,MMCODE) PQTY,
                                                    INQTY,OUTQTY,USEQTY,RSQTY,CHQTY,ADJQTY, LOSSQTY
                                               FROM (
                                                     SELECT WH_NO,MMCODE,INQTY,OUTQTY,USEQTY,RSQTY,CHQTY,ADJQTY, LOSSQTY
                                                       FROM (
                                                             SELECT WH_NO,MMCODE,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE IN ('ADJI','ADJO','MILI','MILO')) THEN
                                                                          DECODE(TR_IO,'I',TR_INV_QTY,TR_INV_QTY*-1)
                                                                         ELSE 0
                                                                        END) ADJQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='CHIO') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) CHQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='BAKI' AND TR_DOCTYPE = 'RS') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) RSQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='USEO' AND SUBSTR(TR_DOCNO,8,4)<>'LOSS' and SUBSTR(TR_DOCNO,8,3)<>'CNS') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) USEQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='USEO' AND (SUBSTR(TR_DOCNO,8,4)='LOSS' or SUBSTR(TR_DOCNO,8,3)='CNS')) THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) LOSSQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE NOT IN ('ADJI','ADJO','CHIO','MILI','MILO','USEO')
                                                                              AND TR_DOCTYPE<>'RS' AND TR_IO='I') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) INQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE NOT IN ('ADJI','ADJO','CHIO','MILI','MILO','USEO')
                                                                              AND TR_DOCTYPE<>'RS' AND TR_IO='O') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) OUTQTY
                                                               FROM MI_WHTRNS
                                                              WHERE WH_NO = :WHNO 
                                                               AND (
                                                                    (TWN_DATE(TR_DATE)=:tmp_Date AND TR_MCODE NOT IN ('WAYI','WAYO','USEO','BAKI','ADJO')) OR
                                                                    (TWN_DATE(TR_DATE)=:tmp_Date AND TR_MCODE='BAKI' AND TR_DOCTYPE<>'RS') OR
                                                                    (TWN_DATE(TR_DATE)=:tmp_Date AND TR_MCODE='ADJO' AND TR_DOCTYPE<>'RR') OR
                                                                    (SUBSTR(TR_DOCNO,1,7)=:tmp_Date AND TR_MCODE='USEO') OR
                                                                    (SUBSTR(TR_DOCNO,1,7)=:tmp_Date AND TR_MCODE='BAKI' AND TR_DOCTYPE='RS') OR
                                                                    (SUBSTR(TR_DOCNO,1,7)=:tmp_Date AND TR_MCODE='ADJO' AND TR_DOCTYPE='RR')
                                                                   ) 
                                                              GROUP BY WH_NO,MMCODE
                                                              )
                                                     UNION ALL
                                                     SELECT WH_NO,MMCODE,0 INQTY,0 OUTQTY,0 USEQTY,0 LOSSQTY,0 RSQTY,0 CHQTY,0 ADJQTY
                                                       FROM MI_WHINV A
                                                      WHERE WH_NO = :WHNO 
                                                            AND NOT EXISTS
                                                            (SELECT 1 FROM MI_WHTRNS 
                                                              WHERE (
                                                                     (TWN_DATE(TR_DATE)=:tmp_Date AND TR_MCODE NOT IN ('WAYI','WAYO','USEO','BAKI','ADJO')) OR
                                                                     (TWN_DATE(TR_DATE)=:tmp_Date AND TR_MCODE='BAKI' AND TR_DOCTYPE<>'RS') OR
                                                                     (TWN_DATE(TR_DATE)=:tmp_Date AND TR_MCODE='ADJO' AND TR_DOCTYPE<>'RR') OR
                                                                     (SUBSTR(TR_DOCNO,1,7)=:tmp_Date AND TR_MCODE='USEO') OR
                                                                     (SUBSTR(TR_DOCNO,1,7)=:tmp_Date AND TR_MCODE='BAKI' AND TR_DOCTYPE='RS') OR
                                                                     (SUBSTR(TR_DOCNO,1,7)=:tmp_Date AND TR_MCODE='ADJO' AND TR_DOCTYPE='RR')
                                                                    ) 
                                                                AND WH_NO=A.WH_NO AND MMCODE=A.MMCODE
                                                            )
                                                   )
                                             ORDER BY WH_NO,MMCODE
                                          )   a
                                      where exists (select 1 from MI_MAST c 
                                                     where 1=1 
                                                       and c.mmcode = a.mmcode
                                                           {0} 
                                                           {1}
                                                           {2} 
                                                           {3} 
                                                    )
                                        {4}
                                     order by a.mmcode
                ", Filter_Medicine
                 , IsHighPrice != string.Empty ? " and c.e_highpriceflag = :IsHighPrice" : string.Empty
                 , IsCDC != string.Empty ? " and c.e_returndrugflag = :IsCDC" : string.Empty
                 , e_orderdcflag != string.Empty ? " and c.e_orderdcflag = :e_orderdcflag" : string.Empty
                 , ctdmdccode != string.Empty ? @"  and exists (select 1 from MI_WINVCTL
                                                                 where wh_no = a.wh_no 
                                                                   and mmcode = a.mmcode 
                                                                   and ctdmdccode = :ctdmdccode)" : string.Empty);

            p.Add(":WHNO", string.Format("{0}", WHNO));
            p.Add(":tmp_Date", string.Format("{0}", tmp_Date));
            p.Add(":IsHighPrice", string.Format("{0}", IsHighPrice));
            p.Add(":IsCDC", string.Format("{0}", IsCDC));
            p.Add(":e_orderdcflag", string.Format("{0}", e_orderdcflag));
            p.Add(":ctdmdccode", string.Format("{0}", ctdmdccode));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public DataTable SearchReportData_Date_notCombo_excel(string wh_type, string MedicineClass, string tmp_Date, string IsHighPrice, string IsCDC, string e_orderdcflag, string ctdmdccode)
        {
            var p = new DynamicParameters();
            string Filter_Medicine = "";

            string wh_type_string = string.Empty;
            if (wh_type == "isPhd")
            {
                wh_type_string = " (select wh_no from MI_WHMAST where wh_kind = '0' and wh_grade in ('2'))";
            }
            if (wh_type == "all")
            {
                wh_type_string = " (select wh_no from MI_WHMAST where wh_kind = '0' and wh_grade in ('1','2','3', '4'))";
            }

            switch (MedicineClass)
            {
                case "1":
                    Filter_Medicine = @" and (c.E_RESTRICTCODE='1' or c.E_RESTRICTCODE='2' or c.E_RESTRICTCODE='3') ";
                    break;
                case "4":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='4' ";
                    break;
                case "0":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='0' ";
                    break;
                case "N":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='N' ";
                    break;
                default:
                    break;
            }
            var sql = string.Format(@"select wh_no as 庫房代碼,
                                             mmcode as 院內碼,
                                             wh_name(wh_no) as 庫房代碼,
                                             mmcode_name(mmcode) as 品項名稱,
                                             PQTY as 前日結存,      --上日結存(A)
                                             INQTY as 入帳,      --入帳(B)
                                             OUTQTY as 出帳,     --出帳C)
                                             USEQTY as 醫令消耗,    --醫令消耗(D)
                                             RSQTY as 醫令退藥,      --醫令退藥(E)
                                             CHQTY as 盤點差,    --盤點(F)
                                             ADJQTY as 調帳, --調帳(G)
                                             LOSSQTY as 耗損,  --耗損(I)
                                             PQTY + INQTY - OUTQTY - USEQTY + RSQTY + CHQTY + ADJQTY - LOSSQTY as 結存 --本日結存(H), 當日結存(日報), 結存(月報)
                                       from (
                                             SELECT WH_NO,MMCODE,
                                                    DAY_PINVQTY(:tmp_Date, WH_NO,MMCODE) PQTY,
                                                    INQTY,OUTQTY,USEQTY,RSQTY,CHQTY,ADJQTY, LOSSQTY
                                               FROM (
                                                     SELECT WH_NO,MMCODE,INQTY,OUTQTY,USEQTY,RSQTY,CHQTY,ADJQTY, LOSSQTY
                                                       FROM (
                                                             SELECT WH_NO,MMCODE,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE IN ('ADJI','ADJO','MILI','MILO')) THEN
                                                                          DECODE(TR_IO,'I',TR_INV_QTY,TR_INV_QTY*-1)
                                                                         ELSE 0
                                                                        END) ADJQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='CHIO') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) CHQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='BAKI' AND TR_DOCTYPE = 'RS') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) RSQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='USEO' AND SUBSTR(TR_DOCNO,8,4)<>'LOSS' AND SUBSTR(TR_DOCNO,8,3)<>'CNS') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) USEQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='USEO' AND (SUBSTR(TR_DOCNO,8,4)='LOSS' or SUBSTR(TR_DOCNO,8,3)='CNS')) THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) LOSSQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE NOT IN ('ADJI','ADJO','CHIO','MILI','MILO','USEO')
                                                                              AND TR_DOCTYPE<>'RS' AND TR_IO='I') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) INQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE NOT IN ('ADJI','ADJO','CHIO','MILI','MILO','USEO')
                                                                              AND TR_DOCTYPE<>'RS' AND TR_IO='O') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) OUTQTY
                                                               FROM MI_WHTRNS
                                                              WHERE WH_NO in {1}  
                                                               AND (
                                                                    (TWN_DATE(TR_DATE)=:tmp_Date AND TR_MCODE NOT IN ('WAYI','WAYO','USEO','BAKI','ADJO')) OR
                                                                    (TWN_DATE(TR_DATE)=:tmp_Date AND TR_MCODE='BAKI' AND TR_DOCTYPE<>'RS') OR
                                                                    (TWN_DATE(TR_DATE)=:tmp_Date AND TR_MCODE='ADJO' AND TR_DOCTYPE<>'RR') OR
                                                                    (SUBSTR(TR_DOCNO,1,7)=:tmp_Date AND TR_MCODE='USEO') OR
                                                                    (SUBSTR(TR_DOCNO,1,7)=:tmp_Date AND TR_MCODE='BAKI' AND TR_DOCTYPE='RS') OR
                                                                    (SUBSTR(TR_DOCNO,1,7)=:tmp_Date AND TR_MCODE='ADJO' AND TR_DOCTYPE='RR')
                                                                   ) 
                                                              GROUP BY WH_NO,MMCODE
                                                              )
                                                     UNION ALL
                                                     SELECT WH_NO,MMCODE,0 INQTY,0 OUTQTY,0 USEQTY,0 LOSSQTY,0 RSQTY,0 CHQTY,0 ADJQTY
                                                       FROM MI_WHINV A
                                                      WHERE WH_NO in {1} 
                                                            AND NOT EXISTS
                                                            (SELECT 1 FROM MI_WHTRNS 
                                                              WHERE (
                                                                     (TWN_DATE(TR_DATE)=:tmp_Date AND TR_MCODE NOT IN ('WAYI','WAYO','USEO','BAKI','ADJO')) OR
                                                                     (TWN_DATE(TR_DATE)=:tmp_Date AND TR_MCODE='BAKI' AND TR_DOCTYPE<>'RS') OR
                                                                     (TWN_DATE(TR_DATE)=:tmp_Date AND TR_MCODE='ADJO' AND TR_DOCTYPE<>'RR') OR
                                                                     (SUBSTR(TR_DOCNO,1,7)=:tmp_Date AND TR_MCODE='USEO') OR
                                                                     (SUBSTR(TR_DOCNO,1,7)=:tmp_Date AND TR_MCODE='BAKI' AND TR_DOCTYPE='RS') OR
                                                                     (SUBSTR(TR_DOCNO,1,7)=:tmp_Date AND TR_MCODE='ADJO' AND TR_DOCTYPE='RR')
                                                                    ) 
                                                                AND WH_NO=A.WH_NO AND MMCODE=A.MMCODE
                                                            )
                                                   )
                                             ORDER BY WH_NO,MMCODE
                                          )   a
                                      where exists (select 1 from MI_MAST c 
                                                     where 1=1 
                                                       and c.mmcode = a.mmcode
                                                           {0} 
                                                       {2}
                                                       {3}  
                                                       {4} 
                                                    )
                                        {5}
                                     order by a.mmcode
                ", Filter_Medicine
                 , wh_type_string
                 , IsHighPrice != string.Empty ? " and c.e_highpriceflag = :IsHighPrice" : string.Empty
                 , IsCDC != string.Empty ? " and c.e_returndrugflag = :IsCDC" : string.Empty
                 , e_orderdcflag != string.Empty ? " and c.e_orderdcflag = :e_orderdcflag" : string.Empty
                 , ctdmdccode != string.Empty ? @"  and exists (select 1 from MI_WINVCTL
                                                                 where wh_no = a.wh_no 
                                                                   and mmcode = a.mmcode 
                                                                   and ctdmdccode = :ctdmdccode)" : string.Empty);

            p.Add(":tmp_Date", string.Format("{0}", tmp_Date));
            p.Add(":IsHighPrice", string.Format("{0}", IsHighPrice));
            p.Add(":IsCDC", string.Format("{0}", IsCDC));
            p.Add(":e_orderdcflag", string.Format("{0}", e_orderdcflag));
            p.Add(":ctdmdccode", string.Format("{0}", ctdmdccode));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public DataTable SearchReportData_Month_excel(string WHNO, string MedicineClass, string tmp_Date, string IsHighPrice, string IsCDC, AB0068_TIME time_range, string e_orderdcflag, string ctdmdccode)
        {
            var p = new DynamicParameters();
            string Filter_Medicine = "";
            switch (MedicineClass)
            {
                case "1":
                    Filter_Medicine = @" and (c.E_RESTRICTCODE='1' or c.E_RESTRICTCODE='2' or c.E_RESTRICTCODE='3') ";
                    break;
                case "4":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='4' ";
                    break;
                case "0":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='0' ";
                    break;
                case "N":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='N' ";
                    break;
                default:
                    break;
            }
            // --本日結存(H)=上日結存(A)+入帳(B)-出帳(C)-醫令消耗(D)+醫令退藥(E)+盤點(F)+調帳(G)
            var sql = string.Format(@"select wh_no as 庫房代碼, mmcode as 院內碼,
                                             wh_name(wh_no) as 庫房名稱,
                                             mmcode_name(mmcode) 品項名稱,
                                             PMN_INVQTY as 上月結存,      --上月結存(A)
                                             INQTY as 入帳,      --入帳(B)
                                             OUTQTY as 出帳C,     --出帳C)
                                             USEQTY as 醫令消耗,    --醫令消耗(D)
                                             RSQTY as 醫令退藥,      --醫令退藥(E)
                                             CHQTY as 盤點差,    --盤點(F)
                                             ADJQTY as 調帳, --調帳(G)
                                             LOSSQTY as 耗損,   --耗損(I)
                                             PMN_INVQTY + INQTY - OUTQTY - USEQTY + RSQTY + CHQTY + ADJQTY - LOSSQTY as 結存 --本日結存(H), 當日結存(日報), 結存(月報)
                                       from (
                                             SELECT A.WH_NO, A.MMCODE,
                                                    {5} PMN_INVQTY,
                                                    (APL_INQTY + TRN_INQTY + BAK_INQTY + EXG_INQTY - NVL(B.RSQTY,0)) INQTY,
                                                    (APL_OUTQTY + TRN_OUTQTY + BAK_OUTQTY + REJ_OUTQTY + DIS_OUTQTY + EXG_OUTQTY) OUTQTY,
                                                    NVL(B.USEQTY,0) USEQTY,
                                                    NVL(B.RSQTY,0) RSQTY,
                                                    NVL(B.CHQTY,0) CHQTY,
                                                    NVL(B.LOSSQTY,0) LOSSQTY,
                                                    (ADJ_INQTY - ADJ_OUTQTY + MIL_INQTY - MIL_OUTQTY) ADJQTY
                                               FROM (
                                                     SELECT WH_NO,MMCODE,APL_INQTY,APL_OUTQTY,TRN_INQTY,TRN_OUTQTY,ADJ_INQTY,
                                                            ADJ_OUTQTY,BAK_INQTY,BAK_OUTQTY,REJ_OUTQTY,DIS_OUTQTY,EXG_INQTY,EXG_OUTQTY,
                                                            MIL_INQTY,MIL_OUTQTY
                                                       FROM MI_WINVMON
                                                      WHERE DATA_YM=:tmp_Date 
                                                        AND WH_NO = :WHNO 
                                                     ) A,
                                                     (
                                                     SELECT WH_NO,MMCODE,
                                                             SUM(CASE
                                                                  WHEN (TR_MCODE='CHIO') THEN
                                                                   TR_INV_QTY
                                                                  ELSE 0
                                                                 END) CHQTY,
                                                             SUM(CASE
                                                                  WHEN (TR_MCODE='BAKI' AND TR_DOCTYPE = 'RS') THEN
                                                                   TR_INV_QTY
                                                                  ELSE 0
                                                                 END) RSQTY,
                                                             SUM(CASE
                                                                  WHEN (TR_MCODE='USEO' AND SUBSTR(TR_DOCNO,8,4)<>'LOSS' AND SUBSTR(TR_DOCNO,8,3)<>'CND') THEN
                                                                   TR_INV_QTY
                                                                  ELSE 0
                                                                 END) USEQTY,
                                                             SUM(CASE
                                                                  WHEN (TR_MCODE='USEO' AND (SUBSTR(TR_DOCNO,8,4)='LOSS' or SUBSTR(TR_DOCNO,8,3)='CNS')) THEN
                                                                   TR_INV_QTY
                                                                  ELSE 0
                                                                 END) LOSSQTY
                                                       FROM MI_WHTRNS
                                                      WHERE WH_NO = :WHNO 
                                                        AND (
                                                             ((TWN_DATE(TR_DATE) BETWEEN :start_date AND :end_date) AND TR_MCODE='CHIO') OR
                                                             ((SUBSTR(TR_DOCNO,1,7) BETWEEN :start_date AND :end_date) AND TR_MCODE='USEO') OR
                                                             ((SUBSTR(TR_DOCNO,1,7) BETWEEN :start_date AND :end_date) AND TR_MCODE='BAKI' AND TR_DOCTYPE='RS')
                                                            )
                                                       GROUP BY WH_NO,MMCODE
                                                     ) B
                                              WHERE A.WH_NO=B.WH_NO(+) AND A.MMCODE=B.MMCODE(+)
                                            ) a
                                      where exists (select 1 from MI_MAST c 
                                                     where 1=1 
                                                       and c.mmcode = a.mmcode
                                                           {0} 
                                                           {1}
                                                           {2} 
                                                           {3} 
                                                    )
                                        {4}
                                     order by a.mmcode
                ", Filter_Medicine
                 , IsHighPrice != string.Empty ? " and c.e_highpriceflag = :IsHighPrice" : string.Empty
                 , IsCDC != string.Empty ? " and c.e_returndrugflag = :IsCDC" : string.Empty
                 , e_orderdcflag != string.Empty ? " and c.e_orderdcflag = :e_orderdcflag" : string.Empty
                 , ctdmdccode != string.Empty ? @"  and exists (select 1 from MI_WINVCTL
                                                                 where wh_no = a.wh_no 
                                                                   and mmcode = a.mmcode 
                                                                   and ctdmdccode = :ctdmdccode)" : string.Empty
                 , tmp_Date == "10908" ? "DAY_PINVQTY('1090801', A.WH_NO, A.MMCODE)" : "PMN_INVQTY(:tmp_Date, A.WH_NO, A.MMCODE)");

            p.Add(":WHNO", string.Format("{0}", WHNO));
            p.Add(":tmp_Date", string.Format("{0}", tmp_Date));
            p.Add(":IsHighPrice", string.Format("{0}", IsHighPrice));
            p.Add(":IsCDC", string.Format("{0}", IsCDC));
            p.Add(":start_date", string.Format("{0}", time_range.SET_BTIME.Substring(0, 7)));
            p.Add(":end_date", string.Format("{0}", time_range.SET_CTIME.Substring(0, 7)));
            p.Add(":e_orderdcflag", string.Format("{0}", e_orderdcflag));
            p.Add(":ctdmdccode", string.Format("{0}", ctdmdccode));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public DataTable SearchReportData_Month_notCombo_excel(string wh_type, string MedicineClass, string tmp_Date, string IsHighPrice, string IsCDC, AB0068_TIME time_range, string e_orderdcflag, string ctdmdccode)
        {
            var p = new DynamicParameters();
            string Filter_Medicine = "";

            string wh_type_string = string.Empty;
            if (wh_type == "isPhd")
            {
                wh_type_string = " (select wh_no from MI_WHMAST where wh_kind = '0' and wh_grade in ('2'))";
            }
            if (wh_type == "all")
            {
                wh_type_string = " (select wh_no from MI_WHMAST where wh_kind = '0' and wh_grade in ('1','2','3', '4'))";
            }

            switch (MedicineClass)
            {
                case "1":
                    Filter_Medicine = @" and (c.E_RESTRICTCODE='1' or c.E_RESTRICTCODE='2' or c.E_RESTRICTCODE='3') ";
                    break;
                case "4":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='4' ";
                    break;
                case "0":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='0' ";
                    break;
                case "N":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='N' ";
                    break;
                default:
                    break;
            }
            // --本日結存(H)=上日結存(A)+入帳(B)-出帳(C)-醫令消耗(D)+醫令退藥(E)+盤點(F)+調帳(G)
            var sql = string.Format(@"select wh_no as 庫房代碼, 
                                             mmcode as 院內碼,
                                             wh_name(wh_no) as 庫房名稱,
                                             mmcode_name(mmcode) 品項名稱,
                                             PMN_INVQTY as 上月結存,      --上月結存(A)
                                             INQTY as 入帳,      --入帳(B)
                                             OUTQTY as 出帳,     --出帳C)
                                             USEQTY as 醫令消耗,    --醫令消耗(D)
                                             RSQTY as 醫令退藥,      --醫令退藥(E)
                                             CHQTY as 盤點差,    --盤點(F)
                                             ADJQTY as 調帳, --調帳(G)
                                             LOSSQTY as 耗損,   --耗損(I)
                                             PMN_INVQTY + INQTY - OUTQTY - USEQTY + RSQTY + CHQTY + ADJQTY - LOSSQTY as 結存 --本日結存(H), 當日結存(日報), 結存(月報)
                                       from (
                                             SELECT A.WH_NO, A.MMCODE,
                                                    {6} PMN_INVQTY,
                                                    (APL_INQTY + TRN_INQTY + BAK_INQTY + EXG_INQTY - NVL(B.RSQTY,0)) INQTY,
                                                    (APL_OUTQTY + TRN_OUTQTY + BAK_OUTQTY + REJ_OUTQTY + DIS_OUTQTY + EXG_OUTQTY) OUTQTY,
                                                    NVL(B.USEQTY,0) USEQTY,
                                                    NVL(B.RSQTY,0) RSQTY,
                                                    NVL(B.CHQTY,0) CHQTY,
                                                    NVL(B.LOSSQTY,0) LOSSQTY,
                                                    (ADJ_INQTY - ADJ_OUTQTY + MIL_INQTY - MIL_OUTQTY) ADJQTY
                                               FROM (
                                                     SELECT WH_NO,MMCODE,APL_INQTY,APL_OUTQTY,TRN_INQTY,TRN_OUTQTY,ADJ_INQTY,
                                                            ADJ_OUTQTY,BAK_INQTY,BAK_OUTQTY,REJ_OUTQTY,DIS_OUTQTY,EXG_INQTY,EXG_OUTQTY,
                                                            MIL_INQTY,MIL_OUTQTY
                                                       FROM MI_WINVMON
                                                      WHERE DATA_YM=:tmp_Date 
                                                        AND WH_NO in {1}  
                                                     ) A,
                                                     (
                                                     SELECT WH_NO,MMCODE,
                                                             SUM(CASE
                                                                  WHEN (TR_MCODE='CHIO') THEN
                                                                   TR_INV_QTY
                                                                  ELSE 0
                                                                 END) CHQTY,
                                                             SUM(CASE
                                                                  WHEN (TR_MCODE='BAKI' AND TR_DOCTYPE = 'RS') THEN
                                                                   TR_INV_QTY
                                                                  ELSE 0
                                                                 END) RSQTY,
                                                             SUM(CASE
                                                                  WHEN (TR_MCODE='USEO' AND SUBSTR(TR_DOCNO,8,4)<>'LOSS' AND SUBSTR(TR_DOCNO,8,3)<>'CNS') THEN
                                                                   TR_INV_QTY
                                                                  ELSE 0
                                                                 END) USEQTY,
                                                             SUM(CASE
                                                                  WHEN (TR_MCODE='USEO' AND (SUBSTR(TR_DOCNO,8,4)='LOSS' or SUBSTR(TR_DOCNO,8,3)='CNS')) THEN
                                                                   TR_INV_QTY
                                                                  ELSE 0
                                                                 END) LOSSQTY
                                                       FROM MI_WHTRNS
                                                      WHERE WH_NO in {1} 
                                                        AND (
                                                             ((TWN_DATE(TR_DATE) BETWEEN :start_date AND :end_date) AND TR_MCODE='CHIO') OR
                                                             ((SUBSTR(TR_DOCNO,1,7) BETWEEN :start_date AND :end_date) AND TR_MCODE='USEO') OR
                                                             ((SUBSTR(TR_DOCNO,1,7) BETWEEN :start_date AND :end_date) AND TR_MCODE='BAKI' AND TR_DOCTYPE='RS')
                                                            )
                                                       GROUP BY WH_NO,MMCODE
                                                     ) B
                                              WHERE A.WH_NO=B.WH_NO(+) AND A.MMCODE=B.MMCODE(+)
                                            ) a
                                      where exists (select 1 from MI_MAST c 
                                                     where 1=1 
                                                       and c.mmcode = a.mmcode
                                                           {0} 
                                                       {2}
                                                       {3}  
                                                       {4} 
                                                    )
                                        {5}
                                     order by a.mmcode
                ", Filter_Medicine
                 , wh_type_string
                 , IsHighPrice != string.Empty ? " and c.e_highpriceflag = :IsHighPrice" : string.Empty
                 , IsCDC != string.Empty ? " and c.e_returndrugflag = :IsCDC" : string.Empty
                 , e_orderdcflag != string.Empty ? " and c.e_orderdcflag = :e_orderdcflag" : string.Empty
                 , ctdmdccode != string.Empty ? @"  and exists (select 1 from MI_WINVCTL
                                                                 where wh_no = a.wh_no 
                                                                   and mmcode = a.mmcode 
                                                                   and ctdmdccode = :ctdmdccode)" : string.Empty
                 , tmp_Date == "10908" ? "DAY_PINVQTY('1090801', A.WH_NO, A.MMCODE)" : "PMN_INVQTY(:tmp_Date, A.WH_NO, A.MMCODE)");

            p.Add(":tmp_Date", string.Format("{0}", tmp_Date));
            p.Add(":IsHighPrice", string.Format("{0}", IsHighPrice));
            p.Add(":IsCDC", string.Format("{0}", IsCDC));
            p.Add(":start_date", string.Format("{0}", time_range.SET_BTIME.Substring(0, 7)));
            p.Add(":end_date", string.Format("{0}", time_range.SET_CTIME.Substring(0, 7)));
            p.Add(":e_orderdcflag", string.Format("{0}", e_orderdcflag));
            p.Add(":ctdmdccode", string.Format("{0}", ctdmdccode));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;

        }


        #endregion


        public IEnumerable<COMBO_MODEL> GetWH_NOComboOne()
        {
            var p = new DynamicParameters();

            string sql = @"select WH_NO ||'_'|| WH_NAME as COMBITEM, WH_NO as VALUE,
                            WH_NAME as TEXT
                            from MI_WHMAST
                            where WH_KIND='0' and WH_GRADE in ('1','2','3','4')
                            order by WH_GRADE, WH_NO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }

        public AB0068_TIME GetMonthTimeRange(string SET_YM)
        {
            var p = new DynamicParameters();

            string sql = @"select TWN_TIME(SET_BTIME) SET_BTIME, 
                                  (case 
                                        when SET_CTIME is null
                                            then TWN_TIME(sysdate)
                                        else
                                            TWN_TIME(SET_CTIME)
                                   end) as  SET_CTIME
                            from MI_MNSET 
                            where SET_YM = :SET_YM";

            p.Add(":SET_YM", string.Format("{0}", SET_YM));

            return DBWork.Connection.QueryFirstOrDefault<AB0068_TIME>(sql, p, DBWork.Transaction);
        }

        public AB0068_TIME GetDateTimeRange(string select_date) {
            string sql = @"select (:select_date||'000000') as SET_BTIME, 
                                  (case 
                                        when(:select_date <  TWN_DATE(sysdate))
                                            then (:select_date||'235959')
                                        else
                                            TWN_SYSTIME
                                   end) as set_ctime
                             from dual";
            return DBWork.Connection.QueryFirstOrDefault<AB0068_TIME>(sql, new { select_date  = select_date }, DBWork.Transaction);
        }


        public IEnumerable<COMBO_MODEL> GetWH_NOComboWard(string userid, string userinid)
        {
            var p = new DynamicParameters();

            string sql = @"select WH_NO||'_'||wh_name(a.wh_no) as COMBITEM,
                                  WH_NO as VALUE, 
                                  wh_name(a.wh_no) as TEXT
                             from 
                                  (select WH_NO 
                                     from MI_WHID b
                                    where TASK_ID='1'
                                      and WH_USERID= :userid
                                      and exists(select 1 from MI_WHMAST 
                                                  where WH_NO=b.WH_NO and WH_GRADE in ('3','4'))
                                  union
                                  select WH_NO
                                    from MI_WHMAST
                                   where (WH_KIND='0' and WH_GRADE in ('3','4'))
                                     and INID= :userinid
                                  ) a
                            order by WH_NO
                             ";

            p.Add(":userid", string.Format("{0}", userid));
            p.Add(":userinid", string.Format("{0}", userinid));

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }


        public COMBO_MODEL GetMaxComsumeDate() {
            string sql = @"select max(data_date||data_etime)  as value,
                                  twn_systime as text
                             from MI_CONSUME_DATE
                            where proc_id = 'Y'";
            return DBWork.Connection.QueryFirstOrDefault<COMBO_MODEL>(sql, DBWork.Transaction);
        }

        public string GetTodayDate() {
            string sql = @"select twn_sysdate from dual";

            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }

        #region 2020-09-04 新增 8月月報表修改

        public IEnumerable<AB0068> SearchReportData_Month_10908(string WHNO, string MedicineClass, string tmp_Date, string IsHighPrice, string IsCDC, bool isGrid, string e_orderdcflag, string ctdmdccode)
        {
            var p = new DynamicParameters();
            string Filter_Medicine = "";
            switch (MedicineClass)
            {
                case "1":
                    Filter_Medicine = @" and (c.E_RESTRICTCODE='1' or c.E_RESTRICTCODE='2' or c.E_RESTRICTCODE='3') ";
                    break;
                case "4":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='4' ";
                    break;
                case "0":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='0' ";
                    break;
                case "N":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='N' ";
                    break;
                default:
                    break;
            }
            // --本日結存(H)=上日結存(A)+入帳(B)-出帳(C)-醫令消耗(D)+醫令退藥(E)+盤點(F)+調帳(G)-耗損(I)
            var sql = string.Format(@"select wh_no,mmcode,
                                             wh_name(wh_no) as wh_name,
                                             mmcode_name(mmcode) mmname_e,
                                             (select e_orderdcflag from MI_MAST
                                               where mmcode = a.mmcode) as e_orderdcflag,
                                             (select ctdmdccode from MI_WINVCTL
                                               where mmcode = a.mmcode and wh_no = a.wh_no) as ctdmdccode,
                                             PQTY as BF_QTY,      --上日結存(A)
                                             INQTY as IN_SUM,      --入帳(B)
                                             OUTQTY as OUT_SUM,     --出帳C)
                                             USEQTY as USEO_qty,    --醫令消耗(D)
                                             RSQTY as RS_qty,      --醫令退藥(E)
                                             CHQTY as CHIO_qty,    --盤點(F)
                                             ADJQTY as adj_qty, --調帳(G)
                                             LOSSQTY as loss_qty,   --耗損(I)
                                             PQTY + INQTY - OUTQTY - USEQTY + RSQTY + CHQTY + ADJQTY - LOSSQTY as AF_QTY --本日結存(H), 當日結存(日報), 結存(月報)
                                       from (
                                             SELECT WH_NO,MMCODE,
                                                    DAY_PINVQTY('1090801', WH_NO,MMCODE) PQTY,
                                                    INQTY,OUTQTY,USEQTY,RSQTY,CHQTY,ADJQTY, LOSSQTY
                                               FROM (
                                                     SELECT WH_NO,MMCODE,INQTY,OUTQTY,USEQTY,RSQTY,CHQTY,ADJQTY, LOSSQTY
                                                       FROM (
                                                             SELECT WH_NO,MMCODE,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE IN ('ADJI','ADJO','MILI','MILO')) THEN
                                                                          DECODE(TR_IO,'I',TR_INV_QTY,TR_INV_QTY*-1)
                                                                         ELSE 0
                                                                        END) ADJQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='CHIO') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) CHQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='BAKI' AND TR_DOCTYPE = 'RS') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) RSQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='USEO' AND SUBSTR(TR_DOCNO,8,4)<>'LOSS' and SUBSTR(TR_DOCNO,8,4)<>'CNS' ) THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) USEQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='USEO' AND (SUBSTR(TR_DOCNO,8,4)='LOSS' or SUBSTR(TR_DOCNO,8,3)='CNS')) THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) LOSSQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE NOT IN ('ADJI','ADJO','CHIO','MILI','MILO','USEO')
                                                                              AND TR_DOCTYPE<>'RS' AND TR_IO='I') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) INQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE NOT IN ('ADJI','ADJO','CHIO','MILI','MILO','USEO')
                                                                              AND TR_DOCTYPE<>'RS' AND TR_IO='O') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) OUTQTY
                                                               FROM MI_WHTRNS
                                                              WHERE WH_NO = :WHNO 
                                                               AND (
                                                                    (TWN_DATE(TR_DATE) between '1090801' and '1090831' AND TR_MCODE NOT IN ('WAYI','WAYO','USEO','BAKI','ADJO')) OR
                                                                    (TWN_DATE(TR_DATE) between '1090801' and '1090831' AND TR_MCODE='BAKI' AND TR_DOCTYPE<>'RS') OR
                                                                    (TWN_DATE(TR_DATE) between '1090801' and '1090831' AND TR_MCODE='ADJO' AND TR_DOCTYPE<>'RR') OR
                                                                    (SUBSTR(TR_DOCNO,1,7) between '1090801' and '1090831' AND TR_MCODE='USEO') OR
                                                                    (SUBSTR(TR_DOCNO,1,7) between '1090801' and '1090831' AND TR_MCODE='BAKI' AND TR_DOCTYPE='RS') OR
                                                                    (SUBSTR(TR_DOCNO,1,7) between '1090801' and '1090831' AND TR_MCODE='ADJO' AND TR_DOCTYPE='RR')
                                                                   )
                                                              GROUP BY WH_NO,MMCODE
                                                              )
                                                     UNION ALL
                                                     SELECT WH_NO,MMCODE,0 INQTY,0 OUTQTY,0 USEQTY,0 LOSSQTY,0 RSQTY,0 CHQTY,0 ADJQTY
                                                       FROM MI_WHINV A
                                                      WHERE WH_NO = :WHNO 
                                                            AND NOT EXISTS
                                                            (SELECT 1 FROM MI_WHTRNS 
                                                              WHERE (
                                                                     (TWN_DATE(TR_DATE) between '1090801' and '1090831' AND TR_MCODE NOT IN ('WAYI','WAYO','USEO','BAKI','ADJO')) OR
                                                                     (TWN_DATE(TR_DATE) between '1090801' and '1090831' AND TR_MCODE='BAKI' AND TR_DOCTYPE<>'RS') OR
                                                                     (TWN_DATE(TR_DATE) between '1090801' and '1090831' AND TR_MCODE='ADJO' AND TR_DOCTYPE<>'RR') OR
                                                                     (SUBSTR(TR_DOCNO,1,7) between '1090801' and '1090831' AND TR_MCODE='USEO') OR
                                                                     (SUBSTR(TR_DOCNO,1,7) between '1090801' and '1090831' AND TR_MCODE='BAKI' AND TR_DOCTYPE='RS') OR
                                                                     (SUBSTR(TR_DOCNO,1,7) between '1090801' and '1090831' AND TR_MCODE='ADJO' AND TR_DOCTYPE='RR')
                                                                    ) 
                                                                AND WH_NO=A.WH_NO AND MMCODE=A.MMCODE
                                                            ) 
                                                   )
                                             ORDER BY WH_NO,MMCODE
                                          )   a
                                      where exists (select 1 from MI_MAST c 
                                                     where 1=1 
                                                       and c.mmcode = a.mmcode
                                                           {0} 
                                                           {1}
                                                           {2} 
                                                           {3} 
                                                    )
                                        {4}
                                     order by a.mmcode
                ", Filter_Medicine
                 , IsHighPrice != string.Empty ? " and c.e_highpriceflag = :IsHighPrice" : string.Empty
                 , IsCDC != string.Empty ? " and c.e_returndrugflag = :IsCDC" : string.Empty
                 , e_orderdcflag != string.Empty ? " and c.e_orderdcflag = :e_orderdcflag" : string.Empty
                 , ctdmdccode != string.Empty ? @"  and exists (select 1 from MI_WINVCTL
                                                                 where wh_no = a.wh_no 
                                                                   and mmcode = a.mmcode 
                                                                   and ctdmdccode = :ctdmdccode)" : string.Empty);

            p.Add(":WHNO", string.Format("{0}", WHNO));
            p.Add(":tmp_Date", string.Format("{0}", tmp_Date));
            p.Add(":IsHighPrice", string.Format("{0}", IsHighPrice));
            p.Add(":IsCDC", string.Format("{0}", IsCDC));
            p.Add(":e_orderdcflag", string.Format("{0}", e_orderdcflag));
            p.Add(":ctdmdccode", string.Format("{0}", ctdmdccode));

            //return DBWork.Connection.Query<AB0068>(sql, p);
            if (isGrid)
            {
                return DBWork.PagingQuery<AB0068>(sql, p, DBWork.Transaction);
            }

            return DBWork.Connection.Query<AB0068>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<AB0068> SearchReportData_Month_notCombo_10908(string wh_type, string MedicineClass, string tmp_Date, string IsHighPrice, string IsCDC, bool isGrid, string e_orderdcflag, string ctdmdccode)
        {
            var p = new DynamicParameters();
            string Filter_Medicine = "";

            string wh_type_string = string.Empty;
            if (wh_type == "isPhd")
            {
                wh_type_string = " (select wh_no from MI_WHMAST where wh_kind = '0' and wh_grade in ('2'))";
            }
            if (wh_type == "all")
            {
                wh_type_string = " (select wh_no from MI_WHMAST where wh_kind = '0' and wh_grade in ('1','2','3', '4'))";
            }

            switch (MedicineClass)
            {
                case "1":
                    Filter_Medicine = @" and (c.E_RESTRICTCODE='1' or c.E_RESTRICTCODE='2' or c.E_RESTRICTCODE='3') ";
                    break;
                case "4":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='4' ";
                    break;
                case "0":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='0' ";
                    break;
                case "N":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='N' ";
                    break;
                default:
                    break;
            }
            var sql = string.Format(@"select wh_no,mmcode,
                                             wh_name(wh_no) as wh_name,
                                             mmcode_name(mmcode) mmname_e,
                                             PQTY as BF_QTY,      --上日結存(A)
                                             INQTY as IN_SUM,      --入帳(B)
                                             OUTQTY as OUT_SUM,     --出帳C)
                                             USEQTY as USEO_qty,    --醫令消耗(D)
                                             RSQTY as RS_qty,      --醫令退藥(E)
                                             CHQTY as CHIO_qty,    --盤點(F)
                                             ADJQTY as adj_qty, --調帳(G)
                                             LOSSQTY as loss_qty,   --耗損(I)
                                             PQTY + INQTY - OUTQTY - USEQTY + RSQTY + CHQTY + ADJQTY - LOSSQTY as AF_QTY --本日結存(H), 當日結存(日報), 結存(月報)
                                       from (
                                             SELECT WH_NO,MMCODE,
                                                    DAY_PINVQTY('1090801', WH_NO,MMCODE) PQTY,
                                                    INQTY,OUTQTY,USEQTY,RSQTY,CHQTY,ADJQTY, LOSSQTY
                                               FROM (
                                                     SELECT WH_NO,MMCODE,INQTY,OUTQTY,USEQTY,RSQTY,CHQTY,ADJQTY, LOSSQTY
                                                       FROM (
                                                             SELECT WH_NO,MMCODE,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE IN ('ADJI','ADJO','MILI','MILO')) THEN
                                                                          DECODE(TR_IO,'I',TR_INV_QTY,TR_INV_QTY*-1)
                                                                         ELSE 0
                                                                        END) ADJQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='CHIO') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) CHQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='BAKI' AND TR_DOCTYPE = 'RS') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) RSQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='USEO' AND SUBSTR(TR_DOCNO,8,4)<>'LOSS' AND SUBSTR(TR_DOCNO,8,3)<>'CNS') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) USEQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='USEO' AND (SUBSTR(TR_DOCNO,8,4)='LOSS' or SUBSTR(TR_DOCNO,8,3)='CNS')) THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) LOSSQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE NOT IN ('ADJI','ADJO','CHIO','MILI','MILO','USEO')
                                                                              AND TR_DOCTYPE<>'RS' AND TR_IO='I') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) INQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE NOT IN ('ADJI','ADJO','CHIO','MILI','MILO','USEO')
                                                                              AND TR_DOCTYPE<>'RS' AND TR_IO='O') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) OUTQTY
                                                               FROM MI_WHTRNS
                                                              WHERE WH_NO in {1} 
                                                               AND (
                                                                    (TWN_DATE(TR_DATE) between '1090801' and '1090831' AND TR_MCODE NOT IN ('WAYI','WAYO','USEO','BAKI','ADJO')) OR
                                                                    (TWN_DATE(TR_DATE) between '1090801' and '1090831' AND TR_MCODE='BAKI' AND TR_DOCTYPE<>'RS') OR
                                                                    (TWN_DATE(TR_DATE) between '1090801' and '1090831' AND TR_MCODE='ADJO' AND TR_DOCTYPE<>'RR') OR
                                                                    (SUBSTR(TR_DOCNO,1,7) between '1090801' and '1090831' AND TR_MCODE='USEO') OR
                                                                    (SUBSTR(TR_DOCNO,1,7) between '1090801' and '1090831' AND TR_MCODE='BAKI' AND TR_DOCTYPE='RS') OR
                                                                    (SUBSTR(TR_DOCNO,1,7) between '1090801' and '1090831' AND TR_MCODE='ADJO' AND TR_DOCTYPE='RR')
                                                                   )
                                                              GROUP BY WH_NO,MMCODE
                                                              )
                                                     UNION ALL
                                                     SELECT WH_NO,MMCODE,0 INQTY,0 OUTQTY,0 USEQTY,0 LOSSQTY,0 RSQTY,0 CHQTY,0 ADJQTY
                                                       FROM MI_WHINV A
                                                      WHERE WH_NO in {1} 
                                                            AND NOT EXISTS
                                                            (SELECT 1 FROM MI_WHTRNS 
                                                              WHERE (
                                                                     (TWN_DATE(TR_DATE) between '1090801' and '1090831' AND TR_MCODE NOT IN ('WAYI','WAYO','USEO','BAKI','ADJO')) OR
                                                                     (TWN_DATE(TR_DATE) between '1090801' and '1090831' AND TR_MCODE='BAKI' AND TR_DOCTYPE<>'RS') OR
                                                                     (TWN_DATE(TR_DATE) between '1090801' and '1090831' AND TR_MCODE='ADJO' AND TR_DOCTYPE<>'RR') OR
                                                                     (SUBSTR(TR_DOCNO,1,7) between '1090801' and '1090831' AND TR_MCODE='USEO') OR
                                                                     (SUBSTR(TR_DOCNO,1,7) between '1090801' and '1090831' AND TR_MCODE='BAKI' AND TR_DOCTYPE='RS') OR
                                                                     (SUBSTR(TR_DOCNO,1,7) between '1090801' and '1090831' AND TR_MCODE='ADJO' AND TR_DOCTYPE='RR')
                                                                    )
                                                                AND WH_NO=A.WH_NO AND MMCODE=A.MMCODE
                                                            )
                                                   )
                                             ORDER BY WH_NO,MMCODE
                                          )   a
                                      where exists (select 1 from MI_MAST c 
                                                     where 1=1 
                                                       and c.mmcode = a.mmcode
                                                           {0} 
                                                       {2}
                                                       {3}  
                                                       {4} 
                                                    )
                                        {5}
                                     order by a.mmcode
                ", Filter_Medicine
                 , wh_type_string
                 , IsHighPrice != string.Empty ? " and c.e_highpriceflag = :IsHighPrice" : string.Empty
                 , IsCDC != string.Empty ? " and c.e_returndrugflag = :IsCDC" : string.Empty
                 , e_orderdcflag != string.Empty ? " and c.e_orderdcflag = :e_orderdcflag" : string.Empty
                 , ctdmdccode != string.Empty ? @"  and exists (select 1 from MI_WINVCTL
                                                                 where wh_no = a.wh_no 
                                                                   and mmcode = a.mmcode 
                                                                   and ctdmdccode = :ctdmdccode)" : string.Empty);

            p.Add(":tmp_Date", string.Format("{0}", tmp_Date));
            p.Add(":IsHighPrice", string.Format("{0}", IsHighPrice));
            p.Add(":IsCDC", string.Format("{0}", IsCDC));
            p.Add(":e_orderdcflag", string.Format("{0}", e_orderdcflag));
            p.Add(":ctdmdccode", string.Format("{0}", ctdmdccode));

            if (isGrid)
            {
                return DBWork.PagingQuery<AB0068>(sql, p, DBWork.Transaction);
            }

            return DBWork.Connection.Query<AB0068>(sql, p, DBWork.Transaction);
        }

        public DataTable SearchReportData_Month_excel_10908(string WHNO, string MedicineClass, string tmp_Date, string IsHighPrice, string IsCDC, string e_orderdcflag, string ctdmdccode)
        {
            var p = new DynamicParameters();
            string Filter_Medicine = "";
            switch (MedicineClass)
            {
                case "1":
                    Filter_Medicine = @" and (c.E_RESTRICTCODE='1' or c.E_RESTRICTCODE='2' or c.E_RESTRICTCODE='3') ";
                    break;
                case "4":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='4' ";
                    break;
                case "0":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='0' ";
                    break;
                case "N":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='N' ";
                    break;
                default:
                    break;
            }
            // --本日結存(H)=上日結存(A)+入帳(B)-出帳(C)-醫令消耗(D)+醫令退藥(E)+盤點(F)+調帳(G)
            var sql = string.Format(@"select wh_no as 庫房代碼,
                                             mmcode as 院內碼,
                                             wh_name(wh_no) as 庫房代碼,
                                             mmcode_name(mmcode) as 品項名稱,
                                             PQTY as 前日結存,      --上日結存(A)
                                             INQTY as 入帳,      --入帳(B)
                                             OUTQTY as 出帳,     --出帳C)
                                             USEQTY as 醫令消耗,    --醫令消耗(D)
                                             RSQTY as 醫令退藥,      --醫令退藥(E)
                                             CHQTY as 盤點差,    --盤點(F)
                                             ADJQTY as 調帳, --調帳(G)
                                             LOSSQTY as 耗損, --耗損(I)
                                             PQTY + INQTY - OUTQTY - USEQTY + RSQTY + CHQTY + ADJQTY - LOSSQTY as 結存 --本日結存(H), 當日結存(日報), 結存(月報)
                                       from (
                                             SELECT WH_NO,MMCODE,
                                                    DAY_PINVQTY('1090801', WH_NO,MMCODE) PQTY,
                                                    INQTY,OUTQTY,USEQTY,RSQTY,CHQTY,ADJQTY, LOSSQTY
                                               FROM (
                                                     SELECT WH_NO,MMCODE,INQTY,OUTQTY,USEQTY,RSQTY,CHQTY,ADJQTY, LOSSQTY
                                                       FROM (
                                                             SELECT WH_NO,MMCODE,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE IN ('ADJI','ADJO','MILI','MILO')) THEN
                                                                          DECODE(TR_IO,'I',TR_INV_QTY,TR_INV_QTY*-1)
                                                                         ELSE 0
                                                                        END) ADJQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='CHIO') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) CHQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='BAKI' AND TR_DOCTYPE = 'RS') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) RSQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='USEO' AND SUBSTR(TR_DOCNO,8,4)<>'LOSS' AND SUBSTR(TR_DOCNO,8,3)<>'CNS') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) USEQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='USEO' AND (SUBSTR(TR_DOCNO,8,4)='LOSS' or SUBSTR(TR_DOCNO,8,3)='CNS')) THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) LOSSQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE NOT IN ('ADJI','ADJO','CHIO','MILI','MILO','USEO')
                                                                              AND TR_DOCTYPE<>'RS' AND TR_IO='I') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) INQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE NOT IN ('ADJI','ADJO','CHIO','MILI','MILO','USEO')
                                                                              AND TR_DOCTYPE<>'RS' AND TR_IO='O') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) OUTQTY
                                                               FROM MI_WHTRNS
                                                              WHERE WH_NO = :WHNO 
                                                               AND (
                                                                    (TWN_DATE(TR_DATE) between '1090801' and '1090831' AND TR_MCODE NOT IN ('WAYI','WAYO','USEO','BAKI','ADJO')) OR
                                                                    (TWN_DATE(TR_DATE) between '1090801' and '1090831' AND TR_MCODE='BAKI' AND TR_DOCTYPE<>'RS') OR
                                                                    (TWN_DATE(TR_DATE) between '1090801' and '1090831' AND TR_MCODE='ADJO' AND TR_DOCTYPE<>'RR') OR
                                                                    (SUBSTR(TR_DOCNO,1,7) between '1090801' and '1090831' AND TR_MCODE='USEO') OR
                                                                    (SUBSTR(TR_DOCNO,1,7) between '1090801' and '1090831' AND TR_MCODE='BAKI' AND TR_DOCTYPE='RS') OR
                                                                    (SUBSTR(TR_DOCNO,1,7) between '1090801' and '1090831' AND TR_MCODE='ADJO' AND TR_DOCTYPE='RR')
                                                                   ) 
                                                              GROUP BY WH_NO,MMCODE
                                                              )
                                                     UNION ALL
                                                     SELECT WH_NO,MMCODE,0 INQTY,0 OUTQTY,0 USEQTY,0 LOSSQTY,0 RSQTY,0 CHQTY,0 ADJQTY
                                                       FROM MI_WHINV A
                                                      WHERE WH_NO = :WHNO 
                                                            AND NOT EXISTS
                                                            (SELECT 1 FROM MI_WHTRNS 
                                                              WHERE (
                                                                     (TWN_DATE(TR_DATE) between '1090801' and '1090831' AND TR_MCODE NOT IN ('WAYI','WAYO','USEO','BAKI','ADJO')) OR
                                                                     (TWN_DATE(TR_DATE) between '1090801' and '1090831' AND TR_MCODE='BAKI' AND TR_DOCTYPE<>'RS') OR
                                                                     (TWN_DATE(TR_DATE) between '1090801' and '1090831' AND TR_MCODE='ADJO' AND TR_DOCTYPE<>'RR') OR
                                                                     (SUBSTR(TR_DOCNO,1,7) between '1090801' and '1090831' AND TR_MCODE='USEO') OR
                                                                     (SUBSTR(TR_DOCNO,1,7) between '1090801' and '1090831' AND TR_MCODE='BAKI' AND TR_DOCTYPE='RS') OR
                                                                     (SUBSTR(TR_DOCNO,1,7) between '1090801' and '1090831' AND TR_MCODE='ADJO' AND TR_DOCTYPE='RR')
                                                                    ) 
                                                                AND WH_NO=A.WH_NO AND MMCODE=A.MMCODE
                                                            )
                                                   )
                                             ORDER BY WH_NO,MMCODE
                                          )   a
                                      where exists (select 1 from MI_MAST c 
                                                     where 1=1 
                                                       and c.mmcode = a.mmcode
                                                           {0} 
                                                           {1}
                                                           {2} 
                                                           {3} 
                                                    )
                                        {4}
                                     order by a.mmcode
                ", Filter_Medicine
                 , IsHighPrice != string.Empty ? " and c.e_highpriceflag = :IsHighPrice" : string.Empty
                 , IsCDC != string.Empty ? " and c.e_returndrugflag = :IsCDC" : string.Empty
                 , e_orderdcflag != string.Empty ? " and c.e_orderdcflag = :e_orderdcflag" : string.Empty
                 , ctdmdccode != string.Empty ? @"  and exists (select 1 from MI_WINVCTL
                                                                 where wh_no = a.wh_no 
                                                                   and mmcode = a.mmcode 
                                                                   and ctdmdccode = :ctdmdccode)" : string.Empty);

            p.Add(":WHNO", string.Format("{0}", WHNO));
            p.Add(":tmp_Date", string.Format("{0}", tmp_Date));
            p.Add(":IsHighPrice", string.Format("{0}", IsHighPrice));
            p.Add(":IsCDC", string.Format("{0}", IsCDC));
            p.Add(":e_orderdcflag", string.Format("{0}", e_orderdcflag));
            p.Add(":ctdmdccode", string.Format("{0}", ctdmdccode));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public DataTable SearchReportData_Month_notCombo_excel_10908(string wh_type, string MedicineClass, string tmp_Date, string IsHighPrice, string IsCDC, string e_orderdcflag, string ctdmdccode)
        {
            var p = new DynamicParameters();
            string Filter_Medicine = "";

            string wh_type_string = string.Empty;
            if (wh_type == "isPhd")
            {
                wh_type_string = " (select wh_no from MI_WHMAST where wh_kind = '0' and wh_grade in ('2'))";
            }
            if (wh_type == "all")
            {
                wh_type_string = " (select wh_no from MI_WHMAST where wh_kind = '0' and wh_grade in ('1','2','3', '4'))";
            }

            switch (MedicineClass)
            {
                case "1":
                    Filter_Medicine = @" and (c.E_RESTRICTCODE='1' or c.E_RESTRICTCODE='2' or c.E_RESTRICTCODE='3') ";
                    break;
                case "4":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='4' ";
                    break;
                case "0":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='0' ";
                    break;
                case "N":
                    Filter_Medicine = @" and c.E_RESTRICTCODE='N' ";
                    break;
                default:
                    break;
            }
            var sql = string.Format(@"select wh_no as 庫房代碼,
                                             mmcode as 院內碼,
                                             wh_name(wh_no) as 庫房代碼,
                                             mmcode_name(mmcode) as 品項名稱,
                                             PQTY as 前日結存,      --上日結存(A)
                                             INQTY as 入帳,      --入帳(B)
                                             OUTQTY as 出帳,     --出帳C)
                                             USEQTY as 醫令消耗,    --醫令消耗(D)
                                             RSQTY as 醫令退藥,      --醫令退藥(E)
                                             CHQTY as 盤點差,    --盤點(F)
                                             ADJQTY as 調帳, --調帳(G)
                                             LOSSQTY as 耗損,  --耗損(I)
                                             PQTY + INQTY - OUTQTY - USEQTY + RSQTY + CHQTY + ADJQTY - LOSSQTY as 結存 --本日結存(H), 當日結存(日報), 結存(月報)
                                       from (
                                             SELECT WH_NO,MMCODE,
                                                    DAY_PINVQTY('1090801', WH_NO,MMCODE) PQTY,
                                                    INQTY,OUTQTY,USEQTY,RSQTY,CHQTY,ADJQTY, LOSSQTY
                                               FROM (
                                                     SELECT WH_NO,MMCODE,INQTY,OUTQTY,USEQTY,RSQTY,CHQTY,ADJQTY, LOSSQTY
                                                       FROM (
                                                             SELECT WH_NO,MMCODE,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE IN ('ADJI','ADJO','MILI','MILO')) THEN
                                                                          DECODE(TR_IO,'I',TR_INV_QTY,TR_INV_QTY*-1)
                                                                         ELSE 0
                                                                        END) ADJQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='CHIO') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) CHQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='BAKI' AND TR_DOCTYPE = 'RS') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) RSQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='USEO' AND SUBSTR(TR_DOCNO,8,4)<>'LOSS' AND SUBSTR(TR_DOCNO,8,3)<>'CNS') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) USEQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE='USEO' AND (SUBSTR(TR_DOCNO,8,4)='LOSS' or SUBSTR(TR_DOCNO,8,3)='CNS')) THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) LOSSQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE NOT IN ('ADJI','ADJO','CHIO','MILI','MILO','USEO')
                                                                              AND TR_DOCTYPE<>'RS' AND TR_IO='I') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) INQTY,
                                                                    SUM(CASE
                                                                         WHEN (TR_MCODE NOT IN ('ADJI','ADJO','CHIO','MILI','MILO','USEO')
                                                                              AND TR_DOCTYPE<>'RS' AND TR_IO='O') THEN
                                                                          TR_INV_QTY
                                                                         ELSE 0
                                                                        END) OUTQTY
                                                               FROM MI_WHTRNS
                                                              WHERE WH_NO in {1}  
                                                               AND (
                                                                    (TWN_DATE(TR_DATE) between '1090801' and '1090831' AND TR_MCODE NOT IN ('WAYI','WAYO','USEO','BAKI','ADJO')) OR
                                                                    (TWN_DATE(TR_DATE) between '1090801' and '1090831' AND TR_MCODE='BAKI' AND TR_DOCTYPE<>'RS') OR
                                                                    (TWN_DATE(TR_DATE) between '1090801' and '1090831' AND TR_MCODE='ADJO' AND TR_DOCTYPE<>'RR') OR
                                                                    (SUBSTR(TR_DOCNO,1,7) between '1090801' and '1090831' AND TR_MCODE='USEO') OR
                                                                    (SUBSTR(TR_DOCNO,1,7) between '1090801' and '1090831' AND TR_MCODE='BAKI' AND TR_DOCTYPE='RS') OR
                                                                    (SUBSTR(TR_DOCNO,1,7) between '1090801' and '1090831' AND TR_MCODE='ADJO' AND TR_DOCTYPE='RR')
                                                                   ) 
                                                              GROUP BY WH_NO,MMCODE
                                                              )
                                                     UNION ALL
                                                     SELECT WH_NO,MMCODE,0 INQTY,0 OUTQTY,0 USEQTY,0 LOSSQTY,0 RSQTY,0 CHQTY,0 ADJQTY
                                                       FROM MI_WHINV A
                                                      WHERE WH_NO in {1} 
                                                            AND NOT EXISTS
                                                            (SELECT 1 FROM MI_WHTRNS 
                                                              WHERE (
                                                                     (TWN_DATE(TR_DATE) between '1090801' and '1090831' AND TR_MCODE NOT IN ('WAYI','WAYO','USEO','BAKI','ADJO')) OR
                                                                     (TWN_DATE(TR_DATE) between '1090801' and '1090831' AND TR_MCODE='BAKI' AND TR_DOCTYPE<>'RS') OR
                                                                     (TWN_DATE(TR_DATE) between '1090801' and '1090831' AND TR_MCODE='ADJO' AND TR_DOCTYPE<>'RR') OR
                                                                     (SUBSTR(TR_DOCNO,1,7) between '1090801' and '1090831' AND TR_MCODE='USEO') OR
                                                                     (SUBSTR(TR_DOCNO,1,7) between '1090801' and '1090831' AND TR_MCODE='BAKI' AND TR_DOCTYPE='RS') OR
                                                                     (SUBSTR(TR_DOCNO,1,7) between '1090801' and '1090831' AND TR_MCODE='ADJO' AND TR_DOCTYPE='RR')
                                                                    ) 
                                                                AND WH_NO=A.WH_NO AND MMCODE=A.MMCODE
                                                            )
                                                   )
                                             ORDER BY WH_NO,MMCODE
                                          )   a
                                      where exists (select 1 from MI_MAST c 
                                                     where 1=1 
                                                       and c.mmcode = a.mmcode
                                                           {0} 
                                                       {2}
                                                       {3}  
                                                       {4} 
                                                    )
                                        {5}
                                     order by a.mmcode
                ", Filter_Medicine
                 , wh_type_string
                 , IsHighPrice != string.Empty ? " and c.e_highpriceflag = :IsHighPrice" : string.Empty
                 , IsCDC != string.Empty ? " and c.e_returndrugflag = :IsCDC" : string.Empty
                 , e_orderdcflag != string.Empty ? " and c.e_orderdcflag = :e_orderdcflag" : string.Empty
                 , ctdmdccode != string.Empty ? @"  and exists (select 1 from MI_WINVCTL
                                                                 where wh_no = a.wh_no 
                                                                   and mmcode = a.mmcode 
                                                                   and ctdmdccode = :ctdmdccode)" : string.Empty);

            p.Add(":tmp_Date", string.Format("{0}", tmp_Date));
            p.Add(":IsHighPrice", string.Format("{0}", IsHighPrice));
            p.Add(":IsCDC", string.Format("{0}", IsCDC));
            p.Add(":e_orderdcflag", string.Format("{0}", e_orderdcflag));
            p.Add(":ctdmdccode", string.Format("{0}", ctdmdccode));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        #endregion
    }

}