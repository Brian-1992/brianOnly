using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models.C;
using System.Collections.Generic;
using TSGH.Models;
using WebApp.Models;

namespace WebApp.Repository.C
{

    public class CE0022Repository : JCLib.Mvc.BaseRepository
    {
        public CE0022Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<CE0022> GetAll(string wh_no, string date, string userId, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT CHK_WH_NO, 
(SELECT WH_NAME FROM MI_WHMAST WHERE WH_NO = a.CHK_WH_NO) as WH_NAME, 
a.CHK_YM, 
a.CHK_WH_GRADE as CHK_WH_GRADE_CODE,
a.CHK_WH_KIND as CHK_WH_KIND_CODE,
a.CHK_PERIOD as CHK_PERIOD_CODE,
a.CHK_TYPE as CHK_TYPE_CODE,
a.CHK_STATUS as CHK_STATUS_CODE,
a.CREATE_USER as CREATE_USER,
(SELECT DISTINCT  DATA_DESC as COMBITEM FROM PARAM_D 
WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_WH_GRADE' AND DATA_VALUE =A.CHK_WH_GRADE) CHK_WH_GRADE, 
(SELECT DISTINCT  DATA_DESC as COMBITEM FROM PARAM_D 
WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_WH_KIND' AND DATA_VALUE =A.CHK_WH_KIND) CHK_WH_KIND, 
(SELECT DISTINCT  DATA_DESC as COMBITEM FROM PARAM_D 
WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_PERIOD' AND DATA_VALUE =A.CHK_PERIOD) CHK_PERIOD, 
a.CHK_TYPE,a.CHK_NUM || '/' || a.CHK_TOTAL as MERGE_NUM_TOTAL, a.CHK_NO, 
(SELECT UNA FROM UR_ID WHERE TUSER = a.CHK_KEEPER) CHK_KEEPER, 
(SELECT DISTINCT  DATA_DESC as COMBITEM FROM PARAM_D 
WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_STATUS' AND DATA_VALUE =A.CHK_STATUS) CHK_STATUS, 
(select UPDN_STATUS from CHK_GRADE2_UPDN where CHK_NO = a.chk_no and chk_uid = :p2) as updn_status 
from CHK_MAST a,CHK_GRADE2_UPDN b WHERE 1=1 ";
            sql += " AND CHK_LEVEL = '2' AND a.chk_no =b.chk_no AND a.CHK_YM =b.CHK_YM and b.chk_uid = :p2";

            if (wh_no != "" && wh_no != null)
            {
                sql += " AND a.chk_wh_no LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", wh_no));
            }
            if (date != "" && date != null)
            {
                sql += " AND a.chk_ym like :p1 ";
                p.Add(":p1", string.Format("%{0}%", date));
                //p.Add(":p1", date);
            }
            sql += @"and a.chk_wh_grade = '2' and a.chk_wh_kind = '0' ";

            p.Add(":p2", userId);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CE0022>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //GET T2Grid
        public IEnumerable<CHK_CE0022VM> GetAllINI(string chk_no, string userId, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select distinct  c.MMCODE, c.MMNAME_C,c.MMNAME_E,c.BASE_UNIT,a.CHK_QTY, 
(select TWN_DATE(chk_pre_date) from CHK_G2_WHINV where chk_no = :chk_no and mmcode = a.mmcode) as chk_pre_date,
(select una from UR_ID where tuser = a.chk_uid) as chk_uid_name, 
a.CHK_TIME,b.updn_status from CHK_G2_DETAIL a, CHK_GRADE2_UPDN b,MI_MAST c  ";

            sql += "where c.mmcode = a.mmcode and a.CHK_NO = b.CHK_NO and a.chk_no = :chk_no and a.chk_uid = :chk_uid and b.chk_uid = a.chk_uid Order by c.mmcode";

            p.Add(":chk_no", chk_no);
            p.Add(":chk_uid", userId);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CHK_CE0022VM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public int UpdateGrade2UpDn(string chk_no, string chk_uid, string updn_status, string update_ip)
        {
            string sql = @"update chk_grade2_updn
                              set updn_status = :updn_status";
            if (updn_status == "2")
            {
                sql += @"         , dn_date = sysdate";
            }
            else if (updn_status == "3")
            {
                sql += @"         , up_date = sysdate";
            }
            sql += @"             , update_time = sysdate,
                                    update_ip = :update_ip,
                                    update_user = :chk_uid
                             where chk_no = :chk_no
                               and chk_uid = :chk_uid";
            return DBWork.Connection.Execute(sql, new
            {
                chk_no = chk_no,
                chk_uid = chk_uid,
                updn_status = updn_status,
                update_ip = update_ip
            }, DBWork.Transaction);
        }

        public DataTable GetExcel(string chk_no, string userId)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"select  distinct 
                                   (select seq from CHK_G2_WHINV where chk_no = a.chk_no and mmcode = a.mmcode) as 項次,
                                   a.CHK_NO as 盤點單號, 
                                   c.MMCODE as 院內碼, 
                                   c.MMNAME_C as 中文品名, 
                                   c.MMNAME_E as 英文品名, 
                                   c.base_unit as 計量單位,
                                   '' as 藥槽號, 
                                   '' as 盤點數量, 
                                   '' as 地點1, 
                                   '' as 地點2, 
                                   '' as 地點3, 
                                   '' as 地點4, 
                                   '' as 地點5, 
                                   '' as 地點6, 
                                   a.chk_uid as 盤點人員, 
(select TWN_DATE(chk_pre_date) from CHK_G2_WHINV where chk_no = :chk_no and mmcode = a.mmcode) as 預計盤點日期,
                                   TWN_TIME(a.CHK_TIME) as 盤點時間 
                             from  CHK_G2_DETAIL a, CHK_GRADE2_UPDN b,MI_MAST c 
                             where c.mmcode = a.mmcode and a.CHK_NO = b.CHK_NO and a.chk_no = :chk_no and a.chk_uid = :chk_uid 
                             order by c.mmcode";

            p.Add(":chk_no", chk_no);
            p.Add(":chk_uid", userId);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public bool CheckExistsS(string wh_no, string mmcode)
        {
            string sql = @"select 1 from CHK_G2_WHINV a 
                            where a.wh_no = :wh_no 
                              and a.mmcode = :mmcode";
            return !(DBWork.Connection.ExecuteScalar(sql,
                                                     new
                                                     {
                                                         wh_no = wh_no,
                                                         mmcode = mmcode,
                                                     },
                                                     DBWork.Transaction) == null);
        }
        public bool CheckDetailSameValue(string chk_no, string mmcode, string chk_qty, string chk_uid)
        {
            string sql = @"select 1 from CHK_G2_DETAIL a 
                            where a.chk_no = :chk_no 
                              and a.mmcode = :mmcode
                              and a.chk_uid = :chk_uid
                              and a.chk_qty = :chk_qty";

            return !(DBWork.Connection.ExecuteScalar(sql,
                                                     new
                                                     {
                                                         chk_no = chk_no,
                                                         mmcode = mmcode,
                                                         chk_uid = chk_uid,
                                                         chk_qty = chk_qty
                                                     },
                                                     DBWork.Transaction) == null);
        }
        public int UpdateCHK_G2_DETAIL(CHK_DETAIL_TEMP temp)
        {
            string sql = @"update CHK_G2_DETAIL set store_loc = :store_loc||:chk_uid, chk_qty = :chk_qty, chk_time = sysdate ,UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP 
                            where chk_no = :chk_no and mmcode = :mmcode and chk_uid = :chk_uid";
            return DBWork.Connection.Execute(sql, temp, DBWork.Transaction);
        }

        public int UpdateCHK_G2_DETAIL_Save(string CHK_QTY, string CHK_NO, string MMCODE, string userid,string userIp)
        {
            var p = new DynamicParameters();

            string sql = @"update CHK_G2_DETAIL set  chk_qty = :chk_qty, chk_time = sysdate , UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP 
                            where chk_no = :chk_no and mmcode = :mmcode and chk_uid = :chk_uid";

            p.Add(":chk_qty", CHK_QTY);
            p.Add(":chk_no", CHK_NO);
            p.Add(":mmcode", MMCODE);
            p.Add(":chk_uid", userid);
            p.Add(":UPDATE_USER", userid);
            p.Add(":UPDATE_IP", userIp);


            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }


        //INI Query UPDN_STATUS

        public IEnumerable<CE0022> UPDN_STATUSGet(string chk_no)
        {
            var p = new DynamicParameters();

            var sql = @"select UPDN_STATUS from chk_grade2_updn where 1=1 ";

            if (chk_no != "" && chk_no != null)
            {
                sql += " AND chk_no = :p0 ";
                p.Add(":p0", chk_no);
            }

            return DBWork.Connection.Query<CE0022>(sql, p, DBWork.Transaction);
        }


        public IEnumerable<CE0022> Get(string chk_no, string mmocde, string store_loc)
        {
            var sql = @"SELECT * FROM CHK_DETAIL WHERE chk_no = :CHK_NO and mmcode = :MMCODE and store_loc = :STORE_LOC";
            return DBWork.Connection.Query<CE0022>(sql, new { CHK_NO = chk_no, MMCODE = mmocde, STORE_LOC = store_loc }, DBWork.Transaction);
        }

        public string SelectCountFinalPro(string chk_no, string userId)
        {
            var sql = @"Select count(*)  as COUNT FROM CHK_G2_DETAIL WHERE chk_no = :CHK_NO and chk_uid = :CHK_UID and CHK_QTY is NULL";
            return DBWork.Connection.ExecuteScalar(sql, new { CHK_NO = chk_no, CHK_UID = userId }, DBWork.Transaction).ToString();
        }
        public IEnumerable<CE0022> SelectFinalPro(string chk_no, string userId)
        {
            var sql = @"Select * FROM CHK_G2_DETAIL WHERE chk_no = :CHK_NO and chk_uid = :CHK_UID and CHK_QTY is NULL";
            return DBWork.Connection.Query<CE0022>(sql, new { CHK_NO = chk_no, CHK_UID = userId }, DBWork.Transaction);
        }

        public int UpdateChk_detail(string chk_no, string userId, string userIp)
        {
            var sql = @"UPDATE CHK_DETAIL SET STATUS_INI = :STATUS_INI, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP 
                                WHERE CHK_NO = :CHK_NO AND CHK_UID = :CHK_UID";
            return DBWork.Connection.Execute(sql, new { STATUS_INI = "2", UPDATE_USER = userId, UPDATE_IP = userIp, CHK_NO = chk_no, CHK_UID = userId }, DBWork.Transaction);
        }

        public int UpdateChk_g2_Status(string chk_no, string userId, string userIp)
        {
            var sql = @"Update chk_g2_detail set STATUS_INI = '2', UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP  where chk_no = :chk_no and chk_uid = :chk_uid";
            return DBWork.Connection.Execute(sql, new { chk_no = chk_no, chk_uid = userId, UPDATE_USER = userId, UPDATE_IP = userIp, CHK_NO2 = chk_no }, DBWork.Transaction);
        }

        public int UpdateChk_mast(string chk_no, string userId, string userIp)
        {
            var sql = @"Update chk_mast  set CHK_NUM = CHK_NUM  + 1, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP 
                       where CHK_NO = :CHK_NO2";
            return DBWork.Connection.Execute(sql, new { CHK_NO = chk_no, CHK_UID = userId, UPDATE_USER = userId, UPDATE_IP = userIp, CHK_NO2 = chk_no }, DBWork.Transaction);
        }

        public int UpdateChk_mast_STATUS(string chk_no, string userId, string userIp)
        {
            var sql = @"UPDATE chk_mast SET CHK_STATUS = :CHK_STATUS, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP 
                                WHERE CHK_NO = :CHK_NO";
            return DBWork.Connection.Execute(sql, new { CHK_STATUS = "2", UPDATE_USER = userId, UPDATE_IP = userIp, CHK_NO = chk_no }, DBWork.Transaction);
        }

        public string SelectChk_mast(string chk_no)
        {
            var sql = @"SELECT CHK_TOTAL || ',' || CHK_NUM as CHKNum from chk_mast Where CHK_NO = :CHK_NO";
            return DBWork.Connection.ExecuteScalar(sql, new { CHK_NO = chk_no }, DBWork.Transaction).ToString();
        }

        public CHK_MAST GetMast(string chk_no)
        {
            string sql = @"select * from CHK_MAST where chk_no = :chk_no";
            return DBWork.Connection.QueryFirst<CHK_MAST>(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }

        public int UpdateChkmastMed(string chk_no, string userId, string userIp)
        {
            string sql = @"update chk_mast 
                              set chk_num = chk_num + 1, 
                                  update_time = sysdate, 
                                  update_user = :userId, 
                                  update_ip = :user_ip
                            where chk_no = :chk_no";
            return DBWork.Connection.Execute(sql, new { chk_no = chk_no, userId = userId, user_ip = userIp }, DBWork.Transaction);
        }

        public int UpdateMastWhoDo(string chk_no, string userId, string createuser)
        {
            var sql = @"UPDATE chk_mast  SET chk_keeper = :chk_keeper, create_user = :create_user WHERE CHK_NO = :CHK_NO";
            return DBWork.Connection.Execute(sql, new { chk_keeper = userId, create_user = createuser, CHK_NO = chk_no }, DBWork.Transaction);
        }
        public int UpdateDetailWhoDo(string chk_no, string userId)
        {
            var sql = @"UPDATE CHK_DETAIL SET CHK_UID = :CHK_UID  WHERE CHK_NO = :CHK_NO ";
            return DBWork.Connection.Execute(sql, new { CHK_UID = userId, CHK_NO = chk_no }, DBWork.Transaction);
        }
    }

}