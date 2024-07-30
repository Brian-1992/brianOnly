using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using TSGH.Models;

namespace WebApp.Repository.AB
{
    public class AB0048Repository : JCLib.Mvc.BaseRepository
    {
        public AB0048Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        // GET api/<controller>
        public IEnumerable<ME_UIMAST> GetAll(string wh_no, string mmcode,string mmcode2, string ctdmdcode, bool showDifferOnly, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.WH_NO,A.MMCODE, B.MMNAME_E, A.PACK_UNIT, A.PACK_QTY,
                               a.pack_times, C.CTDMDCCODE,
                               (SELECT DATA_DESC FROM PARAM_D 
                                 WHERE GRP_CODE='MI_WINVCTL' AND DATA_NAME='CTDMDCCODE' AND DATA_VALUE=C.CTDMDCCODE) CTDMDCCODE_N
                               --,
                               --(select 'N'
                               --   from ME_UIMAST
                               --  where wh_no = 'PH1S'
                               --    and mmcode = a.mmcode
                               --    and ((pack_qty = a.pack_qty and pack_unit = a.pack_unit) or
                               --         (pack_qty1 = a.pack_qty and pack_unit1 = a.pack_unit) or
                               --         (pack_qty2 = a.pack_qty and pack_unit2 = a.pack_unit) or
                               --         (pack_qty3 = a.pack_qty and pack_unit3 = a.pack_unit) or
                               --         (pack_qty4 = a.pack_qty and pack_unit4 = a.pack_unit) or
                               --         (pack_qty5 = a.pack_qty and pack_unit5 = a.pack_unit)
                               --        )
                               --) as differ
                          FROM ME_UIMAST A
                         INNER JOIN MI_MAST B ON A.MMCODE = B.MMCODE 
                          LEFT OUTER JOIN MI_WINVCTL C ON C.WH_NO=A.WH_NO AND C.MMCODE=A.MMCODE
                         WHERE 1 = 1 ";

            if (wh_no != "")
            {
                sql += " AND a.WH_NO = :p0 ";
                p.Add(":p0", string.Format("{0}", wh_no));
            }
            if (mmcode != "" & mmcode2 != "")
            {
                sql += " AND a.MMCODE BETWEEN :p1 AND :p2 ";
                p.Add(":p1", string.Format("{0}", mmcode));
                p.Add(":p2", string.Format("{0}", mmcode2));
            }
            if (mmcode != "" & mmcode2 == "")
            {
                sql += " AND a.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("%{0}%", mmcode));
            }
            if (mmcode == "" & mmcode2 != "")
            {
                sql += " AND a.MMCODE LIKE :p2 ";
                p.Add(":p2", string.Format("%{0}%", mmcode2));
            }

            if (ctdmdcode != "")
            {
                sql += " AND C.CTDMDCCODE=:p3  ";
                p.Add(":p3", string.Format("{0}", ctdmdcode));
            }

            sql = string.Format(@"
                        select A.WH_NO,A.MMCODE, a.MMNAME_E, A.PACK_UNIT, A.PACK_QTY,
                               a.pack_times, a.CTDMDCCODE, a.CTDMDCCODE_N
                               --,
                               --(case when a.differ = 'N' then 'N' else 'Y' end) as differ
                          from (
                                    {0}
                               ) a
                         where 1=1
                    ", sql);
            if (showDifferOnly) {
                sql += "   and a.differ is null";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_UIMAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public int Create(ME_UIMAST me_uimast)
        {
            var sql = @"INSERT INTO ME_UIMAST (WH_NO, MMCODE, PACK_UNIT, PACK_QTY,CREATE_TIME, CREATE_ID,UPDATE_TIME,UPDATE_ID,UPDATE_IP, PACK_TIMES)  
                                VALUES (:WH_NO,:MMCODE,:PACK_UNIT,:PACK_QTY, SYSDATE, :CREATE_USER,SYSDATE, :CREATE_USER,:UPDATE_IP, :PACK_TIMES)";
            return DBWork.Connection.Execute(sql, me_uimast, DBWork.Transaction);
        }

        public int Update(ME_UIMAST me_uimast)
        {
            var sql = @"UPDATE ME_UIMAST SET PACK_UNIT = :PACK_UNIT, PACK_QTY = :PACK_QTY,  UPDATE_TIME = SYSDATE, UPDATE_ID = :UPDATE_USER, UPDATE_IP = :UPDATE_IP,
                                             PACK_TIMES = :PACK_TIMES
                                WHERE WH_NO = :WH_NO and MMCODE = :MMCODE ";
            return DBWork.Connection.Execute(sql, me_uimast, DBWork.Transaction);
        }
        public IEnumerable<MI_WHMAST> GetWH_NOComboNotOne(string userId, string wh_no, int page_index, int page_size, string sorters) //AB0036
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"select {0} A.WH_NO , WH_NAME 
                             from MI_WHMAST A, MI_WHID B
                            WHERE A.WH_NO=B.WH_NO 
                              and b.wh_userid = :userId";
            p.Add(":userId", string.Format("{0}", userId));

            if (wh_no != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.WH_NO, :WH_NO), 1000) + NVL(INSTR(A.WH_NAME, :WH_NAME_I), 100) * 10) IDX,");
                p.Add(":WH_NO", wh_no);
                p.Add(":WH_NAME_I", wh_no);

                sql += " AND (A.WH_NO LIKE :WH_NO ";
                p.Add(":WH_NO", string.Format("{0}%", wh_no));

                sql += " OR A.WH_NAME LIKE :WH_NAME ) ";
                p.Add(":WH_NAME", string.Format("%{0}%", wh_no));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY WH_NO ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_WHMAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
            //string sql = @"SELECT DISTINCT A.WH_NO, WH_NAME from MI_WHMAST A, MI_WHID B WHERE A.WH_NO = B.WH_NO
            //              AND B.WH_USERID =:USERID";
            //return DBWork.Connection.Query<MI_WHMAST>(sql, new { USERID = userId }, DBWork.Transaction);
        }
        public int Delete(string wh_no, string mmcode, string packunit)
        {
            // 刪除資料
            var sql = @"DELETE ME_UIMAST WHERE WH_NO = :WH_NO and MMCODE = :MMCODE and PACK_UNIT = :PACK_UNIT";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = mmcode, PACK_UNIT = packunit }, DBWork.Transaction);
        }

        public bool CheckExists(string WH_NO, string MMCODE)
        {
            string sql = @"SELECT 1 FROM ME_UIMAST WHERE WH_NO=:WH_NO and MMCODE=:MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = WH_NO, MMCODE = MMCODE }, DBWork.Transaction) == null);
        }

        public IEnumerable<ME_UIMAST> Get(string wh_no, string mmcode)
        {
            var sql = @"SELECT A.WH_NO,A.MMCODE, B.MMNAME_E, A.PACK_UNIT, A.PACK_QTY, A.PACK_TIMES FROM ME_UIMAST A, MI_MAST B
                       WHERE A.MMCODE = B.MMCODE AND A.WH_NO = :WH_NO and A.MMCODE = :MMCODE  ";
            return DBWork.Connection.Query<ME_UIMAST>(sql, new { WH_NO = wh_no, MMCODE = mmcode}, DBWork.Transaction);
        }
        public IEnumerable<MI_WHMAST> GetWH_NOComboOne(string userId)      //AA0048
        {
            string sql = @"SELECT a.WH_NO,b.WH_NAME FROM MI_WHID a JOIN MI_WHMAST b ON (a.WH_NO = b.WH_NO) WHERE a.WH_USERID= :WH_USERID";
            return DBWork.Connection.Query<MI_WHMAST>(sql, new { WH_USERID = userId }, DBWork.Transaction);
        }

        public IEnumerable<ME_UIMAST> GetUnitCombo(string p0, int page_index, int page_size, string sorters)
        {
            //string sql = "";
            var p = new DynamicParameters();
            string sql = @"SELECT PACK_UNIT,PACK_QTY FROM V_ME_UIMAST WHERE WH_NO = 'PH1S' AND MMCODE = :MMCODE";

            p.Add(":MMCODE", p0);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<ME_UIMAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<MI_WHMAST> GetWH_NOCombo(string userId) //AB0036
        {
            string sql = @"SELECT A.WH_NO,WH_NAME  FROM MI_WHMAST A, MI_WHID B WHERE A.WH_NO = B.WH_NO  AND B.WH_USERID =:INID ";
            return DBWork.Connection.Query<MI_WHMAST>(sql, new { INID = userId }, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMmcodeCombo2(string mmcode)
        {
            var p = new DynamicParameters();
            string sql = @" SELECT Trim(MMCODE) as MMCODE, case when Trim(MMNAME_C) is not null then Trim(MMNAME_C) else Trim(MMNAME_E) end as MMNAME_E, 
                        Trim(MMCODE) || ' ' || (case when Trim(MMNAME_C) is not null then Trim(MMNAME_C) else Trim(MMNAME_E) end) as MMNAME_C
                        FROM MI_MAST ";
            if (mmcode != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", mmcode);
                p.Add(":MMNAME_E_I", mmcode);
                p.Add(":MMNAME_C_I", mmcode);

                sql += " WHERE ( MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", mmcode));

                sql += " OR MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", mmcode));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", mmcode));

               // sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += "  where ROWNUM <= 1000 ORDER BY MMCODE ";
            }
            return DBWork.Connection.Query<MI_MAST>(sql);
        }
        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, string p1, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,A.M_CONTPRICE    
                        FROM MI_MAST A 
                        WHERE  A.MAT_CLASS = '01'  ";
            if (p1 != "")
            {
                sql += " AND EXISTS (SELECT 1 FROM MI_WHINV B WHERE A.MMCODE = B.MMCODE AND B.INV_QTY > 0 AND B.WH_NO = :WH_NO ) ";
                p.Add(":WH_NO", p1);
            }
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

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetCtdmdCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_WINVCTL' AND DATA_NAME='CTDMDCCODE' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public bool CheckPackExists(string mmcode, string pack_unit, string pack_qty) {
            string sql = @"
                select 1
                  from V_ME_UIMAST a
                 where wh_no = 'PH1S'
                   and mmcode = :mmcode
                   and ((pack_qty = :pack_qty and pack_unit = :pack_unit)
                       )
            ";
            return (DBWork.Connection.ExecuteScalar(sql, new { mmcode, pack_unit, pack_qty }, DBWork.Transaction)) != null;
        }

        public IEnumerable<ME_UIMAST> GetDifferList(string wh_no, string ctdmdccode) {
            string sql = string.Format(@"
                select a.wh_no, A.MMCODE, a.MMNAME_E, A.PACK_UNIT as PACK_UNIT_ori, A.PACK_QTY as PACK_QTY_ori,
                       a.pack_times as pack_times_ori, a.CTDMDCCODE, a.CTDMDCCODE_N ,
                       (to_number(pack_qty) * to_number(pack_times)) as total
                  from (
                        SELECT A.WH_NO,A.MMCODE, B.MMNAME_E, A.PACK_UNIT, A.PACK_QTY,
                               a.pack_times, C.CTDMDCCODE,
                               (SELECT DATA_DESC FROM PARAM_D 
                                 WHERE GRP_CODE='MI_WINVCTL' AND DATA_NAME='CTDMDCCODE' AND DATA_VALUE=C.CTDMDCCODE) CTDMDCCODE_N
                               ,
                               ( case when exists (select 1
                                                     from V_ME_UIMAST
                                                    where wh_no = 'PH1S'
                                                      and mmcode = a.mmcode
                                                      and pack_unit = a.pack_unit and pack_qty = a.pack_qty)
                                  then 'N' else null end) as differ
                          FROM ME_UIMAST A
                         INNER JOIN MI_MAST B ON A.MMCODE = B.MMCODE 
                          LEFT OUTER JOIN MI_WINVCTL C ON C.WH_NO=A.WH_NO AND C.MMCODE=A.MMCODE
                         WHERE 1 = 1
                           and a.wh_no = :wh_no
                           {0}
                       ) a
              where 1=1
                and differ is null
            ", string.IsNullOrEmpty(ctdmdccode) ? string.Empty : "  and c.ctdmdccode = :ctdmdccode");
            return DBWork.PagingQuery<ME_UIMAST>(sql, new { wh_no, ctdmdccode }, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetPackUnitsCombo(string mmcode) {
            string sql = @"SELECT PACK_UNIT as VALUE, PACK_UNIT as TEXT,
                                  PACK_QTY as EXTRA1
                             FROM V_ME_UIMAST WHERE WH_NO = 'PH1S' AND MMCODE = :mmcode";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { mmcode }, DBWork.Transaction);
        }

        public int UpdateUimastByList(string wh_no, string mmcode, string pack_unit, string pack_qty, string pack_times, string userId, string ip) {
            string sql = @"
                update ME_UIMAST
                   set pack_unit = :pack_unit,
                       pack_qty = :pack_qty,
                       pack_times = :pack_times,
                       update_time = sysdate,
                       update_id = :userId,
                       update_ip = :ip
                 where wh_no = :wh_no
                   and mmcode = :mmcode
            ";
            return DBWork.Connection.Execute(sql, new { wh_no, mmcode, pack_unit, pack_qty, pack_times, userId, ip }, DBWork.Transaction);
        }
    }
}