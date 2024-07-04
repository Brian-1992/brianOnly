using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.AB
{
    public class AB0090Repository : JCLib.Mvc.BaseRepository
    {
        public AB0090Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        public class M : JCLib.Mvc.BaseModel
        {
            public string TR_MCODE { get; set; }
            public string WH_NO { get; set; }
            public string CONTRACNO { get; set; }
            public string DRUGPTYCODE1 { get; set; }
            public string DRUGPTYCODE2 { get; set; }
            public string INSUSIGNI { get; set; }
            public string MMCODE { get; set; }
            public string MMNAME_E { get; set; }
            public string E_COMPUNIT { get; set; }
            public string E_ORDERUNIT { get; set; }
            public string UPRICE { get; set; }
            public string M_CONTPRICE { get; set; }
            public string MAMAGERATE { get; set; }
            public string AVG_PRICE { get; set; }
            public string NHI_PRICE { get; set; }
            public string M_NHIKEY { get; set; }
            public string AGEN_NAME { get; set; }
            public string M_AGENLAB_RMK { get; set; }
            public string M_AGENLAB { get; set; }
            public string AGENTNAME1 { get; set; }
            public string AGENTNAME2 { get; set; }
            public string VISIT_KIND { get; set; }
            public string TR_DATE { get; set; }
            public double QTY { get; set; }
            public string E_SCIENTIFICNAME { get; set; }

            public string DISC_UPRICE { get; set; }
            public string DISC_CPRICE { get; set; }
        }
        public class Y
        {
            public string YYYMM { get; set; }
        }

        public IEnumerable<COMBO_MODEL> GetWhnoCombo()
        {
            string sql = @"SELECT WH_NO AS VALUE,WH_NAME(WH_NO) AS TEXT, 
                            WH_NO || ' ' || WH_NAME(WH_NO) AS COMBITEM
                            FROM MI_WHMAST 
                            WHERE WH_KIND = '0'
                              AND cancel_id = 'N'
                            ORDER BY WH_NO
                            ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }
        

        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E 
                        FROM MI_MAST A WHERE 1=1 AND A.MAT_CLASS = '01' ";

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR MMNAME_E LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<Y> GetYYYMM(string apptime_bg, string apptime_ed)
        {
            var p = new DynamicParameters();

            var sql = @"select set_ym as yyymm
                          from MI_MNSET
                         where set_ym between :bg and :ed
                         order by set_ym";
            p.Add(":bg", apptime_bg);
            p.Add(":ed", apptime_ed);
            var result = DBWork.Connection.Query<Y>(sql, p);
            return result;
        }

        #region 2020-09-17新增
        public MI_WINVMON GetInvmon(string data_ym, string wh_no, string mmcode) {
            string sql = string.Format(@"
                           select sum(nvl(apl_inqty, 0)) as apl_inqty,
                                  sum(nvl(apl_outqty, 0)) as apl_outqty
                             from MI_WINVMON
                            where data_ym = :data_ym
                              {0}
                              and mmcode = :mmcode
                            group by mmcode",
                              wh_no == string.Empty? string.Empty : " and wh_no = :wh_no ");
            return DBWork.Connection.QueryFirstOrDefault<MI_WINVMON>(sql, new { data_ym = data_ym, wh_no = wh_no, mmcode = mmcode }, DBWork.Transaction);
        }

        public MI_CONSUME_MN GetConsumeMn(string data_ym, string wh_no, string mmcode)
        {
            string sql = @"select sum(nvl(allqty1, 0)) as allqty1,
                                  sum(nvl(allqty2, 0)) as allqty2,
                                  sum(nvl(allqty3, 0)) as allqty3,
                                  sum(nvl(oth_consume, 0)) as oth_consume,
                                  sum((nvl(allqty1, 0) +  nvl(allqty3, 0) + nvl(oth_consume, 0) + nvl(allqty2, 0))) as allqty
                             from MI_CONSUME_MN_WH
                            where data_ym = :data_ym
                              and wh_no = :wh_no
                              and mmcode = :mmcode
                            group by mmcode";
            return DBWork.Connection.QueryFirstOrDefault<MI_CONSUME_MN>(sql, new { data_ym = data_ym, wh_no = wh_no, mmcode = mmcode }, DBWork.Transaction);
        }

        public IEnumerable<M> GetMs(string start_date, string end_date, string wh_no, string mmcode1, string mmcode2, bool isGrid, string orderdcflag) {
            string sql = string.Format(@"
               with temp_types as (
                     select '{0}' as temp_type from dual
                     union 
                     select '{1}' as temp_type from dual
                 ), mmcodes as (
                     select distinct b.temp_type as tr_mcode, a.mmcode
                       from MI_WINVMON a
                      cross join temp_types b
                      where a.data_ym between :start_date and :end_date
                       {3}
                       {2}"
                , isGrid ? "消" : "消耗"
                , isGrid ? "進" : "進貨"
                , wh_no == string.Empty ? string.Empty : " and a.wh_no = :wh_no "
                , orderdcflag == string.Empty ? string.Empty : " and exists (select 1 from MI_MAST where mmcode = a.mmcode and e_orderdcflag = :orderdcflag )"
            );

            if (mmcode1 != "" & mmcode2 != "")
            {
                sql += " and a.mmcode between :mmcode1 and :mmcode2 ";
            }
            if (mmcode1 != "" & mmcode2 == "")
            {
                sql += " and a.mmcode = :mmcode1 ";
            }
            if (mmcode1 == "" & mmcode2 != "")
            {
                sql += " and a.mmcode = :mmcode2 ";
            }

            sql += @"
                    group by b.temp_type, a.mmcode
                 ), 
                datas as (
                 select a.tr_mcode, a.mmcode, 
                        c.contracno as contracno,
                        b.insusigni as insusigni,
                        c.mmname_e as mmname_e,
                        c.e_compunit as e_compunit,
                        c.e_orderunit as e_orderunit,
                        c.uprice as uprice,
                        c.disc_uprice,
                        c.disc_cprice,
                        c.m_contprice as m_contprice,
                        b.mamagerate as mamagerate,
                        avg_price(twn_date(sysdate),a.mmcode) as avg_price,
                        c.nhi_price as nhi_price,
                        c.m_nhikey as m_nhikey,
                        c.m_agenno || '_' || agen_name(c.m_agenno) as agen_name,
                        ' ' m_agenlab_rmk,
                        c.m_agenlab as m_agenlab,
                        substr(b.agentname,1,instr(b.agentname,';')-1) as agentname1,
                        substr(b.agentname,instr(b.agentname,';')+1) as agentname2,
                        c.E_SCIENTIFICNAME as e_scientificname
                   from mmcodes a, HIS_BASORDD b, MI_MAST c
                   where b.ordercode = a.mmcode
                     and twn_date(sysdate) between b.begindate and b.enddate
                     and c.mmcode = a.mmcode
                 )
                 select * from datas
                  order by mmcode, tr_mcode
            ";

            if (isGrid) {
                return DBWork.PagingQuery<M>(sql, new { mmcode1 = mmcode1, mmcode2 = mmcode2, wh_no = wh_no, start_date = start_date, end_date = end_date, orderdcflag = orderdcflag }, DBWork.Transaction);
            }
            return DBWork.Connection.Query<M>(sql, new { mmcode1 = mmcode1, mmcode2 = mmcode2, wh_no = wh_no, start_date = start_date, end_date = end_date, orderdcflag = orderdcflag }, DBWork.Transaction);
        }

        public string CallProc(string data_ym)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_YM", value: data_ym, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 5);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 2);
            p.Add("O_RETMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 255);

            DBWork.Connection.Query("GEN_CONSUME_MN_WH", p, commandType: CommandType.StoredProcedure);
            string retid = p.Get<OracleString>("O_RETID").Value;
            string errmsg = p.Get<OracleString>("O_RETMSG").Value;
            if (retid == "N")
            {
                retid = errmsg;
            }
            return retid;
        }
        public bool CheckConsumeMnWh(string data_ym) {
            string sql = @"select 1 
                             from MI_CONSUME_MN_WH
                            where data_ym = :data_ym";
            return !(DBWork.Connection.ExecuteScalar(sql, new { data_ym = data_ym }, DBWork.Transaction) == null);
        }
        public string GetPreym() {
            string sql = @"select twn_pym(cur_setym) from dual";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }
        #endregion

        #region 2020-09-29新增: 新增全藥局選項

        public MI_WINVMON GetInvmon_phd(string data_ym, string mmcode) {
            string sql = string.Format(@"
                           select sum(nvl(apl_inqty, 0)) as apl_inqty,
                                  sum(nvl(apl_outqty, 0)) as apl_outqty
                             from MI_WINVMON
                            where data_ym = :data_ym
                              and wh_no in (select wh_no from MI_WHMAST where wh_kind = '0' and wh_grade = '2')
                              and mmcode = :mmcode
                            group by mmcode"
                         );
            return DBWork.Connection.QueryFirstOrDefault<MI_WINVMON>(sql, new { data_ym = data_ym,  mmcode = mmcode }, DBWork.Transaction);
        }

        public MI_CONSUME_MN GetConsumeMn_phd(string data_ym, string mmcode)
        {
            string sql = @"select sum(nvl(allqty1, 0)) as allqty1,
                                  sum(nvl(allqty2, 0)) as allqty2,
                                  sum(nvl(allqty3, 0)) as allqty3,
                                  sum(nvl(oth_consume, 0)) as oth_consume,
                                  sum((nvl(allqty1, 0) +  nvl(allqty3, 0) + nvl(oth_consume, 0) + nvl(allqty2, 0))) as allqty
                             from MI_CONSUME_MN_WH
                            where data_ym = :data_ym
                              and wh_no in (select wh_no from MI_WHMAST where wh_kind = '0' and wh_grade = '2')
                              and mmcode = :mmcode
                            group by mmcode";
            return DBWork.Connection.QueryFirstOrDefault<MI_CONSUME_MN>(sql, new { data_ym = data_ym,  mmcode = mmcode }, DBWork.Transaction);
        }

        public IEnumerable<M> GetMs_phd(string start_date, string end_date, string mmcode1, string mmcode2, bool isGrid, string orderdcflag)
        {
            string sql = string.Format(@"
               with temp_types as (
                     select '{0}' as temp_type from dual
                     union 
                     select '{1}' as temp_type from dual
                 ), mmcodes as (
                     select distinct b.temp_type as tr_mcode, a.mmcode
                       from MI_WINVMON a
                      cross join temp_types b
                      where a.data_ym between :start_date and :end_date
                        {2}
                        and a.wh_no in (select wh_no from MI_WHMAST where wh_kind = '0' and wh_grade = '2' and cancel_id = 'N')"
                , isGrid ? "消" : "消耗"
                , isGrid ? "進" : "進貨"
                , orderdcflag == string.Empty ? string.Empty : " and exists (select 1 from MI_MAST where mmcode = a.mmcode and e_orderdcflag = :orderdcflag) "
            );

            if (mmcode1 != "" & mmcode2 != "")
            {
                sql += " and a.mmcode between :mmcode1 and :mmcode2 ";
            }
            if (mmcode1 != "" & mmcode2 == "")
            {
                sql += " and a.mmcode = :mmcode1 ";
            }
            if (mmcode1 == "" & mmcode2 != "")
            {
                sql += " and a.mmcode = :mmcode2 ";
            }

            sql += @"
                    group by b.temp_type, a.mmcode
                 ), 
                datas as (
                 select a.tr_mcode, a.mmcode, 
                        c.contracno as contracno,
                        b.insusigni as insusigni,
                        c.mmname_e as mmname_e,
                        c.e_compunit as e_compunit,
                        c.e_orderunit as e_orderunit,
                        c.uprice as uprice,
                        c.disc_uprice,
                        c.disc_cprice,
                        c.m_contprice as m_contprice,
                        b.mamagerate as mamagerate,
                        avg_price(twn_date(sysdate),a.mmcode) as avg_price,
                        c.nhi_price as nhi_price,
                        c.m_nhikey as m_nhikey,
                        c.m_agenno || '_' || agen_name(c.m_agenno) as agen_name,
                        ' ' m_agenlab_rmk,
                        c.m_agenlab as m_agenlab,
                        substr(b.agentname,1,instr(b.agentname,';')-1) as agentname1,
                        substr(b.agentname,instr(b.agentname,';')+1) as agentname2,
                        c.E_SCIENTIFICNAME as e_scientificname
                   from mmcodes a, HIS_BASORDD b, MI_MAST c
                   where b.ordercode = a.mmcode
                     and twn_date(sysdate) between b.begindate and b.enddate
                     and c.mmcode = a.mmcode
                 )
                 select * from datas
                  order by mmcode, tr_mcode
            ";

            if (isGrid)
            {
                return DBWork.PagingQuery<M>(sql, new { mmcode1 = mmcode1, mmcode2 = mmcode2, start_date = start_date, end_date = end_date }, DBWork.Transaction);
            }
            return DBWork.Connection.Query<M>(sql, new { mmcode1 = mmcode1, mmcode2 = mmcode2,  start_date = start_date, end_date = end_date }, DBWork.Transaction);
        }

        #endregion
    }
}