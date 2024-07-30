using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using WebApp.Models;
using Dapper;
using System.Data;

namespace WebApp.Repository.F
{
    public class FA0034Repository : JCLib.Mvc.BaseRepository
    {
        public FA0034Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<FA0034> GetAll(string mat_class, string start, string end, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"select mi.mmcode, mmname_c, mmname_e, e_itemarmyno, e_clfarmyno,  cnt, qty
                             from mi_mast mi,
                                  (select distinct mmcode, count(po_no) cnt 
                                     from bc_cs_acc_log  
                                    where to_char(acc_time,'yyyymm') >= :STARTYM 
                                      and to_char(acc_time,'yyyymm') <= :ENDYM  
                                      and substr(po_no,1,3) in ('INV','GEN') 
                                      and unit_swap is not null 
                                      and bw_sqty is not null 
                                    group by mmcode ) t_cnt,
                                  (select mmcode, sum((acc_qty+bw_sqty)/unit_swap) qty 
                                     from bc_cs_acc_log 
                                    where to_char(acc_time,'yyyymm') >= :STARTYM 
                                      and to_char(acc_time,'yyyymm') <= :ENDYM 
                                      and substr(po_no,1,3) in ('INV','GEN') 
                                      and unit_swap is not null 
                                      and bw_sqty is not null 
                                    group by mmcode ) t_qty
                            where mi.mmcode=t_cnt.mmcode
                              and mi.mmcode=t_qty.mmcode
                              and mi.mat_class= :MAT_CLASS
                              and mi.m_matid='Y'
                            order by mi.mmcode";

            p.Add(":MAT_CLASS", string.Format("{0}", mat_class));
            p.Add(":STARTYM", string.Format("{0}", DateTime.Parse(start).ToString("yyyyMM")));
            p.Add(":ENDYM", string.Format("{0}", DateTime.Parse(end).ToString("yyyyMM")));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<FA0034>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetMatclassCombo()
        {
            string sql = @"select mat_class as value, mat_class || ' ' || mat_clsname as text
                             from MI_MATCLASS
                            where mat_clsid in ('2')
                            order by mat_class";
            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }

        public DataTable GetExcel(string mat_class, string start, string end)
        {
            var p = new DynamicParameters();

            string sql = @"select mi.mmcode as 院內碼, 
                                  mmname_e as 英文品名,  
                                  mmname_c as 中文品名,
                                  e_itemarmyno as 軍聯項次號, 
                                  e_clfarmyno as 軍聯項次組別, 
                                  cnt as 次數, 
                                  qty as 申購量
                             from mi_mast mi,
                                    (select distinct mmcode, count(po_no) cnt 
                                     from bc_cs_acc_log  
                                    where to_char(acc_time,'yyyymm') >= :STARTYM 
                                      and to_char(acc_time,'yyyymm') <= :ENDYM  
                                      and substr(po_no,1,3) in ('INV','GEN') 
                                      and unit_swap is not null 
                                      and bw_sqty is not null                                       
                                    group by mmcode ) t_cnt,
                                  (select mmcode, sum((acc_qty+bw_sqty)/unit_swap) qty 
                                     from bc_cs_acc_log 
                                    where to_char(acc_time,'yyyymm') >= :STARTYM 
                                      and to_char(acc_time,'yyyymm') <= :ENDYM 
                                      and substr(po_no,1,3) in ('INV','GEN') 
                                      and unit_swap is not null 
                                      and bw_sqty is not null 
                                    group by mmcode ) t_qty
                            where mi.mmcode=t_cnt.mmcode
                              and mi.mmcode=t_qty.mmcode
                              and mi.mat_class= :MAT_CLASS
                              and mi.m_matid='Y'
                            order by mi.mmcode";

            p.Add(":MAT_CLASS", string.Format("{0}", mat_class));
            p.Add(":STARTYM", string.Format("{0}", DateTime.Parse(start).ToString("yyyyMM")));
            p.Add(":ENDYM", string.Format("{0}", DateTime.Parse(end).ToString("yyyyMM")));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public string GetSql_14() {
            return @"
            select mi.mmcode, mmname_c, mmname_e, e_itemarmyno, e_clfarmyno,  cnt, qty
                             from mi_mast mi,
                                  (select distinct mmcode, count(po_no) cnt 
                                     from bc_cs_acc_log a 
                                    where to_char(acc_time,'yyyymm') >= :STARTYM 
                                      and to_char(acc_time,'yyyymm') <= :ENDYM  
                                      and exists (select 1 from MM_PO_M where po_no = a.po_no and mat_class='02')
                                      and unit_swap is not null 
                                      and bw_sqty is not null 
                                    group by mmcode ) t_cnt,
                                  (select mmcode, sum((acc_qty+bw_sqty)) qty 
                                     from bc_cs_acc_log a
                                    where to_char(acc_time,'yyyymm') >= :STARTYM 
                                      and to_char(acc_time,'yyyymm') <= :ENDYM 
                                      and exists (select 1 from MM_PO_M where po_no = a.po_no and mat_class='02')
                                      and unit_swap is not null 
                                      and bw_sqty is not null 
                                    group by mmcode ) t_qty
                            where mi.mmcode=t_cnt.mmcode
                              and mi.mmcode=t_qty.mmcode
                              and mi.mat_class= :MAT_CLASS
                              and mi.m_matid='Y'
                            order by mi.mmcode
            ";
        }
        public IEnumerable<FA0034> GetAll_14(string mat_class, string start, string end, int page_index, int page_size, string sorters) {
            string sql = GetSql_14();

            var p = new DynamicParameters();
            p.Add(":MAT_CLASS", string.Format("{0}", mat_class));
            p.Add(":STARTYM", string.Format("{0}", DateTime.Parse(start).ToString("yyyyMM")));
            p.Add(":ENDYM", string.Format("{0}", DateTime.Parse(end).ToString("yyyyMM")));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<FA0034>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public DataTable GetExcel_14(string mat_class, string start, string end) {
            var p = new DynamicParameters();
            string sql = string.Format(@"
                select mmcode as 院內碼, 
                                  mmname_e as 英文品名,  
                                  mmname_c as 中文品名,
                                  e_itemarmyno as 軍聯項次號, 
                                  e_clfarmyno as 軍聯項次組別, 
                                  cnt as 次數, 
                                  qty as 申購量
                  from (
            {0}
                    )
            ", GetSql_14());

            p.Add(":MAT_CLASS", string.Format("{0}", mat_class));
            p.Add(":STARTYM", string.Format("{0}", DateTime.Parse(start).ToString("yyyyMM")));
            p.Add(":ENDYM", string.Format("{0}", DateTime.Parse(end).ToString("yyyyMM")));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }


        public string GetHospCode() {
            string sql = @"
                select data_value from PARAM_D
                 where grp_code='HOSP_INFO'
                   and data_name='HospCode' 
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }
    }
}