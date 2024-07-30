using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.BE
{
    public class BE0002Repository : JCLib.Mvc.BaseRepository
    {
        public BE0002Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<PH_VENDER> GetAll(string agen_no, string agen_namec, string agen_namee, string agen_add, string uni_no, string user_inid, string hosp_code, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT AGEN_NO, AGEN_NAMEC, AGEN_NAMEE, AGEN_ADD, AGEN_FAX, AGEN_TEL, AGEN_ACC, UNI_NO, AGEN_BOSS, EMAIL, EMAIL_1,
                            AGEN_BANK, AGEN_SUB, REC_STATUS, EASYNAME, MAIN_INID, MINODR_AMT FROM PH_VENDER WHERE 1=1 ";

            if (hosp_code == "0")
            {
                if (CheckParam(user_inid))
                {
                    sql += " AND MAIN_INID=:USER_INID ";
                    p.Add(":USER_INID", user_inid);
                }
                else
                {
                    sql += " AND MAIN_INID not in (select DATA_VALUE from PARAM_D WHERE GRP_CODE='PH_VENDER' and DATA_NAME='MAIN_INID') ";
                }
            }

            if (agen_no != "")
            {
                sql += " AND AGEN_NO LIKE :p0 ";
                p.Add(":p0", string.Format("{0}%", agen_no));
            }
            if (agen_namec != "")
            {
                sql += " AND AGEN_NAMEC LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", agen_namec));
            }
            if (agen_namee != "")
            {
                sql += " AND AGEN_NAMEE LIKE :p2 ";
                p.Add(":p2", string.Format("%{0}%", agen_namee));
            }
            if (agen_add != "")
            {
                sql += " AND AGEN_ADD LIKE :p3 ";
                p.Add(":p3", string.Format("%{0}%", agen_add));
            }
            if (uni_no != "")
            {
                sql += " AND UNI_NO LIKE :p4 ";
                p.Add(":p4", string.Format("{0}%", uni_no));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PH_VENDER>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public DataTable GetExcel(string agen_no, string agen_namec, string agen_namee, string agen_add, string uni_no, string hosp_code, string user_inid)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @" SELECT AGEN_NO as 廠商碼, AGEN_NAMEC as 廠商中文名稱, AGEN_NAMEE as 廠商英文名稱, AGEN_ADD as 廠商住址, AGEN_FAX as 傳真號碼, AGEN_TEL as 電話號碼, AGEN_ACC as 銀行帳號, UNI_NO as 統一編號, AGEN_BOSS as 負責人姓名, EMAIL as EMAIL, EMAIL_1 as EMAIL_1,
                            AGEN_BANK as 銀行代碼, AGEN_SUB as 銀行分行名, REC_STATUS as 修改狀態碼, MAIN_INID as 資料維護單位, MINODR_AMT as 單筆訂單最低出貨金額 FROM PH_VENDER 
                        WHERE 1=1 ";
            if (hosp_code == "0")
            {
                if (CheckParam(user_inid))
                {
                    sql += " AND MAIN_INID=:USER_INID ";
                    p.Add(":USER_INID", user_inid);
                }
                else
                {
                    sql += " AND MAIN_INID not in (select DATA_VALUE from PARAM_D WHERE GRP_CODE='PH_VENDER' and DATA_NAME='MAIN_INID') ";
                }
            }

            if (agen_no != "" && agen_no != null)
            {
                sql += " AND AGEN_NO LIKE :p0 ";
                p.Add(":p0", string.Format("{0}%", agen_no));
            }
            if (agen_namec != "" && agen_namec != null)
            {
                sql += " AND AGEN_NAMEC LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", agen_namec));
            }
            if (agen_namee != "" && agen_namee != null)
            {
                sql += " AND AGEN_NAMEE LIKE :p2 ";
                p.Add(":p2", string.Format("%{0}%", agen_namee));
            }
            if (agen_add != "" && agen_add != null)
            {
                sql += " AND AGEN_ADD LIKE :p3 ";
                p.Add(":p3", string.Format("%{0}%", agen_add));
            }
            if (uni_no != "" && uni_no != null)
            {
                sql += " AND UNI_NO LIKE :p4 ";
                p.Add(":p4", string.Format("{0}%", uni_no));
            }
            sql += " order by AGEN_NO ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<PH_VENDER> Get(string id)
        {
            var sql = @"SELECT * FROM PH_VENDER WHERE AGEN_NO = :AGEN_NO";
            return DBWork.Connection.Query<PH_VENDER>(sql, new { AGEN_NO = id }, DBWork.Transaction);
        }

        public int Create(PH_VENDER ph_vender)
        {
            var sql = @"INSERT INTO PH_VENDER (AGEN_NO, AGEN_NAMEC, AGEN_NAMEE, AGEN_ADD, AGEN_FAX, AGEN_TEL, AGEN_ACC, UNI_NO, AGEN_BOSS, REC_STATUS, EMAIL, EMAIL_1, AGEN_BANK, AGEN_SUB, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, MAIN_INID, EASYNAME, MINODR_AMT)  
                                VALUES (:AGEN_NO, :AGEN_NAMEC, :AGEN_NAMEE, :AGEN_ADD, :AGEN_FAX, :AGEN_TEL, :AGEN_ACC, :UNI_NO, :AGEN_BOSS, :REC_STATUS, :EMAIL, :EMAIL_1, :AGEN_BANK, :AGEN_SUB, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP, :MAIN_INID, :EASYNAME, :MINODR_AMT)";
            return DBWork.Connection.Execute(sql, ph_vender, DBWork.Transaction);
        }

        public int Update(PH_VENDER ph_vender)
        {
            var sql = @"UPDATE PH_VENDER SET AGEN_NAMEC = :AGEN_NAMEC, AGEN_NAMEE = :AGEN_NAMEE, AGEN_ADD = :AGEN_ADD, AGEN_FAX = :AGEN_FAX, AGEN_TEL = :AGEN_TEL, 
                                AGEN_ACC = :AGEN_ACC, UNI_NO = :UNI_NO, AGEN_BOSS = :AGEN_BOSS, REC_STATUS = :REC_STATUS, EMAIL = :EMAIL, EMAIL_1 = :EMAIL_1, 
                                AGEN_BANK = :AGEN_BANK, AGEN_SUB = :AGEN_SUB, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP,
                                MAIN_INID = :MAIN_INID, EASYNAME = :EASYNAME, MINODR_AMT = :MINODR_AMT
                                WHERE AGEN_NO = :AGEN_NO";
            return DBWork.Connection.Execute(sql, ph_vender, DBWork.Transaction);
        }

        //public int Delete(string agen_no)
        //{
        //    // 資料會在其他地方使用者,刪除時不直接刪除而是加上刪除旗標
        //    var sql = @"UPDATE PH_VENDER SET REC_STATUS = 'X'
        //                        WHERE AGEN_NO = :AGEN_NO";
        //    return DBWork.Connection.Execute(sql, new { AGEN_NO = agen_no }, DBWork.Transaction);
        //}

        public bool CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM PH_VENDER WHERE AGEN_NO=:AGEN_NO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { AGEN_NO = id }, DBWork.Transaction) == null);
        }
        public bool CheckParam(string inid)
        {
            string sql = @"SELECT 1 FROM PARAM_D WHERE GRP_CODE='PH_VENDER' and DATA_NAME='MAIN_INID' and DATA_VALUE=:INID";
            return !(DBWork.Connection.ExecuteScalar(sql, new { INID = inid }, DBWork.Transaction) == null);
        }

        public string GetHospCode()
        {
            var sql = @" SELECT data_value FROM PARAM_D WHERE grp_code = 'HOSP_INFO' AND data_name = 'HospCode' ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }
    }
}