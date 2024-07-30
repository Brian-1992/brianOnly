using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{
    public class AA0059Repository : JCLib.Mvc.BaseRepository
    {
        public AA0059Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0059M> GetAllMI_WHMAST(string p0, string p1, string p2, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT 
                            mi_whmast.wh_no,   
                            mi_whmast.wh_name,   
                            mi_whmast.wh_kind,   
                                (SELECT data_value || ' ' || data_desc 
                                 FROM param_d
                                 WHERE grp_code = 'MI_WHMAST' AND data_name = 'WH_KIND' and data_value = mi_whmast.wh_kind) AS wh_kind_d,
                            mi_whmast.wh_grade,
                                (SELECT data_value || ' ' || data_desc 
                                 FROM param_d
                                 WHERE grp_code = 'MI_WHMAST' AND data_name = 'WH_GRADE' and data_value = mi_whmast.wh_grade) AS wh_grade_d
                        FROM 
                            mi_whmast  
                        WHERE 
                            1=1 AND WH_KIND = '1' AND mi_whmast.cancel_id = 'N' ";

            if (p0 != "")
            {
                sql += " AND wh_no LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", p0));
            }
            if (p1 != "")
            {
                sql += " AND wh_name LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", p1));
            }

            sql += @" AND TRIM(wh_no) NOT IN (SELECT   
                            TRIM(mi_whmm.wh_no)
                            FROM mi_whmm
                            WHERE mmcode=:p2) ";

            p.Add(":p2", string.Format("{0}", p2));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0059M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA0059M> GetAllMI_MAST(string p0, string p1, string p2, string p4, string p5, string p6,  int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT 
                            a.mmcode,   
                            a.mmname_e,
                            a.mmname_c,
                            a.e_manufact,   
                            a.m_agenlab,
                            a.e_drugclass,
                                (SELECT data_value || ' ' || data_desc 
                                 FROM param_d
                                 WHERE grp_code = 'MI_MAST' AND data_name = 'E_DRUGCLASS' and data_value = a.e_drugclass) AS e_drugclass_d,
                            a.e_drugclassify,
                                (SELECT data_value || ' ' || data_desc 
                                 FROM param_d
                                 WHERE grp_code = 'MI_MAST' AND data_name = 'E_DRUGCLASSIFY' and data_value = a.e_drugclassify) AS e_drugclassify_d,
                            a.e_sourcecode,
                                (SELECT data_value || ' ' || data_desc 
                                 FROM param_d
                                 WHERE grp_code = 'MI_MAST' AND data_name = 'E_SOURCECODE' and data_value = a.e_sourcecode) AS e_sourcecode_d,
                            a.e_drugapltype,
                                (SELECT data_value || ' ' || data_desc 
                                 FROM param_d
                                 WHERE grp_code = 'MI_MAST' AND data_name = 'E_DRUGAPLTYPE' and data_value = a.e_drugapltype) AS e_drugapltype_d
                        FROM 
                            MI_MAST a, MI_MATCLASS b
                        WHERE 
                            1=1 
                            and a.mat_class = b.mat_class
                            and b.mat_clsid in ('2', '3')
                            and nvl(a.cancel_id, 'N') <> 'Y'";

            if (p0 != "")
            {
                sql += " AND mmcode LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", p0));
            }
            if (p1 != "")
            {
                sql += " AND mmname_e LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", p1));
            }
            if (p4 != "")
            {
                sql += " AND mmname_c LIKE :p4 ";
                p.Add(":p4", string.Format("%{0}%", p4));
            }
            if (p5 != "")
            {
                sql += " AND e_manufact LIKE :p5 ";
                p.Add(":p5", string.Format("%{0}%", p5));
            }
            if (p6 != "")
            {
                sql += " AND m_agenlab LIKE :p6 ";
                p.Add(":p6", string.Format("%{0}%", p6));
            }

            sql += @" AND TRIM(mmcode) NOT IN (SELECT   
                            TRIM(mi_whmm.mmcode)
                            FROM mi_whmm
                            WHERE wh_no=:p2) ";

            p.Add(":p2", string.Format("{0}", p2));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0059M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA0059M> GetAllMI_WHMM(string p0, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT                             
                            mi_whmm.mmcode,   
                            mi_whmm.wh_no,
                            mi_whmm.create_time,   
                            mi_whmm.create_user,   
                            mi_whmm.update_time,   
                            mi_whmm.update_user,   
                            mi_whmm.update_ip,
                            mi_whmast.wh_name,
                            mi_whmast.wh_kind,
                                (SELECT data_value || ' ' || data_desc 
                                 FROM param_d
                                 WHERE grp_code = 'MI_WHMAST' AND data_name = 'WH_KIND' and data_value = mi_whmast.wh_kind) AS wh_kind_d,
                            mi_whmast.wh_grade,
                                (SELECT data_value || ' ' || data_desc 
                                 FROM param_d
                                 WHERE grp_code = 'MI_WHMAST' AND data_name = 'WH_GRADE' and data_value = mi_whmast.wh_grade) AS wh_grade_d,
                            mi_mast.mmname_e,
                            mi_mast.mmname_c,
                            mi_mast.e_drugclass,
                                (SELECT data_value || ' ' || data_desc 
                                 FROM param_d
                                 WHERE grp_code = 'MI_MAST' AND data_name = 'E_DRUGCLASS' and data_value = mi_mast.e_drugclass) AS e_drugclass_d,
                            mi_mast.e_drugclassify,
                                (SELECT data_value || ' ' || data_desc 
                                 FROM param_d
                                 WHERE grp_code = 'MI_MAST' AND data_name = 'E_DRUGCLASSIFY' and data_value = mi_mast.e_drugclassify) AS e_drugclassify_d
                        FROM 
                            mi_whmm LEFT JOIN mi_mast ON mi_whmm.mmcode = mi_mast.mmcode LEFT JOIN mi_whmast ON mi_whmm.wh_no = mi_whmast.wh_no
                        WHERE
                            1=1 ";

            if (p0 != "")
            {
                sql += " AND mi_whmm.wh_no LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", p0));
            }
            if (p1 != "")
            {
                sql += " AND mi_whmm.mmcode LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", p1));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0059M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA0059M> GetAllMI_MMWH(string p0, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT                             
                            mi_whmm.mmcode,   
                            mi_whmm.wh_no,
                            mi_whmm.create_time,   
                            mi_whmm.create_user,   
                            mi_whmm.update_time,   
                            mi_whmm.update_user,   
                            mi_whmm.update_ip,
                            mi_whmast.wh_name,
                            mi_whmast.wh_kind,
                                (SELECT data_value || ' ' || data_desc 
                                 FROM param_d
                                 WHERE grp_code = 'MI_WHMAST' AND data_name = 'WH_KIND' and data_value = mi_whmast.wh_kind) AS wh_kind_d,
                            mi_whmast.wh_grade,
                                (SELECT data_value || ' ' || data_desc 
                                 FROM param_d
                                 WHERE grp_code = 'MI_WHMAST' AND data_name = 'WH_GRADE' and data_value = mi_whmast.wh_grade) AS wh_grade_d,
                            mi_mast.mmname_e,
                            mi_mast.mmname_c,
                            mi_mast.e_drugclass,
                                (SELECT data_value || ' ' || data_desc 
                                 FROM param_d
                                 WHERE grp_code = 'MI_MAST' AND data_name = 'E_DRUGCLASS' and data_value = mi_mast.e_drugclass) AS e_drugclass_d,
                            mi_mast.e_drugclassify,
                                (SELECT data_value || ' ' || data_desc 
                                 FROM param_d
                                 WHERE grp_code = 'MI_MAST' AND data_name = 'E_DRUGCLASSIFY' and data_value = mi_mast.e_drugclassify) AS e_drugclassify_d
                        FROM 
                            mi_whmm LEFT JOIN mi_mast ON mi_whmm.mmcode = mi_mast.mmcode LEFT JOIN mi_whmast ON mi_whmm.wh_no = mi_whmast.wh_no
                        WHERE
                            1=1 ";

            if (p0 != "")
            {
                sql += " AND mi_whmm.mmcode LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", p0));
            }
            if (p1 != "")
            {
                sql += " AND mi_whmm.wh_no LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", p1));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0059M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public int InsertWH(AA0059M mi_whmm)
        {
            var sql = @"INSERT INTO MI_WHMM (WH_NO,MMCODE) VALUES (:WH_NO,:MMCODE)";

            return DBWork.Connection.Execute(sql, mi_whmm, DBWork.Transaction);
        }

        public int InsertMM(AA0059M mi_whmm)
        {
            var sql = @"INSERT INTO MI_WHMM (WH_NO,MMCODE) VALUES (:WH_NO,:MMCODE)";

            return DBWork.Connection.Execute(sql, mi_whmm, DBWork.Transaction);
        }

        public int Delete(AA0059M mi_whmm)
        {
            var sql = @"DELETE FROM MI_WHMM WHERE WH_NO=:WH_NO AND MMCODE=:MMCODE";
            return DBWork.Connection.Execute(sql, mi_whmm, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMmcode(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.M_CONTPRICE, A.BASE_UNIT FROM MI_MAST A WHERE 1=1 ";


            if (query.MMCODE != "")
            {
                sql += " AND A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", query.MMCODE));
            }

            if (query.MMNAME_C != "")
            {
                sql += " AND A.MMNAME_C LIKE :MMNAME_C ";
                p.Add(":MMNAME_C", string.Format("%{0}%", query.MMNAME_C));
            }

            if (query.MMNAME_E != "")
            {
                sql += " AND A.MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", query.MMNAME_E));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
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

        public IEnumerable<MI_WHMAST> GetWh_no(MI_WHMAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT 
                                A.WH_NO, 
                                A.WH_NAME,
                                (SELECT data_value || ' ' || data_desc 
                                 FROM param_d
                                 WHERE grp_code = 'MI_WHMAST' AND data_name = 'WH_KIND' and data_value = A.wh_kind) AS WH_KIND, 
                                (SELECT data_value || ' ' || data_desc 
                                 FROM param_d
                                 WHERE grp_code = 'MI_WHMAST' AND data_name = 'WH_GRADE' and data_value = A.wh_grade) AS WH_GRADE
                            FROM MI_WHMAST A 
                            WHERE 1=1 AND WH_KIND IN ('0', '1') ";


            if (query.WH_NO != "")
            {
                sql += " AND A.WH_NO LIKE :WH_NO ";
                p.Add(":WH_NO", string.Format("%{0}%", query.WH_NO));
            }

            if (query.WH_NAME != "")
            {
                sql += " AND A.WH_NAME LIKE :WH_NAME ";
                p.Add(":WH_NAME", string.Format("%{0}%", query.WH_NAME));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_WHMAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_WHMAST> GetWH_NoCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.WH_NO, A.WH_NAME, A.WH_KIND, A.WH_GRADE FROM MI_WHMAST A WHERE 1=1 AND WH_KIND IN ('0', '1')  ";


            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.WH_NO, :WH_NO_I), 1000) + NVL(INSTR(A.WH_NAME, :WH_NAME_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大                
                p.Add(":WH_NO_I", p0);
                p.Add(":WH_NAME_I", p0);

                sql += " AND (A.WH_NO LIKE :WH_NO ";
                p.Add(":WH_NO", string.Format("%{0}%", p0));

                sql += " OR A.WH_NAME LIKE :WH_NAME) ";
                p.Add(":WH_NAME", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, WH_NO", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.WH_NO ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_WHMAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);

        }

        public bool CheckWhExist(string mi_whmm)
        {
            var sql = @"SELECT 1 FROM MI_WHMAST WHERE WH_NO=:WH_NO AND WH_KIND IN ('0', '1') ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = mi_whmm }, DBWork.Transaction) == null);
        }
        public bool CheckMmExist(string mi_whmm)
        {
            var sql = @"SELECT 1 FROM MI_MAST WHERE MMCODE=:MMCODE ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mi_whmm }, DBWork.Transaction) == null);
        }

        public bool CheckWHMMExist(string wh_no, string mmcode)
        {
            var sql = @"SELECT 1 FROM MI_WHMM WHERE MMCODE=:MMCODE AND WH_NO=:WH_NO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction) == null);
        }

        public class MI_WHMAST_QUERY_PARAMS
        {
            public string WH_NO;
            public string WH_NAME;
            public string WH_KIND;
            public string WH_GRADE;

            public string MMCODE;
            public string IS_INV;  // 需判斷庫存量>0
            public string E_IFPUBLIC;  // 是否公藥
        }

        public class MI_MAST_QUERY_PARAMS
        {
            public string MMCODE;
            public string MMNAME_C;
            public string MMNAME_E;
            public string MAT_CLASS;

            public string WH_NO;
            public string IS_INV;  // 需判斷庫存量>0
            public string E_IFPUBLIC;  // 是否公藥
        }


    }
}