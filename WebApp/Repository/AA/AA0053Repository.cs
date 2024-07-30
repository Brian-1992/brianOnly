using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using TSGH.Models;
using System.Text;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
namespace WebApp.Repository.AA
{
    public class AA0053Repository : JCLib.Mvc.BaseRepository
    {
        public AA0053Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        // GET api/<controller>
        public IEnumerable<ME_UIMAST> GetAll(string mmcode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT ''WH_NO,MMCODE,MMCODE_NAME(MMCODE) MMNAME_E FROM ME_UIMAST WHERE WH_NO = 'PH1S' ";

            if (mmcode != "")
            {
                sql += " AND MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", mmcode));
            }
           

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_UIMAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ME_UIMAST> GetAllD(string mmcode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT MMCODE,MMCODE_NAME(MMCODE) MMNAME_E,
                               PACK_UNIT0,PACK_QTY0,
                               PACK_UNIT1,PACK_QTY1,
                               PACK_UNIT2,PACK_QTY2,
                               PACK_UNIT3,PACK_QTY3,
                               PACK_UNIT4,PACK_QTY4,
                               PACK_UNIT5,PACK_QTY5
                               FROM ME_UIMAST
                               WHERE WH_NO = 'PH1S'
                                 AND MMCODE = :p0";

            p.Add(":p0", string.Format("{0}", mmcode));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_UIMAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public int Create(ME_UIMAST me_uimast)
        {
            var sql = @"INSERT INTO ME_UIMAST (WH_NO, MMCODE, CREATE_TIME, CREATE_ID,UPDATE_TIME,UPDATE_ID,UPDATE_IP)  
                                VALUES (:WH_NO,:MMCODE, SYSDATE, :CREATE_USER,SYSDATE, :CREATE_USER,:UPDATE_IP)";
            return DBWork.Connection.Execute(sql, me_uimast, DBWork.Transaction);
        }

        public int Update(ME_UIMAST me_uimast)
        {
            var sql = @"UPDATE ME_UIMAST SET 
                            PACK_UNIT0 = :PACK_UNIT0, 
                            PACK_QTY0 = :PACK_QTY0, 
                            PACK_UNIT1 = :PACK_UNIT1, 
                            PACK_QTY1 = :PACK_QTY1, 
                            PACK_UNIT2 = :PACK_UNIT2, 
                            PACK_QTY2 = :PACK_QTY2, 
                            PACK_UNIT3 = :PACK_UNIT3, 
                            PACK_QTY3 = :PACK_QTY3, 
                            PACK_UNIT4 = :PACK_UNIT4, 
                            PACK_QTY4 = :PACK_QTY4, 
                            PACK_UNIT5 = :PACK_UNIT5, 
                            PACK_QTY5 = :PACK_QTY5, 
                            UPDATE_TIME = SYSDATE, UPDATE_ID = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                            WHERE WH_NO = :WH_NO and MMCODE = :MMCODE ";
            return DBWork.Connection.Execute(sql, me_uimast, DBWork.Transaction);
        }
        public IEnumerable<MI_WHMAST> GetWH_NOComboNotOne(string userId) //AB0036
        {
            string sql = @"SELECT DISTINCT A.WH_NO, WH_NAME from MI_WHMAST A, MI_WHID B WHERE A.WH_NO = B.WH_NO
                          AND B.WH_USERID =:USERID";
            return DBWork.Connection.Query<MI_WHMAST>(sql, new { USERID = userId }, DBWork.Transaction);
        }
        public int Delete(string wh_no, string mmcode, string packunit)
        {
            // 刪除資料
            var sql = @"DELETE ME_UIMAST WHERE WH_NO = :WH_NO and MMCODE = :MMCODE and PACK_UNIT = :PACK_UNIT";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = mmcode,PACK_UNIT=packunit }, DBWork.Transaction);
        }

        public bool CheckExists(string WH_NO, string MMCODE)
        {
            string sql = @"SELECT 1 FROM ME_UIMAST WHERE WH_NO=:WH_NO and MMCODE=:MMCODE ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = WH_NO, MMCODE = MMCODE }, DBWork.Transaction) == null);
        }

        public IEnumerable<ME_UIMAST> Get(string wh_no, string mmcode)
        {
            var sql = @"SELECT '' WH_NO,A.MMCODE, B.MMNAME_E,
                        PACK_UNIT0,PACK_QTY0,
                        PACK_UNIT1,PACK_QTY1,
                        PACK_UNIT2,PACK_QTY2,
                        PACK_UNIT3,PACK_QTY3,
                        PACK_UNIT4,PACK_QTY4,
                        PACK_UNIT5,PACK_QTY5
                        FROM ME_UIMAST A, MI_MAST B
                        WHERE A.MMCODE = B.MMCODE 
                          AND A.WH_NO = :WH_NO 
                          and A.MMCODE = :MMCODE  ";
            return DBWork.Connection.Query<ME_UIMAST>(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction);
        }
        public IEnumerable<MI_WHMAST> GetWH_NOComboOne(string userId)      //AA0048
        {
            string sql = @"SELECT a.WH_NO,b.WH_NAME FROM MI_WHID a JOIN MI_WHMAST b ON (a.WH_NO = b.WH_NO) WHERE a.WH_USERID= :WH_USERID";
            return DBWork.Connection.Query<MI_WHMAST>(sql, new { WH_USERID = userId }, DBWork.Transaction);
        }
        public IEnumerable<MI_MAST> GetMMCODECombo(string p0, string p1, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E 
                        FROM MI_MAST A,MI_WHINV B 
                        WHERE A.MMCODE=B.MMCODE AND A.MAT_CLASS = '01' 
                          AND B.INV_QTY <> 0 AND B.WH_NO = :WH_NO";

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR MMNAME_E LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY MMCODE ";
            }
            p.Add(":WH_NO", p1);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<MI_MAST> GetMMCODECombo2(string p0, string p1, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E 
                        FROM MI_MAST A,MI_WHINV B 
                        WHERE A.MMCODE=B.MMCODE AND A.MAT_CLASS = '01' 
                          AND B.INV_QTY <> 0 AND B.WH_NO = :WH_NO
                          AND NOT EXISTS(SELECT 'X' FROM ME_UIMAST C WHERE C.WH_NO=B.WH_NO AND C.MMCODE = A.MMCODE)";

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR MMNAME_E LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY MMCODE ";
            }
            p.Add(":WH_NO", p1);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        
    }
}