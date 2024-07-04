using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using WebApp.Models;
using Dapper;
using System.Data;
namespace WebApp.Repository.AA
{
    public class AA0088Repository : JCLib.Mvc.BaseRepository
    {
        public AA0088Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        // GET api/<controller>
        // 報表取明細資料
        public IEnumerable<ME_DOCM> GetReport(string apptime_bg, string apptime_ed,bool isHospCode0)
        {
            var p = new DynamicParameters();
            string sql = string.Empty;
            if (isHospCode0)
            {
                sql = @"SELECT ROWNUM as SEQ, 
                               a.contracno,
                               a.mmcode,
                               a.mmname_e,
                               a.e_compunit,
                               (select a.m_agenno || agen_namee 
                                  from PH_VENDER 
                                 where agen_no = a.m_agenno) as agen_name,
                               a.e_manufact, 
                               a.base_unit,
                               a.M_CONTPRICE,
                               b.inv_qty,  
                               a.E_SCIENTIFICNAME,
                               ((case when a.M_CONTPRICE is null 
                                         then 1 
                                         else a.M_CONTPRICE end)  
                                   *  b.inv_qty) as total,
                               a.m_agenno  as  agen_no
                          FROM mi_mast a, mi_whinv b , MI_WINVCTL c
                         WHERE ( b.mmcode = a.mmcode ) 
                           and ( b.wh_no = WHNO_1X('0') )
                           and substr(a.mmcode, 1, 3) = '004'
                           and c.mmcode = b.mmcode
                           and c.wh_no = b.wh_no
                           and c.ctdmdccode = '0'
                           and b.inv_qty <> 0";
            }
            else
            {
                sql = @"SELECT ROWNUM as SEQ, 
                               a.contracno,
                               a.mmcode,
                               a.mmname_e,
                               a.e_compunit,
                               (select a.m_agenno || agen_namee 
                                  from PH_VENDER 
                                 where agen_no = a.m_agenno) as agen_name,
                               a.e_manufact, 
                               a.base_unit,
                               a.disc_cprice,
                               (case when b.inv_qty > c.war_qty then c.war_qty else b.inv_qty end) as inv_qty,  
                               a.E_SCIENTIFICNAME,
                               ((case when a.disc_cprice is null 
                                         then 1 
                                         else a.disc_cprice end)  
                                   *  c.war_qty) as total,
                               a.m_agenno  as  agen_no
                          FROM mi_mast a, mi_whinv b , MI_BASERO_14 c
                         WHERE b.mmcode = a.mmcode  
                           and b.wh_no = WHNO_ME1 
                           and c.war_qty <> 0
                           and c.wh_no=b.wh_no
                           and c.mmcode = a.mmcode
                ";
            }

            //UNION ALL
            //select null,null,null,null,null,null,null,null,null from UR_MENU where rownum <=10-
            //(select count(*) from PH_SMALL_D where DN = :DN)"; 填入空白資料補到10筆

            if (apptime_bg != "")
            {
                sql += " AND TO_CHAR(TO_NUMBER(TO_CHAR(mi_mast.E_CODATE,'YYYYMMDD'))-19110000) >= Substr(:apptime_bg, 1, 7) ";
                p.Add(":apptime_bg", apptime_bg);
            }
            if (apptime_ed != "")
            {
                sql += " AND TO_CHAR(TO_NUMBER(TO_CHAR(mi_mast.E_CODATE,'YYYYMMDD'))-19110000)<= Substr(:apptime_ed, 1, 7) ";
                p.Add(":apptime_ed", apptime_ed);
            }

            sql += " ORDER BY b.mmcode asc";

            return DBWork.Connection.Query<ME_DOCM>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCM> GetAll(string apptime_bg, string apptime_ed, int page_index, int page_size, string sorters, string wh_userId, bool isHospCode0)
        {
            var p = new DynamicParameters();

            var sql = string.Empty;

            if (isHospCode0)
            {
                sql = @"SELECT ROWNUM as SEQ, 
                               a.contracno,
                               a.mmcode,
                               a.mmname_e,
                               a.e_compunit,
                               (select a.m_agenno || agen_namee 
                                  from PH_VENDER 
                                 where agen_no = a.m_agenno) as agen_name,
                               a.e_manufact, 
                               a.base_unit,
                               a.M_CONTPRICE,
                               b.inv_qty,  
                               a.E_SCIENTIFICNAME,
                               ((case when a.M_CONTPRICE is null 
                                         then 1 
                                         else a.M_CONTPRICE end)  
                                   *  b.inv_qty) as total,
                               a.m_agenno  as  agen_no
                          FROM mi_mast a, mi_whinv b , MI_WINVCTL c
                         WHERE ( b.mmcode = a.mmcode ) 
                           and ( b.wh_no = WHNO_1X('0') )
                           and substr(a.mmcode, 1, 3) = '004'
                           and c.mmcode = b.mmcode
                           and c.wh_no = b.wh_no
                           and c.ctdmdccode = '0'
                           and b.inv_qty <> 0";
            }
            else {
                sql = @"SELECT ROWNUM as SEQ, 
                               a.contracno,
                               a.mmcode,
                               a.mmname_e,
                               a.e_compunit,
                               (select a.m_agenno || agen_namee 
                                  from PH_VENDER 
                                 where agen_no = a.m_agenno) as agen_name,
                               a.e_manufact, 
                               a.base_unit,
                               a.disc_cprice,
                               (case when b.inv_qty > c.war_qty then c.war_qty else b.inv_qty end) as inv_qty,  
                               a.E_SCIENTIFICNAME,
                               ((case when a.disc_cprice is null 
                                         then 1 
                                         else a.disc_cprice end)  
                                   *  c.war_qty) as total,
                               a.m_agenno  as  agen_no
                          FROM mi_mast a, mi_whinv b , MI_BASERO_14 c
                         WHERE b.mmcode = a.mmcode  
                           and b.wh_no = WHNO_ME1 
                           and c.war_qty <> 0
                           and c.wh_no=b.wh_no
                           and c.mmcode = a.mmcode
                ";
            }
            


            if (apptime_bg != "")
            {
                sql += " AND TO_CHAR(TO_NUMBER(TO_CHAR(mi_mast.E_CODATE,'YYYYMMDD'))-19110000) >= Substr(:p1, 1, 7) ";
                p.Add(":p1", apptime_bg);
            }
            if (apptime_ed != "")
            {
                sql += " AND TO_CHAR(TO_NUMBER(TO_CHAR(mi_mast.E_CODATE,'YYYYMMDD'))-19110000)<= Substr(:p2, 1, 7) ";
                p.Add(":p2", apptime_ed);
            }
            
            sql += " order by b.MMCODE asc";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public string GetUna(string tuser) {
            string sql = @"select una from UR_ID where tuser = :tuser";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { tuser = tuser}, DBWork.Transaction);
        }

        public string GetHospCode() {
            string sql = @"
                select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode'
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }
    }
}