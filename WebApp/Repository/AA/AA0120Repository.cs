using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using Dapper;
using WebApp.Models;

namespace WebApp.Repository.AA
{
    public class AA0120Repository : JCLib.Mvc.BaseRepository
    {
        public AA0120Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MI_UNITEXCH> GetAll(string mmcode, string unit_code, string agen_no, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT a.*, b.MMNAME_C, c.UI_CHANAME, d.AGEN_NAMEC FROM MI_UNITEXCH a
                            INNER JOIN MI_MAST b ON a.MMCODE=b.MMCODE
                            INNER JOIN MI_UNITCODE c ON a.UNIT_CODE=c.UNIT_CODE
                            INNER JOIN PH_VENDER d ON a.AGEN_NO=d.AGEN_NO WHERE 1=1 ";

            if (mmcode != "")
            {
                sql += " AND a.MMCODE LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", mmcode));
            }
            if (unit_code != "")
            {
                sql += " AND a.UNIT_CODE LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", unit_code));
            }
            if (agen_no != "")
            {
                sql += " AND a.AGEN_NO LIKE :p2 ";
                p.Add(":p2", string.Format("%{0}%", agen_no));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_UNITEXCH>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT, 
                        (SELECT UNIT_CODE || ' ' || UI_CHANAME from MI_UNITCODE where A.M_PURUN = UNIT_CODE) as M_PURUN, 
                        NVL(B.AGEN_NO || ' ' || B.AGEN_NAMEC, '請維護藥衛材基本檔廠商代碼') as AGEN_NAMEC 
                        FROM MI_MAST A left outer join PH_VENDER B on A.M_AGENNO=B.AGEN_NO WHERE 1=1 ";
            //if (wh_no != "")
            //{
            //    sql += " AND B.WH_NO = :WH_NO ";
            //    p.Add(":WH_NO", string.Format("{0}", wh_no));
            //}

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(A.MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(A.MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (upper(A.MMCODE) LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

                sql += " OR upper(A.MMNAME_E) LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR upper(A.MMNAME_C) LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMiMast()
        {
            var p = new DynamicParameters();

            var sql = @"SELECT a.*, MMCODE || ' ' || MMNAME_C as MMNAME FROM MI_MAST a WHERE 1=1 ";

            return DBWork.Connection.Query<MI_MAST>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<MI_UNITCODE> GetMiUnitcode()
        {
            var p = new DynamicParameters();

            var sql = @"SELECT a.*, UNIT_CODE || ' ' || UI_CHANAME as UI_NAME FROM MI_UNITCODE a WHERE 1=1 ";

            return DBWork.Connection.Query<MI_UNITCODE>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<PH_VENDER> GetPhVendor()
        {
            var p = new DynamicParameters();

            //var sql = @"SELECT AGEN_NO, AGEN_NO || ' ' || AGEN_NAMEC as AGEN_NAMEC FROM PH_VENDER WHERE REC_STATUS='A' ";
            var sql = @"SELECT AGEN_NO, AGEN_NO || ' ' || AGEN_NAMEC as AGEN_NAMEC FROM PH_VENDER";

            return DBWork.Connection.Query<PH_VENDER>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<MI_UNITEXCH> Get(string mmcode, string unitcode, string agen_no)
        {
            var sql = @"SELECT a.*, b.MMNAME_C, c.UI_CHANAME, d.AGEN_NAMEC FROM MI_UNITEXCH a
                            INNER JOIN MI_MAST b ON a.MMCODE=b.MMCODE
                            INNER JOIN MI_UNITCODE c ON a.UNIT_CODE=c.UNIT_CODE
                            INNER JOIN PH_VENDER d ON a.AGEN_NO=d.AGEN_NO
                        WHERE a.MMCODE=:MMCODE AND a.UNIT_CODE=:UNIT_CODE AND a.AGEN_NO=:AGEN_NO";
            return DBWork.Connection.Query<MI_UNITEXCH>(sql, new { MMCODE = mmcode, UNIT_CODE = unitcode, AGEN_NO = agen_no }, DBWork.Transaction);
        }

        public int Create(MI_UNITEXCH mi_unitexch)
        {
            var sql = @"INSERT INTO MI_UNITEXCH (MMCODE, UNIT_CODE, AGEN_NO, EXCH_RATIO, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                VALUES (:MMCODE, :UNIT_CODE, :AGEN_NO, :EXCH_RATIO, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, mi_unitexch, DBWork.Transaction);
        }

        public int Update(MI_UNITEXCH mi_unitexch)
        {
            var sql = @"UPDATE MI_UNITEXCH SET MMCODE = :MMCODE, UNIT_CODE = :UNIT_CODE, AGEN_NO = :AGEN_NO, EXCH_RATIO = :EXCH_RATIO,
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE AGEN_NO = :AGEN_NO and UNIT_CODE = :UNIT_CODE and MMCODE = :MMCODE";
            return DBWork.Connection.Execute(sql, mi_unitexch, DBWork.Transaction);
        }

        public bool CheckExists(string mmcode, string unit_code, string agen_no)
        {
            string sql = @"SELECT 1 FROM MI_UNITEXCH WHERE MMCODE=:MMCODE and UNIT_CODE=:UNIT_CODE and AGEN_NO=:AGEN_NO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode, UNIT_CODE = unit_code, AGEN_NO = agen_no }, DBWork.Transaction) == null);
        }
    }
}