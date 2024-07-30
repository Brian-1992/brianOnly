using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{
    public class AA0092Repository : JCLib.Mvc.BaseRepository
    {
        public AA0092Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0092> GetNow(string WH_NO, string MMCODE, string p2, int page_index, int page_size, string sorters, bool isHospCode0)
        {
            var p = new DynamicParameters();

            var sql = string.Empty;

            if (isHospCode0) {
                sql = @"
                    SELECT CASE WHEN S.WH_NO = 'PH1S' THEN '藥庫'          
                                    WHEN S.WH_NO = 'PH1A' THEN '住院藥局' 
                                    WHEN S.WH_NO = 'PH1C' THEN '內湖門診藥局'  
                                    WHEN S.WH_NO = 'PH1R' THEN '內湖急診藥局' 
                                    WHEN S.WH_NO = 'PHMC' THEN '汀州門診藥局' 
                                    END AS WH_NO ,  -- 庫別代碼
                               S.MMCODE ,           -- 藥品藥材代碼
                               B.MMNAME_E ,         -- 英文名稱
                               S.INV_QTY ,          -- 現有存量
                               CASE WHEN B.E_VACCINE = 'Y' 
                                    THEN '是' 
                                    ELSE '否' 
                                    END AS  VACKIND -- 是否疫苗
                        FROM MI_WHINV S , MI_MAST B 
                        WHERE S.WH_NO IN ('PH1S','PH1C','PH1A','PHMC','PH1R') 
                        AND S.MMCODE = B.MMCODE 
                        AND B.E_RETURNDRUGFLAG = 'Y'
                ";
            }
            else {
                sql = @"
                    SELECT C.WH_NAME AS WH_NO ,  -- 庫別代碼
                               S.MMCODE ,           -- 藥品藥材代碼
                               B.MMNAME_E ,         -- 英文名稱
                               S.INV_QTY ,          -- 現有存量
                               CASE WHEN B.E_VACCINE = 'Y' 
                                    THEN '是' 
                                    ELSE '否' 
                                    END AS  VACKIND -- 是否疫苗
                        FROM MI_WHINV S , MI_MAST B , MI_WHMAST C
                        WHERE S.WH_NO =C.WH_NO
                        AND C.WH_KIND='0' AND C.WH_GRADE in ('1','2')
                        AND S.MMCODE = B.MMCODE 
                        AND B.E_RETURNDRUGFLAG = 'Y'
                ";
            }

            if (WH_NO != "")
            {
                sql += " AND S.WH_NO  = :p0 ";
                p.Add(":p0", string.Format("{0}", WH_NO));
            }
            if (MMCODE != "")
            {
                sql += " AND S.MMCODE = :p1 ";
                p.Add(":p1", string.Format("{0}", MMCODE));
            }
            if (p2 != "")
            {
                 sql += " AND B.E_VACCINE  = :p2 "; 
                p.Add(":p2", string.Format("{0}", p2));
            }


            sql += " ORDER BY S.WH_NO,B.E_VACCINE,S.MMCODE";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0092>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT FROM MI_MAST A WHERE 1=1 ";


            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(A.MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(A.MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

                sql += " OR A.MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR A.MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, MMCODE", sql);
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

        public IEnumerable<COMBO_MODEL> GetWhnoCombo(bool hospcode)
        {
            string sql = "";

            if (hospcode)
            {
                sql = @" select WH_NO as VALUE, WH_NAME as TEXT, WH_NO || ' ' || WH_NAME as COMBITEM
                            from MI_WHMAST where WH_NO IN ('PH1S','PH1C','PH1A','PHMC','PH1R') 
                        order by VALUE ";
            }
            else
            {
                sql = @" select WH_NO as VALUE, WH_NAME as TEXT, WH_NO || ' ' || WH_NAME as COMBITEM
                            from MI_WHMAST where WH_KIND='0' and WH_GRADE in ('1','2')
                        order by VALUE ";
            }

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public DataTable GetExcel(string WH_NO, string MMCODE, string p2, bool isHospCode0)
        {
            var p = new DynamicParameters();

            var sql = string.Empty;

            if (isHospCode0)
            {
                sql = @"SELECT CASE WHEN S.WH_NO = 'PH1S' THEN '藥庫'          
                                    WHEN S.WH_NO = 'PH1A' THEN '住院藥局' 
                                    WHEN S.WH_NO = 'PH1C' THEN '內湖門診藥局'  
                                    WHEN S.WH_NO = 'PH1R' THEN '內湖急診藥局' 
                                    WHEN S.WH_NO = 'PHMC' THEN '汀州門診藥局' 
                                    END AS 庫別,
                               S.MMCODE  AS 藥品藥材代碼,           -- 藥品藥材代碼
                               B.MMNAME_E AS 英文名稱,         -- 英文名稱
                               S.INV_QTY AS 現有存量 ,          -- 現有存量
                               CASE WHEN B.E_VACCINE = 'Y' 
                                    THEN '是' 
                                    ELSE '否' 
                                    END AS  是否疫苗 -- 是否疫苗
                        FROM MI_WHINV S , MI_MAST B 
                        WHERE S.WH_NO IN ('PH1S','PH1C','PH1A','PHMC','PH1R') 
                        AND S.MMCODE = B.MMCODE 
                        AND B.E_RETURNDRUGFLAG = 'Y' ";
            }
            else {
                sql = @"
                    SELECT C.WH_NAME AS 庫別,
                               S.MMCODE  AS 藥品藥材代碼,           -- 藥品藥材代碼
                               B.MMNAME_E AS 英文名稱,         -- 英文名稱
                               S.INV_QTY AS 現有存量 ,          -- 現有存量
                               CASE WHEN B.E_VACCINE = 'Y' 
                                    THEN '是' 
                                    ELSE '否' 
                                    END AS  是否疫苗 -- 是否疫苗
                        FROM MI_WHINV S , MI_MAST B, MI_WHMAST C 
                        WHERE S.WH_NO=C.WH_NO
                        AND C.WH_KIND='0' AND C.WH_GRADE in ('1','2')
                        AND S.MMCODE = B.MMCODE 
                        AND B.E_RETURNDRUGFLAG = 'Y'
                ";
            }

            if (WH_NO != "")
            {
                sql += " AND S.WH_NO  = :p0 ";
                p.Add(":p0", string.Format("{0}", WH_NO));
            }
            if (MMCODE != "")
            {
                sql += " AND S.MMCODE = :p1 ";
                p.Add(":p1", string.Format("{0}", MMCODE));
            }
            if (p2 != "")
            {
                sql += " AND B.E_VACCINE  = :p2 ";
                p.Add(":p2", string.Format("{0}", p2));
            }


            sql += " ORDER BY S.WH_NO,B.E_VACCINE,S.MMCODE";



            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public string GetHospCode()
        {
            string sql = @"
                select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode'
            ";

            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }
    }
}