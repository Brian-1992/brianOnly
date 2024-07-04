using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.B
{
    public class BA0005Repository : JCLib.Mvc.BaseRepository
    {
        public BA0005Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<BA0005> GetAll(string cls, string dateFrom, string dateTo, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"";

            if (cls == "02")
            {
                sql += @"SELECT 
                            a.*, (select FLOWNAME from ME_FLOW where DOCTYPE = c.DOCTYPE and FLOWID = c.FLOWID) AS FLOWID, c.APPDEPT,   
                                 (SELECT MAT_CLASS || ' ' || MAT_CLSNAME TEXT FROM MI_MATCLASS WHERE MAT_CLASS = substr(c.DOCNO, 20, 2)) AS MAT_CLASS
                         FROM
                            ME_DOCD a, MI_MAST b, ME_DOCM c
                         WHERE
                            a.DOCNO = c.DOCNO AND a.MMCODE = b.MMCODE AND c.DOCTYPE = 'MR4'
                            AND c.FLOWID = '2' AND c.APPLY_KIND = '2' AND APPQTY > 0 AND RDOCNO IS NULL
                            AND b.M_STOREID = '0' AND (b.M_APPLYID NOT IN ('E', 'P') OR b.M_APPLYID IS NULL)
                            AND substr (c.DOCNO, 20, 2) = '02'
                            AND substr (a.DOCNO, 7, 7) >= TWN_DATE(TO_DATE(:dateFrom, 'YYYY/mm/dd'))
                            AND substr (a.DOCNO, 7, 7) <= TWN_DATE(TO_DATE(:dateTo, 'YYYY/mm/dd')) 
                         ORDER BY 
                            c.APPDEPT, a.DOCNO, a.MMCODE ";
            }
            else
            {
                sql += @"SELECT
                            a.*, (select FLOWNAME from ME_FLOW where DOCTYPE = c.DOCTYPE and FLOWID = c.FLOWID) AS FLOWID, c.APPDEPT, 
                                 (SELECT MAT_CLASS || ' ' || MAT_CLSNAME TEXT FROM MI_MATCLASS WHERE MAT_CLASS = substr(c.DOCNO, 20, 2)) AS MAT_CLASS
                         FROM
                            ME_DOCD a, MI_MAST b, ME_DOCM c
                         WHERE
                            a.DOCNO = c.DOCNO AND a.MMCODE = b.MMCODE AND c.DOCTYPE = 'MR3'
                            AND c.FLOWID = '2' AND c.APPLY_KIND = '2' AND APPQTY > 0 AND RDOCNO IS NULL
                            AND b.M_STOREID = '0' AND (b.M_APPLYID NOT IN ('E', 'P') OR b.M_APPLYID IS NULL)
                            AND substr(c.DOCNO, 20, 2) = :cls
                            AND substr(a.DOCNO, 7, 7) >= TWN_DATE(TO_DATE(:dateFrom, 'YYYY/mm/dd'))
                            AND substr(a.DOCNO, 7, 7) <= TWN_DATE(TO_DATE(:dateTo, 'YYYY/mm/dd'))
                         ORDER BY 
                            c.APPDEPT, a.DOCNO, a.MMCODE ";
            }

            if (cls != "")
            {                
                p.Add(":cls", string.Format("{0}", cls));
            }
            if (dateFrom != "")
            {               
                p.Add(":dateFrom", string.Format("{0}", DateTime.Parse(dateFrom).ToString("yyyy/MM/dd")));
            }
            if (dateTo != "")
            {                
                p.Add(":dateTo", string.Format("{0}", DateTime.Parse(dateTo).ToString("yyyy/MM/dd")));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BA0005>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //public IEnumerable<PH_VENDER> GetAll(string agen_no, string agen_namec, string agen_namee, string agen_add, string uni_no, int page_index, int page_size, string sorters)
        //{
        //    var p = new DynamicParameters();

        //    var sql = @"SELECT * FROM PH_VENDER WHERE 1=1 ";

        //    if (agen_no != "")
        //    {
        //        sql += " AND AGEN_NO LIKE :p0 ";
        //        p.Add(":p0", string.Format("%{0}%", agen_no));
        //    }
        //    if (agen_namec != "")
        //    {
        //        sql += " AND AGEN_NAMEC LIKE :p1 ";
        //        p.Add(":p1", string.Format("%{0}%", agen_namec));
        //    }
        //    if (agen_namee != "")
        //    {
        //        sql += " AND AGEN_NAMEE LIKE :p2 ";
        //        p.Add(":p2", string.Format("%{0}%", agen_namee));
        //    }
        //    if (agen_add != "")
        //    {
        //        sql += " AND AGEN_ADD LIKE :p3 ";
        //        p.Add(":p3", string.Format("%{0}%", agen_add));
        //    }
        //    if (uni_no != "")
        //    {
        //        sql += " AND UNI_NO LIKE :p4 ";
        //        p.Add(":p4", string.Format("%{0}%", uni_no));
        //    }
        //    p.Add("OFFSET", (page_index - 1) * page_size);
        //    p.Add("PAGE_SIZE", page_size);

        //    return DBWork.Connection.Query<PH_VENDER>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        //}

        //public IEnumerable<PH_VENDER> Get(string id)
        //{
        //    var sql = @"SELECT * FROM PH_VENDER WHERE AGEN_NO = :AGEN_NO";
        //    return DBWork.Connection.Query<PH_VENDER>(sql, new { AGEN_NO = id }, DBWork.Transaction);
        //}

        //public int Create(PH_VENDER ph_vender)
        //{
        //    var sql = @"INSERT INTO PH_VENDER (AGEN_NO, AGEN_NAMEC, AGEN_NAMEE, AGEN_ADD, AGEN_FAX, AGEN_TEL, AGEN_ACC, UNI_NO, AGEN_BOSS, REC_STATUS, EMAIL, EMAIL_1, AGEN_BANK, AGEN_SUB, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
        //                        VALUES (:AGEN_NO, :AGEN_NAMEC, :AGEN_NAMEE, :AGEN_ADD, :AGEN_FAX, :AGEN_TEL, :AGEN_ACC, :UNI_NO, :AGEN_BOSS, :REC_STATUS, :EMAIL, :EMAIL_1, :AGEN_BANK, :AGEN_SUB, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
        //    return DBWork.Connection.Execute(sql, ph_vender, DBWork.Transaction);
        //}

        //public int Update(PH_VENDER ph_vender)
        //{
        //    var sql = @"UPDATE PH_VENDER SET AGEN_NAMEC = :AGEN_NAMEC, AGEN_NAMEE = :AGEN_NAMEE, AGEN_ADD = :AGEN_ADD, AGEN_FAX = :AGEN_FAX, AGEN_TEL = :AGEN_TEL, 
        //                        AGEN_ACC = :AGEN_ACC, UNI_NO = :UNI_NO, AGEN_BOSS = :AGEN_BOSS, REC_STATUS = :REC_STATUS, EMAIL = :EMAIL, EMAIL_1 = :EMAIL_1, 
        //                        AGEN_BANK = :AGEN_BANK, AGEN_SUB = :AGEN_SUB, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
        //                        WHERE AGEN_NO = :AGEN_NO";
        //    return DBWork.Connection.Execute(sql, ph_vender, DBWork.Transaction);
        //}

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

        public IEnumerable<ComboItemModel> GetCLSNAME()
        {
            string sql = @"  SELECT MAT_CLASS VALUE,
                                    MAT_CLASS || ' ' || MAT_CLSNAME TEXT
                             FROM MI_MATCLASS
                             WHERE 
                                    MAT_CLSID IN ('2', '3')
                             ORDER BY MAT_CLASS";
            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }

        public string CreateOrder(string cls, string dateFrom, string dateTo, string userid, string updip)
        //public void CreateOrder(string cls, string dateFrom, string dateTo, string userid, string updip)
        {
            var p = new OracleDynamicParameters();
            p.Add("i_yyymmdd_s", value: dateFrom, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 10);
            p.Add("i_yyymmdd_e", value: dateTo, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 10);
            p.Add("i_mat_class", value: cls, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 2);
            p.Add("i_inid", value: USERID_TO_INID(userid), dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 6);
            p.Add("i_userid", value: userid, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 8);
            p.Add("i_ip", value: updip, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);
            p.Add("ret_code", value: updip, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 4);

            DBWork.Connection.Query("BA0005", p, commandType: CommandType.StoredProcedure);
            string ret_code = p.Get<OracleString>("ret_code").Value;

            //string errmsg = string.Empty;
            //if (p.Get<OracleString>("O_ERRMSG") != null)
            //{
            //    errmsg = p.Get<OracleString>("O_ERRMSG").Value;
            //}

            return ret_code;
        }

        public string USERID_TO_INID(string userId)
        {
            DynamicParameters p = new DynamicParameters();
            var sql = @"select USER_INID(:USRID) from dual where 1 = 1";
            return DBWork.Connection.ExecuteScalar(sql, new { USRID = userId }, DBWork.Transaction).ToString();
        }
    }
}