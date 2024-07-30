using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.C
{
    public class CD0003Repository : JCLib.Mvc.BaseRepository
    {
        public CD0003Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<CD0003M> GetID(string p0, string p1, string p2, string p3, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT 
                            ur_id.tuser,
                            ur_id.una,
                            ur_id.inid,
                            ur_inid.inid_name
                        FROM 
                            ur_id, ur_inid
                        WHERE 
                            1=1 AND ur_id.inid = ur_inid.inid ";

            if (p0 != "")
            {
                sql += " AND tuser LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", p0));
            }
            if (p1 != "")
            {
                sql += " AND una LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", p1));
            }
            if (p2 != "")
            {
                sql += " AND ur_id.inid LIKE :p2 ";
                p.Add(":p2", string.Format("%{0}%", p2));
            }

            sql += @" AND TRIM(tuser) NOT IN (SELECT   
                            TRIM(bc_whid.wh_userid)
                            FROM bc_whid
                            WHERE wh_no=:p3 OR wh_no IS NULL) ";

            p.Add(":p3", string.Format("{0}", p3));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CD0003M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<CD0003M> GetAll(string p0, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT  
                            bc_whid.wh_userid,   
                            bc_whid.wh_no,
                            (select una from ur_id where tuser = bc_whid.wh_userid) AS wh_username,
                            bc_whid.is_duty,
                            bc_whid.create_date,   
                            bc_whid.create_user,   
                            bc_whid.update_time,   
                            bc_whid.update_user,   
                            bc_whid.update_ip,
                            mi_whmast.wh_name,
                            mi_whmast.wh_kind,
                                (SELECT data_value || ' ' || data_desc 
                                 FROM param_d
                                 WHERE grp_code = 'MI_WHMAST' AND data_name = 'WH_KIND' and data_value = mi_whmast.wh_kind) AS wh_kind_d,
                            mi_whmast.wh_grade,
                                (SELECT data_value || ' ' || data_desc 
                                 FROM param_d
                                 WHERE grp_code = 'MI_WHMAST' AND data_name = 'WH_GRADE' and data_value = mi_whmast.wh_grade) AS wh_grade_d,
                            ur_id.tuser,
                            ur_id.una,
                            ur_id.inid                            
                        FROM 
                            bc_whid LEFT JOIN ur_id ON bc_whid.wh_userid = ur_id.tuser LEFT JOIN mi_whmast ON bc_whid.wh_no = mi_whmast.wh_no
                        WHERE
                            1=1 ";

            if (p0 != "")
            {
                sql += " AND bc_whid.wh_no LIKE :p0 ";
                p.Add(":p0", string.Format("{0}%", p0));
            }

            sql += "order by wh_userid";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CD0003M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }


        public int Insert(CD0003M bc_whid)
        {
            var sql = @"INSERT INTO BC_WHID (WH_NO,WH_USERID,IS_DUTY,CREATE_DATE,CREATE_USER,UPDATE_TIME,UPDATE_USER,UPDATE_IP) 
                                    VALUES (:WH_NO,:WH_USERID,'',SYSDATE,:CREATE_USER,'','','')";

            return DBWork.Connection.Execute(sql, bc_whid, DBWork.Transaction);
        }

        public int Delete(CD0003M bc_whid)
        {
            var sql = @"DELETE FROM BC_WHID WHERE WH_NO=:WH_NO AND WH_USERID=:WH_USERID";
            return DBWork.Connection.Execute(sql, bc_whid, DBWork.Transaction);
        }
        
        public bool CheckWhExist(string bc_whid)
        {
            var sql = @"SELECT 1 FROM MI_WHMAST WHERE WH_NO=:WH_NO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = bc_whid }, DBWork.Transaction) == null);
        }

        public bool CheckUserExist(string wh_no, string wh_userid)
        {
            var sql = @"SELECT 1 FROM BC_WHID WHERE WH_USERID=:TUSER AND WH_NO=:WH_NO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = wh_no, TUSER = wh_userid }, DBWork.Transaction) == null);
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
                            WHERE 1=1 ";


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

            var sql = @"SELECT {0} A.WH_NO, A.WH_NAME, A.WH_KIND, A.WH_GRADE FROM MI_WHMAST A WHERE 1=1 ";


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

        public IEnumerable<UR_INID> GetInidWhCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.INID, A.INID_NAME FROM UR_INID A WHERE 1=1 ";


            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.INID, :INID_I), 1000) + NVL(INSTR(A.INID_NAME, :INID_NAME_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大                
                p.Add(":INID_I", p0);
                p.Add(":INID_NAME_I", p0);

                sql += " AND (A.INID LIKE :INID ";
                p.Add(":INID", string.Format("%{0}%", p0));

                sql += " OR A.INID_NAME LIKE :INID_NAME) ";
                p.Add(":INID_NAME", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, INID", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.INID ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<UR_INID>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
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



    }
}