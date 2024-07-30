using System;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using System.Text;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

namespace WebApp.Repository.AB
{
    public class AB0076ReportMODEL : JCLib.Mvc.BaseModel
    {
        public string F1 { get; set; }
        public string F2 { get; set; }
        public string F3 { get; set; }
        public string F4 { get; set; }
        public float F5 { get; set; }
        public float F6 { get; set; }
        public float F7 { get; set; }
        public string F8 { get; set; }

    }
    public class AB0076Repository : JCLib.Mvc.BaseRepository
    {
        public AB0076Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AB0076ReportMODEL> GetPrintData(string p1, string p2, string p3, string p4, string p5)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT WH_NAME(A.WH_NO) F2, A.MMCODE F3, 
                        MMCODE_NAME(A.MMCODE) F4, SUM(A.INSU_QTY) F5,SUM(A.HOSP_QTY) F6 ,
                        SUM(A.INSU_QTY) + SUM(A.HOSP_QTY) F7,''F8
                        FROM MI_CONSUME_DATE A WHERE 1 = 1 AND PROC_ID = 'Y' ";

            if (p1 != "" & p2 != "")
            {
                sql += " AND A.DATA_DATE BETWEEN :p1 AND :p2 ";
                p.Add(":p1", string.Format("{0}", p1));
                p.Add(":p2", string.Format("{0}", p2));
            }
            if (p1 != "" & p2 == "")
            {
                sql += " AND A.DATA_DATE >= :p1 ";
                p.Add(":p1", string.Format("{0}", p1));
            }
            if (p1 == "" & p2 != "")
            {
                sql += " AND A.DATA_DATE <= :p2 ";
                p.Add(":p2", string.Format("{0}", p2));
            }
            if (p3 != "")
            {
                sql += " AND A.WH_NO = :p3 ";
                p.Add(":p3", string.Format("{0}", p3));
            }
            if (p4 != "")
            {
                sql += " AND A.MMCODE = :p4 ";
                p.Add(":p4", string.Format("{0}", p4));
            }
            if (p5 != "")
            {
                sql += " AND ( A.VISIT_KIND = :p5 OR A.VISIT_KIND = '0') ";
                p.Add(":p5", string.Format("{0}", p5));
            }

            sql += " GROUP BY A.WH_NO, A.MMCODE ";
            sql += " ORDER BY A.MMCODE ASC ";
            return DBWork.Connection.Query<AB0076ReportMODEL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetKindCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_AB0076' AND DATA_NAME='KIND' 
                        ORDER BY DATA_VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetWhmastCombo()
        {
            string sql = @"SELECT DISTINCT WH_NO as VALUE, WH_NAME as TEXT ,
                        WH_NO || ' ' || WH_NAME as COMBITEM 
                        FROM MI_WHMAST
                        WHERE WH_KIND='0' AND WH_GRADE='2' 
                        ORDER BY WH_NO ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
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
    }
}
