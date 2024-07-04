using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using TSGH.Models;
using WebApp.Models.F;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.F
{
    public class FA0026Repository : JCLib.Mvc.BaseRepository
    {
        public FA0026Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }


        public IEnumerable<FA0026> GetAll(string MATCLS, string MMCODE, string M_StoreID)
        {
            var p = new DynamicParameters();

            var sql = @"select mmcode, mmname_c, mmname_e, base_unit, m_purun, ROUND(uprice, 2) uprice,  
                        ROUND(m_contprice, 2) m_contprice, 
                        UNIT_EXCHRATIO(mmcode,m_purun,m_agenno) as exch_ratio,m_agenno, 
                        (select agen_namec from PH_VENDER where agen_no=a.m_agenno) as agen_namec, 
                        m_storeid, m_contid, m_matid, ROUND(disc_uprice, 2) disc_uprice, ROUND(disc_cprice, 2) disc_cprice, 
                        m_applyid, m_voll, m_volw, m_volh, m_volc, m_swap, m_phctnco
                        from MI_MAST a
                        where 1=1 ";


            if (!string.IsNullOrEmpty(MATCLS))
            {
                sql += @" and mat_class = :MATCLS ";
                p.Add(":MATCLS", string.Format("{0}", MATCLS));
            }
            if (!string.IsNullOrEmpty(MMCODE))
            {
                sql += @" and mmcode = :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}", MMCODE));
            }
            if (!string.IsNullOrEmpty(M_StoreID))
            {
                sql += @" and m_storeid = :M_StoreID ";
                p.Add(":M_StoreID", string.Format("{0}", M_StoreID));
            }

            sql += @" ORDER BY mmcode";

            return DBWork.PagingQuery<FA0026>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetMatClassCombo()
        {
            string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM
                        FROM MI_MATCLASS 
                        ORDER BY MAT_CLASS";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<MI_MAST> GetMMCODECombo(string mmcode, int page_index, int page_size, string sorters)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"SELECT DISTINCT {0} MMCODE , MMNAME_C, MMNAME_E from MI_MAST A WHERE 1=1 ";

            if (mmcode != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", mmcode);
                p.Add(":MMNAME_E_I", mmcode);
                p.Add(":MMNAME_C_I", mmcode);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", mmcode));

                sql += " OR MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", mmcode));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", mmcode));

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

        public DataTable GetExcel(string MATCLS, string MMCODE, string M_StoreID)
        {
            var p = new DynamicParameters();

            var sql = @"select mmcode 院內碼, 
                        mmname_c 中文品名, 
                        mmname_e 英文品名, 
                        base_unit 計量單位, 
                        m_purun 包裝單位, 
                        ROUND(uprice, 2) 進貨單價, 
                        ROUND(m_contprice, 2) 合約價, 
                        UNIT_EXCHRATIO(mmcode,m_purun,m_agenno) as 申購轉換率, 
                        m_agenno 廠編, 
                        (select agen_namec from PH_VENDER where agen_no=a.m_agenno) as 廠商名稱, 
                        m_storeid 是否庫備, 
                        m_contid 是否合約, 
                        m_matid 是否聯標, 
                        ROUND(disc_uprice, 2) 優惠後進貨單價, 
                        ROUND(disc_cprice, 2) 優惠後合約價, 
                        m_applyid 特殊識別碼, 
                        m_voll 長, 
                        m_volw 寬, 
                        m_volh 高, 
                        m_volc 圓周, 
                        m_swap 材積轉換率, 
                        m_phctnco 環保或衛署證號
                        from MI_MAST a
                        where 1=1 ";


            if (!string.IsNullOrEmpty(MATCLS))
            {
                sql += @" and mat_class = :MATCLS ";
                p.Add(":MATCLS", string.Format("{0}", MATCLS));
            }
            if (!string.IsNullOrEmpty(MMCODE))
            {
                sql += @" and mmcode = :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}", MMCODE));
            }
            if (!string.IsNullOrEmpty(M_StoreID))
            {
                sql += @" and m_storeid = :M_StoreID ";
                p.Add(":M_StoreID", string.Format("{0}", M_StoreID));
            }

            sql += @" ORDER BY mmcode";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<FA0026> GetPrintData(string MATCLS, string MMCODE, string M_StoreID)
        {
            var p = new DynamicParameters();

            var sql = @"select mmcode, mmname_c, mmname_e, base_unit, m_purun, ROUND(uprice, 2) uprice, 
                        ROUND(m_contprice, 2) m_contprice, 
                        UNIT_EXCHRATIO(mmcode,m_purun,m_agenno) as exch_ratio,m_agenno, 
                        (select agen_namec from PH_VENDER where agen_no=a.m_agenno) as agen_namec, 
                        m_storeid, m_contid, m_matid, ROUND(disc_uprice, 2) disc_uprice, ROUND(disc_cprice, 2) disc_cprice, 
                        m_applyid, m_voll, m_volw, m_volh, m_volc, m_swap, m_phctnco
                        from MI_MAST a
                        where 1=1 ";


            if (!string.IsNullOrEmpty(MATCLS))
            {
                sql += @" and mat_class = :MATCLS ";
                p.Add(":MATCLS", string.Format("{0}", MATCLS));
            }
            if (!string.IsNullOrEmpty(MMCODE))
            {
                sql += @" and mmcode = :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}", MMCODE));
            }
            if (!string.IsNullOrEmpty(M_StoreID))
            {
                sql += @" and m_storeid = :M_StoreID ";
                p.Add(":M_StoreID", string.Format("{0}", M_StoreID));
            }

            sql += @" ORDER BY mmcode";

            return DBWork.Connection.Query<FA0026>(sql, p, DBWork.Transaction);
        }        
    }
}