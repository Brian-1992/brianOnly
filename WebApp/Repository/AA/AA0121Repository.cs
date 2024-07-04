using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using Dapper;
using WebApp.Models;

namespace WebApp.Repository.AA                      // WebApp\Repository\AA\AA0121Repository.cs          
{
    public class AA0121Repository : JCLib.Mvc.BaseRepository 
    {
        public AA0121Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MI_WHID> GetMasterAllByWH(string whCode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();


            var sql  = " SELECT a.WH_NO, a.WH_USERID,(select b.UNA from UR_ID b  where trim(a.WH_USERID)= trim(b.TUSER) ) WH_UNA,";
                sql += " a.TASK_ID, (select WH_KIND from MI_WHMAST  where trim(WH_NO)= trim(a.WH_NO) ) WH_KIND ";
                sql += " , a.task_id as ori_task_id";
                sql += " FROM MI_WHID a ";
                sql += " where 1=1 ";  
            
            //var sql = @"SELECT * FROM MI_WHID WHERE 1=1 ";

            if (whCode.Trim() != "")
            {
                sql += " AND a.WH_NO = :p0 ";
                p.Add(":p0", string.Format("{0}", whCode));
            }
         
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_WHID>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_WHID> GetMasterAllByUser(string userCode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = " SELECT a.WH_USERID, ";
            sql += " (select b.UNA from UR_ID b  where trim(a.WH_USERID)= trim(b.TUSER) ) WH_UNA ,";
            sql += " a.WH_NO ,";
            sql += " (select WH_NAME from MI_WHMAST  where trim(WH_NO)= trim(a.WH_NO) ) WH_NAME, ";
            sql += " (select WH_KIND from MI_WHMAST  where trim(WH_NO)= trim(a.WH_NO) ) WH_KIND, a.TASK_ID ";
            sql += " FROM MI_WHID a WHERE 1 = 1 ";

            //         SELECT a.WH_USERID,

            //(select b.UNA from UR_ID b  where trim(a.WH_USERID)= trim(b.TUSER) ) UNA , 
            //a.WH_NO ,

            //(select b.WH_NAME from MI_WHMAST b  where trim(a.WH_NO)= trim(b.WH_NO) ) WH_NAME ,  


            //a.TASK_ID

            //FROM MI_WHID a




            //AND WH_USERID = :p0
            //order by a.WH_NOUSERID asc

            //var sql = @"SELECT * FROM MI_WHID WHERE 1=1 ";

            if (userCode.Trim() != "")
            {
                sql += " AND WH_USERID = :p0 ";
                p.Add(":p0", string.Format("{0}", userCode));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_WHID>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<UR_ID> GetDetailAll(string initCode, string whCode, string whUna, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT * FROM UR_ID a WHERE 1=1 AND FL=1";

            if (initCode.Trim() != "")
            {
                sql += " AND INID like :p0 ";
                p.Add(":p0", string.Format("%{0}%", initCode));
            }

            if (whUna.Trim() != "")
            {
                sql += " AND UNA like :p4 ";
                p.Add(":p4", string.Format("%{0}%", whUna));
            }

            if ((getWhKind(whCode) == "0" && GetHospCode()=="0") || GetHospCode() != "0") // 三總庫房為藥品庫(或國軍醫院),則需剔除此庫房已選人員
            {
                sql += " and not exists (select 'x' from MI_WHID where WH_NO = :p1 and WH_USERID = a.TUSER) ";
                p.Add(":p1", string.Format("{0}", whCode));
            }
                
            sql += " ORDER BY INID, UNA ";
            

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<UR_ID>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<UR_ID> GetUserCode(string usrCode, string whCode, int page_index, int page_size, string sorters) 
        {
            var p = new DynamicParameters();

            var sql = @"SELECT * FROM UR_ID WHERE 1=1 ";

            if (usrCode.Trim() != "")
            {
                sql += " AND TUSER like :p0 ";
                p.Add(":p0", string.Format("%{0}%", usrCode));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<UR_ID>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<UR_ID> GetUserPopWindow(string usrCode, string usrNm, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT * FROM UR_ID WHERE 1=1 ";

            if (usrCode.Trim() == "" && usrNm.Trim() != "")
            {
                sql += " and UPPER(UNA) like :p3 ";
                /////sql += " or UPPER(UNA) like '%" + usrNm.ToUpper() + "%'";
                p.Add(":p3", string.Format("%{0}%", usrNm.ToUpper()));
            }else if (usrCode.Trim() != "" && usrNm.Trim() == "") { 
                sql += " AND UPPER(TUSER) like :p0 ";
                /////sql += " AND UPPER(TUSER) like '%" + usrCode.ToUpper() + "%'";
                p.Add(":p0", string.Format("%{0}%", usrCode.ToUpper()));
            }else if (usrCode.Trim() != "" && usrNm.Trim() != "") { 
                sql += " and ( UPPER(TUSER) like :p0 ";
                p.Add(":p0", string.Format("%{0}%", usrCode.ToUpper()));

                sql += " or UPPER(UNA) like :p3 ) ";
                p.Add(":p3", string.Format("%{0}%", usrNm.ToUpper()));
            }

            p.Add("OFFSET", (page_index - 1) * page_size); 
            p.Add("PAGE_SIZE", page_size);

           return DBWork.Connection.Query<UR_ID>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);

        }


        public IEnumerable<MI_WHMAST> GetWHMASTAll(string p0, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT * FROM MI_WHMAST WHERE 1=1 ";

            if (p0.Trim() != "")
            {
                sql += " AND WH_KIND = :p0 ";
                p.Add(":p0", string.Format("{0}", p0));
            }

            if (p1.Trim() != "")
            {
                sql += " AND WH_GRADE = :p1 ";
                p.Add(":p1", string.Format("{0}", p1));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_WHMAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_WHMAST> GetWHMASTByWH(string p0,int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            
            var sql = @"SELECT * FROM MI_WHMAST a WHERE 1=1 ";

            if (p0.Trim() != "")
            {
                sql += " AND WH_NO = :p0 ";
                p.Add(":p0", string.Format("{0}", p0));
            }
       
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_WHMAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_WHMAST> GetWHMASTByUser(string whid, string userid, string inid, string whname, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT * FROM MI_WHMAST a WHERE 1=1 ";

            if (whid.Trim() != "")
            {
                sql += " AND UPPER(WH_NO) like :p0 ";
                p.Add(":p0", string.Format("%{0}%", whid.ToUpper() ));
            }

            if (whname.Trim() != "")
            {
                sql += " AND WH_NAME like :p4";
                p.Add(":p4", string.Format("%{0}%", whname));
            }

            if (inid.Trim() != "")
            {
                sql += " AND INID = :p3 ";
                p.Add(":p3", string.Format("{0}", inid));
            }

            if (GetHospCode() != "0") // 若為國軍醫院需剔除此庫房已選人員
            {
                sql += "  and not exists(select 'x' from MI_WHID where WH_USERID = :p1 and WH_NO = a.WH_NO) ";
            }

            p.Add(":p1", string.Format("{0}", userid));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_WHMAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_WHMAST> GetWHMASTByPopupWindow(string p0, string p1, int page_index, int page_size, string sorters)
        {
            // for tab2 Detail Grid show data
            var p = new DynamicParameters();

            var sql = @"SELECT * FROM MI_WHMAST WHERE 1=1 ";

            if (p0.Trim() != "")
            {
                sql += " AND WH_KIND = :p0 ";
                p.Add(":p0", string.Format("{0}", p0));
            }

            if (p1.Trim() != "")
            {
                sql += " AND WH_GRADE = :p1 ";
                p.Add(":p1", string.Format("{0}", p1));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_WHMAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public int DeleteM(string wh_no, string wh_userid, string task_id) {
            var sql = "";
            
            var p = new DynamicParameters();
            
                
                sql = @"DELETE MI_WHID WHERE WH_NO = :p0 AND WH_USERID = :p1";
            if (task_id == string.Empty)
            {
                sql += "  and task_id is null";
            }
            else {
                sql += "    and task_id = :p2";
            }
                p.Add(":p0", string.Format("{0}", wh_no));
                p.Add(":p1", string.Format("{0}", wh_userid));
            p.Add(":p2", string.Format("{0}", task_id));

            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);

        }

        public int InsertM(string wh_no, string user_id, string create_user, string ip)
        {

            var TASK_ID = "";
            var CREATE_USER = DBWork.ProcUser;     // 登入者 ID
            var UPDATE_USER = DBWork.ProcUser;     // 登入者 ID
            var UPDATE_IP = DBWork.ProcIP;         // 登入者 IP
            string sql = string.Empty;
            sql = @"INSERT INTO MI_WHID (WH_NO, WH_USERID, TASK_ID, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP) ";
            sql += " VALUES (:WH_NO, :WH_USERID, :TASK_ID, sysdate, :CREATE_USER, sysdate, :UPDATE_USER, :UPDATE_IP)";

            var p = new DynamicParameters();

            p.Add(":WH_NO", string.Format("{0}", wh_no));
            p.Add(":WH_USERID", string.Format("{0}", user_id));
            p.Add(":TASK_ID", string.Format("{0}", TASK_ID));
            p.Add(":CREATE_USER", string.Format("{0}", create_user));
            p.Add(":UPDATE_USER", string.Format("{0}", create_user));
            p.Add(":UPDATE_IP", string.Format("{0}", ip));

            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public bool CheckUserTaskNullExists(string wh_no, string userid)
        {
            var sql = @"select 1 from MI_WHID where wh_no = :wh_no and wh_userid = :wh_userid and task_id is null";
            return !(DBWork.Connection.ExecuteScalar(sql,
                                                     new
                                                     {
                                                         wh_no = wh_no,
                                                         wh_userid = userid
                                                     },
                                                     DBWork.Transaction) == null);
        }

        //public IEnumerable<MI_WHMAST> GetWh_no(MI_WHMAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        //{
        //    var p = new DynamicParameters();
        //    string sql = @"SELECT 
        //                        A.WH_NO, 
        //                        A.WH_NAME,
        //                        (SELECT data_value || ' ' || data_desc 
        //                         FROM param_d
        //                         WHERE grp_code = 'MI_WHMAST' AND data_name = 'WH_KIND' and data_value = A.wh_kind) AS WH_KIND, 
        //                        (SELECT data_value || ' ' || data_desc 
        //                         FROM param_d
        //                         WHERE grp_code = 'MI_WHMAST' AND data_name = 'WH_GRADE' and data_value = A.wh_grade) AS WH_GRADE
        //                    FROM MI_WHMAST A 
        //                    WHERE 1=1 ";


        //    if (query.WH_NO != "")
        //    {
        //        sql += " AND A.WH_NO LIKE :WH_NO ";
        //        p.Add(":WH_NO", string.Format("%{0}%", query.WH_NO));
        //    }

        //    if (query.WH_NAME != "")
        //    {
        //        sql += " AND A.WH_NAME LIKE :WH_NAME ";
        //        p.Add(":WH_NAME", string.Format("%{0}%", query.WH_NAME));
        //    }

        //    p.Add("OFFSET", (page_index - 1) * page_size);
        //    p.Add("PAGE_SIZE", page_size);

        //    return DBWork.Connection.Query<MI_WHMAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        //}

        //public class MI_WHMAST_QUERY_PARAMS
        //{
        //    public string WH_NO;
        //    public string WH_NAME;
        //    public string WH_KIND;
        //    public string WH_GRADE;

        //    public string MMCODE;
        //    public string IS_INV;  // 需判斷庫存量>0
        //    public string E_IFPUBLIC;  // 是否公藥
        //}

        public IEnumerable<UR_ID> GetUsrNameCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.UNA, A.UENA, A.TUSER FROM UR_ID A WHERE 1=1 and fl = 1 ";


            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.UNA, :UNA_I), 1000) + NVL(INSTR(A.UENA, :UENA_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大                
                p.Add(":UNA_I", p0);
                p.Add(":UENA_I", p0);

                sql += " AND (A.UNA LIKE :UNA ";
                p.Add(":UNA", string.Format("%{0}%", p0));

                sql += " OR A.UENA LIKE :UENA ";
                p.Add(":UENA", string.Format("%{0}%", p0));

                sql += " OR A.tuser LIKE :UENA) ";
                p.Add(":UENA", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, UNA", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.UNA ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<UR_ID>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);

        }

        public IEnumerable<UR_ID> GetUsrCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.TUSER, (select INID_NAME from UR_INID B where A.INID=B.INID) as INID_NAME, A.INID, A.UNA FROM UR_ID A WHERE 1=1 ";

            if (p0 != "")
            {
                sql += " AND A.TUSER LIKE :TUSER ";
                p.Add(":TUSER", string.Format("%{0}%", p0));
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.TUSER ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<UR_ID>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetTaskIdCombo()
        {
            string sql = @" SELECT  DATA_VALUE||' '||DATA_DESC AS TEXT, DATA_VALUE AS VALUE
                            FROM PARAM_D 
                            WHERE GRP_CODE = 'MI_WHID' AND DATA_NAME = 'TASK_ID'
                            and DATA_VALUE in ('2', '3', '4', '5', '6')
                            ORDER BY VALUE ";

            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }

        public int UpdateTaskId(MI_WHID mi_whid)
        {
            var sql = @"UPDATE MI_WHID SET TASK_ID = :TASK_ID
                        WHERE WH_NO = :WH_NO AND WH_USERID = :WH_USERID";
            if (mi_whid.ORI_TASK_ID == null)
            {
                sql += " and task_id is null";
            }
            else {
                sql += " and task_id  = :ori_task_id";
            }
            
            return DBWork.Connection.Execute(sql, mi_whid, DBWork.Transaction);
        }

        public bool getChkTuser(string chkVal)
        {
            var sql = @"SELECT 1 FROM UR_ID WHERE TUSER = :CHKVAL or UNA = :CHKVAL";
            return !(DBWork.Connection.ExecuteScalar(sql, new { CHKVAL = chkVal }, DBWork.Transaction) == null);
        }

        public bool getChkWhNo(string chkVal)
        {
            var sql = @"SELECT 1 FROM MI_WHMAST WHERE WH_NO = :CHKVAL or WH_NAME = :CHKVAL";
            return !(DBWork.Connection.ExecuteScalar(sql, new { CHKVAL = chkVal }, DBWork.Transaction) == null);
        }

        public bool CheckTaskIdExist(string taskid)
        {
            var sql = @"SELECT 1 FROM MI_WHID WHERE TASK_ID = :TASK_ID";
            return !(DBWork.Connection.ExecuteScalar(sql, new { TASK_ID = taskid }, DBWork.Transaction) == null);
        }

        public bool CheckUserTaskExists(string wh_no, string userid, string task_id) {
            var sql = @"select 1 from MI_WHID where wh_no = :wh_no and wh_userid = :wh_userid and task_id = :task_id";
            return !(DBWork.Connection.ExecuteScalar(sql,
                                                     new
                                                     {
                                                         wh_no = wh_no, wh_userid = userid, task_id = task_id
                                                     },
                                                     DBWork.Transaction) == null);
        }

        public string getWhKind(string wh_no)
        {
            var sql = @"select WH_KIND from MI_WHMAST where WH_NO = :WH_NO ";
            return DBWork.Connection.ExecuteScalar(sql, new { WH_NO = wh_no }, DBWork.Transaction).ToString();
        }

        public string GetHospCode()
        {
            string sql = @"
                select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode'
            ";
            return DBWork.Connection.ExecuteScalar(sql, null, DBWork.Transaction).ToString();
        }
    }
}