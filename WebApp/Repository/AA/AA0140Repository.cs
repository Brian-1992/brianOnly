using JCLib.DB; //處理跟資料庫連接的函數
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Data;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using WebApp.Models.AA;

namespace WebApp.Repository.AA
{
    public class AA0140Repository : JCLib.Mvc.BaseRepository
    {
        public AA0140Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0140> All(string mmcode)
        {
            DynamicParameters p = new DynamicParameters();
            string sql = @"
                           select MMCODE, SELF_CONT_BDATE, SELF_CONT_EDATE,
                                  SELF_CONTRACT_NO, SELF_PUR_UPPER_LIMIT,
                                  to_char(twn_todate(SELF_CONT_BDATE),'yyyy/mm/dd') as SELF_CONT_BDATE_virtual, --轉為西元年
                                  to_char(twn_todate(SELF_CONT_EDATE),'yyyy/mm/dd') as SELF_CONT_EDATE_virtual --轉為西元年
                             from MED_SELFPUR_DEF
                            where 1=1 ";
            if (string.IsNullOrEmpty(mmcode) == false)
            {
                sql += @"  and MMCODE like :mmcode";
                p.Add("mmcode", string.Format("%{0}%", mmcode));
            }
            return DBWork.PagingQuery<AA0140>(sql, p, DBWork.Transaction);
        }

        public bool CheckExists_Mimast(string mmcode)
        {
            string sql = @"select 1 
                             from MI_MAST 
                            where MMCODE=:mmcode                             
                          ";
            return (DBWork.Connection.ExecuteScalar(sql, new { mmcode = mmcode }, DBWork.Transaction) == null);
        }

        public bool ChkExists_MED_SELFPUR_DEF(string mmcode)
        {
            string sql = @"select 1 
                             from MED_SELFPUR_DEF 
                            where MMCODE=:mmcode                             
                          ";
            return (DBWork.Connection.ExecuteScalar(sql, new { mmcode = mmcode }, DBWork.Transaction) == null);
        }

        public string ChkMaxVids_SELF_CONT_EDATE(string mmcode, string self_cont_edate)
        {
            DynamicParameters p = new DynamicParameters();
            //DB存放民國年，先轉為西元年字串，再至Controller轉為DateTime比較日期合理值
            string sql = @"select to_char(twn_todate(max(SELF_CONT_EDATE)), 'yyyy/MM/dd')
                             from MED_SELFPUR_DEF
                            where MMCODE=:mmcode
                          ";
            if (string.IsNullOrEmpty(self_cont_edate) == false)
            {
                //sql += @"  and SELF_CONT_EDATE<> :self_cont_edate";
                sql += @"  and SELF_CONT_EDATE<>twn_date(to_date(:self_cont_edate, 'yyyy/mm/dd'))";                
                p.Add("self_cont_edate", string.Format("%{0}%", self_cont_edate));
            }
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { mmcode = mmcode, self_cont_edate = self_cont_edate }, DBWork.Transaction); //查詢單一筆資料時用QueryFirstOrDefault
        }

        public string ChkMaxTwn_SELF_CONT_EDATE(string mmcode,string self_cont_edate)
        {
            DynamicParameters p = new DynamicParameters();
            //DB存放民國年
            string sql = @"select max(SELF_CONT_EDATE)
                             from MED_SELFPUR_DEF
                            where MMCODE=:mmcode
                          ";
            if (string.IsNullOrEmpty(self_cont_edate) == false)
            {
                sql += @"  and SELF_CONT_EDATE<> :self_cont_edate";
                p.Add("self_cont_edate", string.Format("%{0}%", self_cont_edate));
            }
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { mmcode = mmcode,self_cont_edate= self_cont_edate }, DBWork.Transaction); //查詢單一筆資料時用QueryFirstOrDefault
        }

        public int Count_MED_SELFPUR_DEF(string mmcode)
        {
            string sql = @"select count(*)
                             from MED_SELFPUR_DEF
                            where MMCODE=:mmcode
                            group by MMCODE
                          ";
            return DBWork.Connection.QueryFirstOrDefault<int>(sql, new { mmcode = mmcode }, DBWork.Transaction); //查詢單一筆資料時用QueryFirstOrDefault
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"
                                    SELECT {0} 
                                           a.mmcode,
                                           a.mmname_c,
                                           a.mmname_e
                                     FROM  MI_MAST a
                                    WHERE  1 = 1 
                                ";

            if (!string.IsNullOrWhiteSpace(p0))
            {
                sql = string.Format(sql, "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(A.MMNAME_E), UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(A.MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql = string.Format("SELECT * FROM ({0}) TMP WHERE 1=1 ", sql);

                sql += " AND (UPPER(MMCODE) LIKE UPPER(:MMCODE) ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

                sql += " OR UPPER(MMNAME_E) LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR UPPER(MMNAME_C) LIKE UPPER(:MMNAME_C)) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public int Create(AA0140 def)
        {
            var sql = @"insert into MED_SELFPUR_DEF (MMCODE,SELF_CONT_BDATE,SELF_CONT_EDATE,
                                                     SELF_CONTRACT_NO, SELF_PUR_UPPER_LIMIT,
                                                     CreateUser, CreateTime, UpdateUser, UpdateTime, UpdateIp)  
                        values (:MMCODE,twn_date(to_date(:SELF_CONT_BDATE, 'yyyy/mm/dd')),twn_date(to_date(:SELF_CONT_EDATE,'yyyy/mm/dd')),
                                :SELF_CONTRACT_NO,:SELF_PUR_UPPER_LIMIT,
                                :CreateUser, sysdate, :UpdateUser, sysdate, :UpdateIp)";
            return DBWork.Connection.Execute(sql, def, DBWork.Transaction);
        }
        
        public int Create_import(AA0140 def)
        {
            var sql = @"insert into MED_SELFPUR_DEF (MMCODE,SELF_CONT_BDATE,SELF_CONT_EDATE,
                                                     SELF_CONTRACT_NO, SELF_PUR_UPPER_LIMIT,
                                                     CreateUser, CreateTime, UpdateUser, UpdateTime, UpdateIp)  
                        values (:MMCODE,:SELF_CONT_BDATE,:SELF_CONT_EDATE,
                                :SELF_CONTRACT_NO,:SELF_PUR_UPPER_LIMIT,
                                :CreateUser, sysdate, :UpdateUser, sysdate, :UpdateIp)";
            return DBWork.Connection.Execute(sql, def, DBWork.Transaction);
        }

        public int Delete_import(string mmcode, string self_cont_bdate)
        {
            string sql = @"delete from MED_SELFPUR_DEF
                            where MMCODE=:mmcode   
                              and SELF_CONT_BDATE= :self_cont_bdate
                          ";
            return DBWork.Connection.Execute(sql, new { mmcode = mmcode, self_cont_bdate = self_cont_bdate }, DBWork.Transaction);
        }

        public IEnumerable<AA0140> Get(string mmcode)
        {
            string sql = @"
                           select MMCODE, SELF_CONT_BDATE, SELF_CONT_EDATE,
                                  SELF_CONTRACT_NO, SELF_PUR_UPPER_LIMIT
                             from MED_SELFPUR_DEF
                            where 1=1 
                              and mmcode = :mmcode
                           ";
            return DBWork.PagingQuery<AA0140>(sql, new { mmcode = mmcode }, DBWork.Transaction);
        }

        public int Update(AA0140 def)
        {
            var sql = @"update MED_SELFPUR_DEF 
                           set SELF_CONT_BDATE = twn_date(to_date(:SELF_CONT_BDATE, 'yyyy/mm/dd')), 
                               SELF_CONT_EDATE = twn_date(to_date(:SELF_CONT_EDATE, 'yyyy/mm/dd')), 
                               SELF_CONTRACT_NO= :SELF_CONTRACT_NO,
                               SELF_PUR_UPPER_LIMIT= :SELF_PUR_UPPER_LIMIT,
                               UpdateUser = :UpdateUser,
                               UpdateTime = sysdate,
                               UpdateIp = :UpdateIp
                         where MMCODE = :mmcode
                           and SELF_CONT_BDATE=twn_date(to_date(:SELF_CONT_BDATE_virtual, 'yyyy/mm/dd'))
                        ";
            return DBWork.Connection.Execute(sql, def, DBWork.Transaction);
        }

        public int Delete (string mmcode,string self_cont_bdate)
        {
            string sql = @"delete from MED_SELFPUR_DEF
                            where MMCODE=:mmcode   
                              and SELF_CONT_BDATE= twn_date(to_date(:self_cont_bdate, 'yyyy/mm/dd')）
                          ";
            return DBWork.Connection.Execute(sql, new { mmcode = mmcode, self_cont_bdate= self_cont_bdate }, DBWork.Transaction);
        }
        public DataTable GetExcel(string mmcode)
        {
            DynamicParameters p = new DynamicParameters();
            string sql = @"
                           select MMCODE as 院內碼, 
                                  SELF_CONT_BDATE as 藥品契約生效起日, 
                                  SELF_CONT_EDATE as 藥品契約生效迄日,
                                  SELF_CONTRACT_NO as 合約案號,
                                  SELF_PUR_UPPER_LIMIT as 採購上限金額
                             from MED_SELFPUR_DEF
                            where 1=1 ";
            if (string.IsNullOrEmpty(mmcode) == false)
            {
                sql += @"  and MMCODE like :mmcode";
                p.Add("mmcode", string.Format("%{0}%", mmcode));
            }

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
    }
}