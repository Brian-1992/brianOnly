using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository
{
    public class BE0008Repository : JCLib.Mvc.BaseRepository
    {
        public BE0008Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        private string GetSql(string agen_no, string agen_namec, string agen_add, string uni_no)
        {
            var sql = @"SELECT AGEN_NO, AGEN_NAMEC,  AGEN_ADD, AGEN_FAX, AGEN_TEL, 
                               trim(AGEN_ACC) as AGEN_ACC, UNI_NO, AGEN_BOSS, EMAIL, EMAIL_1,
                               EASYNAME,
                               AGEN_FREETEL1, AGEN_FREETEL2,
                               AGEN_ZIP, AGEN_ZIP1, AGEN_ZIP2,
                               AGEN_ADD1, AGEN_ADD2,
                               AGEN_CONTACT, AGEN_CONTACT1, AGEN_CONTACT2,
                               AGEN_CONCELL, AGEN_CONCELL1, AGEN_CONCELL2,
                               AGEN_TEL2, AGEN_TEL3, AGEN_TEL4,
                               AGEN_FAX2, AGEN_FAX3, AGEN_FAX4,
                               AGEN_WEB1, AGEN_WEB2,
                               trim(AGEN_BANK_14) as AGEN_BANK_14, PROCFEE, POSTFEE,
                               ACCZIP, ACCADDRESS, ACCTEL, ACCCONTACT, ACCEMAIL,
                               (select bankname from PH_BANK_AF where agen_bank_14 = a.agen_bank_14) as bank_name
                          FROM PH_VENDER a
                         WHERE 1=1 ";

            if (agen_no != "")
            {
                sql += string.Format(" AND AGEN_NO LIKE '%{0}%' ", agen_no);
            }
            if (agen_namec != "")
            {
                sql += string.Format(" AND AGEN_NAMEC LIKE '%{0}%' ", agen_namec);
            }
            if (agen_add != "")
            {
                sql += string.Format(" AND AGEN_ADD LIKE '%{0}%' ", agen_add);
            }
            if (uni_no != "")
            {
                sql += string.Format(" AND UNI_NO LIKE '%{0}%' ", uni_no);
            }
            return sql;
        }

        public IEnumerable<PH_VENDER> GetAll(string agen_no, string agen_namec, string agen_add, string uni_no)
        {
            var sql = GetSql(agen_no, agen_namec, agen_add, uni_no);
            return DBWork.PagingQuery<PH_VENDER>(sql, new { agen_no, agen_namec, agen_add, uni_no }, DBWork.Transaction);
        }

        public DataTable GetExcel(string agen_no, string agen_namec, string agen_add, string uni_no)
        {
            string sql = string.Format(@" 
                SELECT AGEN_NO as 廠商碼, AGEN_NAMEC as 名稱, EASYNAME as 簡稱, 
                       UNI_NO as 統一編號, AGEN_BOSS as 負責人,
                       AGEN_FREETEL1 as 免費電話一, AGEN_FREETEL2 as 免費電話二,
                       AGEN_ZIP as 郵遞區號一, AGEN_ADD as 地址一, AGEN_CONTACT as 聯絡人一, AGEN_CONCELL as 行動電話一,
                       AGEN_ZIP1 as 郵遞區號二, AGEN_ADD1 as 廠商地址二, AGEN_CONTACT1 as 連絡人二, AGEN_CONCELL1 as 行動電話二,
                       AGEN_ZIP2 as 郵遞區號三, AGEN_ADD2 as 廠商地址三, AGEN_CONTACT2 as 連絡人三, AGEN_CONCELL2 as 行動電話三,
                       AGEN_TEL as 電話一, AGEN_TEL2 as 電話二, AGEN_TEL3 as 電話三, AGEN_TEL4 as 電話四,
                       AGEN_FAX as 傳真一, AGEN_FAX2 as 傳真二, AGEN_FAX3 as 傳真三, AGEN_FAX4 as 傳真四,  
                       EMAIL as EMAIL一, EMAIL_1 as EMAIL二,
                       AGEN_WEB1 as 公司網址一, AGEN_WEB2 as 公司網址二,
                       AGEN_BANK_14 as 銀行代碼,  bank_name as 銀行名稱,
                       AGEN_ACC as 銀行帳號, PROCFEE as 匯款手續費, POSTFEE as 郵寄費用,
                       ACCZIP as 帳務郵遞區號, ACCADDRESS as 帳務地址, ACCTEL as 帳務電話, 
                       ACCCONTACT as 帳務連絡人, ACCEMAIL as 帳務連絡Email          
                  FROM ( {0} )"
            , GetSql(agen_no, agen_namec, agen_add, uni_no));


            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, new { agen_no, agen_namec, agen_add, uni_no }, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<PH_VENDER> Get(string agen_no)
        {
            string sql = GetSql(agen_no, string.Empty, string.Empty, string.Empty);
            return DBWork.Connection.Query<PH_VENDER>(sql, new { agen_no }, DBWork.Transaction);
        }

        public int Create(PH_VENDER ph_vender)
        {
            var sql = @"INSERT INTO PH_VENDER (AGEN_NO, AGEN_NAMEC, AGEN_NAMEE, AGEN_ADD, AGEN_FAX, 
                                               AGEN_TEL, AGEN_ACC, UNI_NO, AGEN_BOSS, REC_STATUS, EMAIL,
                                               EMAIL_1, AGEN_BANK, AGEN_SUB, 
                                               CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, 
                                               MAIN_INID, EASYNAME, MINODR_AMT,
                                               AGEN_FREETEL1, AGEN_FREETEL2,
                                               AGEN_ZIP, AGEN_ZIP1, AGEN_ZIP2,
                                               AGEN_ADD1, AGEN_ADD2,
                                               AGEN_CONTACT, AGEN_CONTACT1, AGEN_CONTACT2,
                                               AGEN_CONCELL, AGEN_CONCELL1, AGEN_CONCELL2,
                                               AGEN_TEL2, AGEN_TEL3, AGEN_TEL4,
                                               AGEN_FAX2, AGEN_FAX3, AGEN_FAX4,
                                               AGEN_WEB1, AGEN_WEB2,
                                               AGEN_BANK_14, PROCFEE, POSTFEE,
                                               ACCZIP, ACCADDRESS, ACCTEL, ACCCONTACT, ACCEMAIL
                        )  
                        VALUES (:AGEN_NO, :AGEN_NAMEC, :AGEN_NAMEE, :AGEN_ADD, :AGEN_FAX, :AGEN_TEL, 
                                :AGEN_ACC, :UNI_NO, :AGEN_BOSS, :REC_STATUS, :EMAIL, 
                                :EMAIL_1, :AGEN_BANK, :AGEN_SUB, SYSDATE, 
                                :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP, 
                                :MAIN_INID, :EASYNAME, :MINODR_AMT,
                                :AGEN_FREETEL1, :AGEN_FREETEL2,
                                :AGEN_ZIP, :AGEN_ZIP1, :AGEN_ZIP2,
                                :AGEN_ADD1, :AGEN_ADD2,
                                :AGEN_CONTACT, :AGEN_CONTACT1, :AGEN_CONTACT2,
                                :AGEN_CONCELL, :AGEN_CONCELL1, :AGEN_CONCELL2,
                                :AGEN_TEL2, :AGEN_TEL3, :AGEN_TEL4,
                                :AGEN_FAX2, :AGEN_FAX3, :AGEN_FAX4,
                                :AGEN_WEB1, :AGEN_WEB2,
                                :AGEN_BANK_14, :PROCFEE, :POSTFEE,
                                :ACCZIP, :ACCADDRESS, :ACCTEL, :ACCCONTACT, :ACCEMAIL
                        )";
            return DBWork.Connection.Execute(sql, ph_vender, DBWork.Transaction);
        }

        public int Update(PH_VENDER ph_vender)
        {
            var sql = @"UPDATE PH_VENDER SET AGEN_NAMEC = :AGEN_NAMEC, AGEN_NAMEE = :AGEN_NAMEE, AGEN_ADD = :AGEN_ADD, AGEN_FAX = :AGEN_FAX, AGEN_TEL = :AGEN_TEL, 
                                AGEN_ACC = :AGEN_ACC, UNI_NO = :UNI_NO, AGEN_BOSS = :AGEN_BOSS, REC_STATUS = :REC_STATUS, EMAIL = :EMAIL, EMAIL_1 = :EMAIL_1, 
                                AGEN_BANK = :AGEN_BANK, AGEN_SUB = :AGEN_SUB, 
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP,
                                MAIN_INID = :MAIN_INID, EASYNAME = :EASYNAME, MINODR_AMT = :MINODR_AMT,
                                AGEN_FREETEL1 = :AGEN_FREETEL1, AGEN_FREETEL2 = :AGEN_FREETEL2,
                                AGEN_ZIP = :AGEN_ZIP, AGEN_ZIP1 = :AGEN_ZIP1, AGEN_ZIP2 = :AGEN_ZIP2,
                                AGEN_ADD1 = :AGEN_ADD1, AGEN_ADD2 = :AGEN_ADD2,
                                AGEN_CONTACT = :AGEN_CONTACT, AGEN_CONTACT1 = :AGEN_CONTACT1, AGEN_CONTACT2 = :AGEN_CONTACT2,
                                AGEN_CONCELL = :AGEN_CONCELL, AGEN_CONCELL1 = :AGEN_CONCELL1, AGEN_CONCELL2 = :AGEN_CONCELL2,
                                AGEN_TEL2 = :AGEN_TEL2, AGEN_TEL3 = :AGEN_TEL3, AGEN_TEL4 = :AGEN_TEL4,
                                AGEN_FAX2 = :AGEN_FAX2, AGEN_FAX3 = :AGEN_FAX3, AGEN_FAX4 = :AGEN_FAX4,
                                AGEN_WEB1 = :AGEN_WEB1, AGEN_WEB2 = :AGEN_WEB2,
                                AGEN_BANK_14 = :AGEN_BANK_14, PROCFEE = :PROCFEE, POSTFEE = :POSTFEE,
                                ACCZIP = :ACCZIP, ACCADDRESS = :ACCADDRESS, ACCTEL = :ACCTEL, ACCCONTACT = :ACCCONTACT, ACCEMAIL = :ACCEMAIL
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

        public IEnumerable<COMBO_MODEL> GetAgenBank14Combo()
        {
            string sql = @" SELECT TRIM(AGEN_BANK_14) AS VALUE, BANKNAME AS TEXT from PH_BANK_AF ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
    }
}