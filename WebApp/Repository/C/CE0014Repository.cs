using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.C
{
    public class CE0014Repository : JCLib.Mvc.BaseRepository
    {
        public CE0014Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<BC_WHCHKID> GetAll(string wh_no, int page_index, int page_size, string sorters) {

            var p = new DynamicParameters();

            string sql = @"SELECT a.WH_NO as WH_NO,
                                  b.WH_NAME as WH_NAME,
                                  a.WH_CHKUID as WH_CHKUID,
                                  c.UNA as WH_CHKUID_NAME,
                                  (b.WH_GRADE || ' ' || d.DATA_DESC) as WH_GRADE,
                                  (b.WH_KIND || ' ' || e.DATA_DESC) as WH_KIND
                            FROM BC_WHCHKID a, MI_WHMAST b, UR_ID c, PARAM_D d,PARAM_D e
                           WHERE a.WH_NO = :p0
                             AND b.WH_NO = a.WH_NO
                             AND c.TUSER = a.WH_CHKUID
                             AND (d.GRP_CODE = 'MI_WHMAST' AND d.DATA_NAME = 'WH_GRADE' AND d.DATA_VALUE = b.WH_GRADE)
                             AND (e.GRP_CODE = 'MI_WHMAST' AND e.DATA_NAME = 'WH_KIND' AND e.DATA_VALUE = b.WH_KIND)";

            p.Add(":p0", string.Format("{0}", wh_no));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BC_WHCHKID>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<URID> GetUrid(string tuser, string una, string inid, string wh_no, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT a.TUSER as TUSER,
                               a.ADUSER as ADUSER,
                               a.UNA as UNA,
                               b.INID as INID,
                               b.INID_NAME as INID_NAME
                          FROM UR_ID a , UR_INID b
                         WHERE 1=1 
                           AND a.INID = b.INID ";

            if (tuser != "")
            {
                sql += " AND a.TUSER LIKE :p0 ";
                p.Add(":p0", string.Format("{0}%", tuser));
            }
            if (una != "")
            {
                sql += " AND a.UNA LIKE :p1 ";
                p.Add(":p1", string.Format("{0}%", una));
            }
            if (inid != "")
            {
                sql += " AND a.INID = :p2 ";
                p.Add(":p2", string.Format("{0}", inid));
            }

            if (wh_no != string.Empty) {
                sql += @" AND TRIM(a.TUSER) NOT IN 
                            ( SELECT TRIM(BC_WHCHKID.WH_CHKUID) FROM BC_WHCHKID
                            WHERE WH_NO=:p3 OR WH_NO IS NULL ) ";

                p.Add(":p3", string.Format("{0}", wh_no));
            }
            sql += @"     and a.FL = '1'";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<URID>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public class URID : JCLib.Mvc.BaseModel
        {
            public string TUSER { get; set; }
            public string UNA { get; set; }
            public string INID { get; set; }
            public string INID_NAME { get; set; }
            public string ADUSER { get; set; }
        }


        public int Create(BC_WHCHKID bc_whchkid) {
            var sql = @"Insert INTO BC_WHCHKID (WH_NO, WH_CHKUID, CREATE_DATE, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)
                         VALUES (:WH_NO, :WH_CHKUID, SYSDATE, :UPDATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";

            return DBWork.Connection.Execute(sql, bc_whchkid, DBWork.Transaction);

        }

        public int Delete(BC_WHCHKID bc_whchkid)
        {
            var sql = @"DELETE FROM BC_WHCHKID
                         WHERE WH_NO = :WH_NO AND WH_CHKUID = :WH_CHKUID";

            return DBWork.Connection.Execute(sql, bc_whchkid, DBWork.Transaction);

        }

        public bool CheckExist(BC_WHCHKID bc_whchkid) {
            var sql = @"SELECT 1 FROM BC_WHCHKID
                         WHERE WH_NO = :WH_NO AND WH_CHKUID = :WH_CHKUID";

            return !(DBWork.Connection.ExecuteScalar(sql, bc_whchkid, DBWork.Transaction) == null);
        }

        #region combo

        public IEnumerable<MI_WHMAST> GetWhnoCombo(string p0, string userid, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.WH_NO, A.WH_NAME, A.WH_KIND, A.WH_GRADE FROM MI_WHMAST A, MI_WHID b WHERE 1=1 and b.wh_userid = :userid and b.wh_no = a.wh_no";


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

            p.Add("userid", userid);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_WHMAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<UR_INID> GetInidCombo(string p0, int page_index, int page_size, string sorters)
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


        #endregion

        #region popup

        public IEnumerable<MI_WHMAST> GetWhno(MI_WHMAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
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

        public class MI_WHMAST_QUERY_PARAMS
        {
            public string WH_NO;
            public string WH_NAME;
            public string WH_KIND;
            public string WH_GRADE;
        }

        #endregion
    }
}