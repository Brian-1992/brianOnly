using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.BE
{
    public class BG0007Repository : JCLib.Mvc.BaseRepository
    {
        public BG0007Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<BG0007> GetAll(string p0, string p1, string p2, string p3, string p4, string p5, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select x.* from (
                        select a.po_no, b.mmcode, mmname_c, mmname_e, 
                        b.m_purun, b.po_price, b.po_qty, b.po_amt,  
                        (select sum(po_amt) from MM_PO_D where po_no=a.po_no )as totsum
                        from MM_PO_M a, MM_PO_D b, mi_mast m
                        where a.po_no=b.po_no and b.mmcode=m.mmcode
                        and po_status <> '87' and b.status ='N' ";

            if (p0 != "")
            {
                sql += " AND a.mat_class = :p0 ";
                p.Add(":p0", string.Format("{0}", p0));
            }
            if (p1 != "" & p2 != "")
            {
                sql += " AND TWN_DATE(A.po_time) BETWEEN :p1 AND :p2 ";
                p.Add(":p1", string.Format("{0}", p1));
                p.Add(":p2", string.Format("{0}", p2));
            }
            if (p1 != "" & p2 == "")
            {
                sql += " AND TWN_DATE(A.po_time) >= :p1 ";
                p.Add(":p1", string.Format("{0}", p1));
            }
            if (p1 == "" & p2 != "")
            {
                sql += " AND TWN_DATE(A.po_time) <= :p2 ";
                p.Add(":p2", string.Format("{0}", p2));
            }
            if (p3 != "")
            {
                sql += " AND m.m_storeid = :p3 ";
                p.Add(":p3", string.Format("{0}", p3));
            }
            if (p4 != "")
            {
                sql += " AND a.m_contid = :p4 ";
                p.Add(":p4", string.Format("{0}", p4));
            }

            sql += " ) x where 1=1  ";

            if (p5 != "")
            {
                sql += " and totsum > :p5  ";
                p.Add(":p5", string.Format("{0}", p5));
            }

            sql += " order by mmcode,po_no ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BG0007>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetMatclassCombo()
        {
            string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
                        FROM MI_MATCLASS WHERE MAT_CLSID IN ('1','2','3')    
                        ORDER BY MAT_CLASS";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetStroeidCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MM_PO_M' AND DATA_NAME='M_STOREID' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetContidCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MM_PO_M' AND DATA_NAME='M_CONTID' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
    }
}