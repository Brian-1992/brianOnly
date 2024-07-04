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

    public class CE0003Repository : JCLib.Mvc.BaseRepository
    {
        public CE0003Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<CE0003> GetAll(string wh_no, string date, string userId, int page_index, int page_size, string sorters)
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
                               from CHK_MAST a WHERE 1=1 ";
            sql += " AND CHK_LEVEL = '1' ";

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
            sql += @"and  chk_no  in  (  select DISTINCT chk_no from CHK_DETAIL where CHK_UID =  :p2 
                     union 
                     select distinct chk_no from CHK_GRADE2_UPDN where  CHK_UID =  :p2 
                     union
                        select distinct chk_no from CHK_NOUID where chk_uid = :p2
                     union 
                     select chk_no  from  chk_mast where CHK_KEEPER  in ( select  distinct inid  from ur_id  where tuser  =  :p2))";
            //藥庫 union這個人的責任中心 MI_WHMAST INID; 人的UR_ID TUSER INID欄位庫房代碼與此盤點單的庫房代碼的類別 (master部份)

            p.Add(":p2", userId);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CE0003>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<CE0003> GetChkData(string wh_no, string date, string chk_no, string barcode, string sort_by, string sort_order, string userId, string ischk)
        {
            var p = new DynamicParameters();

            var sql = @" Select * from CHK_DETAIL 
                            where chk_no = :chk_no ";

            if (userId != "" && userId != null)
                sql += " and (chk_uid = :userid or exists(select 1 from CHK_NOUID where chk_no = :chk_no and chk_uid = :userid)) ";

            if (ischk == "Y")
                sql += " and chk_time is not null ";
            else if (ischk == "N")
                sql += " and chk_time is null ";

            //if (grid_sort == "1")
            //    sql += " order by MMCODE ";
            //else if (grid_sort == "2")
            //    sql += " order by STORE_LOC ";
            string other_sort_column = sort_by.ToUpper() == "STORE_LOC" ? "MMCODE" : (sort_by.ToUpper() == "MMCODE" ? "STORE_LOC" : "STORE_LOC");
            if (wh_no == "PH1S") {
                if (sort_by.ToUpper() == "STORE_LOC") {
                    sort_by = "substr(nvl(STORE_LOC, ''), 3, (length(nvl(STORE_LOC, ''))-2))";
                }
                if (other_sort_column.ToUpper() == "STORE_LOC") {
                    other_sort_column = "substr(nvl(STORE_LOC, ''), 3, (length(nvl(STORE_LOC, ''))-2))";
                }
            }
            sql += string.Format("       order by {0} {1}, {2} asc", sort_by, sort_order, other_sort_column);
            p.Add(":chk_no", chk_no);
            p.Add(":userid", userId);

            return DBWork.PagingQuery<CE0003>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<CE0003> GetChkBarcodeData(string barcode, string chk_no, string userId, string grid_sort)
        {
            var p = new DynamicParameters();

            var sql = string.Format(@" Select * from CHK_DETAIL 
                        where chk_no = :chk_no 
                        {0}
                        and (mmcode = :barcode 
                        or store_loc like :store_loc 
                        or mmcode in (select mmcode from BC_BARCODE where barcode = :barcode))", 
                        userId != string.Empty ? "and chk_uid = :userid" : string.Empty);
            // and chk_time is null 掃描條碼已盤和未盤都可掃

            if (grid_sort == "1")
                sql += " order by MMCODE ";
            else if (grid_sort == "2")
                sql += " order by STORE_LOC ";

            p.Add(":barcode", barcode);
            p.Add(":store_loc", string.Format("%0{0}%", barcode));
            p.Add(":chk_no", chk_no);
            p.Add(":userid", userId);

            return DBWork.PagingQuery<CE0003>(sql, p, DBWork.Transaction);
        }
        

        //GET T2Grid
        public IEnumerable<CE0003> GetAllINIPDA(string chk_no, string mmcodeorstore, string userId, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select a.CHK_NO, 
                               a.WH_NO as CHK_WH_NO, 
                                a.mmcode, a.MMNAME_C, a.MMNAME_E, a.STORE_LOC,
                               a.LOC_NAME, a.BASE_UNIT, a.STORE_QTYC, 
                              (case
                                                    when (a.wh_no = 'PH1S' and a.chk_time is null)
                                                        then store_qtyc
                                                    else CHK_QTY
                                                  end) as CHK_QTY,
                                (case
                                                    when (a.chk_time is null)
                                                        then ' '
                                                    else TO_CHAR(a.CHK_QTY - a.store_qtyc)
                                                  end) as DIFF_QTY,
                                 a.chk_uid, 
                                (select una from UR_ID where tuser = a.chk_uid) as chk_uid_name,
                                a.CHK_REMARK, a.CHK_TIME, a.STATUS_INI  FROM CHK_DETAIL a WHERE 1=1 ";

            if (chk_no != "" && chk_no != null)
            {
                sql += " AND a.chk_no = :p0 ";
                p.Add(":p0", chk_no);
            }
            //if (userId != "" && userId != null)
            //{
            //    sql += " AND CHK_UID = :p1 ";
            //    p.Add(":p1", userId);
            //}

            if (mmcodeorstore != "" && mmcodeorstore != null)
            {
                sql += " AND ( a.MMCODE LIKE :p2 OR a.STORE_LOC LIKE :p2 ) ";
                p.Add(":p2", string.Format("%{0}%", mmcodeorstore));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CE0003>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //GET T2Grid
        public IEnumerable<CE0003> GetSign(string chk_no, string userId, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select CHK_NO, 
                               WH_NO as CHK_WH_NO, 
                                mmcode,MMNAME_C,MMNAME_E,STORE_LOC,LOC_NAME,BASE_UNIT,STORE_QTYC, 
                              (case
                                                    when (wh_no = 'PH1S' and chk_time is null)
                                                        then store_qtyc
                                                    else CHK_QTY
                                                  end) as CHK_QTY,
                                (case
                                                    when (chk_time is null)
                                                        then ' '
                                                    else TO_CHAR(CHK_QTY - store_qtyc)
                                                  end) as DIFF_QTY,
                                CHK_REMARK, CHK_TIME,STATUS_INI  FROM CHK_DETAIL WHERE 1=1 ";

            if (chk_no != "" && chk_no != null)
            {
                sql += " AND chk_no = :p0 ";
                p.Add(":p0", chk_no);
            }
            if (userId != "" && userId != null)
            {
                sql += " AND CHK_UID = :p1 ";
                p.Add(":p1", userId);
            }
            sql += "AND CHK_TIME is not null";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CE0003>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //GET T2Grid
        public IEnumerable<CE0003> GetNoSign(string chk_no, string userId, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select CHK_NO, 
                               WH_NO as CHK_WH_NO, 
                                mmcode,MMNAME_C,MMNAME_E,STORE_LOC,LOC_NAME,BASE_UNIT,STORE_QTYC, 
                              (case
                                                    when (wh_no = 'PH1S' and chk_time is null)
                                                        then store_qtyc
                                                    else CHK_QTY
                                                  end) as CHK_QTY,
                                (case
                                                    when (chk_time is null)
                                                        then ' '
                                                    else TO_CHAR(CHK_QTY - store_qtyc)
                                                  end) as DIFF_QTY,
                                CHK_REMARK, CHK_TIME,STATUS_INI  FROM CHK_DETAIL WHERE 1=1 ";

            if (chk_no != "" && chk_no != null)
            {
                sql += " AND chk_no = :p0 ";
                p.Add(":p0", chk_no);
            }
            if (userId != "" && userId != null)
            {
                sql += " AND CHK_UID = :p1 ";
                p.Add(":p1", userId);
            }
            sql += "AND CHK_TIME is null";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CE0003>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //GET T2Grid
        public IEnumerable<CE0003> GetAllINIotherPDA(string chk_no, string mmcodeorstore, string userId, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select CHK_NO, 
                               WH_NO as CHK_WH_NO, 
                                mmcode,MMNAME_C,MMNAME_E,STORE_LOC,LOC_NAME,BASE_UNIT,STORE_QTYC, 
                              (case
                                                    when (wh_no = 'PH1S' and chk_time is null)
                                                        then store_qtyc
                                                    else CHK_QTY
                                                  end) as CHK_QTY,
                                (case
                                                    when (chk_time is null)
                                                        then ' '
                                                    else TO_CHAR(CHK_QTY - store_qtyc)
                                                  end) as DIFF_QTY,
                                CHK_REMARK, CHK_TIME,STATUS_INI  FROM CHK_DETAIL WHERE 1=1 ";

            if (chk_no != "" && chk_no != null)
            {
                sql += " AND chk_no = :p0 ";
                p.Add(":p0", chk_no);
            }
            if (userId != "" && userId != null)
            {
                sql += " AND CHK_UID = :p1 ";
                p.Add(":p1", userId);
            }
            if (mmcodeorstore != "" && mmcodeorstore != null)
            {
                sql += " AND ( MMCODE LIKE :p2 OR STORE_LOC LIKE :p2 ) ";
                p.Add(":p2", string.Format("%{0}%", mmcodeorstore));
            }
            sql += "AND CHK_TIME is null";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CE0003>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        //GET ifAutoLoad
        public IEnumerable<CE0003> GetAllINIAutoLoadPDA(string chk_no, string mmcodeorstore, string userId)
        {
            var p = new DynamicParameters();

            var sql = @"select CHK_NO, mmcode,MMNAME_C,MMNAME_E,STORE_LOC,LOC_NAME,BASE_UNIT,STORE_QTYC,CHK_QTY,CHK_REMARK, CHK_TIME,STATUS_INI  FROM CHK_DETAIL WHERE 1=1 ";

            if (chk_no != "" && chk_no != null)
            {
                sql += " AND chk_no = :p0 ";
                p.Add(":p0", chk_no);
            }
            if (userId != "" && userId != null)
            {
                sql += " AND CHK_UID = :p1 ";
                p.Add(":p1", userId);
            }

            if (mmcodeorstore != "" && mmcodeorstore != null)
            {
                sql += " AND ( MMCODE LIKE :p2 OR STORE_LOC LIKE :p2 ) ";
                p.Add(":p2", string.Format("%{0}%", mmcodeorstore));
            }
            sql += "AND CHK_TIME is null Order by STORE_LOC ASC";

            return DBWork.Connection.Query<CE0003>(sql, p, DBWork.Transaction);
        }

        public string GetUpdnStatus(string chk_no)
        {
            var p = new DynamicParameters();

            var sql = @"select UPDN_STATUS from chk_grade2_updn where chk_no = :chk_no ";

            return DBWork.Connection.QueryFirst<string>(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }

        //INI Query UPDN_STATUS

        public IEnumerable<CE0003> UPDN_STATUSGet(string chk_no)
        {
            var p = new DynamicParameters();

            var sql = @"select UPDN_STATUS from chk_grade2_updn where 1=1 ";

            if (chk_no != "" && chk_no != null)
            {
                sql += " AND chk_no = :p0 ";
                p.Add(":p0", chk_no);
            }

            return DBWork.Connection.Query<CE0003>(sql, p, DBWork.Transaction);
        }



        public int UpdateCE0003_INI(CE0003 ce0003)
        {
            var sql = @"UPDATE CHK_DETAIL SET CHK_QTY = :CHK_QTY, CHK_REMARK = :CHK_REMARK,CHK_UID = :UPDATE_USER, CHK_TIME = SYSDATE, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP 
                                WHERE CHK_NO = :CHK_NO AND MMCODE = :MMCODE AND STORE_LOC = :STORE_LOC";
            return DBWork.Connection.Execute(sql, ce0003, DBWork.Transaction);
        }

        public int UpdateCE0003_INIPC(string CHK_QTY, string CHK_REMARK, string user, string ip, string CHK_NO, string MMCODE, string STORE_LOC)
        {
            var p = new DynamicParameters();

            var sql = @"UPDATE CHK_DETAIL SET CHK_QTY = :CHK_QTY, CHK_REMARK = :CHK_REMARK, CHK_TIME = SYSDATE, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE CHK_NO = :CHK_NO AND MMCODE = :MMCODE AND STORE_LOC = :STORE_LOC";
            p.Add(":CHK_QTY", CHK_QTY);
            p.Add(":CHK_REMARK", CHK_REMARK);
            p.Add(":UPDATE_USER", user);
            p.Add(":UPDATE_IP", ip);
            p.Add(":CHK_NO", CHK_NO);
            p.Add(":MMCODE", MMCODE);
            p.Add(":STORE_LOC", STORE_LOC);


            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public int UpdateCE0003_INI_NOTIME_PC(string CHK_QTY, string CHK_REMARK, string user, string ip, string CHK_NO, string MMCODE, string STORE_LOC)
        {
            var p = new DynamicParameters();

            var sql = @"UPDATE CHK_DETAIL SET CHK_QTY = :CHK_QTY, CHK_REMARK = :CHK_REMARK, CHK_TIME = :CHK_TIME, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE CHK_NO = :CHK_NO AND MMCODE = :MMCODE AND STORE_LOC = :STORE_LOC";
            p.Add(":CHK_TIME", "");
            p.Add(":CHK_QTY", CHK_QTY);
            p.Add(":CHK_REMARK", CHK_REMARK);
            p.Add(":UPDATE_USER", user);
            p.Add(":UPDATE_IP", ip);
            p.Add(":CHK_NO", CHK_NO);
            p.Add(":MMCODE", MMCODE);
            p.Add(":STORE_LOC", STORE_LOC);


            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public IEnumerable<CE0003> Get(string chk_no, string mmocde, string store_loc)
        {
            var sql = @"SELECT * FROM CHK_DETAIL WHERE chk_no = :CHK_NO and mmcode = :MMCODE and store_loc = :STORE_LOC";
            return DBWork.Connection.Query<CE0003>(sql, new { CHK_NO = chk_no, MMCODE = mmocde, STORE_LOC = store_loc }, DBWork.Transaction);
        }

        public string SelectCountFinalPro(string chk_no)
        {
            var sql = @"Select count(*)  as COUNT FROM CHK_DETAIL WHERE chk_no = :CHK_NO and chk_time is NULL";
            return DBWork.Connection.ExecuteScalar(sql, new { CHK_NO = chk_no}, DBWork.Transaction).ToString();
        }
        public string SelectCountFinalPro(string chk_no, string userId)
        {
            var sql = @"Select count(*)  as COUNT FROM CHK_DETAIL WHERE chk_no = :CHK_NO and chk_uid = :CHK_UID and chk_time is NULL";
            return DBWork.Connection.ExecuteScalar(sql, new { CHK_NO = chk_no, CHK_UID = userId }, DBWork.Transaction).ToString();
        }
        public IEnumerable<CE0003> SelectFinalPro(string chk_no, string userId)
        {
            var sql = @"Select * FROM CHK_DETAIL WHERE chk_no = :CHK_NO and chk_uid = :CHK_UID and chk_time is NULL";
            return DBWork.Connection.Query<CE0003>(sql, new { CHK_NO = chk_no, CHK_UID = userId }, DBWork.Transaction);
        }
        public IEnumerable<CE0003> SelectCountSign(string chk_no) //已盤
        {
            var sql = @"Select count(*)  as sign FROM CHK_DETAIL WHERE chk_no = :CHK_NO and chk_time is not NULL";
            return DBWork.Connection.Query<CE0003>(sql, new { CHK_NO = chk_no }, DBWork.Transaction);
        }
        public IEnumerable<CE0003> SelectCountNoSign(string chk_no) //未盤
        {
            var sql = @"Select count(*)  as nosign FROM CHK_DETAIL WHERE chk_no = :CHK_NO and chk_time is NULL";
            return DBWork.Connection.Query<CE0003>(sql, new { CHK_NO = chk_no }, DBWork.Transaction);
        }

        public IEnumerable<CE0003> GetMsgCount(string chk_no)
        {
            var sql = @"Select a.chk_total,
                                (select count(*) from CHK_DETAIL where chk_no = a.chk_no and chk_time is null) as NOSIGN
                                From CHK_MAST a where a.chk_no = :CHK_NO ";
            return DBWork.Connection.Query<CE0003>(sql, new { CHK_NO = chk_no }, DBWork.Transaction);
        }

        public int UpdateChk_detail(string chk_no, string userId, string userIp)
        {
            var sql = @"UPDATE CHK_DETAIL SET STATUS_INI = :STATUS_INI, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP 
                                WHERE CHK_NO = :CHK_NO AND CHK_UID = :CHK_UID";
            return DBWork.Connection.Execute(sql, new { STATUS_INI = "2", UPDATE_USER = userId, UPDATE_IP = userIp, CHK_NO = chk_no, CHK_UID = userId }, DBWork.Transaction);
        }

        public int UpdateDetailStat(string chk_no, string chk_status, string userId, string userIp)
        {
            var sql = @"UPDATE CHK_MAST SET CHK_STATUS = :CHK_STATUS, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP 
                                WHERE CHK_NO = :CHK_NO ";
            return DBWork.Connection.Execute(sql, new { CHK_STATUS = chk_status, UPDATE_USER = userId, UPDATE_IP = userIp, CHK_NO = chk_no }, DBWork.Transaction);
        }

        public int UpdateMastStat(string chk_no, string status_ini,string userId, string userIp)
        {
            var sql = @"UPDATE CHK_DETAIL SET STATUS_INI = :STATUS_INI, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP 
                                WHERE CHK_NO = :CHK_NO ";
            return DBWork.Connection.Execute(sql, new { STATUS_INI = status_ini, UPDATE_USER = userId, UPDATE_IP = userIp, CHK_NO = chk_no }, DBWork.Transaction);
        }

        public int UpdateChk_mastCHK_NUM(string chk_no, string userId, string userIp)
        {
            var sql = @"Update chk_mast  set CHK_NUM = :CHK_NUM, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP 
                       where CHK_NO = :CHK_NO2";
            return DBWork.Connection.Execute(sql, new { CHK_NUM = 0, UPDATE_USER = userId, UPDATE_IP = userIp, CHK_NO2 = chk_no }, DBWork.Transaction);
        }
        public int UpdateChk_mast(string chk_no, string userId, string userIp)
        {
            var sql = @"Update chk_mast  set CHK_NUM = CHK_NUM  + ( Select count(*)  from chk_detail Where CHK_NO = :CHK_NO AND STATUS_INI = '2'), UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP 
                       where CHK_NO = :CHK_NO2";
            return DBWork.Connection.Execute(sql, new { CHK_NO = chk_no, UPDATE_USER = userId, UPDATE_IP = userIp, CHK_NO2 = chk_no }, DBWork.Transaction);
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

        public IEnumerable<MI_WHMAST> GetWhnoCombo(string chk_ym, string chk_level, string chk_period, string userid)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.WH_NO, A.WH_NAME, A.WH_KIND, A.WH_GRADE FROM MI_WHMAST A
                        WHERE A.WH_NO in 
                        (
                        (
                        select chk_wh_no from CHK_MAST b where b.chk_ym = :chk_ym and b.chk_level = :chk_level and b.chk_period = :chk_period
                        and exists(select 1 from CHK_NOUID where chk_no = b.chk_no and chk_uid = :chk_uid)
                        )
                        union 
                        (
                        select chk_wh_no from CHK_MAST b where b.chk_ym = :chk_ym and b.chk_level = :chk_level and b.chk_period = :chk_period
                        and exists(select 1 from CHK_DETAIL where chk_no = b.chk_no and chk_uid = :chk_uid)
                        )
                        )
                       ORDER BY A.WH_NO";

            p.Add("chk_uid", userid);
            p.Add("chk_ym", chk_ym);
            p.Add("chk_level", chk_level);
            p.Add("chk_period", chk_period);

            return DBWork.Connection.Query<MI_WHMAST>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<CE0003> GetChknoCombo(string wh_no, string chk_ym, string chk_level, string chk_period,  string userId, string chk_type)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT
                        a.chk_no, 
                        a.chk_period, 
                        a.chk_ym, a.chk_wh_kind, a.chk_type, a.chk_class, a.chk_status, a.CHK_WH_GRADE,
                        (select data_desc from PARAM_D where grp_code = 'CHK_MAST' and data_name = 'CHK_STATUS'  and data_value = a.chk_status) as chk_status_name,
                        (select count(*) from CHK_NOUID where chk_no = a.chk_no) as chk_uid_count
                        from CHK_MAST a
                        where a.chk_level = :p3
                          and a.chk_no  in  
                                (select DISTINCT chk_no from CHK_DETAIL where CHK_UID = :p2 
                                    union 
                                 select distinct chk_no from CHK_NOUID where chk_uid = :p2) ";

            if (wh_no != "" && wh_no != null)
            {
                sql += " AND a.chk_wh_no = :p0 ";
                p.Add(":p0", wh_no);
            }
            if (chk_ym != "" && chk_ym != null)
            {
                sql += " AND a.chk_ym = :p1 ";
                p.Add(":p1", string.Format("{0}", chk_ym));
            }
            if (chk_type != "" && chk_type != null)
            {
                sql += " AND a.chk_type = :p4 ";
                p.Add(":p4", string.Format("{0}", chk_type));
            }

            p.Add(":p2", userId);
            p.Add(":p3", chk_level);

            return DBWork.Connection.Query<CE0003>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> PeopleCombo(string CHK_NO)
        {
            string sql = @"select distinct a.CHK_UID as VALUE,b.UNA as TEXT  from CHK_DETAIL a , UR_ID b 
                        where a.chk_no = :chk_no 
                        and a.CHK_UID = b.TUSER ";
            return DBWork.Connection.Query<ComboItemModel>(sql, new { chk_no = CHK_NO }, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> PeopleNouidCombo(string CHK_NO)
        {
            string sql = @"select distinct a.CHK_UID as VALUE,b.UNA as TEXT  from CHK_NOUID a , UR_ID b 
                        where a.chk_no = :chk_no 
                        and a.CHK_UID = b.TUSER ";
            return DBWork.Connection.Query<ComboItemModel>(sql, new { chk_no = CHK_NO }, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> ChkTypeCombo(string chk_ym, string chk_period, string wh_no, string wh_kind, string chk_uid) {
            string sql = string.Format(@"select distinct a.chk_type as value,
                                                (select data_desc from PARAM_D 
                                                  where grp_code = 'CHK_MAST'
                                                    and data_name = 'CHK_WH_KIND_{0}'
                                                    and data_value = a.chk_type) as text
                                           from CHK_MAST a
                                          where a.chk_wh_no = :wh_no
                                            and a.chk_ym = :chk_ym
                                            and a.chk_period = :chk_period
                                            and a.chk_no  in  
                                                  (select DISTINCT chk_no from CHK_DETAIL where CHK_UID = :chk_uid 
                                                      union 
                                                   select distinct chk_no from CHK_NOUID where chk_uid = :chk_uid)
                                          order by a.chk_type
                                        ", wh_kind);
            return DBWork.Connection.Query<ComboItemModel>(sql, new { chk_ym = chk_ym, chk_period = chk_period, wh_no = wh_no, chk_uid = chk_uid }, DBWork.Transaction);
        }

        public CE0003 GetSingleItem(string chk_no, string chk_uid, string sort_order, string sort_by, string mmcode, string store_loc) {
            var p = new DynamicParameters();
            string sql = string.Format(@"select * from CHK_DETAIL 
                                          where chk_no = :chk_no
                                          {0}
                                            and chk_time is null", chk_uid == string.Empty ? string.Empty : "   and chk_uid = :chk_uid");

            if (sort_by.ToUpper() == "MMCODE") {
                string param = string.Empty;
                if (mmcode != string.Empty)
                {
                    if (sort_order.ToUpper() == "ASC")
                    {
                        param = ">";
                    }
                    if (sort_order.ToUpper() == "DESC")
                    {
                        param = "<";
                    }
                    sql += string.Format("  and mmcode {0} :mmcode", param);
                }
            }
            if (sort_by.ToUpper() == "STORE_LOC") {
                string param = string.Empty;
                if (store_loc != string.Empty)
                {
                    if (store_loc.ToUpper() == "暫存區")
                    {
                        int tempCount = int.Parse(GetTempCount(chk_no, sort_order, mmcode));

                        if (tempCount > 0)
                        {
                            sql += string.Format(" and mmcode > :mmcode and store_loc = '暫存區'", param);
                        }
                        else {
                            sql += string.Format("  and store_loc != '暫存區'", param);
                        }
                    }
                    else
                    {
                        if (sort_by.ToUpper() == "STORE_LOC" && sort_order.ToUpper() == "ASC")
                        {
                            param = ">=";
                        }
                        if (sort_by.ToUpper() == "STORE_LOC" && sort_order.ToUpper() == "DESC")
                        {
                            param = "<=";
                        }
                        sql += string.Format("  and substr(nvl(store_loc, ''), 3, (length(nvl(store_loc, ''))-2)) {0} substr(nvl(:store_loc, ''), 3, (length(nvl(:store_loc, ''))-2))", param);
                    }
                }
            }

            string other_sort_column = sort_by.ToUpper() == "STORE_LOC" ? "MMCODE" : (sort_by.ToUpper() == "MMCODE" ? "STORE_LOC" : "STORE_LOC");

            if (chk_no.IndexOf("PH1S") >= 0)
            {
                if (sort_by.ToUpper() == "STORE_LOC")
                {
                    sort_by = "substr(nvl(STORE_LOC, ''), 3, (length(nvl(STORE_LOC, ''))-2))";
                }
                if (other_sort_column.ToUpper() == "STORE_LOC")
                {
                    other_sort_column = "substr(nvl(STORE_LOC, ''), 3, (length(nvl(STORE_LOC, ''))-2))";
                }
            }
            sql += string.Format(" order by {0} {1}, {2} asc", sort_by, sort_order, other_sort_column);

            p.Add(":chk_no", chk_no);
            p.Add(":chk_uid", chk_uid);
            p.Add(":mmcode", mmcode);
            p.Add(":store_loc", store_loc);

            return DBWork.Connection.QueryFirstOrDefault<CE0003>(sql, p, DBWork.Transaction);
        }
        public string GetTempCount(string chk_no, string sort_order, string mmcode) {
            string sql = string.Format(@"select count(*) from CHK_DETAIL
                                          where chk_no = :chk_no
                                            and chk_time is null
                                            and store_loc = '暫存區'
                                            and mmcode > :mmcode");
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { chk_no = chk_no,mmcode = mmcode}, DBWork.Transaction);
        }
    }

}