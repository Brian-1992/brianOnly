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

    public class CE0006Repository : JCLib.Mvc.BaseRepository
    {
        public CE0006Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<CE0006> GetAll(string wh_no, string date, string userId, int page_index, int page_size, string sorters)
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
            sql += " AND CHK_LEVEL = '2' ";

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
                        select distinct chk_no from CHK_NOUID where chk_uid = :p2                     
                    union 
                     select distinct chk_no from CHK_GRADE2_UPDN where  CHK_UID =  :p2)";
            p.Add(":p2", userId);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CE0006>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //GET T2Grid
        public IEnumerable<CE0006> GetAllINI(string chk_no, string mmcodeorstore, string userId)
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
            sql += "Order by CHK_TIME nulls first ,STORE_LOC, MMCODE desc";


            return DBWork.PagingQuery<CE0006>(sql, p, DBWork.Transaction);
        }

        //GET T2Grid
        public IEnumerable<CE0006> GetAllINIPDA(string chk_no, string mmcodeorstore, string userId, int page_index, int page_size, string sorters)
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
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CE0006>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //GET T2Grid
        public IEnumerable<CE0006> GetSign(string chk_no, string userId, int page_index, int page_size, string sorters)
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

            return DBWork.Connection.Query<CE0006>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //GET T2Grid
        public IEnumerable<CE0006> GetNoSign(string chk_no, string userId, int page_index, int page_size, string sorters)
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

            return DBWork.Connection.Query<CE0006>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //GET T2Grid
        public IEnumerable<CE0006> GetAllINIotherPDA(string chk_no, string mmcodeorstore, string userId, int page_index, int page_size, string sorters)
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

            return DBWork.Connection.Query<CE0006>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        //GET ifAutoLoad
        public IEnumerable<CE0006> GetAllINIAutoLoadPDA(string chk_no, string mmcodeorstore, string userId)
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

            return DBWork.Connection.Query<CE0006>(sql, p, DBWork.Transaction);
        }



        //INI Query
        public IEnumerable<CE0006> GetCE0006_INI(string chk_no)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT CHK_WH_NO, 
(SELECT WH_NAME FROM MI_WHMAST WHERE WH_NO = a.CHK_WH_NO) as WH_NAME, 
a.CHK_YM, 
(SELECT DISTINCT  DATA_DESC as COMBITEM FROM PARAM_D 
WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_WH_GRADE' AND DATA_VALUE =A.CHK_WH_GRADE) CHK_WH_GRADE, 
(SELECT DISTINCT  DATA_DESC as COMBITEM FROM PARAM_D 
WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_WH_KIND' AND DATA_VALUE =A.CHK_WH_KIND) CHK_WH_KIND, 
(SELECT DISTINCT  DATA_DESC as COMBITEM FROM PARAM_D 
WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_PERIOD' AND DATA_VALUE =A.CHK_PERIOD) CHK_PERIOD, 
a.CHK_TYPE,a.CHK_NUM || '/' || a.CHK_TOTAL as MERGE_NUM_TOTAL, a.CHK_NO, 
(SELECT UNA FROM UR_ID WHERE TUSER = a.CHK_KEEPER) CHK_KEEPER, 
(SELECT DISTINCT  DATA_DESC as COMBITEM FROM PARAM_D 
WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_STATUS' AND DATA_VALUE =A.CHK_STATUS) CHK_STATUS 
from CHK_MAST a WHERE 1=1 ";

            if (chk_no != "" && chk_no != null)
            {
                sql += " AND a.chk_no = :p0 ";
                p.Add(":p0", chk_no);
            }

            return DBWork.Connection.Query<CE0006>(sql, p, DBWork.Transaction);
        }

        public int UpdateCE0006_INI(CE0006 ce0006)
        {
            var sql = @"UPDATE CHK_DETAIL SET CHK_QTY = :CHK_QTY, CHK_REMARK = :CHK_REMARK,CHK_UID = :UPDATE_USER, CHK_TIME = SYSDATE, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP 
                                WHERE CHK_NO = :CHK_NO AND MMCODE = :MMCODE AND STORE_LOC = :STORE_LOC";
            return DBWork.Connection.Execute(sql, ce0006, DBWork.Transaction);
        }

        public IEnumerable<CE0006> Get(string chk_no, string mmocde, string store_loc)
        {
            var sql = @"SELECT * FROM CHK_DETAIL WHERE chk_no = :CHK_NO and mmcode = :MMCODE and store_loc = :STORE_LOC";
            return DBWork.Connection.Query<CE0006>(sql, new { CHK_NO = chk_no, MMCODE = mmocde, STORE_LOC = store_loc }, DBWork.Transaction);
        }

        public string SelectCountFinalPro(string chk_no, string userId)
        {
            var sql = @"Select count(*)  as COUNT FROM CHK_DETAIL WHERE chk_no = :CHK_NO and chk_uid = :CHK_UID and chk_time is NULL";
            return DBWork.Connection.ExecuteScalar(sql, new { CHK_NO = chk_no, CHK_UID = userId }, DBWork.Transaction).ToString();
        }
        public IEnumerable<CE0006> SelectFinalPro(string chk_no, string userId)
        {
            var sql = @"Select * FROM CHK_DETAIL WHERE chk_no = :CHK_NO and chk_uid = :CHK_UID and chk_time is NULL";
            return DBWork.Connection.Query<CE0006>(sql, new { CHK_NO = chk_no, CHK_UID = userId }, DBWork.Transaction);
        }

        public int UpdateChk_detail(string chk_no, string userId, string userIp)
        {
            var sql = @"UPDATE CHK_DETAIL SET STATUS_INI = :STATUS_INI, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP 
                                WHERE CHK_NO = :CHK_NO AND CHK_UID = :CHK_UID";
            return DBWork.Connection.Execute(sql, new { STATUS_INI = "2", UPDATE_USER = userId, UPDATE_IP = userIp, CHK_NO = chk_no, CHK_UID = userId }, DBWork.Transaction);
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

        public int UpdateCE0006_INIPC(string CHK_QTY, string CHK_REMARK, string user, string ip, string CHK_NO, string MMCODE, string STORE_LOC)
        {
            var p = new DynamicParameters();

            var sql = @"UPDATE CHK_DETAIL SET CHK_QTY = :CHK_QTY, CHK_REMARK = :CHK_REMARK, CHK_TIME = SYSDATE, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE CHK_NO = :CHK_NO AND MMCODE = :MMCODE AND STORE_LOC = :STORE_LOC and CHK_UID = :UPDATE_USER";
            p.Add(":CHK_QTY", CHK_QTY);
            p.Add(":CHK_REMARK", CHK_REMARK);
            p.Add(":UPDATE_USER", user);
            p.Add(":UPDATE_IP", ip);
            p.Add(":CHK_NO", CHK_NO);
            p.Add(":MMCODE", MMCODE);
            p.Add(":STORE_LOC", STORE_LOC);


            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public int UpdateCE0006_INI_NOTIME_PC(string CHK_QTY, string CHK_REMARK, string user, string ip, string CHK_NO, string MMCODE, string STORE_LOC)
        {
            var p = new DynamicParameters();

            var sql = @"UPDATE CHK_DETAIL SET CHK_QTY = :CHK_QTY, CHK_REMARK = :CHK_REMARK, CHK_TIME = :CHK_TIME, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE CHK_NO = :CHK_NO AND MMCODE = :MMCODE AND STORE_LOC = :STORE_LOC  and CHK_UID = :UPDATE_USER";
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
        public CHK_MAST GetMast(string chk_no)
        {
            string sql = @"select * from CHK_MAST where chk_no = :chk_no";
            return DBWork.Connection.QueryFirst<CHK_MAST>(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }
        public IEnumerable<CE0006> SelectCountSign(string chk_no) //已盤
        {
            var sql = @"Select count(*)  as sign FROM CHK_DETAIL WHERE chk_no = :CHK_NO and chk_time is not NULL";
            return DBWork.Connection.Query<CE0006>(sql, new { CHK_NO = chk_no }, DBWork.Transaction);
        }
        public IEnumerable<CE0006> SelectCountNoSign(string chk_no) //未盤
        {
            var sql = @"Select count(*)  as nosign FROM CHK_DETAIL WHERE chk_no = :CHK_NO and chk_time is NULL";
            return DBWork.Connection.Query<CE0006>(sql, new { CHK_NO = chk_no }, DBWork.Transaction);
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

        public IEnumerable<ComboItemModel> PeopleCombo(string CHK_NO)
        {
            string sql = @"select distinct a.CHK_UID as VALUE,b.UNA as TEXT  from CHK_DETAIL a , UR_ID b where a.chk_no = :chk_no and a.CHK_UID = b.tuser";
            return DBWork.Connection.Query<ComboItemModel>(sql, new { chk_no = CHK_NO }, DBWork.Transaction);
        }




    }

}