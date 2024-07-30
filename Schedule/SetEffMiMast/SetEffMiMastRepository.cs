using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using WebApp.Models;

namespace SetEffMiMast
{
    class SetEffMiMastRepository : JCLib.Mvc.BaseRepository
    {
        public SetEffMiMastRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public int MergeIntoMI_MAST()
        {
            #region 
            string strSql = @"merge into mi_mast a
                    using(
                      with max_seq as (
                        select max(MIMASTHIS_SEQ) max_seq
                        from mi_mast_history
                        where twn_date(EffStartDate)=twn_date(sysdate)
                        group by mmcode
                      )
                      select
                        MIMASTHIS_SEQ, MMCODE, CANCEL_ID, 
                        E_ORDERDCFLAG, M_NHIKEY, HealthOwnExp, DRUGSNAME, MMNAME_E, 
                        MMNAME_C, M_PHCTNCO, M_ENVDT, IssueSupply, E_MANUFACT, 
                        BASE_UNIT, M_PURUN, TrUTRate, MAT_CLASS_SUB, E_RESTRICTCODE, 
                        WarBak, OneCost, HealthPay, CostKind, WastKind, 
                        SpXfee, OrderKind, CaseDoct, DrugKind, M_AGENNO, 
                        M_AGENLAB, CaseNo, E_SOURCECODE, M_CONTID, E_ITEMARMYNO, 
                        NHI_PRICE, DISC_CPRICE, M_CONTPRICE, E_CODATE, ContractAmt, 
                        ContractSum, TouchCase, BEGINDATE_14, IssPriceDate, SpDrug, 
                        FastDrug, COMMON, 
                        SpMMCODE, UnitRate, DISCOUNT_QTY, APPQTY_TIMES, DISC_COST_UPRICE, 
                        isIV, (case MAT_CLASS_SUB when '1' then '01' else '02' end) as MAT_CLASS,
                        M_STOREID
                      from mi_mast_history a, max_seq b
                      where a.mimasthis_seq = b.max_seq
                    ) b
                    on (a.mmcode=b.mmcode)
                    when matched then
                      update set 
                        MIMASTHIS_SEQ   =b.MIMASTHIS_SEQ,
                        CANCEL_ID       =b.CANCEL_ID,
                        E_ORDERDCFLAG   =b.E_ORDERDCFLAG,
                        M_NHIKEY        =b.M_NHIKEY,
                        HealthOwnExp    =b.HealthOwnExp,
                        DRUGSNAME       =b.DRUGSNAME,
                        MMNAME_E        =b.MMNAME_E,
                        MMNAME_C        =b.MMNAME_C,
                        M_PHCTNCO       =b.M_PHCTNCO,
                        M_ENVDT         =b.M_ENVDT,
                        IssueSupply     =b.IssueSupply,
                        E_MANUFACT      =b.E_MANUFACT,
                        BASE_UNIT       =b.BASE_UNIT,
                        M_PURUN         =b.M_PURUN,
                        TrUTRate        =b.TrUTRate,
                        MAT_CLASS_SUB   =b.MAT_CLASS_SUB,
                        E_RESTRICTCODE  =b.E_RESTRICTCODE,
                        WarBak          =b.WarBak,
                        OneCost         =b.OneCost,
                        HealthPay       =b.HealthPay,
                        CostKind        =b.CostKind,
                        WastKind        =b.WastKind,
                        SpXfee          =b.SpXfee,
                        OrderKind       =b.OrderKind,
                        CaseDoct        =b.CaseDoct,
                        DrugKind        =b.DrugKind,
                        M_AGENNO        =b.M_AGENNO,
                        M_AGENLAB       =b.M_AGENLAB,
                        CaseNo          =b.CaseNo,
                        E_SOURCECODE    =b.E_SOURCECODE,
                        M_CONTID        =b.M_CONTID,
                        E_ITEMARMYNO    =b.E_ITEMARMYNO,
                        NHI_PRICE       =b.NHI_PRICE,
                        DISC_CPRICE     =b.DISC_CPRICE,
                        M_CONTPRICE     =b.M_CONTPRICE,
                        E_CODATE        =b.E_CODATE,
                        ContractAmt     =b.ContractAmt,
                        ContractSum     =b.ContractSum,
                        TouchCase       =b.TouchCase,
                        BEGINDATE_14    =b.BEGINDATE_14,
                        IssPriceDate    =b.IssPriceDate,
                        SpDrug          =b.SpDrug,
                        FastDrug        =b.FastDrug,
                        UPDATE_TIME     =sysdate,
                        UPDATE_USER     ='生效日自動轉檔',
                        COMMON          =b.COMMON,
                        SpMMCODE        =b.SpMMCODE,
                        UnitRate        =b.UnitRate,
                        DISCOUNT_QTY    =b.DISCOUNT_QTY,
                        APPQTY_TIMES    =b.APPQTY_TIMES,
                        DISC_COST_UPRICE=b.DISC_COST_UPRICE,
                        isIV            =b.isIV,
                        MAT_CLASS       =b.MAT_CLASS,
                        M_STOREID       =b.M_STOREID
                    when not matched then
                      insert(MIMASTHIS_SEQ, MMCODE, CANCEL_ID,
                        E_ORDERDCFLAG, M_NHIKEY, HealthOwnExp, DRUGSNAME, MMNAME_E, 
                        MMNAME_C, M_PHCTNCO, M_ENVDT, IssueSupply, E_MANUFACT, 
                        BASE_UNIT, M_PURUN, TrUTRate, MAT_CLASS_SUB, E_RESTRICTCODE, 
                        WarBak, OneCost, HealthPay, CostKind, WastKind, 
                        SpXfee, OrderKind, CaseDoct, DrugKind, M_AGENNO, 
                        M_AGENLAB, CaseNo, E_SOURCECODE, M_CONTID, E_ITEMARMYNO, 
                        NHI_PRICE, DISC_CPRICE, M_CONTPRICE, E_CODATE, ContractAmt, 
                        ContractSum, TouchCase, BEGINDATE_14, IssPriceDate, SpDrug, 
                        FastDrug, CREATE_TIME, CREATE_USER, COMMON, 
                        SpMMCODE, UnitRate, DISCOUNT_QTY, APPQTY_TIMES, DISC_COST_UPRICE, 
                        isIV, MAT_CLASS, M_STOREID)
                      values(b.MIMASTHIS_SEQ, b.MMCODE, b.CANCEL_ID,
                        b.E_ORDERDCFLAG, b.M_NHIKEY, b.HealthOwnExp, b.DRUGSNAME, b.MMNAME_E,
                        b.MMNAME_C, b.M_PHCTNCO, b.M_ENVDT, b.IssueSupply, b.E_MANUFACT,
                        b.BASE_UNIT, b.M_PURUN, b.TrUTRate, b.MAT_CLASS_SUB, b.E_RESTRICTCODE,
                        b.WarBak, b.OneCost, b.HealthPay, b.CostKind, b.WastKind,
                        b.SpXfee, b.OrderKind, b.CaseDoct, b.DrugKind, b.M_AGENNO,
                        b.M_AGENLAB, b.CaseNo, b.E_SOURCECODE, b.M_CONTID, b.E_ITEMARMYNO,
                        b.NHI_PRICE, b.DISC_CPRICE, b.M_CONTPRICE, b.E_CODATE, b.ContractAmt,
                        b.ContractSum, b.TouchCase, b.BEGINDATE_14, b.IssPriceDate, b.SpDrug,
                        b.FastDrug, sysdate, '生效日自動轉檔', b.COMMON,
                        b.SpMMCODE, b.UnitRate, b.DISCOUNT_QTY, b.APPQTY_TIMES, b.DISC_COST_UPRICE,
                        b.isIV, b.MAT_CLASS, b.M_STOREID)";
            #endregion

            return DBWork.Connection.Execute(strSql);
        }

        public int MergeIntoMI_UNITEXCH()
        {
            string strSql = @"merge into MI_UNITEXCH a
                        using (
                          select
                              MMCODE, BASE_UNIT as UNIT_CODE, 
                              NVL(M_AGENNO, ' ') as AGEN_NO, 1 as EXCH_RATIO
                           from mi_mast_history
                          where twn_date(EffStartDate)=twn_date(sysdate)
                          union
                          select 
                              MMCODE, M_PURUN as UNIT_CODE, 
                              NVL(M_AGENNO, ' ') as AGEN_NO, UnitRate as EXCH_RATIO
                           from mi_mast_history
                          where twn_date(EffStartDate)=twn_date(sysdate)
                          and nvl(BASE_UNIT,' ') <> nvl(M_PURUN,' ')
                        ) b
                        on (a.MMCODE=b.MMCODE and a.UNIT_CODE=b.UNIT_CODE and 
                            a.AGEN_NO=b.AGEN_NO and a.EXCH_RATIO=b.EXCH_RATIO)
                        when not matched then
                          insert
                            (MMCODE, UNIT_CODE, AGEN_NO, EXCH_RATIO, CREATE_TIME, CREATE_USER)
                          values
                            (b.MMCODE, b.UNIT_CODE, b.AGEN_NO, b.EXCH_RATIO, sysdate, '生效日自動轉檔')
                        ";

            return DBWork.Connection.Execute(strSql);
        }

        public IEnumerable<MI_MAST_HISTORY> GetMmcodesFromMi_Mast_History()
        {
            string strSql = @"select MMCODE 
              from mi_mast_history
             where twn_date(EffStartDate) = twn_date(sysdate)
               and MAT_CLASS_SUB='1'";

            return DBWork.Connection.Query<MI_MAST_HISTORY>(strSql);
        }

        public IEnumerable<MI_WHMAST> GetWH_NosFromMi_Whmast()
        {
            string strSql = @"select WH_NO
              from mi_whmast
             where wh_kind='0'
               and wh_grade in ('1','2')
               and nvl(cancel_id,'N')='N'";

            return DBWork.Connection.Query<MI_WHMAST>(strSql);
        }

        public string GetMI_WINVCTL(string strMmcode, string strWhNo)
        {
            string strSql = @"select 1 from MI_WINVCTL where mmcode=:MMCODE and wh_no= :WH_NO";

            return DBWork.Connection.ExecuteScalar<string>(strSql, new { mmcode = strMmcode, wh_no = strWhNo});
        }

        public int InsertIntoMI_WINVCTL(string strMmcode, string strWhNo)
        {
            string strSql = @"insert into MI_WINVCTL
                (WH_NO,MMCODE,SUPPLY_WHNO,CREATE_TIME,CREATE_USER)
              values
                ( :WH_NO, :MMCODE,WHNO_ME1,sysdate,'生效日自動轉檔')";

            return DBWork.Connection.Execute(strSql, new { MMCODE = strMmcode, WH_NO = strWhNo });
        }
    }
}
