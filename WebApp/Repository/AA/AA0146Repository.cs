using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using JCLib.DB; //處理跟資料庫連接的函數
using Dapper;
using WebApp.Models;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using WebApp.Models.AA;

namespace WebApp.Repository.AA
{
    public class AA0146Repository : JCLib.Mvc.BaseRepository  //一定要寫
    {
        public AA0146Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        public IEnumerable<COMBO_MODEL> GetFL_NAMECombo()
        {
            string sql = @"
                  select distinct FL_NAME as value,FL_NAME as text
                    from WARRES_CTL
                   order by FL_NAME
            ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }

        public bool CheckExists_Mimast(string mmcode)
        {
            string sql = @"select 1 
                             from MI_MAST 
                            where MMCODE=:mmcode                             
                          ";
            return (DBWork.Connection.ExecuteScalar(sql, new { mmcode = mmcode }, DBWork.Transaction) == null);
        }

        public bool CheckExists_Warres_Ctl(string wres_mmcode)
        {
            string sql = @"select 1 
                             from WARRES_CTL 
                            where WRES_MMCODE=:wres_mmcode                             
                          ";
            return (DBWork.Connection.ExecuteScalar(sql, new { WRES_MMCODE = wres_mmcode }, DBWork.Transaction) != null);
        }

        //查詢
        #region 查詢
        public IEnumerable<AA0146> GetAll(string fl_name,string Control_Id, bool isHospCode0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = string.Empty;
            if (isHospCode0)
            {
                sql = @"select ROWNUM as SEQ,  --報表的項次
                               a.CONTRACNO,  --聯標合約項次
                               a.MMCODE,  --院內碼
                               a.MMNAME_E,  --商品名
                               a.E_SCIENTIFICNAME,  --成分名
                               a.E_MANUFACT,  --廠牌 
                               a.BASE_UNIT,  --單位 
                               a.M_CONTPRICE,  --單價
                               b.INV_QTY,  --囤儲數量
                               ((case when a.M_CONTPRICE is null 
                                      then 1 
                                 else a.M_CONTPRICE
                                 end) * b.inv_qty) as TOTAL,  --金額
                               (select a.m_agenno || agen_namee 
                                  from PH_VENDER 
                                 where agen_no=a.m_agenno) as AGEN_NAME,  --廠商                          
                               (select WResQty from warres_ctl
                                 where FL_NAME=:fl_name
                                   and a.mmcode=wres_mmcode and rownum=1) as WResQty,  --規定屯量
                                '' as NOTE  --備考       
                         from mi_mast a, mi_whinv b, MI_WINVCTL c
                        where (b.mmcode = a.mmcode) 
                          and (b.wh_no = 'PH1X')
                          and substr(a.mmcode,1,3)='004'
                          and c.mmcode = b.mmcode
                          and c.wh_no = b.wh_no
                          and c.ctdmdccode = '0'
                          and b.inv_qty <> 0 
                       ";
            }
            else {
                sql = @"select ROWNUM as SEQ,  --報表的項次
                               a.CONTRACNO,  --聯標合約項次
                               a.MMCODE,  --院內碼
                               a.MMNAME_E,  --商品名
                               a.E_SCIENTIFICNAME,  --成分名
                               a.E_MANUFACT,  --廠牌 
                               a.BASE_UNIT,  --單位 
                               a.M_CONTPRICE,  --單價
                               b.INV_QTY,  --囤儲數量
                               ((case when a.M_CONTPRICE is null 
                                      then 1 
                                 else a.M_CONTPRICE
                                 end) * b.inv_qty) as TOTAL,  --金額
                               (select a.m_agenno || agen_namee 
                                  from PH_VENDER 
                                 where agen_no=a.m_agenno) as AGEN_NAME,  --廠商                          
                               (select WResQty from warres_ctl
                                 where FL_NAME=:fl_name
                                   and a.mmcode=wres_mmcode and rownum=1) as WResQty,  --規定屯量
                                '' as NOTE  --備考       
                         from mi_mast a, mi_whinv b
                        where (b.mmcode = a.mmcode) 
                          and (b.wh_no = WHNO_1X('0'))
                          and b.inv_qty <> 0 
                       ";
            }
            if (Control_Id == "1")
            {
                sql += @" and(select 1 from warres_ctl
                               where FL_NAME=:fl_name             
                                 and a.mmcode = wres_mmcode) = 1";
            }
            if (Control_Id == "2")
            {
                sql += @" and(select 1 from warres_ctl
                               where FL_NAME=:fl_name             
                                 and a.mmcode = wres_mmcode) is null ";
            }

            sql += @" order by b.MMCODE asc";

            p.Add(":fl_name", fl_name);            
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<AA0146>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA0146> GetControl(string p2, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"select :p2 as FL_NAME,SEQ_NO, NOB_NO, MAT_NAME, MMNAME, E_SPECNUNIT,
                               E_DRUGFORM, WRESQTY, TRANSQTY, PUR_MMCODE, WRES_MMCODE,
                               MMNAME_E, MMNAME_C, AGEN_NAME, E_ITEMARMYNO, M_CONTPRICE,
                               DISC_CPRICE, PUR_QTY, PUR_AMT, M_STOREID, CREATE_TIME,
                               CREATE_USER
                          from warres_ctl
                         where 1=1
                           and FL_NAME=:p2
                         order by SEQ_NO asc
                       ";

            p.Add(":p2", p2);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<AA0146>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        #endregion

        //匯出EXCEL
        #region 匯出EXCEL
        public DataTable GetExcel(string fl_name)
        {
            DynamicParameters p = new DynamicParameters();
            string sql = @"SELECT
                                fl_name, seq_no, nob_no, mat_name, mmname,
                                e_specnunit, e_drugform, wresqty, transqty, pur_mmcode,
                                wres_mmcode, mmname_e, mmname_c, agen_name, e_itemarmyno,
                                m_contprice, disc_cprice, pur_qty, pur_amt, m_storeid
                            FROM
                                warres_ctl
                            WHERE
                                fl_name =:fl_name
                                AND   ROWNUM < 4
                            ORDER BY
                                seq_no";

            p.Add(":fl_name", fl_name);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
        #endregion
        public int Delete_import(string fl_name, string seq_no) 
        {
            string sql = @"delete from WARRES_CTL 
                            where FL_NAME=:fl_name   
                              and SEQ_NO= :seq_no
                          ";
            return DBWork.Connection.Execute(sql, new { FL_NAME = fl_name, SEQ_NO = seq_no }, DBWork.Transaction);
        }
        public int Create_import(AA0146 def) //待改
        {
            var sql = @"insert into WARRES_CTL(FL_NAME,SEQ_NO, NOB_NO, MAT_NAME, MMNAME, E_SPECNUNIT,
                                               E_DRUGFORM, WRESQTY, TRANSQTY, PUR_MMCODE, WRES_MMCODE,
                                               MMNAME_E, MMNAME_C, AGEN_NAME, E_ITEMARMYNO, M_CONTPRICE,
                                               DISC_CPRICE, PUR_QTY, PUR_AMT, M_STOREID, 
                                               CREATE_TIME, CREATE_USER,UPDATE_IP)                                                     
                        values (:FL_NAME, :SEQ_NO, :NOB_NO, :MAT_NAME, :MMNAME, :E_SPECNUNIT,
                                :E_DRUGFORM, :WRESQTY, :TRANSQTY, :PUR_MMCODE, :WRES_MMCODE,
                                :MMNAME_E, :MMNAME_C, :AGEN_NAME, :E_ITEMARMYNO, :M_CONTPRICE,
                                :DISC_CPRICE, :PUR_QTY, :PUR_AMT, :M_STOREID,
                                sysdate, :CreateUser, :UpdateIp)";
            return DBWork.Connection.Execute(sql, def, DBWork.Transaction);
        }

        //報表
        #region 報表
        public IEnumerable<AA0146> GetReport(string fl_name, string Control_Id, bool isHospCode0)
        {
            var p = new DynamicParameters();
            var sql = "";
            if (isHospCode0)
            {
                 sql = @"select ROWNUM as SEQ,  --報表的項次
                               a.CONTRACNO,  --聯標合約項次
                               a.MMCODE,  --院內碼
                               a.MMNAME_E,  --商品名
                               a.E_SCIENTIFICNAME,  --成分名
                               a.E_MANUFACT,  --廠牌 
                               a.BASE_UNIT,  --單位 
                               a.M_CONTPRICE,  --單價
                               b.INV_QTY,  --囤儲數量
                               ((case when a.M_CONTPRICE is null 
                                      then 1 
                                 else a.M_CONTPRICE
                                 end) * b.inv_qty) as TOTAL,  --金額
                               (select a.m_agenno || agen_namee 
                                  from PH_VENDER 
                                 where agen_no=a.m_agenno) as AGEN_NAME,  --廠商                          
                               (select WResQty from warres_ctl
                                 where FL_NAME=:fl_name
                                   and a.mmcode=wres_mmcode) as WResQty,  --規定屯量
                                '' as NOTE  --備考       
                         from mi_mast a, mi_whinv b, MI_WINVCTL c
                        where (b.mmcode = a.mmcode) 
                          and (b.wh_no = 'PH1X')
                          and substr(a.mmcode,1,3)='004'
                          and c.mmcode = b.mmcode
                          and c.wh_no = b.wh_no
                          and c.ctdmdccode = '0'
                          and b.inv_qty <> 0 
                       ";
            }
            else {
                sql = @"select ROWNUM as SEQ,  --報表的項次
                               a.CONTRACNO,  --聯標合約項次
                               a.MMCODE,  --院內碼
                               a.MMNAME_E,  --商品名
                               a.E_SCIENTIFICNAME,  --成分名
                               a.E_MANUFACT,  --廠牌 
                               a.BASE_UNIT,  --單位 
                               a.M_CONTPRICE,  --單價
                               b.INV_QTY,  --囤儲數量
                               ((case when a.M_CONTPRICE is null 
                                      then 1 
                                 else a.M_CONTPRICE
                                 end) * b.inv_qty) as TOTAL,  --金額
                               (select a.m_agenno || agen_namee 
                                  from PH_VENDER 
                                 where agen_no=a.m_agenno) as AGEN_NAME,  --廠商                          
                               (select WResQty from warres_ctl
                                 where FL_NAME=:fl_name
                                   and a.mmcode=wres_mmcode and rownum=1) as WResQty,  --規定屯量
                                '' as NOTE  --備考       
                         from mi_mast a, mi_whinv b
                        where (b.mmcode = a.mmcode) 
                          and (b.wh_no = WHNO_1X('0'))
                          and b.inv_qty <> 0 
                       ";
            }
            if (Control_Id == "1")
            {
                sql += @" and(select 1 from warres_ctl
                               where FL_NAME=:fl_name             
                                 and a.mmcode = wres_mmcode) = 1";
            }
            if (Control_Id == "2")
            {
                sql += @" and(select 1 from warres_ctl
                               where FL_NAME=:fl_name             
                                 and a.mmcode = wres_mmcode) is null ";
            }

            sql += @" order by b.MMCODE asc";
            p.Add(":fl_name", fl_name);
            return DBWork.Connection.Query<AA0146>(sql, p, DBWork.Transaction);
        }

        public string GetUna(string tuser)
        {
            string sql = @"select una from UR_ID where tuser = :tuser";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { tuser = tuser }, DBWork.Transaction);
        }
        #endregion

        public string GetHospCode() {
            string sql = @"
                select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode'
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }
    }
}