using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.H
{
    public class HA0003Repository : JCLib.Mvc.BaseRepository
    {
        public HA0003Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<PH_BANK_AF> GetAll(string agen_bank_14, string bankname, int page_index, int page_size, string sorters) {
            var p = new DynamicParameters();

            string sql = @"
                select agen_bank_14, bankname from PH_BANK_AF
                 where 1=1
            ";
            if (string.IsNullOrEmpty(agen_bank_14) == false) {
                sql += " and agen_bank_14 like :AGEN_BANK_14 ";
                p.Add(":AGEN_BANK_14", string.Format("%{0}%", agen_bank_14));
            }
            if (string.IsNullOrEmpty(bankname) == false)
            {
                sql += " and bankname like :BANKNAME ";
                p.Add(":BANKNAME", string.Format("%{0}%", bankname));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PH_BANK_AF>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public bool CheckExists(string agen_bank_14) {
            string sql = @"
                select 1 from PH_BANK_AF where agen_bank_14 = :agen_bank_14
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { agen_bank_14 }, DBWork.Transaction) != null;
        }

        public int Create(PH_BANK_AF af)
        {
            var sql = @"INSERT INTO PH_BANK_AF (agen_bank_14, bankname, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                VALUES (:AGEN_BANK_14, :BANKNAME, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, af, DBWork.Transaction);
        }
        public int Update(PH_BANK_AF af)
        {
            var sql = @"UPDATE PH_BANK_AF SET bankname = :BANKNAME, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE agen_bank_14 = :AGEN_BANK_14";
            return DBWork.Connection.Execute(sql, af, DBWork.Transaction);
        }
        public bool CheckPhVenderExists(string agen_bank_14) {
            string sql = @"
                select 1 from PH_VENDER
                 where agen_bank_14 = :agen_bank_14
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { agen_bank_14 }, DBWork.Transaction) != null;
        }

        public int Delete(string agen_bank_14)
        {
            // 刪除資料
            var sql = @"delete PH_BANK_AF WHERE agen_bank_14 = :agen_bank_14";
            return DBWork.Connection.Execute(sql, new { agen_bank_14 }, DBWork.Transaction);
        }

        public DataTable GetExcelExample()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>{
                { "銀行代碼", "" },
                { "銀行名稱", "" }
            };
            return ConvertDict2DataTable(dict);
        }

        private DataTable ConvertDict2DataTable(Dictionary<string, string> dict)
        {
            DataTable dt = new DataTable();
            List<string> values = new List<string> { };

            foreach (KeyValuePair<string, string> kp in dict)
            {
                dt.Columns.Add(kp.Key, typeof(string));
                values.Add(kp.Value);
            }
            dt.Rows.Add(values.ToArray());

            return dt;
        }

    }
}