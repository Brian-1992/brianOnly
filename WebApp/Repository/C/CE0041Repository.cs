using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using WebApp.Models.C;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using WebApp.Models.MI;

namespace WebApp.Repository.C
{
    public class CE0041Repository : JCLib.Mvc.BaseRepository
    {

        public CE0041Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }


        public IEnumerable<CE0003> GetAllM(
            string chk_no, // 盤點單號
            string chk_ym, // 盤點年月
            string chk_wh_no, // 庫房
            string mmcode, // 院內碼
            string mmname_e, // 英文品名
            string store_loc, // 儲位
            string disc_cprice, // 價格
            string isIV, // 是否點滴
            string drug_kind,
            string CB_1, // (近6個月進貨)或(近6個月醫令耗用)或(庫量<>0)或(庫存=0且無作廢)
            string CB_2, // (期初庫存<>0)或(期初=0但有進出)
            int page_index, int page_size, string sorters
        )
        {
            var p = new DynamicParameters();

            var sql = @"
           with set_times as ( 
               select set_ym as set_ym,  
               twn_date(set_btime) as set_btime,  
               twn_date(set_ctime) as set_ctime 
               from MI_MNSET -- 盤點年月 
               where ( 
               	select create_date - 1 
               	from CHK_MAST where 1=1 -- 盤點主檔 
               	and chk_no = :chk_no 
               ) between set_btime and nvl(set_ctime+1, sysdate)  
               and rownum = 1  
               order by set_ym desc 
           ) 
           select  
           a.chk_no, a.mmcode, a.mmname_c, a.mmname_e, a.BASE_UNIT, m.chk_status, 
           m.chk_ym, m.chk_wh_grade, m.chk_wh_kind, m.chk_wh_no as wh_no,
           ( 
               select listagg(store_loc, ', ') within group (order by store_loc)  
               from MI_WLOCINV where wh_no = a.wh_no and mmcode = a.mmcode 
           ) STORE_LOC, -- 儲位 
           a.store_qtyc as STORE_QTYC, -- 電腦量 
           a.CHK_QTY, -- 盤點量 
           a.chk_qty as ori_chk_qty, -- 原始盤點量
           a.ALTERED_USE_QTY, -- 調整消耗 
           a.CHK_REMARK, -- 備註 
           a.INVENTORY, -- 差異量 
           (a.inventory * a.disc_cprice) DIFF_PRICE, -- 差異金額
           (a.pym_inv_qty -- 上月庫存數量 
            +a.apl_inqty   -- 月結入庫總量 
            -a.apl_outqty  -- 月結撥發總量 
            +a.trn_inqty   -- 月結調撥入總量 
            -a.trn_outqty  -- 月結調撥入總量 
            +a.adj_inqty   -- 月結調帳入總量 
            -a.adj_outqty  -- 月結調帳出總量 
            +a.bak_inqty   -- 月結繳回入庫總量 
            -a.bak_outqty  -- 月結繳回出庫總量 
            -a.rej_outqty  -- 月結退貨總量 
            -a.dis_outqty  -- 月結報廢總量
            +a.exg_inqty   -- 月結換貨入庫總量 
            -a.exg_outqty  -- 月結換貨出庫總量 
            -nvl( 
                nvl(a.altered_use_qty, a.use_qty), -- 調整消耗(只有藥局可以用), 月結耗用量=醫令扣庫 
                0 
            ) ) THEORY_QTY,
           NVL(a.PYM_INV_QTY,0) PYM_INV_QTY, -- 上期結存 
           NVL(TO_CHAR(apl_inqty),NVL((case when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no)) then (select TO_CHAR(apl_inqty) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode) else (select TO_CHAR(apl_inqty) from MI_WINVMON where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no) and wh_no = a.wh_no and mmcode = a.mmcode) end),' ')) as APL_INQTY, -- 進貨/撥發入 
           NVL(TO_CHAR(apl_outqty),NVL((case when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no)) then (select TO_CHAR(apl_outqty) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode) else (select TO_CHAR(apl_outqty) from MI_WINVMON where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no) and wh_no = a.wh_no and mmcode = a.mmcode) end),' ')) as APL_OUTQTY, -- 撥發出
           NVL(TO_CHAR(TRN_INQTY),NVL((case when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no)) then (select TO_CHAR(TRN_INQTY) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode) else (select TO_CHAR(TRN_INQTY) from MI_WINVMON where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no) and wh_no = a.wh_no and mmcode = a.mmcode) end),' ')) as TRN_INQTY, -- 調撥入 
           NVL(TO_CHAR(TRN_OUTQTY),NVL((case  when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no)) then (select TO_CHAR(TRN_OUTQTY) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode) else (select TO_CHAR(TRN_OUTQTY) from MI_WINVMON where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no) and wh_no = a.wh_no and mmcode = a.mmcode) end),' ')) as TRN_OUTQTY, -- 調撥出 
           NVL(TO_CHAR(ADJ_INQTY),NVL((case when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no)) then (select TO_CHAR(ADJ_INQTY) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode) else (select TO_CHAR(ADJ_INQTY) from MI_WINVMON where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no) and wh_no = a.wh_no and mmcode = a.mmcode) end),' ')) as ADJ_INQTY, -- 調帳入 
           NVL(TO_CHAR(ADJ_OUTQTY),NVL((case when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no)) then (select TO_CHAR(ADJ_OUTQTY) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode) else (select TO_CHAR(ADJ_OUTQTY) from MI_WINVMON where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no) and wh_no = a.wh_no and mmcode = a.mmcode) end),' ')) as ADJ_OUTQTY, -- 調帳出 
           NVL(TO_CHAR(bak_inqty),NVL((case when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no)) then (select TO_CHAR(bak_inqty) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode) else (select TO_CHAR(bak_inqty) from MI_WINVMON where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no) and wh_no = a.wh_no and mmcode = a.mmcode) end),' ')) as BAK_INQTY, -- 繳回入 
           NVL(TO_CHAR(bak_outqty),NVL((case when (TWN_YYYMM(sysdate) = (select SUBSTR(chk_ym, 1,5) from CHK_MAST where chk_no = a.chk_no)) then (select TO_CHAR(bak_outqty) from MI_WHINV where wh_no = a.wh_no and mmcode = a.mmcode) else (select TO_CHAR(bak_outqty) from MI_WINVMON where data_ym = (select substr(chk_ym,1,5) from CHK_MAST where chk_no = a.chk_no) and wh_no = a.wh_no and mmcode = a.mmcode) end),' ')) as BAK_OUTQTY, -- 繳回出
           a.REJ_OUTQTY, -- 退貨量 
           a.DIS_OUTQTY, -- 報廢量 
           a.exg_inqty, -- 換貨入 
           a.EXG_OUTQTY, -- 換貨出 
           ( 
               case  
               	when a.chk_time is null then  
               		a.use_qty  
               	when m.chk_wh_grade = '2' and m.chk_wh_kind='0' then -- 沒★b.chk_wh_grade這欄位, 庫別級別(1庫 2局(衛星庫) 3病房 4科室 5戰備庫 M醫院軍 S學院軍), 庫別分類(0藥品庫 1衛材庫 E能設 C通信) 
               	 	nvl(a.altered_use_qty,a.use_qty)  
               	else nvl(a.use_qty_af_chk, a.use_qty)  
               end 
           ) USE_QTY, -- 消耗量 
           a.DISC_CPRICE, -- 優惠後單價 
           a.G34_MAX_APPQTY, -- 單位請領基準量, 
           a.EST_APPQTY, -- 下月預估申請量, 
           (case when (a.chk_uid is null ) then ' ' else (select una from UR_ID where tuser = a.chk_uid) end) as CHK_UID_NAME, -- 盤點人員 
           twn_time(a.chk_time) as chk_time_string, -- 盤點時間
           a.chk_time, -- 盤點時間
           a.STATUS_INI, -- 狀態_畫面不顯示 
           ( 
             case  
               when 1=1  
                   and m.chk_wh_kind  = '0' -- 庫別分類(0藥品庫 1衛材庫 E能設 C通信) 
                   and m.chk_wh_grade = '2' -- 庫別級別(1庫 2局(衛星庫) 3病房 4科室 5戰備庫 M醫院軍 S學院軍) 
                   then  
                       'y' 
                   else  
                       'n' 
             end  
           ) HID_ALTERED_USE_QTY, 

           (select round(sum((case  
                  when ta.chk_time is null then  
                    ta.use_qty  
     	          when tm.chk_wh_grade = '2' and tm.chk_wh_kind='0' then -- 沒★b.chk_wh_grade這欄位, 庫別級別(1庫 2局(衛星庫) 3病房 4科室 5戰備庫 M醫院軍 S學院軍), 庫別分類(0藥品庫 1衛材庫 E能設 C通信) 
               	    nvl(ta.altered_use_qty,ta.use_qty)  
               	else nvl(ta.use_qty_af_chk, ta.use_qty)  
               end))/6/2*3,4)
            from CHK_DETAIL ta, CHK_MAST tm
            where ta.CHK_NO=tm.CHK_NO(+) and ta.mmcode=a.mmcode and tm.chk_wh_no=m.chk_wh_no
            and tm.CHK_YM between twn_yyymm(add_months(sysdate,-6)) and twn_yyymm(sysdate)) as BASE_QTY_45,  -- 45天基準量

            '' endl 
           from CHK_DETAIL a, -- 盤點明細 
                CHK_MAST m,   -- 盤點主檔 
                set_times b  
           where 1=1 
           and a.CHK_NO=m.CHK_NO(+) 
                ";
            //sql += sBr + "and rownum < 10 ";
            if (!String.IsNullOrEmpty(chk_no))
            {
                sql += "and a.chk_no = :chk_no  ";
                p.Add(":chk_no", string.Format("{0}", chk_no));
            }
            else if (String.IsNullOrEmpty(chk_no))
            {
                sql +="and 1=2  ";
            }
            //if (!String.IsNullOrEmpty(chk_ym))
            //{
            //    sql += sBr + "and a.chk_ym = :chk_ym  ";
            //    p.Add(":chk_ym", string.Format("{0}", chk_ym));
            //}
            //if (!String.IsNullOrEmpty(chk_wh_no))
            //{
            //    sql += sBr + "and m.chk_wh_no = :chk_wh_no  ";
            //    p.Add(":chk_wh_no", string.Format("{0}", chk_wh_no));
            //}
            if (!String.IsNullOrEmpty(mmcode) || !String.IsNullOrEmpty(mmname_e) || !String.IsNullOrEmpty(store_loc) || !String.IsNullOrEmpty(disc_cprice))
            {
                var temp_string = string.Empty;

                temp_string += @" and (";
                if (!String.IsNullOrEmpty(mmcode)) // 院內碼
                {
                    temp_string += "upper(a.MMCODE) like upper(:mmcode)  ";
                    p.Add(":mmcode", string.Format("{0}", "%" + mmcode + "%"));
                }
                if (!String.IsNullOrEmpty(mmname_e)) // 英文品名
                {
                    temp_string += "upper(a.MMNAME_E) like upper(:mmname_e)  ";
                    p.Add(":mmname_e", string.Format("{0}", "%" + mmname_e + "%"));
                }
                if (!String.IsNullOrEmpty(store_loc)) // 儲位
                {
                    if (temp_string != @" and (")
                    {
                        temp_string += " and ";
                    }
                    temp_string += "a.store_loc like :store_loc  ";
                    p.Add(":store_loc", string.Format("{0}", "%" + store_loc + "%"));
                }
                if (!String.IsNullOrEmpty(disc_cprice)) // 價格
                {
                    if (temp_string != @" and (")
                    {
                        temp_string += " and ";
                    }
                    temp_string += "a.disc_cprice >= :disc_cprice  ";
                    p.Add(":disc_cprice", disc_cprice);
                }

                temp_string += @"
                )";
                
                sql += temp_string;
            }

            if (!String.IsNullOrEmpty(isIV)) // 是否點滴
            {
                sql += "and exists (select 1 from MI_MAST where mmcode = a.mmcode and isIV=:isIV)  ";
                p.Add(":isIV", isIV);
            }

            if (!string.IsNullOrEmpty(drug_kind))
            {
                sql += " and exists (select 1 from MI_MAST where mmcode = a.mmcode and drugkind=:drug_kind) ";
                p.Add(":drug_kind", drug_kind);
            }

            if (CB_1=="true")
            {
                sql += @"and (
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM>=
        substr(twn_date(ADD_MONTHS(twn_todate((select max(set_ym) from mi_mnset where set_status='C')||'01'),-5)),1,5)
    and (APL_INQTY>0 or TRN_INQTY>0 or USE_QTY>0)
    and wh_no=a.wh_no)
or 
exists (select 1 from mi_winvmon
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY<>0
    and wh_no=a.wh_no)
or
exists (select 1 from mi_winvmon b
  where mmcode=a.mmcode
    and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
    and INV_QTY=0
    and wh_no=a.wh_no
    and exists(select 1 from mi_mast_mon where mmcode=b.mmcode
          and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
          and CANCEL_ID='N')
  )
)";
            }
            if (CB_2 == "true") {
                sql += @"
                    and (
                        NVL(a.PYM_INV_QTY,0) > 0
                        or
                        (NVL(a.PYM_INV_QTY,0) = 0 and not (a.apl_inqty = 0                       
                                                   and a.trn_inqty = 0        
                                                   and a.trn_outqty = 0        
                                                   and a.adj_inqty = 0        
                                                   and a.adj_outqty = 0        
                                                   and a.bak_outqty = 0
                                                    and a.bak_inqty=0
                                                    and nvl(nvl(a.altered_use_qty, a.use_qty),0)=0
                                                )
                        )
                    )
                ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CE0003>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MNSET> GetMnSet()
        {
            string sql = " select * from MI_MNSET where set_status = 'N' order by set_ym desc ";
            return DBWork.Connection.Query<MNSET>(sql, DBWork.Transaction);
        }

        public IEnumerable<CHK_NO_ST> GetChkNoSt(string setym, string whno)
        {
            string sql = @"
            with temp_mnset as ( 
            	select set_btime, nvl(set_etime, set_ctime) as set_etime, set_ctime from MI_MNSET where set_ym = :setym 
            ), temp_chk_mnset as (
                select chk_endtime from chk_mnset where chk_ym = :setym 
            )
            select chk_no, 
            a.chk_status, -- 狀態(0開立, 1盤中, 2調整, 3鎖單, C下一盤, P已過帳) 
            ( 
            	select distinct data_desc  
            	from PARAM_D where 1=1  
            	and grp_code ='CHK_MAST'  
            	and data_name = 'CHK_STATUS'  
            	and data_value = a.chk_status 
            ) as CHK_STATUS_T, 
            ( 
            	select max(data_date||data_etime)  
            	from MI_CONSUME_DATE b, -- 扣庫日期檔 
            	temp_mnset c -- 盤點年月檔 
            	where 1=1  
            	and b.proc_id = 'Y'  
            	and b.data_date >= twn_date(c.set_btime)  
            	and b.data_date < twn_date(c.set_etime) 
            ) as LAST_UPDATE, -- MI_CONSUME_DATE.data_date||data_etime 
            ( 
              select twn_date(set_ctime) from temp_mnset where 1=1 -- and set_ym = 盤點年月 
            ) as SET_CTIME, -- 月結日期 
             a.chk_wh_kind chk_wh_kind, -- 庫房類別_不顯示, 庫別分類(0藥品庫 1衛材庫 E能設 C通信) 
             a.chk_wh_grade chk_wh_grade, -- 庫房級別_不顯示, 庫別級別(1庫 2局(衛星庫) 3病房 4科室 5戰備庫 M醫院軍 S學院軍) 
             '' endl, 
            a.CHK_WH_KIND, -- 庫別分類(0藥品庫 1衛材庫 E能設 C通信)
            (
            select data_value  
            from PARAM_D where 1=1 
            and grp_code='HOSP_INFO'  
            and data_name='HospCode'
            ) as HospCode,
            case 
                when (select twn_date(chk_endtime) from chk_mnset where chk_ym = :setym) < twn_date(sysdate) then   
                    'n'   
                else   
                    'y'  
            end can_chk_endtime
            from CHK_MAST a -- 盤點 
            where 1=1 
            and chk_ym = :setym  
            and chk_wh_no = :whno  
            and rownum = 1 ";
            return DBWork.Connection.Query<CHK_NO_ST>(sql, new { setym = setym, whno = whno }, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, string p1, string chk_no, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"  SELECT {0}
                                    MMCODE, 
                                    MMNAME_C,  
                                    MMNAME_E 
                             FROM MI_MAST a
                             WHERE 1 = 1 
                               and not exists (select 1 from CHK_DETAIL where chk_no = :chk_no and mmcode = a.mmcode)
                               and nvl(a.cancel_id, 'N')='N'
                        ";

            p.Add("chk_no", chk_no);

            if (!String.IsNullOrEmpty(p1))
            {
                sql += " AND a.mat_class =:p1";
                // kind : mat_class, 0 : 01, 1 : 02
                if (p1 == "0")
                {
                    p.Add("p1", "01");
                }
                else
                {
                    p.Add("p1", "02");
                }
            }

            if (p0.Trim() != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += @"     AND ( upper(MMCODE) LIKE upper(:MMCODE)
                                    OR upper(MMNAME_E) LIKE UPPER(:MMNAME_E) 
                                    OR upper(MMNAME_C) LIKE UPPER(:MMNAME_C) )   ";
                p.Add(":MMCODE", string.Format("{0}%", p0));
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));
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

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p);
        }

        public IEnumerable<COMBO_MODEL> GetSetymCombo()
        {
            string sql = "select set_ym AS VALUE,set_ym AS TEXT from MI_MNSET where set_status='C' order by set_ym desc ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetDrugKindCombo()
        {
            string sql = @" SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE||' '||DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_MAST' AND DATA_NAME='DRUGKIND' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        //藥局
        public IEnumerable<COMBO_MODEL> Get藥局(string chk_no)
        {
            string sql = @"
            select distinct  
            CHK_WH_KIND,  
            CHK_WH_GRADE, 
            CHK_NO EXTRA1, 
            case 
                when CHK_WH_KIND='0' and CHK_WH_GRADE='2' then   
                    'y'   
                else   
                    'n'  
            end VALUE -- 「調整消耗」僅藥局可看到 (藥局：chk_wh_kind = '0' and chk_wh_grade = '2')  
            from CHK_MAST where 1=1 ";

            var p = new DynamicParameters();
            if (!String.IsNullOrEmpty(chk_no))
            {
                sql += "and CHK_NO=:chk_no ";
                p.Add(":chk_no", string.Format("{0}", chk_no));
            }
            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }

        public IEnumerable<COMBO_MODEL> get花蓮且衛星庫房(string chk_no)
        {
            String sql = @"
            select  
            m.chk_wh_grade, -- 庫別級別 
            m.chk_wh_kind, -- 庫別分類(0藥品庫 1衛材庫 E能設 C通信) 
            (select data_value from PARAM_D where grp_code = 'HOSP_INFO' and data_name = 'HospCode') data_value,  
            case  
                when exists( -- 是花蓮 
                        select data_value  
                        from PARAM_D where 1=1 
                        and grp_code='HOSP_INFO'  
                        and data_name='HospCode' 
                        and data_value='805' 
                     )  
                     and (m.chk_WH_KIND='1' and m.chk_WH_GRADE='2') -- 且 衛星庫房 
            then 'y'  
            else 'n' 
            end VALUE, -- [y(花蓮 且 衛星庫房)|n], 「單位請領基準量」、「下月預估申請量」僅花蓮衛星庫房可看到 
            '' endl  
            from  
                 CHK_MAST m   -- 盤點主檔  
            where 1=1  
            and (select data_value from PARAM_D where grp_code = 'HOSP_INFO' and data_name = 'HospCode') = 805 
            and m.chk_wh_kind='1' 
            and m.chk_wh_grade='2' 
             ";
            var p = new DynamicParameters();
            if (!String.IsNullOrEmpty(chk_no))
            {
                sql += "and m.chk_no = :chk_no  ";
                p.Add(":chk_no", string.Format("{0}", chk_no));
            }

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }

        public IEnumerable<COMBO_MODEL> GetWhNoCombo(string id)
        {
            string sql = @"
            select distinct  
            a.wh_no AS VALUE,  
            wh_name(a.wh_no) AS TEXT,  
            wh_kind as KIND,
            A.WH_NO || ' ' || wh_name(a.wh_no) as COMBITEM, 
            -- 「調整消耗」僅藥局可看到 
            case  
                when WH_KIND='0' and WH_GRADE='2' then  
                    'y'  
                else  
                    'n' 
            end EXTRA1, -- 「調整消耗」僅藥局可看到 (藥局：chk_wh_kind = '0' and chk_wh_grade = '2') 
            --「單位請領基準量」、「下月預估申請量」僅花蓮衛星庫房可看到 
            -- 花蓮(select data_value from PARAM_D where grp_cpde = 'HOSP_INFO' and data_name = 'HospCode')= 805 
            -- 衛星庫房：chk_wh_kind = '1' and chk_wh_grade = '2' 
            case  
                when exists( -- 是花蓮 
                        select data_value  
                        from PARAM_D where 1=1 
                        and grp_code='HOSP_INFO'  
                        and data_name='HospCode' 
                        and data_value='805' 
                     )  
                     and (WH_KIND='1' and WH_GRADE='2') -- 且 衛星庫房 
            then 'y'  
            else 'n' 
            end EXTRA2 -- [y(花蓮 且 衛星庫房)|n], 「單位請領基準量」、「下月預估申請量」僅花蓮衛星庫房可看到 
            from MI_WHID a, MI_WHMAST b 
            where 1=1  
            and a.wh_userid = :TUSER 
            and a.wh_no = b.wh_no 
            and nvl(b.cancel_id,'N') ='N' 
            order by a.wh_no  ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id });
        }

        public IEnumerable<MI_WHMAST> GetWhNoData(string wh_no)
        {
            string sql = @"SELECT WH_KIND, WH_GRADE
                           FROM MI_WHMAST
                           WHERE WH_NO = :wh_no
                           AND ROWNUM = 1";
            return DBWork.Connection.Query<MI_WHMAST>(sql, new { wh_no = wh_no }, DBWork.Transaction);
        }

        public int UpdateChkDetail(CHK_DETAIL d)
        {
            string sql = @" 
                update CHK_DETAIL a 
                   set a.chk_qty = :chk_qty, -- 盤點量
                       a.altered_use_qty = :altered_use_qty, -- 調整消耗
                       a.chk_uid = :chk_uid, -- 登入人員帳號
                       a.chk_time = sysdate, a.status_ini = :status_ini,
            ";
            if (!String.IsNullOrEmpty(d.EST_APPQTY))
                sql += "a.est_appqty = :est_appqty, -- 下月預估申請量 ";
            if (!String.IsNullOrEmpty(d.G34_MAX_APPQTY))
                sql += "a.g34_max_appqty = :g34_max_appqty, -- 單位請領基準量 ";
            sql += @"
                      a.update_time = sysdate,  
                      a.update_user = :update_user, 
                      a.update_ip = :update_ip, A.CHK_REMARK = :chk_remark
               where 1=1 
                 and a.chk_no = :chk_no and a.mmcode = :mmcode
            ";
            return DBWork.Connection.Execute(sql, d, DBWork.Transaction);
        } // 

        public int BtnOneKeyMatch(CHK_DETAIL d)
        {
            string sql = @"
            update CHK_DETAIL set  
            chk_qty = store_qtyc,  
            chk_uid = :chk_uid, -- 登入人員帳號 
            chk_time = sysdate,  
            update_user = :update_user, -- 登入人員帳號 
            update_ip = :update_ip, -- 登入IP 
            update_time = sysdate  
            where 1=1  
            and chk_no = :chk_no -- 盤點單號  
            and chk_time is null ";


            return DBWork.Connection.Execute(sql, d, DBWork.Transaction);
        } //

        public int UpdateChkDetail_c1(string chk_no)
        { // --1.一級庫計算差異 = 盤盈虧
            string sql = @"
            update CHK_DETAIL a set  
            a.inventory = (a.chk_qty-a.store_qtyc) -- 盤盈虧 = 差異量(盤點量-庫備數量, 病房/單位：若CHK_QTY > STORE_QTYC則為CHK_QTY-STORE_QTYC，否則為0) 
            where 1=1  
            and chk_no = :CHK_NO  
            and chk_time is not null 
            and exists ( 
                select 1 from CHK_MAST where 1=1  
                and chk_no = a.chk_no  
                and chk_wh_grade = '1' -- 庫別級別(1庫 2局(衛星庫) 3病房 4科室 5戰備庫 M醫院軍 S學院軍) 
            ) ";


            return DBWork.Connection.Execute(sql, new { CHK_NO = chk_no }, DBWork.Transaction);
        } // 
        public int UpdateChkDetail_c2(string chk_no, string wh_no_c)
        {// --2.供應中心計算差異 = 盤盈虧
            string sql = @"
            update CHK_DETAIL a set  
            a.inventory = (a.chk_qty-a.store_qtyc)  
            where 1=1  
            and chk_no = :CHK_NO -- 盤點單號 
            and chk_time is not null  
            and wh_name(a.wh_no) = :WH_NO_C -- 供應中心 
             ";
            return DBWork.Connection.Execute(sql, new { CHK_NO = chk_no, WH_NO_C = wh_no_c }, DBWork.Transaction);
        } // 
        public int UpdateChkDetail_c3(string chk_no)
        {//-- 3.藥局可調整消耗，需重新計算差異量
            string sql = @"
            update CHK_DETAIL a set  
            a.inventory = ( 
            	a.chk_qty 
            	-( 
            		 a.pym_inv_qty -- 上月庫存數量 
            		+a.apl_inqty   -- 月結入庫總量 
            		-a.apl_outqty  -- 月結撥發總量 
            		+a.trn_inqty   -- 月結調撥入總量 
            		-a.trn_outqty  -- 月結調撥入總量 
            		+a.adj_inqty   -- 月結調帳入總量 
            		-a.adj_outqty  -- 月結調帳出總量 
            		+a.bak_inqty   -- 月結繳回入庫總量 
            		-a.bak_outqty  -- 月結繳回出庫總量 
            		-a.rej_outqty  -- 月結退貨總量 
            		-a.dis_outqty  -- 月結報廢總量
            		+a.exg_inqty   -- 月結換貨入庫總量 
            		-a.exg_outqty  -- 月結換貨出庫總量 
            		-nvl( 
            			nvl(a.altered_use_qty, a.use_qty), -- 調整消耗(只有藥局可以用), 月結耗用量=醫令扣庫 
            			0 
            		) 
            	) 
            )  
            where 1=1  
            and chk_no = :CHK_NO -- 盤點單號  
            and chk_time is not null  
            and exists ( 
                select 1 from CHK_MAST where 1=1 
                and chk_no = a.chk_no  
                and chk_wh_grade='2' -- 庫別級別(1庫 2局(衛星庫) 3病房 4科室 5戰備庫 M醫院軍 S學院軍) 
                and chk_wh_kind ='0' -- 庫別分類(0藥品庫 1衛材庫 E能設 C通信) 
            ) 
             ";

            return DBWork.Connection.Execute(sql, new { CHK_NO = chk_no }, DBWork.Transaction);
        } // 
        public int UpdateChkDetail_c4(string chk_no, string wh_no_c)
        {//-- 4.單位若chk_qty <= store_qtyc，消耗 = store_qtyc - chk_qty；若否，消耗 = 0，差異 = (chk_qty - store_qtyc)
            string sql = @"
            update CHK_DETAIL a set  
            use_qty_af_chk = ( -- 盤點計算消耗(只有病房/單位可以用) 
                case 
                    when a.store_qtyc < a.chk_qty then -- 庫存量-盤點量<0時，為0 
                        0 
                    else 
                        a.store_qtyc-a.chk_qty 
                end 
            ),  
            inventory=( -- 盤盈虧 = 差異量(病房/單位：若CHK_QTY > STORE_QTYC則為CHK_QTY-STORE_QTYC，否則為0) 
                case 
                    when a.store_qtyc < a.chk_qty then 
                        (a.chk_qty-a.store_qtyc) 
                    else 
                        0 
                end 
            ) 
            where 1=1  
            and a.chk_no = :CHK_NO -- 盤點單號 
            and chk_time is not null 
            and exists ( 
                select 1 from CHK_MAST where 1=1 
                and chk_no = a.chk_no  
                and not chk_wh_grade = '1' -- 庫別級別(1庫 2局(衛星庫) 3病房 4科室 5戰備庫 M醫院軍 S學院軍) 
                and not ( 
                        chk_wh_grade = '2' -- 庫別級別(1庫 2局(衛星庫) 3病房 4科室 5戰備庫 M醫院軍 S學院軍) 
                        and chk_wh_kind = '0' -- 庫別分類(0藥品庫 1衛材庫 E能設 C通信) 
                )  
                and not wh_name(a.wh_no) = nvl(:WH_NO_C,' ') -- 供應中心 
            ) ";

            return DBWork.Connection.Execute(sql, new { CHK_NO = chk_no, WH_NO_C = wh_no_c }, DBWork.Transaction);
        } // 

        public bool ChceckChkStatus(string chk_no)
        {
            string sql = @"
                select 1 from CHK_MAST
                 where CHK_NO = :CHK_NO
                   and CHK_STATUS = '1' -- CHK_STATUS 狀態[0開立|1盤中|2調整|3鎖單|C下一盤|P已過帳]
            ";

            return DBWork.Connection.ExecuteScalar(sql, new { CHK_NO = chk_no }, DBWork.Transaction) != null;
        }

        public bool CheckExistsCHK_NO(string chk_no)
        {
            string sql = @" SELECT 1 FROM CHK_MAST 
                          WHERE CHK_NO = :CHK_NO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { CHK_NO = chk_no }, DBWork.Transaction) == null);
        }

        public bool CheckExistsMMCODE(string mmcode)
        {
            string sql = @" SELECT 1 FROM MI_MAST 
                          WHERE MMCODE = :MMCODE  ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode }, DBWork.Transaction) == null);
        }

        public bool CheckExistsWhmast(string tuser, string chk_no)
        {
            string sql = @" select 1 from MI_WHID a, MI_WHMAST b 
                            where a.wh_userId = :TUSER 
                            and a.wh_no = (select chk_wh_no from CHK_MAST where chk_no = :CHK_NO) 
                            and a.wh_no = b.wh_no and nvl(b.cancel_id,'N') = 'N'
                                ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { TUSER = tuser, CHK_NO = chk_no }, DBWork.Transaction) == null);
        }

        public bool CheckChkExistsMMCODE(string chk_no, string mmcode)
        {
            string sql = @"SELECT 1 FROM CHK_DETAIL WHERE CHK_NO=:CHK_NO AND MMCODE=:MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { CHK_NO = chk_no, MMCODE = mmcode }, DBWork.Transaction) == null);
        }

        public bool is花蓮且衛星庫房(
            WebApp.Controllers.C.CE0041Controller c,
            String chk_no
        )
        {
            using (WorkSession session = new WorkSession(c))
            {
                var DBWork = session.UnitOfWork;
                try
                {
                    // var repo = new CE0041Repository(DBWork);
                    IEnumerable<COMBO_MODEL> 花蓮且衛星庫房s = get花蓮且衛星庫房(chk_no); //repo.get花蓮且衛星庫房(chk_no);
                    return 花蓮且衛星庫房s.Any();
                }
                catch
                {
                    throw;
                }
            }
            return false;
        }

        /// <summary>
        /// 檢查是否為花蓮醫院
        /// </summary>
        public string GetHospCode()
        {
            var sql = @" SELECT data_value FROM PARAM_D WHERE grp_code = 'HOSP_INFO' AND data_name = 'HospCode' ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }
        public MI_MAST GetMI_MAST(string mmcode)
        {
            string strSql = @"select * from mi_mast where mmcode = :mmcode";
            return DBWork.Connection.QueryFirstOrDefault<MI_MAST>(strSql, new { mmcode = mmcode });
        }

        public CHK_DETAIL GetCHK_DETAIL(string chk_no, string mmcode)
        {
            string strSql = @"select * from chk_detail where chk_no =:chk_no and mmcode = :mmcode";
            return DBWork.Connection.QueryFirstOrDefault<CHK_DETAIL>(strSql, new { chk_no = chk_no, mmcode = mmcode });
        }

        public int CreateCHK_DETAIL(CHK_DETAIL chk_detail)
        {
            string sql = @"
                    insert into chk_detail (
                        CHK_NO, MMCODE, MMNAME_C, MMNAME_E, BASE_UNIT, 
                        M_CONTPRICE, WH_NO, MAT_CLASS, M_STOREID, STORE_QTYC, 
                        STORE_QTYM, STORE_QTYS, STATUS_INI,CREATE_DATE, CREATE_USER, 
                        UPDATE_TIME, PYM_INV_QTY, APL_INQTY, APL_OUTQTY, TRN_INQTY, 
                        TRN_OUTQTY, ADJ_INQTY, ADJ_OUTQTY, BAK_INQTY, BAK_OUTQTY, 
                        REJ_OUTQTY, DIS_OUTQTY, EXG_INQTY, EXG_OUTQTY, MIL_INQTY, 
                        MIL_OUTQTY, USE_QTY, MAT_CLASS_SUB, DISC_CPRICE, PYM_CONT_PRICE, 
                        PYM_DISC_CPRICE, WAR_QTY, UNITRATE, M_CONTID, E_SOURCECODE, 
                        E_RESTRICTCODE, COMMON, FASTDRUG, DRUGKIND, TOUCHCASE, 
                        ORDERKIND, SPDRUG, EST_APPQTY, G34_MAX_APPQTY
                    ) values (
                        :CHK_NO, :MMCODE, :MMNAME_C, :MMNAME_E, :BASE_UNIT, 
                        :M_CONTPRICE, :WH_NO, :MAT_CLASS, :M_STOREID, :STORE_QTYC,
                        :STORE_QTYM, :STORE_QTYS, :STATUS_INI, SYSDATE, :CREATE_USER,
                        SYSDATE, :PYM_INV_QTY, :APL_INQTY, :APL_OUTQTY, :TRN_INQTY,
                        :TRN_OUTQTY, :ADJ_INQTY, :ADJ_OUTQTY, :BAK_INQTY, :BAK_OUTQTY,
                        :REJ_OUTQTY, :DIS_OUTQTY, :EXG_INQTY, :EXG_OUTQTY, :MIL_INQTY,
                        :MIL_OUTQTY, :USE_QTY, :MAT_CLASS_SUB, :DISC_CPRICE, :PYM_CONT_PRICE,
                        :PYM_DISC_CPRICE, :WAR_QTY, :UNITRATE, :M_CONTID, :E_SOURCECODE,
                        :E_RESTRICTCODE, :COMMON, :FASTDRUG, :DRUGKIND, :TOUCHCASE,
                        :ORDERKIND, :SPDRUG, :EST_APPQTY, :G34_MAX_APPQTY
                    )";
            return DBWork.Connection.Execute(sql, chk_detail, DBWork.Transaction);
        }

        public int CreateMI_WINVMON(MI_WINVMON mi_winvmon)
        {
            string sql = @"insert into mi_winvmon (
                    DATA_YM, WH_NO, MMCODE, INV_QTY, APL_INQTY, 
                    APL_OUTQTY, TRN_INQTY, TRN_OUTQTY, ADJ_INQTY, ADJ_OUTQTY, 
                    BAK_INQTY, BAK_OUTQTY, REJ_OUTQTY, DIS_OUTQTY, EXG_INQTY, 
                    EXG_OUTQTY, MIL_INQTY, MIL_OUTQTY, INVENTORYQTY, TUNEAMOUNT, 
                    USE_QTY, TURNOVER, SAFE_QTY, OPER_QTY, SHIP_QTY, 
                    DAVG_USEQTY, ONWAY_QTY, SAFE_DAY, OPER_DAY, SHIP_DAY, 
                    HIGH_QTY, ORI_INV_QTY, ORI_USE_QTY
                ) values (
                    :DATA_YM, :WH_NO, :MMCODE, :INV_QTY, :APL_INQTY,
                    :APL_OUTQTY, :TRN_INQTY, :TRN_OUTQTY, :ADJ_INQTY, :ADJ_OUTQTY,
                    :BAK_INQTY, :BAK_OUTQTY, :REJ_OUTQTY, :DIS_OUTQTY, :EXG_INQTY,
                    :EXG_OUTQTY, :MIL_INQTY, :MIL_OUTQTY, :INVENTORYQTY, :TUNEAMOUNT,
                    :USE_QTY, :TURNOVER, :SAFE_QTY, :OPER_QTY, :SHIP_QTY, :DAVG_USEQTY,
                    :ONWAY_QTY, :SAFE_DAY, :OPER_DAY, :SHIP_DAY, :HIGH_QTY,
                    :ORI_INV_QTY, :ORI_USE_QTY
                 )";
            return DBWork.Connection.Execute(sql, mi_winvmon);
        }

        public int CreateMI_WHINV(MI_WHINV mi_whinv)
        {
            string sql = @"insert into mi_whinv values (
                        :WH_NO, :MMCODE, :INV_QTY, :ONWAY_QTY, :APL_INQTY,
                        :APL_OUTQTY, :TRN_INQTY, :TRN_OUTQTY, :ADJ_INQTY, :ADJ_OUTQTY,
                        :BAK_INQTY, :BAK_OUTQTY, :REJ_OUTQTY, :DIS_OUTQTY, :EXG_INQTY,
                        :EXG_OUTQTY, :MIL_INQTY, :MIL_OUTQTY, :INVENTORYQTY, :TUNEAMOUNT,
                        :USE_QTY, :TRANS_ID
                        )";

            return DBWork.Connection.Execute(sql, mi_whinv);
        }

        public int CreateMI_WINVCTL(MI_WINVCTL mi_winvctl)
        {
            string sql = @"insert into mi_winvctl (
                        WH_NO, MMCODE, SAFE_DAY, OPER_DAY, SHIP_DAY, 
                        SAFE_QTY, OPER_QTY, SHIP_QTY, DAVG_USEQTY, HIGH_QTY, 
                        LOW_QTY, MIN_ORDQTY, SUPPLY_WHNO
                    )values(
                        :WH_NO, :MMCODE, :SAFE_DAY, :OPER_DAY, :SHIP_DAY,
                        :SAFE_QTY, :OPER_QTY, :SHIP_QTY, :DAVG_USEQTY,:HIGH_QTY,
                        :LOW_QTY, :MIN_ORDQTY,:SUPPLY_WHNO
                    )";
            return DBWork.Connection.Execute(sql, mi_winvctl);
        }
        public string GetWhno_me1()
        {
            string sql = @"SELECT WHNO_ME1 FROM DUAL ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }

        public string GetWhno_mm1()
        {
            string sql = @"SELECT WHNO_MM1 FROM DUAL ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }

        /// <summary>
        /// 判斷是否為藥局，非藥局不可匯出與匯入調整消耗欄位
        /// </summary>
        /// <param name="strChkWhNo"></param>
        /// <returns></returns>
        public bool is藥局(
            String strChkWhNo)
        {
            try
            {
                string strSql = @"
                    SELECT chk_wh_no FROM chk_mast
                     WHERE chk_wh_no =:chk_wh_no
                       AND chk_wh_kind = '0' AND chk_wh_grade = '2'
";
                IEnumerable<CHK_MAST> result = DBWork.Connection.Query<CHK_MAST>(strSql, new { chk_wh_no = strChkWhNo });
                return result.Any();
            }
            catch (InvalidOperationException e)
            {
                return false;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        ///
        public DataTable GetExcel(
            bool is花蓮且衛星庫房,
            bool is藥局,
            bool is花蓮,
            string chk_no, // 盤點單號
            string chk_ym, // 盤點
            string chk_wh_no, // 盤點
            string mmcode, // 院內碼
            string mmname_e, // 英文品名
            string store_loc, // 儲位
            string disc_cprice,  // 價錢(成本價)大於
            string isIV,  // 是否點滴
            string drug_kind,
            string CB_1, // (近6個月進貨)或(近6個月醫令耗用)或(庫量<>0)或(庫存=0且無作廢)
            string CB_2 // (期初庫存<>0)或(期初=0但有進出)
        )
        { // 【匯出】按鈕
            DynamicParameters p = new DynamicParameters();

            var sql = @"
            select  
            :chk_no 盤點單號, -- 盤點單號 
            a.mmcode 院內碼, -- 院內碼 
            a.mmname_c 中文品名, -- 中文品名 
            a.mmname_e 英文品名, -- 英文品名 
            a.BASE_UNIT 計量單位, -- 計量單位 
            ( 
                select listagg(store_loc,',') within group (order by store_loc)  
                from MI_WLOCINV  
                where 1=1  
                and wh_no=a.wh_no  
                and mmcode=a.mmcode 
            ) 儲位, --  儲位 
            a.store_qtyc 電腦量, -- 電腦量 
            a.CHK_QTY 盤點量, -- 盤點量 
            ";
            if (is花蓮且衛星庫房)
            {
                sql += @"a.G34_MAX_APPQTY 單位請領基準量, -- 單位請領基準量 
                         a.EST_APPQTY 下月預估申請量, -- 下月預估申請量 ";
            }
            if (is藥局)
            {
                sql += @"a.altered_use_qty 調整消耗, -- 調整消耗 ";
            }
            sql += @"
                         a.chk_remark 備註,  -- 備註 
                         a.inventory 差異量, -- 差異量 
                         (a.inventory * a.disc_cprice) 差異金額, -- 差異金額 
                         a.pym_inv_qty 上期結存, -- 上期結存 
                         a.apl_inqty 進貨_撥發入, -- 進貨_撥發入 
                         a.apl_outqty 撥發出, -- 撥發出 
                         a.trn_inqty 調撥入, -- 調撥入 
                         a.trn_outqty 調撥出, -- 調撥出 
                         a.adj_inqty 調帳入, -- 調帳入 
                         a.adj_outqty 調帳出, -- 調帳出 
                         a.bak_inqty 繳回入, -- 繳回入 
                         a.bak_outqty 繳回出, -- 繳回出 
                         a.rej_outqty 退貨量, -- 退貨量 
                         a.dis_outqty 報廢量, -- 報廢量 
                         a.exg_inqty 換貨入,  -- 換貨入 
                         a.exg_outqty 換貨出, -- 換貨出 
                         ( 
                             case  
                                 when chk_time is null then  
                                     a.use_qty  
                                 when m.chk_wh_grade = '2' and m.chk_wh_kind='0' then -- 庫別級別(1庫 2局(衛星庫) 3病房 4科室 5戰備庫 M醫院軍 S學院軍), 庫別分類(0藥品庫 1衛材庫 E能設 C通信) 
                                     nvl(a.altered_use_qty,a.use_qty)  
                                 else nvl(a.use_qty_af_chk,a.use_qty)  
                             end 
                         ) 消耗量, -- 消耗量 ";

            if (is花蓮)
            {
                sql += @"
                         (select round(sum((case  
                  when ta.chk_time is null then  
                    ta.use_qty  
     	          when tm.chk_wh_grade = '2' and tm.chk_wh_kind='0' then -- 沒★b.chk_wh_grade這欄位, 庫別級別(1庫 2局(衛星庫) 3病房 4科室 5戰備庫 M醫院軍 S學院軍), 庫別分類(0藥品庫 1衛材庫 E能設 C通信) 
               	    nvl(ta.altered_use_qty,ta.use_qty)  
               	else nvl(ta.use_qty_af_chk, ta.use_qty)  
               end))/6/2*3,4)
            from CHK_DETAIL ta, CHK_MAST tm
            where ta.CHK_NO=tm.CHK_NO(+) and ta.mmcode=a.mmcode and tm.chk_wh_no=m.chk_wh_no
            and tm.CHK_YM between twn_yyymm(add_months(sysdate,-6)) and twn_yyymm(sysdate)) as ""45天基準量"", ";
            }

            sql += @"a.disc_cprice 優惠後單價, -- 優惠後單價 
                         wh_name(a.wh_no) 庫房名稱, -- 供應中心 
                         ( 
                             case  
                                 when (a.chk_uid is null ) then  
                                     ' '  
                                 else  
                                     (select una from UR_ID where tuser = a.chk_uid)  
                                 end 
                         ) 盤點人員,  -- 盤點人員 
                         twn_time(a.chk_time) 盤點時間 -- 盤點時間  
                         -- select * 
                         from CHK_DETAIL a, 
                              CHK_MAST m   -- 盤點主檔 
                         where 1=1 
                         and a.chk_no=m.chk_no(+) ";

            if (!String.IsNullOrEmpty(chk_no))
            {
                sql += "and a.chk_no = :chk_no  ";
                p.Add(":chk_no", string.Format("{0}", chk_no));
            }
            if (!String.IsNullOrEmpty(mmcode) || !String.IsNullOrEmpty(mmname_e) || !String.IsNullOrEmpty(store_loc) || !String.IsNullOrEmpty(disc_cprice))
            {
                var temp_string = string.Empty;

                temp_string += @" and (";
                if (!String.IsNullOrEmpty(mmcode)) // 院內碼
                {
                    temp_string += "upper(a.MMCODE) like upper(:mmcode)  ";
                    p.Add(":mmcode", string.Format("{0}", "%" + mmcode + "%"));
                }
                if (!String.IsNullOrEmpty(mmname_e)) // 英文品名
                {
                    temp_string += "upper(a.MMNAME_E) like upper(:mmname_e)  ";
                    p.Add(":mmname_e", string.Format("{0}", "%" + mmname_e + "%"));
                }
                if (!String.IsNullOrEmpty(store_loc)) // 儲位
                {
                    if (temp_string != @" and (")
                    {
                        temp_string += " and ";
                    }
                    temp_string += "a.store_loc like :store_loc  ";
                    p.Add(":store_loc", string.Format("{0}", "%" + store_loc + "%"));
                }
                if (!String.IsNullOrEmpty(disc_cprice)) // 價格
                {
                    if (temp_string != @" and (")
                    {
                        temp_string += " and ";
                    }
                    temp_string += "a.disc_cprice >= :disc_cprice  ";
                    p.Add(":disc_cprice", disc_cprice);
                }


                temp_string += @"
                )";

                sql += temp_string;
            }

            if (!String.IsNullOrEmpty(isIV)) // 是否點滴
            {
                sql += "and exists (select 1 from MI_MAST where mmcode = a.mmcode and isIV=:isIV)  ";
                p.Add(":isIV", isIV);
            }

            if (!String.IsNullOrEmpty(drug_kind))
            {
                sql += "and exists (select 1 from MI_MAST where mmcode = a.mmcode and drugkind=:drug_kind)  ";
                p.Add(":drug_kind", drug_kind);
            }

            if (CB_1 == "true")
            {
                sql += @"and (
                            exists (select 1 from mi_winvmon
                              where mmcode=a.mmcode
                                and DATA_YM>=
                                    substr(twn_date(ADD_MONTHS(twn_todate((select max(set_ym) from mi_mnset where set_status='C')||'01'),-5)),1,5)
                                and (APL_INQTY>0 or TRN_INQTY>0 or USE_QTY>0)
                                and wh_no=a.wh_no)
                            or 
                            exists (select 1 from mi_winvmon
                              where mmcode=a.mmcode
                                and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
                                and INV_QTY<>0
                                and wh_no=a.wh_no)
                            or
                            exists (select 1 from mi_winvmon b
                              where mmcode=a.mmcode
                                and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
                                and INV_QTY=0
                                and wh_no=a.wh_no
                                and exists(select 1 from mi_mast_mon where mmcode=b.mmcode
                                      and DATA_YM=(select max(set_ym) from mi_mnset where set_status='C')
                                      and CANCEL_ID='N')
                              )
                            )";
            }

            if (CB_2 == "true")
            {
                sql += @"
                    and (
                        NVL(a.PYM_INV_QTY,0) > 0
                        or
                        (NVL(a.PYM_INV_QTY,0) = 0 and not (a.apl_inqty = 0                       
                                                   and a.trn_inqty = 0        
                                                   and a.trn_outqty = 0        
                                                   and a.adj_inqty = 0        
                                                   and a.adj_outqty = 0        
                                                   and a.bak_outqty = 0
                                                    and a.bak_inqty=0
                                                    and nvl(nvl(a.altered_use_qty, a.use_qty),0)=0
                                                )
                        )
                    )
                ";
            }

            sql += "order by a.mmcode ";


            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public class MNSET
        {
            public string PYM { get; set; }
            public string SET_YM { get; set; }
            public string NYM { get; set; }
        }
        public class CHK_NO_ST
        {
            public string CHK_NO { get; set; }
            public string CHK_STATUS { get; set; }
            public string CHK_STATUS_T { get; set; }
            public string LAST_UPDATE { get; set; }
            public string CHK_WH_KIND { get; set; }
            public string CAN_CHK_ENDTIME { get; set; }
            public string HospCode { get; set; }
            public string SET_CTIME { get; set; }
        }

        public bool CheckIsPhrByChkno(string chk_no)
        {
            string sql = @"
                select (case when chk_wh_kind = '0' and chk_wh_grade='2' then 'Y' else 'N' end)
                  from CHK_MAST 
                 where chk_no = :chk_no
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { chk_no }, DBWork.Transaction) == "Y";
        }

        public bool CheckWhidExists(string userId, string wh_no)
        {
            string sql = @"
                select 1 from MI_WHID where wh_no = :wh_no and wh_userid = :userId
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { userId, wh_no }, DBWork.Transaction) != null;
        }

        public bool CheckMiWinvmonExists(string ym, string wh_no, string mmcode) {
            string sql = @"
                select 1 from MI_WINVMON
                 where data_ym=:ym and wh_no = :wh_no and mmcode = :mmcode
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { ym, wh_no, mmcode }, DBWork.Transaction) != null;
        }

        public bool CheckMiWhinvExists(string wh_no, string mmcode)
        {
            string sql = @"
                select 1 from MI_WHINV
                 where wh_no = :wh_no and mmcode = :mmcode
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { wh_no, mmcode }, DBWork.Transaction) != null;
        }
        public bool CheckMiWinvctlExists(string wh_no, string mmcode)
        {
            string sql = @"
                select 1 from MI_WINVCTL
                 where wh_no = :wh_no and mmcode = :mmcode
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { wh_no, mmcode }, DBWork.Transaction) != null;
        }
        public bool CheckMiWinvmonExistsByChkNo(string chk_no, string mmcode)
        {
            string sql = @"
                select 1 from MI_WINVMON a
                 where data_ym=(select chk_ym from CHK_MAST where chk_no=:chk_no)
                   and mmcode = :mmcode
                   and wh_no = (select chk_wh_no from CHK_MAST where chk_no=:chk_no)
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { chk_no, mmcode }, DBWork.Transaction) != null;
        }

        public string GetChkYmByChkno(string chk_no) {
            string sql = @"
                select chk_ym from CHK_MAST where chk_no=:chk_no
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { chk_no }, DBWork.Transaction);
        }
        public string GetChkWhKind(string chk_no) {
            string sql = @"
                select chk_wh_kind from CHK_MAST where chk_no=:chk_no
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { chk_no }, DBWork.Transaction);
        }
        public int UpdateChkTotal(string chk_no) {
            string sql = @"
                update CHK_MAST a
                   set chk_total = (select count(*) from CHK_DETAIL where chk_no = a.chk_no)
                 where a.chk_no = :chk_no
            ";
            return DBWork.Connection.Execute(sql, new { chk_no }, DBWork.Transaction);
        }
        public int UpdateChkNum(string chk_no)
        {
            string sql =@"
                update CHK_MASt a
                   set chk_num=(select count(*) from CHK_DETAIL where chk_no =a.chk_no and chk_time is not null)
                 where chk_no = :chk_no
            ";
            return DBWork.Connection.Execute(sql, new { chk_no }, DBWork.Transaction);

        }

        public int UpdateGapToUse(string chk_no) {
            string sql = @"
                update CHK_DETAIL a
                   set altered_use_qty =nvl(a.altered_use_qty, a.use_qty) - a.inventory ,
                       inventory = 0
                 where a.chk_no = :chk_no
                   and a.inventory <> 0
            ";

            return DBWork.Connection.Execute(sql, new { chk_no }, DBWork.Transaction);
        }

        #region 20240216: 增加更新CHK_DETAILTOT、MI_WINVMON、MI_WHINV
        public int UpdateChkDetailtot(CHK_DETAIL item)
        {
            string sql = @"
                update CHK_DETAILTOT cd
                   set GAP_T = :CHK_QTY - cd.STORE_QTY,
                       GAP_C = :CHK_QTY - cd.STORE_QTY,
                       PRO_LOS_QTY = (CASE WHEN (select chk_wh_grade from CHK_MAST where chk_no = :CHK_NO) = 1 THEN (:CHK_QTY - CD.STORE_QTY)
                                            WHEN (select chk_wh_grade from CHK_MAST where chk_no = :CHK_NO) = 2 AND (select chk_wh_kind from CHK_MAST where chk_no = :CHK_NO) = 0 THEN (:CHK_QTY - CD.STORE_QTY) 
                                            WHEN (select wh_name(chk_wh_no) from CHK_MAST where chk_no = :CHK_NO) ='供應中心' THEN (:CHK_QTY - cd.STORE_QTY) ELSE 0 END),
                      CHK_QTY1 = :CHK_QTY,
                      update_time = sysdate, update_user = :UPDATE_USER, update_ip = :UPDATE_IP
                where chk_no = :CHK_NO
                  and mmcode = :MMCODE
            ";

            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);
        }
        public int UpdateMiwinvmonRollback(CHK_DETAIL item) {
            string sql = @"
                update MI_WINVMON a
                   set inv_qty = :STORE_QTYC,
                       use_qty = ori_use_qty,
                       inventoryqty = 0
                 where data_ym = :CHK_YM
                   and wh_no = :CHK_WH_NO
                   and mmcode = :MMCODE
            ";
            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);
        }
        public int UpdateMiwhinvRollback(CHK_DETAIL item) {
            string sql = @"
                update MI_WHINV a
                   set inv_qty = inv_qty + (:STORE_QTYC - :ORI_CHK_QTY)
                 where wh_no = :CHK_WH_NO
                   and mmcode = :MMCODE
                       
            ";
            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);
        }

        public int InsertWhtrnsRollback(CHK_DETAIL item) {
            string sql = @"
                INSERT INTO MI_WHTRNS (WH_NO, TR_DATE, TR_SNO, MMCODE, TR_INV_QTY, TR_ONWAY_QTY, TR_DOCNO, TR_IO, TR_MCODE, 
                                                    BF_TR_INVQTY, AF_TR_INVQTY)
                             SELECT A.WH_NO, SYSDATE, WHTRNS_SEQ.NEXTVAL, A.MMCODE, (a.STORE_QTYC - :ORI_CHK_QTY), 0, A.CHK_NO, 'I', 'CHIO', 
                                    b.INV_QTY, b.INV_QTY + (a.STORE_QTYC - :ORI_CHK_QTY)
                             FROM CHK_DETAIL A , MI_WHINV b
                             WHERE A.CHK_NO = :CHK_NO  
                             AND A.MMCODE = :MMCODE
                             and b.wh_no = :CHK_WH_NO
                             and b.mmcode = a.mmcode
            ";
            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);
        }
        public string GetWhname(string chk_wh_no) {
            string sql = @"
                select wh_name(:chk_wh_no) from dual
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { chk_wh_no }, DBWork.Transaction);
        }
        public string GetTrinvqty(CHK_DETAIL item)
        {
            // use_qty + inventory
            string sql = @"
                select nvl((CASE WHEN :CHK_WH_GRADE = 1 THEN 0 
                             WHEN :CHK_WH_GRADE = 2 AND :CHK_WH_KIND = 0 THEN NVL(TO_NUMBER(a.ALTERED_USE_QTY), TO_NUMBER(a.USE_QTY))
                             WHEN :WH_NAME = '供應中心' THEN 0 
                             ELSE TO_NUMBER(a.USE_QTY_AF_CHK) END)
                            , 0) * (-1)
                        + a.inventory
                  from CHK_DETAIL a
                 where chk_no = :CHK_NO
                   and mmcode = :MMCODE
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, item, DBWork.Transaction);
        }
        public string GetUseQtyAfChk(CHK_DETAIL item) {
            string sql = @"
                select use_qty_af_chk from CHK_DETAIL where chk_no=:CHK_NO and mmcode = :MMCODE
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, item, DBWork.Transaction);
        }
        public string GetInventory(CHK_DETAIL item) {
            string sql = @"
                select inventory from CHK_DETAIL where chk_no=:CHK_NO and mmcode = :MMCODE
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, item, DBWork.Transaction);
        }
        public string GetUseQty(CHK_DETAIL item)
        {
            string sql = @"
                select use_qty from CHK_DETAIL where chk_no=:CHK_NO and mmcode = :MMCODE
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, item, DBWork.Transaction);
        }
        public string GetAlteredUseQty(CHK_DETAIL item)
        {
            string sql = @"
                select altered_use_qty from CHK_DETAIL where chk_no=:CHK_NO and mmcode = :MMCODE
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, item, DBWork.Transaction);
        }

        public int UpdateMiWinvmon(CHK_DETAIL item)
        {
            string sql = @"  UPDATE MI_WINVMON
                                SET USE_QTY = (CASE WHEN :CHK_WH_GRADE = 1 THEN 0 
                                                    WHEN :CHK_WH_GRADE = 2 AND :CHK_WH_KIND = 0 THEN NVL(TO_NUMBER(nvl(:ALTERED_USE_QTY, 0)), TO_NUMBER(nvl(:USE_QTY, 0)))
                                                    WHEN :WH_NAME = '供應中心' THEN 0 
                                                    ELSE TO_NUMBER(nvl(:USE_QTY_AF_CHK, 0)) END),
                                          INVENTORYQTY = :INVENTORY, 
                                          INV_QTY = nvl(to_number(INV_QTY), 0) + nvl(:GAP_T, 0)
                              WHERE DATA_YM = :CHK_YM
                              AND WH_NO = :CHK_WH_NO
                              AND MMCODE = :MMCODE ";
            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);
        }
        
        public int UpdateMiwhinv(CHK_DETAIL item) {
            string sql = @"
                UPDATE MI_WHINV 
                   SET INV_QTY = nvl(to_number(INV_QTY), 0) + nvl(to_number(:GAP_T), 0)
                 WHERE WH_NO = :CHK_WH_NO 
                   AND MMCODE = :MMCODE
            ";
            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);
        }

        public int InsertWhtrns(CHK_DETAIL item)
        {
            string sql = @"
                INSERT INTO MI_WHTRNS (WH_NO, TR_DATE, TR_SNO, MMCODE, TR_INV_QTY, TR_ONWAY_QTY, TR_DOCNO, TR_IO, TR_MCODE, 
                                                    BF_TR_INVQTY, AF_TR_INVQTY)
                             SELECT A.WH_NO, SYSDATE, WHTRNS_SEQ.NEXTVAL, A.MMCODE,  nvl(:GAP_T, 0), 0, A.CHK_NO, 'I', 'CHIO', 
                                    b.INV_QTY, b.INV_QTY + nvl(:GAP_T, 0)
                             FROM CHK_DETAIL A , MI_WHINV b
                             WHERE A.CHK_NO = :CHK_NO  
                             AND A.MMCODE = :MMCODE
                             and b.wh_no = :CHK_WH_NO
                             and b.mmcode = a.mmcode
            ";
            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);
        }

        public bool CheckChkTotExistsMMCODE(string chk_no, string mmcode) {
            string sql = @"
                select 1 from CHK_DETAILTOT
                 where chk_no = :chk_no and mmcode = :mmcode
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { chk_no, mmcode }, DBWork.Transaction) != null;
        }

        public int InsertChkDetailtot(string chk_no, string mmcode, string userId, string ip) {
            string sql = @"
                INSERT INTO CHK_DETAILTOT(CHK_NO, MMCODE, MMNAME_C, MMNAME_E, BASE_UNIT, M_CONTPRICE, WH_NO, STORE_LOC,
                                                         MAT_CLASS, M_STOREID, STORE_QTY, STORE_QTYC, GAP_T, 
                                                         GAP_C, 
                                                         PRO_LOS_QTY, 
                                                         MISS_PER, APL_OUTQTY, CHK_QTY1, STATUS_TOT, CREATE_DATE, CREATE_USER, 
                                                         UPDATE_TIME, UPDATE_USER, UPDATE_IP, STORE_QTY1, CONSUME_QTY, CONSUME_AMOUNT)
                               SELECT CD.CHK_NO, CD.MMCODE, CD.MMNAME_C, CD.MMNAME_E, CD.BASE_UNIT, CD.M_CONTPRICE, CD.WH_NO, CD.STORE_LOC, 
                                      CD.MAT_CLASS, CD.M_STOREID, CD.STORE_QTYC, CD.STORE_QTYC, (CD.CHK_QTY - CD.STORE_QTYC),
                                      (CD.CHK_QTY - CD.STORE_QTYC), 
                                      (CASE WHEN (select chk_wh_grade from CHK_MAST where chk_no = CD.CHK_NO) = 1 THEN (CD.CHK_QTY - CD.STORE_QTYC)
                                            WHEN (select chk_wh_grade from CHK_MAST where chk_no = CD.CHK_NO) = 2 AND (select chk_wh_kind from CHK_MAST where chk_no = CD.CHK_NO) = 0 THEN (CD.CHK_QTY - CD.STORE_QTYC) 
                                            WHEN (select wh_name(chk_wh_no) from CHK_MAST where chk_no = CD.CHK_NO) ='供應中心' THEN (CD.CHK_QTY - CD.STORE_QTYC) ELSE 0 END),
                                      0, CD.APL_OUTQTY, CD.CHK_QTY, '1', SYSDATE, :userId, 
                                      SYSDATE, :userId, :ip, CD.STORE_QTYC, 0, 0
                               FROM CHK_DETAIL CD 
                               WHERE CD.CHK_NO = :chk_no
                                 and CD.MMCODE = :mmcode
            ";
            return DBWork.Connection.Execute(sql, new { chk_no, mmcode, userId, ip }, DBWork.Transaction);
        }
        #endregion

        #region 批號效期
        public IEnumerable<CHK_EXPLOC> GetChkexpinvtrns(string chk_no, string mmcode) {
            string sql = @"
                select a.chk_no, a.wh_no, a.mmcode,  twn_date(a.exp_date) as exp_date, a.lot_no, nvl(a.trn_qty, 0) trn_qty
                  from CHK_EXPINV_TRNS a
                 where chk_no = :chk_no and mmcode = :mmcode
            ";
            return DBWork.Connection.Query<CHK_EXPLOC>(sql, new { chk_no, mmcode }, DBWork.Transaction);
        }

        public int UpdateMiwexpinvRollback(CHK_EXPLOC exp) {
            string sql = @"
                update MI_WEXPINV 
                   set inv_qty = nvl(inv_qty, 0) + (to_number(:TRN_QTY))
                 where wh_no = :WH_NO and mmcode = :MMCODE
                   and twn_date(exp_date) = :EXP_DATE
                   and lot_no = :LOT_NO
            ";
            return DBWork.Connection.Execute(sql, exp, DBWork.Transaction);
        }

        public int DeleteChkexpinvtrns(string chk_no, string mmcode) {
            string sql = @"
                delete from CHK_EXPINV_TRNS
                 where chk_no = :chk_no and mmcode = :mmcode
            ";
            return DBWork.Connection.Execute(sql, new { chk_no, mmcode }, DBWork.Transaction);
        }

        public IEnumerable<CHK_EXPLOC> GetExpinvs(string chk_no, string wh_no, string mmcode)
        {
            string sql = @"
                select * from (
                    select b.chk_no, b.wh_no, b.mmcode, twn_date(a.exp_date) as exp_date, a.lot_no, nvl(a.inv_qty, 0) as ori_inv_qty,
                           b.use_qty,
                           (case when twn_date(a.exp_date) < twn_sysdate then 'Y' else 'N' end) as isexpired
                      from CHK_DETAIL b, MI_WEXPINV a 
                     where b.chk_no = :chk_no
                       and b.mmcode = :mmcode
                       and b.wh_no = a.wh_no and a.mmcode = b.mmcode
                )
                order by mmcode, exp_date, lot_no
            ";
            return DBWork.Connection.Query<CHK_EXPLOC>(sql, new { chk_no, wh_no, mmcode }, DBWork.Transaction);
        }
        public string GetNewUseQty(CHK_DETAIL item)
        {
            string sql = @"
                select (case when (:CHK_WH_GRADE = 2 AND :CHK_WH_KIND = 1 ) 
                                then nvl(nvl(a.use_qty_af_chk, a.use_qty), 0) + nvl(a.inventory, 0) --單位抓耗用量+差異量
                             when (:CHK_WH_GRADE = 2 AND :CHK_WH_KIND = 0) 
                                then nvl(a.altered_use_qty, a.use_qty) - nvl(a.inventory, 0) - nvl(a.use_qty, 0)   --藥局抓 消耗+差異 - 原始消耗
                             when (:CHK_WH_GRADE = 1)
                                then nvl(a.inventory, 0) -- 庫房抓差異量
                        end) use_qty 
                  from CHK_DETAIL a
                 where chk_no=:CHK_NO and mmcode = :MMCODE
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, item, DBWork.Transaction);
        }
        public string GetExpInvDiff(string chk_no, string mmcode)
        {
            string sql = @"
                select a.store_qtyc - nvl((select sum(nvl(inv_qty, 0)) from MI_WEXPINV where wh_no = a.wh_no and mmcode = a.mmcode),0)
                  from CHK_DETAIL a
                  where chk_no=:chk_no and mmcode = :mmcode
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { chk_no, mmcode }, DBWork.Transaction);
        }
        public int MergeExpinv(string wh_no, string mmcode, string diff)
        {
            string sql = @"
                merge into MI_WEXPINV a
                using ( 
                  select :wh_no as wh_no, :mmcode as mmcode, '9991231' as exp_date, 'TMPLOT' as lot_no, 0 as inv_qty
                    from dual
                ) b 
                on (a.wh_no = b.wh_no and a.mmcode = b.mmcode and twn_date(a.exp_date) = b.exp_date and a.lot_no = b.lot_no)
                when matched then
                  update set a.inv_qty = nvl(a.inv_qty, 0) + :diff
                when not matched then
                  insert (wh_no, mmcode, exp_date, lot_no, inv_qty) 
                  values (:wh_no, :mmcode, twn_todate('9991231'), 'TMPLOT', :diff)
            ";
            return DBWork.Connection.Execute(sql, new { wh_no, mmcode, diff }, DBWork.Transaction);
        }

        public int InsertChkExpinvTrns(CHK_EXPLOC exp)
        {
            string sql = @"
                insert into CHK_EXPINV_TRNS (chk_no, wh_no, mmcode, exp_date, lot_no, trn_qty)
                values (:CHK_NO, :WH_NO, :MMCODE, twn_todate(:EXP_DATE), :LOT_NO, :TRN_QTY)
            ";
            return DBWork.Connection.Execute(sql, exp, DBWork.Transaction);
        }


        public bool CheckMiwexpinvExists(CHK_EXPLOC exp)
        {
            string sql = @"
                select 1 from MI_WEXPINV
                 where wh_no = :WH_NO and mmcode = :MMCODE
                   and exp_date = twn_todate(:EXP_DATE)
                   and lot_no = :LOT_NO
            ";
            return DBWork.Connection.ExecuteScalar(sql, exp, DBWork.Transaction) != null;
        }
        public int UpdateMiwexpinv(CHK_EXPLOC exp)
        {
            string sql = @"
                update MI_WEXPINV
                   set inv_qty = inv_qty - to_number(:TRN_QTY)
                 where wh_no = :WH_NO and mmcode = :MMCODE
                   and exp_date = twn_todate(:EXP_DATE)
                   and lot_no = :LOT_NO
            ";

            return DBWork.Connection.Execute(sql, exp, DBWork.Transaction);
        }
        public int InsertMiwexpinv(CHK_EXPLOC exp)
        {
            string sql = @"
                insert into MI_WEXPINV (wh_no, mmcode, exp_date, lot_no, inv_Qty)
                values (:WH_NO, :MMCODE, twn_todate(:EXP_DATE), :LOT_NO, (-1) * :TRN_QTY )
            ";
            return DBWork.Connection.Execute(sql, exp, DBWork.Transaction);
        }
        #endregion

        #region 儲位
        public IEnumerable<CHK_EXPLOC> GetChklocinvtrns(string chk_no, string mmcode)
        {
            string sql = @"
                select chk_no, wh_no, mmcode,  store_loc, trn_qty
                  from CHK_LOCINV_TRNS
                 where chk_no = :chk_no and mmcode = :mmcode
            ";
            return DBWork.Connection.Query<CHK_EXPLOC>(sql, new { chk_no, mmcode }, DBWork.Transaction);
        }

        public int UpdateMiwlocinvRollback(CHK_EXPLOC exp)
        {
            string sql = @"
                update MI_WLOCINV 
                   set inv_qty = nvl(inv_qty, 0) + (to_number(:TRN_QTY))
                 where wh_no = :WH_NO and mmcode = :MMCODE
                   and store_loc = :STORE_LOC
            ";
            return DBWork.Connection.Execute(sql, exp, DBWork.Transaction);
        }

        public int DeleteChklocinvtrns(string chk_no, string mmcode)
        {
            string sql = @"
                delete from CHK_LOCINV_TRNS
                 where chk_no = :chk_no and mmcode = :mmcode
            ";
            return DBWork.Connection.Execute(sql, new { chk_no, mmcode }, DBWork.Transaction);
        }
        public IEnumerable<CHK_EXPLOC> GetLocinvs(string chk_no, string wh_no, string mmcode)
        {
            string sql = @"
                select b.chk_no, b.wh_no, b.mmcode, a.store_loc, nvl(a.inv_qty, 0) as ori_inv_qty,
                       b.use_qty
                  from CHK_DETAIL b, MI_WLOCINV a
                 where b.chk_no = :chk_no
                   and b.mmcode = :mmcode
                   and b.wh_no = a.wh_no and a.mmcode = b.mmcode
                 order by b.mmcode, a.store_loc
            ";
            return DBWork.Connection.Query<CHK_EXPLOC>(sql, new { chk_no, wh_no, mmcode }, DBWork.Transaction);
        }
        public string GetLocInvDiff(string chk_no, string mmcode)
        {
            string sql = @"
                select a.store_qtyc - nvl((select sum(nvl(inv_qty, 0)) from MI_WLOCINV where wh_no = a.wh_no and mmcode = a.mmcode),0)
                  from CHK_DETAIL a
                  where chk_no=:chk_no and mmcode = :mmcode
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { chk_no, mmcode }, DBWork.Transaction);
        }
        public int MergeLocinv(string wh_no, string mmcode, string diff)
        {
            string sql = @"
                merge into MI_WLOCINV a
                using ( 
                  select :wh_no as wh_no, :mmcode as mmcode, 'TMPLOC' as store_loc, 0 as inv_qty
                    from dual
                ) b 
                on (a.wh_no = b.wh_no and a.mmcode = b.mmcode and a.store_loc = b.store_loc)
                when matched then
                  update set a.inv_qty = nvl(a.inv_qty, 0) + :diff
                when not matched then
                  insert (wh_no, mmcode, store_loc, inv_qty) 
                  values (:wh_no, :mmcode, 'TMPLOC', :diff)
            ";
            return DBWork.Connection.Execute(sql, new { wh_no, mmcode, diff }, DBWork.Transaction);
        }

        public int InsertChkLocinvTrns(CHK_EXPLOC exp)
        {
            string sql = @"
                insert into CHK_LOCINV_TRNS (chk_no, wh_no, mmcode, store_loc, trn_qty)
                values (:CHK_NO, :WH_NO, :MMCODE, :STORE_LOC, :TRN_QTY)
            ";
            return DBWork.Connection.Execute(sql, exp, DBWork.Transaction);
        }
        public bool CheckMiwlocinvExists(CHK_EXPLOC exp)
        {
            string sql = @"
                select 1 from MI_WLOCINV
                 where wh_no = :WH_NO and mmcode = :MMCODE
                   and store_loc = :STORE_LOC
            ";
            return DBWork.Connection.ExecuteScalar(sql, exp, DBWork.Transaction) != null;
        }
        public int UpdateMiwlocinv(CHK_EXPLOC exp)
        {
            string sql = @"
                update MI_WLOCINV
                   set inv_qty = inv_qty - to_number(:TRN_QTY)
                 where wh_no = :WH_NO and mmcode = :MMCODE
                   and store_loc = :STORE_LOC
            ";

            return DBWork.Connection.Execute(sql, exp, DBWork.Transaction);
        }
        public int InsertMiwlocinv(CHK_EXPLOC exp)
        {
            string sql = @"
                insert into MI_WLOCINV (wh_no, mmcode, store_loc, inv_qty)
                values (:WH_NO, :MMCODE, :STORE_LOC, (-1) * :TRN_QTY)
            ";
            return DBWork.Connection.Execute(sql, exp, DBWork.Transaction);
        }
        #endregion

        public string GetChkWhNo(string chk_no) {
            string sql = @"
                select chk_wh_no from CHK_MAST where chk_no = :chk_no
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { chk_no }, DBWork.Transaction);
        }

        public string GetChkWhGrade(string chk_no)
        {
            string sql = @"
                select chk_wh_grade from CHK_MAST where chk_no = :chk_no
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { chk_no }, DBWork.Transaction);
        }
        public IEnumerable<CHK_DETAIL> GetAllDetails(string chk_no) {
            string sql = @"
                select b.*, a.chk_wh_kind, a.chk_wh_grade, a.chk_ym, a.chk_wh_no, b.chk_qty as ori_chk_qty
                  from CHK_MAST a, CHK_DETAIL b
                 where a.chk_no = :chk_no
                   and a.chk_no = b.chk_no
                   and b.chk_time is not null
            ";
            return DBWork.Connection.Query<CHK_DETAIL>(sql, new { chk_no }, DBWork.Transaction);
        }
    }
}