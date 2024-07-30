using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.F
{

    public class FA0056ReportMODEL : JCLib.Mvc.BaseModel
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
    public class FA0056Repository : JCLib.Mvc.BaseRepository
    {
        public FA0056Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }


        public IEnumerable<FA0056> GetAllM(string rep_time,int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select p.seq ,p.mat_class ,p.skey ,p.mat_clsname ,p.inv_type ,
                        p.p_amt ,p.in_amt ,p.out_amt ,p.check_p_amt ,p.check_m_amt ,
                        p.check_amt ,p.inv_amt , :rep_time rep_time  
                     from (
            select seq,c.mat_class,c.skey,c.mat_clsname,inv_type, 
             nvl(pmn_cost,0) as p_amt,nvl(in_cost,0) as in_amt,nvl(out_cost,0) as out_amt,
             nvl((select invt_cost from dual where invt_cost>=0),0) as check_p_amt,
             nvl((select invt_cost from dual where invt_cost<=0),0) as check_m_amt,
             nvl((select invt_cost from dual),0) as check_amt,nvl(mn_cost,0) as inv_amt 
        from (
              select mat_class||skey as seq,skey,mat_class,(select mat_clsname from dual where skey=1) as mat_clsname,
                     (select '醫院軍存量' from dual where skey=1)||
                     (select '醫院民存量' from dual where skey=2)||
                     (select '學院軍存量' from dual where skey=3) as inv_type
                from (select mat_class,mat_clsname,b.skey from MI_MATCLASS a 
                join (select 1 as skey from dual 
                      union select 2 as skey from dual 
                      union select 3 as skey from dual) b on 1=1
               where mat_class in ('14','15','16','17','18','19','20','21','22', '23'))
               order by seq
             ) c left outer join TMP_INVCOST_ADJ_RP t 
                 on c.mat_class=t.mat_class 
                    and c.skey=t.skey 
                    and t.rep_time=to_date(:rep_time,'yyyymmddhh24miss')
       where 1=1
      union select mat_class||'4' as seq,mat_class,4 as skey,'' as mat_clsname,'合計存量' as inv_type,
                   nvl((select sum(pmn_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and mat_class=d.mat_class),0) as p_amt,
                   nvl((select sum(in_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and mat_class=d.mat_class),0) as in_amt,
                   nvl((select sum(out_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and mat_class=d.mat_class),0) as out_amt,
                   nvl((select sum(invt_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and mat_class=d.mat_class and invt_cost>=0),0) as check_p_amt,
                   nvl((select sum(invt_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and mat_class=d.mat_class and invt_cost<=0),0) as check_m_amt,
                   nvl((select sum(invt_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and mat_class=d.mat_class),0) as check_amt,
                   nvl((select sum(mn_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and mat_class=d.mat_class),0) as inv_amt
              from MI_MATCLASS d
             where mat_class in ('14','15','16','17','18','19','20','21','22','23') 
      union select '99'||f.skey as seq,'' as mat_class,f.skey,(select '合計' from dual where f.skey=1) as mat_clsname,
                   (select '醫院軍存量' from dual where f.skey=1)||(select '醫院民存量' from dual where f.skey=2)||(select '學院軍存量' from dual where f.skey=3) as inv_type,
                   nvl((select sum(pmn_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and skey=f.skey),0) as p_amt,
                   nvl((select sum(in_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and skey=f.skey),0) as in_amt,
                   nvl((select sum(out_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and skey=f.skey),0) as out_amt,
                   nvl((select sum(invt_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and skey=f.skey and invt_cost>=0),0) as check_p_amt,
                   nvl((select sum(invt_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and skey=f.skey and invt_cost<=0),0) as check_m_amt,
                   nvl((select sum(invt_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and skey=f.skey),0) as check_amt,
                   nvl((select sum(mn_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and skey=f.skey),0) as inv_amt
              from (select 1 as skey from dual 
                    union select 2 as skey from dual 
                    union select 3 as skey from dual) f
      union select '999' as seq,'' as mat_class,9 as skey,'總計' as mat_clsname,'全部總計' as inv_type,
                   nvl((select sum(pmn_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss')),0) as p_amt,
                   nvl((select sum(in_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss')),0) as in_amt,
                   nvl((select sum(out_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss')),0) as out_amt,
                   nvl((select sum(invt_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and invt_cost>=0),0) as check_p_amt,
                   nvl((select sum(invt_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and invt_cost<=0),0) as check_m_amt,
                   nvl((select sum(invt_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss')),0) as check_amt,
                   nvl((select sum(mn_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss')),0) as inv_amt from dual
            ) p where 1 = 1
             ";
            p.Add(":rep_time", rep_time);
            sql += " order by SEQ ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<FA0056>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<FA0056ReportMODEL> GetPrintData(string rep_time)
        {
            var p = new DynamicParameters();
            var sql = @" select p.seq F1,p.mat_class F2,p.skey F3,p.mat_clsname F4,inv_type F5,
                        p.p_amt F6,p.in_amt F7,p.out_amt F8,p.check_p_amt F9,p.check_m_amt F10,
                        p.check_amt F11,p.inv_amt F12 
                     from (
                     select seq,c.mat_class,c.skey,c.mat_clsname,inv_type, 
                        nvl(pmn_cost,0) as p_amt,nvl(in_cost,0) as in_amt,nvl(out_cost,0) as out_amt,
                        nvl((select invt_cost from dual where invt_cost>=0),0) as check_p_amt,
                        nvl((select invt_cost from dual where invt_cost<=0),0) as check_m_amt,
                        nvl((select invt_cost from dual),0) as check_amt,nvl(mn_cost,0) as inv_amt 
                from (
                      select mat_class||skey as seq,skey,mat_class,(select mat_clsname from dual where skey=1) as mat_clsname,
                             (select '醫院軍存量' from dual where skey=1)||
                             (select '醫院民存量' from dual where skey=2)||
                             (select '學院軍存量' from dual where skey=3) as inv_type
                        from (select mat_class,mat_clsname,b.skey from MI_MATCLASS a 
                        join (select 1 as skey from dual 
                              union select 2 as skey from dual 
                              union select 3 as skey from dual) b on 1=1
                       where mat_class in ('14','15','16','17','18','19','20','21','22', '23'))
                       order by seq
                     ) c left outer join TMP_INVCOST_ADJ_RP t 
                         on c.mat_class=t.mat_class 
                            and c.skey=t.skey 
                            and t.rep_time=to_date(:rep_time,'yyyymmddhh24miss')
               where 1=1
              union select mat_class||'4' as seq,mat_class,4 as skey,'' as mat_clsname,'合計存量' as inv_type,
                           nvl((select sum(pmn_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and mat_class=d.mat_class),0) as p_amt,
                           nvl((select sum(in_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and mat_class=d.mat_class),0) as in_amt,
                           nvl((select sum(out_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and mat_class=d.mat_class),0) as out_amt,
                           nvl((select sum(invt_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and mat_class=d.mat_class and invt_cost>=0),0) as check_p_amt,
                           nvl((select sum(invt_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and mat_class=d.mat_class and invt_cost<=0),0) as check_m_amt,
                           nvl((select sum(invt_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and mat_class=d.mat_class),0) as check_amt,
                           nvl((select sum(mn_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and mat_class=d.mat_class),0) as inv_amt
                      from MI_MATCLASS d
                     where mat_class in ('14','15','16','17','18','19','20','21','22','23') 
              union select '99'||f.skey as seq,'' as mat_class,f.skey,(select '合計' from dual where f.skey=1) as mat_clsname,
                           (select '醫院軍存量' from dual where f.skey=1)||(select '醫院民存量' from dual where f.skey=2)||(select '學院軍存量' from dual where f.skey=3) as inv_type,
                           nvl((select sum(pmn_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and skey=f.skey),0) as p_amt,
                           nvl((select sum(in_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and skey=f.skey),0) as in_amt,
                           nvl((select sum(out_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and skey=f.skey),0) as out_amt,
                           nvl((select sum(invt_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and skey=f.skey and invt_cost>=0),0) as check_p_amt,
                           nvl((select sum(invt_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and skey=f.skey and invt_cost<=0),0) as check_m_amt,
                           nvl((select sum(invt_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and skey=f.skey),0) as check_amt,
                           nvl((select sum(mn_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and skey=f.skey),0) as inv_amt
                      from (select 1 as skey from dual 
                            union select 2 as skey from dual 
                            union select 3 as skey from dual) f
                union select '999' as seq,'' as mat_class,9 as skey,'總計' as mat_clsname,'全部總計' as inv_type,
                           nvl((select sum(pmn_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss')),0) as p_amt,
                           nvl((select sum(in_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss')),0) as in_amt,
                           nvl((select sum(out_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss')),0) as out_amt,
                           nvl((select sum(invt_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and invt_cost>=0),0) as check_p_amt,
                           nvl((select sum(invt_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and invt_cost<=0),0) as check_m_amt,
                           nvl((select sum(invt_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss')),0) as check_amt,
                           nvl((select sum(mn_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss')),0) as inv_amt from dual
                     ) p where 1 = 1
                     ";

            sql += " order by SEQ ";

            p.Add(":rep_time", rep_time);

            return DBWork.Connection.Query<FA0056ReportMODEL>(sql, p, DBWork.Transaction);
        }

        //匯出
        public DataTable GetExcel(string rep_time)
        {
            DynamicParameters p = new DynamicParameters();
            var sql = @"select p.mat_clsname 物料分類,inv_type 存量分類,
                        p.p_amt 期初存貨成本,p.in_amt 進貨成本,p.out_amt 撥發成本,p.check_p_amt 盤盈,p.check_m_amt 盤虧,
                        p.check_amt 小計,p.inv_amt 調整後期末存貨成本 
                     from (
                    select seq,c.mat_class,c.skey,c.mat_clsname,inv_type, 
                         nvl(pmn_cost,0) as p_amt,nvl(in_cost,0) as in_amt,nvl(out_cost,0) as out_amt,
                         nvl((select invt_cost from dual where invt_cost>=0),0) as check_p_amt,
                         nvl((select invt_cost from dual where invt_cost<=0),0) as check_m_amt,
                         nvl((select invt_cost from dual),0) as check_amt,nvl(mn_cost,0) as inv_amt
                    from (
                          select mat_class||skey as seq,skey,mat_class,(select mat_clsname from dual where skey=1) as mat_clsname,
                                 (select '醫院軍存量' from dual where skey=1)||
                                 (select '醫院民存量' from dual where skey=2)||
                                 (select '學院軍存量' from dual where skey=3) as inv_type
                            from (select mat_class,mat_clsname,b.skey from MI_MATCLASS a 
                            join (select 1 as skey from dual 
                                  union select 2 as skey from dual 
                                  union select 3 as skey from dual) b on 1=1
                           where mat_class in ('14','15','16','17','18','19','20','21','22', '23'))
                           order by seq
                         ) c left outer join TMP_INVCOST_ADJ_RP t 
                             on c.mat_class=t.mat_class 
                                and c.skey=t.skey 
                                and t.rep_time=to_date(:rep_time,'yyyymmddhh24miss')
                   where 1=1
                  union select mat_class||'4' as seq,mat_class,4 as skey,'' as mat_clsname,'合計存量' as inv_type,
                               nvl((select sum(pmn_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and mat_class=d.mat_class),0) as p_amt,
                               nvl((select sum(in_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and mat_class=d.mat_class),0) as in_amt,
                               nvl((select sum(out_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and mat_class=d.mat_class),0) as out_amt,
                               nvl((select sum(invt_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and mat_class=d.mat_class and invt_cost>=0),0) as check_p_amt,
                               nvl((select sum(invt_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and mat_class=d.mat_class and invt_cost<=0),0) as check_m_amt,
                               nvl((select sum(invt_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and mat_class=d.mat_class),0) as check_amt,
                               nvl((select sum(mn_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and mat_class=d.mat_class),0) as inv_amt
                          from MI_MATCLASS d
                         where mat_class in ('14','15','16','17','18','19','20','21','22','23') 
                  union select '99'||f.skey as seq,'' as mat_class,f.skey,(select '合計' from dual where f.skey=1) as mat_clsname,
                               (select '醫院軍存量' from dual where f.skey=1)||(select '醫院民存量' from dual where f.skey=2)||(select '學院軍存量' from dual where f.skey=3) as inv_type,
                               nvl((select sum(pmn_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and skey=f.skey),0) as p_amt,
                               nvl((select sum(in_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and skey=f.skey),0) as in_amt,
                               nvl((select sum(out_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and skey=f.skey),0) as out_amt,
                               nvl((select sum(invt_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and skey=f.skey and invt_cost>=0),0) as check_p_amt,
                               nvl((select sum(invt_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and skey=f.skey and invt_cost<=0),0) as check_m_amt,
                               nvl((select sum(invt_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and skey=f.skey),0) as check_amt,
                               nvl((select sum(mn_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and skey=f.skey),0) as inv_amt
                          from (select 1 as skey from dual 
                                union select 2 as skey from dual 
                                union select 3 as skey from dual) f
                  union select '999' as seq,'' as mat_class,9 as skey,'總計' as mat_clsname,'全部總計' as inv_type,
                               nvl((select sum(pmn_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss')),0) as p_amt,
                               nvl((select sum(in_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss')),0) as in_amt,
                               nvl((select sum(out_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss')),0) as out_amt,
                               nvl((select sum(invt_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and invt_cost>=0),0) as check_p_amt,
                               nvl((select sum(invt_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss') and invt_cost<=0),0) as check_m_amt,
                               nvl((select sum(invt_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss')),0) as check_amt,
                               nvl((select sum(mn_cost) from TMP_INVCOST_ADJ_RP where rep_time=to_date(:rep_time,'yyyymmddhh24miss')),0) as inv_amt from dual
                   ) p where 1 = 1 
             ";

            p.Add(":rep_time", rep_time);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public string GetUserName(string id)
        {
            string sql = @"SELECT TUSER || ' ' || UNA FROM UR_ID WHERE UR_ID.TUSER=:TUSER";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { TUSER = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetUserInid(string id)
        {
            string sql = @"SELECT INID_NAME(INID) || '-' || INID FROM UR_ID WHERE UR_ID.TUSER=:TUSER";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { TUSER = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetSysdate()
        {
            string sql = @"select to_char(sysdate,'yyyymmddhh24miss') as rep_time from dual";
            string rtn = DBWork.Connection.ExecuteScalar(sql,  DBWork.Transaction).ToString();
            return rtn;
        }
        public int DeleteTMP()
        {
            var sql = @"delete from TMP_INVCOST_ADJ_RP where rep_time<trunc(sysdate) ";
            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }
        public int InsertTMP(string id, string reptime)
        {
            var sql = @"insert into TMP_INVCOST_ADJ_RP 
                    (rep_time,data_ym,mat_class,skey,
                        pmn_cost, in_cost,out_cost,invt_cost,mn_cost)
                     select to_date(:rep_time,'yyyymmddhh24miss') as rep_time,data_ym,mat_class,skey,
                        pmn_cost, in_cost,out_cost,invt_cost,mn_cost
                       from V_COST_ECM where data_ym=:data_ym order by mat_class,skey
            ";
            return DBWork.Connection.Execute(sql, new { data_ym = id,rep_time = reptime }, DBWork.Transaction); ;
        }

        //匯出
        public DataTable GetExcel3(string apptime1, string apptime2)
        {
            var p = new DynamicParameters();

            var sql = @"
                    SELECT A.TOWH 責任中心,WH_NAME(A.TOWH) 單位名稱,C.MAT_CLASS 類別,B.MMCODE 院內碼,C.MMNAME_C 中文品名,C.BASE_UNIT 計量單位,
                    '醫院軍' 軍民別,M_APPQTY 數量,
                    NVL((SELECT AVG_PRICE FROM MI_WHCOST_EC WHERE DATA_YM=SUBSTR(TWN_DATE(A.APPTIME),1,5) AND MMCODE=B.MMCODE AND MC_ID='1'),C.DISC_UPRICE) 消耗單價,
                    NVL((SELECT AVG_PRICE FROM MI_WHCOST_EC WHERE DATA_YM=SUBSTR(TWN_DATE(A.APPTIME),1,5) AND MMCODE=B.MMCODE AND MC_ID='1'),C.DISC_UPRICE)
                    *(M_APPQTY) 消耗總價,
                    A.DOCNO 申請單號,A.JCN 工單編號,NVL(B.APVTIME,A.APPTIME) 交易日期
                    FROM ME_DOCM A, ME_DOCD_EC B, MI_MAST C
                    WHERE A.DOCNO=B.DOCNO AND B.MMCODE=C.MMCODE
                          AND DOCTYPE='EF1' AND FLOWID='2'
                    ";
            if (apptime1 != "" & apptime2 != "")
            {
                sql += " AND  TWN_DATE(NVL(B.APVTIME,A.APPTIME)) BETWEEN :p0 AND :p1";
                p.Add(":p0", string.Format("{0}", apptime1));
                p.Add(":p1", string.Format("{0}", apptime2));
            }
            if (apptime1 != "" & apptime2 == "")
            {
                sql += " AND  TWN_DATE(NVL(B.APVTIME,A.APPTIME)) >= :p0 ";
                p.Add(":p0", string.Format("{0}", apptime1));
            }
            if (apptime1 == "" & apptime2 != "")
            {
                sql += " AND  TWN_DATE(NVL(B.APVTIME,A.APPTIME)) <= :p1 ";
                p.Add(":p1", string.Format("{0}", apptime2));
            }
            sql += @"
                    UNION ALL 
                    SELECT A.TOWH 責任中心,WH_NAME(A.TOWH) 單位名稱,C.MAT_CLASS 類別,B.MMCODE 院內碼,C.MMNAME_C 中文品名,C.BASE_UNIT 計量單位,
                    '醫院民' 軍民別,
                    APPQTY 數量,
                    NVL((SELECT AVG_PRICE FROM MI_WHCOST_EC WHERE DATA_YM=SUBSTR(TWN_DATE(A.APPTIME),1,5) AND MMCODE=B.MMCODE AND MC_ID='2'),C.DISC_UPRICE) 消耗單價,
                    NVL((SELECT AVG_PRICE FROM MI_WHCOST_EC WHERE DATA_YM=SUBSTR(TWN_DATE(A.APPTIME),1,5) AND MMCODE=B.MMCODE AND MC_ID='2'),C.DISC_UPRICE)
                    *(APPQTY) 消耗總價,
                    A.DOCNO 申請單號,A.JCN 工單編號,NVL(B.APVTIME,A.APPTIME) 交易日期
                    FROM ME_DOCM A, ME_DOCD_EC B, MI_MAST C
                    WHERE A.DOCNO=B.DOCNO AND B.MMCODE=C.MMCODE
                          AND DOCTYPE='EF1' AND FLOWID='2'
                    ";
            if (apptime1 != "" & apptime2 != "")
            {
                sql += " AND  TWN_DATE(NVL(B.APVTIME,A.APPTIME)) BETWEEN :p0 AND :p1";
                p.Add(":p0", string.Format("{0}", apptime1));
                p.Add(":p1", string.Format("{0}", apptime2));
            }
            if (apptime1 != "" & apptime2 == "")
            {
                sql += " AND  TWN_DATE(NVL(B.APVTIME,A.APPTIME)) >= :p0 ";
                p.Add(":p0", string.Format("{0}", apptime1));
            }
            if (apptime1 == "" & apptime2 != "")
            {
                sql += " AND  TWN_DATE(NVL(B.APVTIME,A.APPTIME)) <= :p1 ";
                p.Add(":p1", string.Format("{0}", apptime2));
            }
            sql += @"
                    UNION ALL 
                    SELECT A.TOWH 責任中心,WH_NAME(A.TOWH) 單位名稱,C.MAT_CLASS 類別,B.MMCODE 院內碼,C.MMNAME_C 中文品名,C.BASE_UNIT 計量單位,
                    '學院軍' 軍民別,
                    S_APPQTY 數量,
                    NVL((SELECT AVG_PRICE FROM MI_WHCOST_EC WHERE DATA_YM=SUBSTR(TWN_DATE(A.APPTIME),1,5) AND MMCODE=B.MMCODE AND MC_ID='3'),C.DISC_UPRICE) 消耗單價,
                    NVL((SELECT AVG_PRICE FROM MI_WHCOST_EC WHERE DATA_YM=SUBSTR(TWN_DATE(A.APPTIME),1,5) AND MMCODE=B.MMCODE AND MC_ID='3'),C.DISC_UPRICE)
                    *(S_APPQTY) 消耗總價,
                    A.DOCNO 申請單號,A.JCN 工單編號,NVL(B.APVTIME,A.APPTIME) 交易日期
                    FROM ME_DOCM A, ME_DOCD_EC B, MI_MAST C
                    WHERE A.DOCNO=B.DOCNO AND B.MMCODE=C.MMCODE
                          AND DOCTYPE='EF1' AND FLOWID='2'
                     ";
            if (apptime1 != "" & apptime2 != "")
            {
                sql += " AND  TWN_DATE(NVL(B.APVTIME,A.APPTIME)) BETWEEN :p0 AND :p1";
                p.Add(":p0", string.Format("{0}", apptime1));
                p.Add(":p1", string.Format("{0}", apptime2));
            }
            if (apptime1 != "" & apptime2 == "")
            {
                sql += " AND  TWN_DATE(NVL(B.APVTIME,A.APPTIME)) >= :p0 ";
                p.Add(":p0", string.Format("{0}", apptime1));
            }
            if (apptime1 == "" & apptime2 != "")
            {
                sql += " AND  TWN_DATE(NVL(B.APVTIME,A.APPTIME)) <= :p1 ";
                p.Add(":p1", string.Format("{0}", apptime2));
            }
            sql += " ORDER BY 1,11,7 DESC ";
            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        //匯出
        public DataTable GetExcel4(string apptime1, string apptime2)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.MMCODE 院內碼,B.MAT_CLASS 類別,B.MMNAME_C 中文品名,B.BASE_UNIT 計量單位, SUM(APVQTY+M_APVQTY+S_APVQTY) 消耗量
                        FROM MM_ECUSE A,MI_MAST B
                        WHERE A.MMCODE=B.MMCODE AND EC_ID='E' ";
            if (apptime1 != "" & apptime2 != "")
            {
                sql += " AND  A.USE_YM BETWEEN :p0 AND :p1";
                p.Add(":p0", string.Format("{0}", apptime1));
                p.Add(":p1", string.Format("{0}", apptime2));
            }
            if (apptime1 != "" & apptime2 == "")
            {
                sql += " AND  A.USE_YM >= :p0 ";
                p.Add(":p0", string.Format("{0}", apptime1));
            }
            if (apptime1 == "" & apptime2 != "")
            {
                sql += " AND  A.USE_YM <= :p1 ";
                p.Add(":p1", string.Format("{0}", apptime2));
            }
            sql += " GROUP BY A.MMCODE,B.MAT_CLASS,B.MMNAME_C,B.BASE_UNIT ";
            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

    }
}
