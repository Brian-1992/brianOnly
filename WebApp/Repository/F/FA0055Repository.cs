using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using WebApp.Models.MI;

namespace WebApp.Repository.F
{

    public class FA0055ReportMODEL : JCLib.Mvc.BaseModel
    {
        public string F1 { get; set; }
        public string F2 { get; set; }
        public string F3 { get; set; }
        public string F4 { get; set; }
        public string F5 { get; set; }
        public double F6 { get; set; }
        public double F7 { get; set; }
        public double F8 { get; set; }
        public double F9 { get; set; }
        public double F10 { get; set; }
        public double F11 { get; set; }
        public double F12 { get; set; }
        

    }
    public class FA0055Detail : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }
        public string F2 { get; set; }
        public string F3 { get; set; }
        public string F4 { get; set; }
        public string F5 { get; set; }
        public string F6 { get; set; }
        public string F7 { get; set; }
        public string F8 { get; set; }
        public string F9 { get; set; }
        public string F10 { get; set; }
        public string F11 { get; set; }
        public string F12 { get; set; }
        public string F13 { get; set; }
        public string F14 { get; set; }
        public string F15 { get; set; }
        public string F16 { get; set; }
        public string F17 { get; set; }
        public string F18 { get; set; }
        public string F19 { get; set; }
        public string F20 { get; set; }
        public string F21 { get; set; }
        public string F22 { get; set; }
        public string F23 { get; set; }
        public string F24 { get; set; }
        public string F25 { get; set; }
        public string F26 { get; set; }
        public string F27 { get; set; }
        public string F28 { get; set; }
        public string DATA_YM { get; set; }

        public string F29 { get; set; }
        public string F30 { get; set; }

        public string F31 { get; set; }
        public string F32 { get; set; }
        public string F33 { get; set; }
        public string F34 { get; set; }
    }
    public class FA0055Repository : JCLib.Mvc.BaseRepository
    {
        public FA0055Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }


        public IEnumerable<FA0055> GetAllM(string yyymm,int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT X.DATA_YM F1,                -- 月份
                               X.P_INV_COST F2,             -- 上月結存成本
                               (x.IN_COST - x.TPEO_INCOST + x.OTH_INCOST + x.OTH_INCOST_2 + x.OTH_INCOST_3) F3,                -- 本月買進成本
                               (x.USE_COST_O + x.USE_COST_H + x.USE_COST_E) F4,               -- 本月醫令消耗成本
                               (X.INVENT_COST - X.INVENT_COST_O) F5,            -- 藥局盤盈虧調整金額
                               X.DIS_COST F6,               -- 銷毀過效期品
                               X.REJ_COST F7,               -- 退貨金額
                               X.ADJ_COST_24 F8,            -- 2-4級庫調整金額
                               X.INV_COST F9,               -- 本月結存成本
                               X.ADJ_COST_OTH F10,          -- 調整金額
                               (x.USE_COST_O + x.USE_COST_H + x.USE_COST_E + X.USE_COST_OTH + X.ADJ_COST_24 + X.DIS_COST) F11,                  -- 藥材成本
                               X.USE_COST_H F12,                  -- 住院藥費消耗成本
                               X.USE_COST_O F13,             -- 門診藥費消耗成本
                               X.USE_COST_E F24,             -- 急診藥費消耗成本
                               X.USE_COST_OTH F17,             -- 其他消耗成本(L)
                               x.INCOME_AMT as F14   ,               -- 藥材總收入
                               x.TPEO_INCOST as F15  ,          -- 台北門診中心買藥金額
                               (x.OTH_INCOST + x.OTH_INCOST_2 + x.OTH_INCOST_3) as F16            -- 本院買(管制藥、抗瘧藥、蛇毒血清)金額
                          FROM ME_FA_INVMON X 
                         WHERE 1=1
                           AND X.DATA_YM=:SETYM
             ";
            p.Add(":SETYM", yyymm);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<FA0055>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        
        //匯出
        public DataTable GetExcel(string yyymm)
        {
            DynamicParameters p = new DynamicParameters();
            var sql = @"SELECT X.DATA_YM  as 月份,                
                               X.P_INV_COST as 上月結存成本,            
                               (x.IN_COST - x.TPEO_INCOST + x.OTH_INCOST + x.OTH_INCOST_2 + x.OTH_INCOST_3) as 本月買進成本,     
                               (x.USE_COST_O + x.USE_COST_H + x.USE_COST_E) as 本月醫令消耗成本,       
                               (X.INVENT_COST - X.INVENT_COST_O) as 藥局盤盈虧調整金額,          
                               X.DIS_COST as 銷毀過效期品,            
                               X.REJ_COST as 退貨金額,             
                               X.ADJ_COST_24 as ""2-4級庫調整金額"",         
                               X.INV_COST 本月結存成本,       
                               X.ADJ_COST_OTH 調整金額,            
                               (x.USE_COST_O + x.USE_COST_H + x.USE_COST_E + X.USE_COST_OTH + X.ADJ_COST_24 + X.DIS_COST) 藥材成本,              
                               X.USE_COST_H 住院藥費消耗成本,           
                               X.USE_COST_O 門診藥費消耗成本,  
                               X.USE_COST_E 急診藥費消耗成本,
                               X.USE_COST_OTH ""其他消耗成本(L)"",    
                               x.INCOME_AMT as 藥材總收入   ,         
                               x.TPEO_INCOST as 北門買藥金額,     
                                (x.OTH_INCOST + x.OTH_INCOST_2 + x.OTH_INCOST_3) as ""本院買(管抗蛇)金額""  
                          FROM ME_FA_INVMON X WHERE 1=1
                           AND X.DATA_YM=:SETYM
                 ";

            p.Add(":SETYM", yyymm);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public string CallProc(string id, string TPEO_INCOST, string OTH_INCOST, string OTH_INCOST2, string OTH_INCOST3)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_YM", value: id, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 5);
            p.Add("I_TPEO_INCOST", value: TPEO_INCOST, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);
            p.Add("I_OTH_INCOST", value: OTH_INCOST, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);
            p.Add("I_OTH_INCOST2", value: OTH_INCOST2, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);
            p.Add("I_OTH_INCOST3", value: OTH_INCOST3, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 2);
            p.Add("O_RETMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 255);

            DBWork.Connection.Query("GEN_FA_INVMON", p, commandType: CommandType.StoredProcedure);
            string retid = p.Get<OracleString>("O_RETID").Value;
            string errmsg = p.Get<OracleString>("O_RETMSG").Value;
            if (retid == "N")
            {
                retid = errmsg;
            }
            return retid;
        }
        public IEnumerable<ComboItemModel> GetYMCombo()
        {
            var p = new DynamicParameters();

            string sql = @" SELECT  SET_YM VALUE, SET_YM TEXT
                            FROM    MI_MNSET
                            WHERE SET_STATUS = 'C'
                            ORDER BY SET_YM DESC ";

            return DBWork.Connection.Query<ComboItemModel>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<FA0055ReportMODEL> GetTpeoOth(string data_ym) {
            string sql = @"select TPEO_INCOST as F1, 
                                  OTH_INCOST as F2, 
                                  OTH_INCOST_2 as F3, 
                                  OTH_INCOST_3 as F4
                             from ME_FA_INVMON
                            where 1=1
                              and data_ym = :data_ym";
            return DBWork.Connection.Query<FA0055ReportMODEL>(sql, new { data_ym = data_ym }, DBWork.Transaction);
        }


        #region 明細
        // 查詢與匯出相同SQL部分
        private string GetDetailQuerySql(string status) {
            string status_string = status == string.Empty ? string.Empty    // 空白
                                   : status == "1" ? " and F18 >= 0 "       // 1: >= 0 
                                   : "and F18 < 0";                         // 2: < 0

            string sql = string.Format(@"
            with detail as (
                 select MMCODE,to_char(F2) as F2,(F2*F24) as F3,F4,F5,
                        F6,F7,(F4+F6) as F8,(F5+F7) as F9,to_char(F10) as F10,
                        F11,to_char(F12) as F12,to_char(F13) as F13,to_char(F14) as F14,F15,
                        to_char(F16) as F16,to_char(F17) as F17,to_char(F18) as F18,F19,
                        to_char(F20) as F20,
                        to_char(F21) as F21,to_char(F22) as F22,to_char(F23) as F23,to_char(F24) F24,
                        (select MMNAME_E from MI_MAST where MMCODE=B.MMCODE) as F25,
                        to_char(F26) as F26,to_char(F27) as F27,to_char(F28) as F28,
                        to_char(F29) as F29, F30,
                        :data_ym as DATA_YM, 
                        to_char(F31) as F31, F32, to_char(F33) as F33, to_char(F34) as F34
                 from 
                 (
                 select A.*,
                        ((F12+F16+F33)*F23) as F4,((F12+F16+F33)*F21) as F5,
                        ((F13+F17+F34)*F23) as F6,((F13+F17+F34)*F20) as F7,
                        (F10*F23) as F11,(F14*F23) as F15,(F18*F23) as F19,
                        (F29*F23) as F30,
                        (F31*F23) as F32
                   from
                     (select MMCODE,
                             sum(P_INV_QTY) as F2,
                             sum(INV_QTY) as F18,
                             sum(ALLQTY1) as F10,sum(HOSPQTY1) as F12,sum(INSUQTY1) as F13,
                             sum(ALLQTY3) as F14,sum(HOSPQTY3) as F16,sum(INSUQTY3) as F17,
                             sum(ALLQTY2) as F31,sum(HOSPQTY2) as F33,sum(INSUQTY2) as F34,
                             INSU_PRICE as F20,HOSP_PRICE as F21,
                             DISC_UPRICE as F22,
                             AVG_PRICE as F23,
                             PMN_AVGPRICE as F24,
                             M_NHIKEY as F26,
                             BASE_UNIT as F27,E_ORDERDCFLAG as F28,
                             sum(OTH_CONSUME) as F29
                        from MI_CONSUME_MN a1
                       where DATA_YM=:data_ym
                         and exists (select 1 from MI_MAST where mat_class = '01' and mmcode = a1.mmcode)
                       group by MMCODE, INSU_PRICE, HOSP_PRICE, M_NHIKEY, BASE_UNIT, E_ORDERDCFLAG, DISC_UPRICE, AVG_PRICE, PMN_AVGPRICE
                     ) A
                 ) B
                 where 1=1

                 --若 查詢條件期末存量 有值
                   {0}
                 )  --end of with detail
                 select '合計' as MMCODE,'' as F2,sum(F3) as F3,sum(F4) as F4,sum(F5) as F5,
                        sum(F6) as F6,sum(F7) as F7,sum(F8) as F8,sum(F9) as F9,'' as F10,
                        sum(F11) as F11,'' as F12,'' as F13,'' as F14,sum(F15) as F15,
                        '' as F16,'' as F17,'' as F18,sum(F19) as F19,'' as F20,
                        '' as F21,'' as F22,'' as F23,'' as F24,'' as F25,
                        '' as F26,'' as F27,'' as F28,'' as F29, sum(F30) as F30, DATA_YM,
                        '' as F31, sum(F32) as F32, '' as F33, '' as F34
                   from detail
                  group by DATA_YM  --加總
                 union
                 select * from detail  --明細
                 order by MMCODE
            ", status_string);

            return sql;
        }
        public IEnumerable<FA0055Detail> GetDetails(string data_ym, string status) {

            string sql = GetDetailQuerySql(status);
            return DBWork.PagingQuery<FA0055Detail>(sql, new { data_ym = data_ym}, DBWork.Transaction);
        }

        public DataTable GetDetailsExcel(string data_ym, string status) {
            string sql = GetDetailQuerySql(status);

            sql = string.Format(@"
                        select MMCODE as 院內碼,F2 as 期初存量,F3 as 期初成本,
                               F4 as 自費消耗成本,F5 as 自費消耗收入,
                               F6 as 健保消耗成本,F7 as 健保消耗收入,
                               F8 as 全院消耗成本,F9 as 全院消耗額收入,
                               F10 as 門診耗量,F11 as 門診耗量成本,F12 as 門診自費耗量,F13 as 門診健保耗量,
                               F31 as 急診耗量,F32 as 急診耗量成本,F33 as 急診自費耗量,F34 as 急診健保耗量,
                               F14 as 住院耗量,F15 as 住院耗量成本,F16 as 住院自費耗量,F17 as 住院健保耗量,
                               F18 as 期末存量,
                               F19 as ""期末存量成本(表)"",F20 as 健保價,
                               F21 as 自費價, F22 as 進價, F23 as 移動加權平均單價,
                               F24 as 上月移動加權平均單價, F25 as 英文名稱, F26 as 健保碼,
                               F27 as 扣庫單位, F28 as 全院停用碼, 
                               F29 as 其他耗量, F30 as 其他消耗成本, 
                               DATA_YM as 查詢條件月結年月
                          from (
                                 {0}
                               ) AA
                  ", sql);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, new { data_ym = data_ym}, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        #endregion

        #region 列印
        public MI_MNSET GetMimnset(string set_ym) {
            string sql = @"select twn_date(set_btime) as set_btime,
                                  twn_date(set_ctime) as set_ctime
                             from MI_MNSET
                            where set_ym = :set_ym";
            return DBWork.Connection.QueryFirstOrDefault<MI_MNSET>(sql, new { set_ym = set_ym }, DBWork.Transaction);
        }
        public IEnumerable<FA0055> GetPrint(string yyymm)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT X.DATA_YM F1,                -- 月份
                               round(X.P_INV_COST, 2) F2,             -- 上月結存成本
                               round((x.IN_COST - x.TPEO_INCOST + x.OTH_INCOST + x.OTH_INCOST_2 + x.OTH_INCOST_3), 2) F3,                -- 本月買進成本
                               round((x.USE_COST_O + x.USE_COST_H + X.USE_COST_E), 2) F4,               -- 本月醫令消耗成本
                               round((X.INVENT_COST - X.INVENT_COST_O), 2) F5,            -- 藥局盤盈虧調整金額
                               round(X.DIS_COST, 2) F6,               -- 銷毀過效期品
                               round(X.REJ_COST, 2) F7,               -- 退貨金額
                               round(X.ADJ_COST_24, 2) F8,            -- 2-4級庫調整金額
                               round(X.INV_COST, 2) F9,               -- 本月結存成本
                               round(X.ADJ_COST_OTH, 2) F10,          -- 調整金額
                               round((x.USE_COST_O + x.USE_COST_H + X.USE_COST_E + X.USE_COST_OTH + X.ADJ_COST_24 + X.DIS_COST), 2) F11,                  -- 藥材成本
                               round(X.USE_COST_H, 2) F12,                  -- 住院藥費消耗成本
                               round(X.USE_COST_O, 2) F13,             -- 門診藥費消耗成本
                               round(X.USE_COST_E, 2) F24,             -- 急診藥費消耗成本
                               round(x.INCOME_AMT, 2) as F14   ,               -- 藥材總收入
                               round(x.TPEO_INCOST, 2) as F15  ,          -- 台北門診中心買藥金額
                               round((x.OTH_INCOST + x.OTH_INCOST_2 + x.OTH_INCOST_3), 2) as F16,            -- 本院買(管制藥、抗瘧藥、蛇毒血清)金額
                               round(X.USE_COST_OTH, 2) F17,             -- 其他消耗成本(L)
                               round(x.INVENT_COST, 2) as F18,      -- 藥局盤盈調整金額
                               round(x.INVENT_COST_O, 2) as F19,    -- 藥局盤虧調整金額
                               round(x.OTH_INCOST, 2) as F20,       -- 本院買(管制藥)金額 
                               round(x.OTH_INCOST_2, 2) as F21,     -- 本院買(抗瘧藥)金額
                               round(x.OTH_INCOST_3, 2) as F22,     -- 本院買(蛇毒血清)金額
                               round(x.IN_COST, 2) as F23           -- 一般藥材費用
                          FROM ME_FA_INVMON X 
                         WHERE 1=1
                           AND X.DATA_YM=:SETYM
             ";
            p.Add(":SETYM", yyymm);

            return DBWork.Connection.Query<FA0055>(sql, p, DBWork.Transaction);
        }
        #endregion

        #region 2021-02-03 其他耗用成本
        public DataTable GetOtherDetailsExcel(string data_ym) {
            string sql = @"
                select mmcode as 院內碼, typename as 類別, wh_no as 庫房, use_time as 異動時間, use_qty as 數量, other as 備註,
                       (select avg_price from MI_WHCOST where data_ym = :data_ym and mmcode = a.mmcode) as 平均單價
                  from (select 0 as ordercolumn,
                               mmcode,
                               wh_no,
                               '盤點功能問題修改資料' as typename,
                               twn_time(tr_date) as use_time,
                               tr_inv_qty as use_qty,
                               '' as other
                          from MI_WHTRNS a
                         where tr_date between (select set_btime from MI_MNSET where set_ym = :data_ym) 
                                           and (select set_ctime from MI_MNSET where set_ym = :data_ym)
                           and tr_docno = 'CHK ROLLBACK'
                        union
                        select 1 as ordercolumn,
                               mmcode,
                               wh_no,
                               '盤點' as typename,
                               twn_time(post_time) as use_time,
                               (store_qty - chk_qty) as use_qty,
                               chk_no as other
                          from MM_INVPOST a
                         where a.chk_no in (select chk_no from CHK_MAST 
                                           where create_date between (select set_btime from MI_MNSET where set_ym = :data_ym) 
                                                                 and (select set_ctime from MI_MNSET where set_ym = :data_ym))
                           and af_inventoryqty = bf_inventoryqty
                           and exists (select 1 from MI_WHMAST where wh_kind = '0' and wh_grade in ('3', '4') and wh_no = a.wh_no)
                           and (store_qty - chk_qty) <> 0
                           and exists (select 1 from MI_WINVCTL where wh_no = a.wh_no and mmcode = a.mmcode and nowconsumeflag = 'N')
                        union
                        select 2 as ordercolumn,
                               mmcode,
                               wh_no,
                               '耗損' as typename,
                               data_date as use_time,
                               sum(parent_consume_qty)   as use_qty,
                               ' ' as other
                          from MI_CONSUME_DATE a
                         where data_date between twn_date((select set_btime from MI_MNSET where set_ym = :data_ym)) 
                                             and twn_date((select set_ctime from MI_MNSET where set_ym = :data_ym) )
                           and proc_id ='Y' and data_id='L' 
                           and not exists (select 1 from MI_WINVCTL where wh_no = a.wh_no and mmcode = a.mmcode and nowconsumeflag = 'Y')
                         group by mmcode, wh_no, data_date
                        union
                        select 3 as ordercolumn,
                               a.mmcode,
                               a.wh_no,
                               '撥發即消耗' as typename,
                               ' ' as use_time,
                               use_qty - (select nvl(sum(parent_consume_qty), 0) 
                                            from MI_CONSUME_DATE
                                           where data_date between (select twn_date(set_btime) from MI_MNSET where set_ym = :data_ym) 
                                                               and (select twn_date(set_ctime) from MI_MNSET where set_ym = :data_ym)
                                             and wh_no = a.wh_no
                                             and mmcode = a.mmcode
                                             and data_id = 'H' and proc_id = 'Y'),
                               '' as other
                          from MI_WINVMON a, MI_WINVCTL b
                         where a.data_ym = :data_ym
                           and b.mmcode = a.mmcode
                           and b.wh_no = a.wh_no
                           and b.NOWCONSUMEFLAG = 'Y'
                           and use_qty <> 0
                           and exists (select 1 from MI_WHMAST 
                                        where wh_no = a.wh_no and wh_kind = '0' and wh_grade ='2')
                        union
                        select 4 as ordercolumn,
                               a.mmcode,
                               a.wh_no,
                               '撥發即消耗' as typename,
                               ' ' as use_time,
                               use_qty - (select nvl(sum(parent_consume_qty), 0) 
                                            from MI_CONSUME_DATE
                                           where data_date between (select twn_date(set_btime) from MI_MNSET where set_ym = :data_ym) 
                                                               and (select twn_date(set_ctime) from MI_MNSET where set_ym = :data_ym)
                                             and wh_no = a.wh_no
                                             and mmcode = a.mmcode
                                             and data_id = 'H' and proc_id = 'Y'),
                               '' as other
                          from MI_WINVMON a, MI_WINVCTL b
                         where a.data_ym = :data_ym
                           and b.mmcode = a.mmcode
                           and b.wh_no = a.wh_no
                           and b.NOWCONSUMEFLAG = 'Y'
                           and use_qty <> 0
                           and exists (select 1 from MI_WHMAST 
                                        where wh_no = a.wh_no and wh_kind = '0' and wh_grade in ('3', '4'))
                        ) a
                 where exists (select 1 from MI_MAST where mmcode = a.mmcode and mat_class='01')
                 order by mmcode, ordercolumn, wh_no
            ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, new { data_ym = data_ym }, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        #endregion

        #region 2-4級庫調整金額明細
        public DataTable GetAdjCost24Excel(string data_ym) {
            string sql = @"
                select mmcode as 院內碼,
                       tr_date as 異動日期,
                       wh_no as 庫房,
                       tr_doctype as 異動類別,
                       nvl(tr_inv_qty, 0) as 異動數量,
                       tr_docno as 調帳單號,
                       trans_kind as 調帳原因,
                       (select avg_price from MI_WHCOST where data_ym = :data_ym and mmcode = a.mmcode) as 平均單價,
                       (nvl(tr_inv_qty, 0) * (select avg_price from MI_WHCOST where data_ym = :data_ym and mmcode = a.mmcode))  as 調整金額                  
                  from (select mmcode, twn_date(tr_date) as tr_date, wh_no, 
                               (select doctype_name 
                                  from MI_DOCTYPE 
                                 where doctype = a.tr_doctype) as tr_doctype, 
                               sum((case when tr_mcode = 'ADJO' then tr_inv_qty else (tr_inv_qty * (-1) ) end)) as tr_inv_qty, 
                               tr_docno, 
                               (select data_desc 
                                  from PARAM_D
                                 where grp_code = 'ME_DOCD' and data_name = 'TRANSKIND'
                                   and data_value = (select transkind from ME_DOCD where docno = a.tr_docno and mmcode = a.mmcode)   
                                ) as trans_kind
                          from MI_WHTRNS a
                         where 1=1
                           and twn_date(tr_date) between (select twn_date(set_btime) from MI_MNSET where set_ym = :data_ym) 
                                                     and (select twn_date(set_ctime) from MI_MNSET where set_ym = :data_ym) 
                           and tr_mcode in ('ADJO', 'ADJI') and tr_doctype<>'RR'
                           AND EXISTS
                               (SELECT 1 FROM MI_WHMAST WHERE WH_NO=A.WH_NO AND WH_KIND='0' 
                               AND WH_GRADE IN ('2','3','4'))
                               group by wh_no, mmcode, tr_doctype, tr_date, tr_docno
                        union      
                        select mmcode, SUBSTR(TR_DOCNO,1,7) as tr_date, wh_no, 
                               (select doctype_name 
                                  from MI_DOCTYPE 
                                 where doctype = a.tr_doctype) as tr_doctype,  
                               sum((case when tr_mcode = 'ADJO' then tr_inv_qty else (tr_inv_qty * (-1) ) end)) as tr_inv_qty, '' as tr_docno,
                               '' as trans_kind
                          from MI_WHTRNS a
                         where 1=1
                           and SUBSTR(TR_DOCNO,1,7) between (select twn_date(set_btime) from MI_MNSET where set_ym = :data_ym) 
                                                        and (select twn_date(set_ctime) from MI_MNSET where set_ym = :data_ym) 
                           and tr_mcode in ('ADJO', 'ADJI') AND TR_DOCTYPE='RR'
                           AND EXISTS
                               (SELECT 1 FROM MI_WHMAST WHERE WH_NO=A.WH_NO AND WH_KIND='0' 
                               AND WH_GRADE IN ('2','3','4'))
                         group by mmcode, SUBSTR(TR_DOCNO,1,7), wh_no, tr_doctype 
                        union      
                        select mmcode, SUBSTR(TR_DOCNO,1,7) as tr_date, wh_no, 
                               (select doctype_name 
                                  from MI_DOCTYPE 
                                 where doctype = a.tr_doctype) as tr_doctype,  
                               sum((case when tr_mcode = 'ADJO' then tr_inv_qty else (tr_inv_qty * (-1) ) end)) as tr_inv_qty, 
                               tr_docno as tr_docno,
                               '扣庫異常上簽奉核調帳' as trans_kind
                          from MI_WHTRNS a
                         where 1=1
                           and SUBSTR(TR_DOCNO,1,7) between (select twn_date(set_btime) from MI_MNSET where set_ym = :data_ym) 
                                                        and (select twn_date(set_ctime) from MI_MNSET where set_ym = :data_ym) 
                           and tr_mcode in ('ADJO', 'ADJI') AND TR_DOCTYPE='NAJ'
                           AND EXISTS
                               (SELECT 1 FROM MI_WHMAST WHERE WH_NO=A.WH_NO  
                               AND WH_GRADE IN ('2','3','4'))
                         group by mmcode, SUBSTR(TR_DOCNO,1,7), wh_no, tr_doctype, tr_docno
                       ) a
                 order by mmcode, tr_date, wh_no
            ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, new { data_ym = data_ym }, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        #endregion
    }
}
