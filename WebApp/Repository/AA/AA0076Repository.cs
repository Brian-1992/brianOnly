using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using WebApp.Models;
using Dapper;
using System.Data;

namespace WebApp.Repository.AA
{
    public class AA0076Repository : JCLib.Mvc.BaseRepository
    {
        public AA0076Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MI_WEXPINV> GetAll(string wh_no, string mmcode, string lot_no, string exp_date1,string storehourse,string e_vaccine, int page_index, int page_size, string sorters, string wh_userId)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT a.WH_NO AS WH_NO, 
                               b.WH_NAME AS WH_NAME,
                               b.WH_NAME AS WH_NAME_DISPLAY,                                
                               a.MMCODE AS MMCODE,
                               c.MMNAME_C AS MMNAME_C,
                               c.MMCODE || ' ' || c.MMNAME_C AS MMCODE_DISPLAY,
                               c.MMCODE || ' ' || c.MMNAME_C AS MMCODE_TEXT,
                               c.MMNAME_E AS  MMNAME_E,
                               a.LOT_NO AS LOT_NO,
                               TWN_DATE(a.EXP_DATE) AS EXP_DATE,
                               TWN_DATE(a.EXP_DATE) AS ORI_EXP_DATE,
                               a.INV_QTY AS INV_QTY 
                          FROM MI_WEXPINV a, MI_WHMAST b, MI_MAST c
                         WHERE 1=1 ";

            if (wh_no != "")
            {
                sql += " AND a.WH_NO =:p0 ";
                p.Add(":p0", string.Format("{0}", wh_no));
            }
            if (mmcode != "")
            {
                sql += " AND a.MMCODE = :p1 ";
                p.Add(":p1", string.Format("{0}", mmcode));
            }
            if (lot_no != "")
            {
                sql += " AND a.LOT_NO LIKE :p2 ";
                p.Add(":p2", string.Format("%{0}%", lot_no));
            }
            if (exp_date1 != "")
            {
                sql += "AND To_char(TWN_DATE(a.EXP_DATE)) = :p3 ";
                //sql += " AND a.EXP_DATE BETWEEN TWN_TODATE(:p3) AND TWN_TODATE(:p4) ";
                p.Add(":p3", exp_date1);
            }

            if (storehourse != "")
            {
                if(storehourse == "0")
                {
                    sql += "AND a.INV_QTY = 0 ";
                }
                else if(storehourse == "1")
                {
                    sql += "AND a.INV_QTY <> 0 ";
                }
            }

            if (string.IsNullOrEmpty(e_vaccine) == false) {
                if (e_vaccine == "Y")
                {
                    sql += "AND nvl(c.E_VACCINE, 'N') = 'Y' ";
                }
                else
                {
                    sql += "AND nvl(c.E_VACCINE, 'N') <> 'Y' ";
                }
            }
            sql += " AND b.WH_NO = a.WH_NO ";
            sql += " AND c.MMCODE = a.MMCODE";
            //sql += @" AND EXISTS (
            //                         SELECT*
            //                           FROM MI_WHID b
            //                          WHERE a.WH_NO = b.WH_NO
            //                            AND WH_USERID = :WH_USERID
            //                )";
            //p.Add(":WH_USERID", string.Format("{0}", wh_userId));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_WEXPINV>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_WEXPINV> Get(string wh_no, string mmcode, string lot_no, string exp_date)
        {
            var sql = @"SELECT a.WH_NO AS WH_NO, 
                               b.WH_NAME AS WH_NAME,
                               b.WH_NAME AS WH_NAME_DISPLAY,  
                               a.MMCODE AS MMCODE,
                               c.MMNAME_C AS MMNAME_C,
                               c.MMNAME_C AS MMCODE_DISPLAY,
                               c.MMNAME_C AS MMCODE_TEXT,
                               a.LOT_NO AS LOT_NO,
                               a.EXP_DATE AS EXP_DATE,
                               a.INV_QTY AS INV_QTY
                          FROM MI_WEXPINV a, MI_WHMAST b, MI_MAST c 
                         WHERE a.WH_NO = :wh_no 
                           AND a.MMCODE = :mmcode
                           AND a.LOT_NO = :lot_no
                           AND TRUNC(a.EXP_DATE, 'DD') = TO_DATE(:exp_date,'YYYY-MM-DD') 
                           AND b.WH_NO = a.WH_NO
                           AND c.MMCODE = a.MMCODE";
            return DBWork.Connection.Query<MI_WEXPINV>(sql,
                                                       new {
                                                           WH_NO = wh_no,
                                                           MMCODE = mmcode,
                                                           LOT_NO = lot_no,
                                                           EXP_DATE = string.Format("{0}", DateTime.Parse(exp_date).ToString("yyyy-MM-dd"))
                                                       }, DBWork.Transaction);
        }

        public int Create(MI_WEXPINV mi_wexpinv)
        {

            //var sql = @"INSERT INTO MI_WEXPINV (WH_NO, MMCODE, EXP_DATE, LOT_NO, INV_QTY, CREATE_DATE, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
            //            VALUES (:WH_NO, :MMCODE, TO_DATE(:EXP_DATE,'YYYY-MM-DD'), :LOT_NO, :INV_QTY, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";

            var sql = @"INSERT INTO MI_WEXPINV (WH_NO, MMCODE, EXP_DATE, LOT_NO, INV_QTY, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                        VALUES (:WH_NO, :MMCODE, TO_DATE(:EXP_DATE,'YYYY-MM-DD'), :LOT_NO, :INV_QTY,  :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, mi_wexpinv, DBWork.Transaction);
        }

        public int Update(MI_WEXPINV mi_wexpinv)
        {
            var sql = @"UPDATE MI_WEXPINV 
                           SET INV_QTY = :INV_QTY, EXP_DATE = TO_DATE(:exp_date,'YYYY-MM-DD'), 
                               DOCNO = '',
                               UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                         WHERE WH_NO = :wh_no 
                           AND MMCODE = :mmcode 
                           AND TRUNC(EXP_DATE, 'DD') = TWN_TODATE(:ori_exp_date)
                           AND LOT_NO = :lot_no";
            return DBWork.Connection.Execute(sql, mi_wexpinv, DBWork.Transaction);
        }

        public int Delete(string wh_no, string mmcode, string lot_no, string exp_date)
        {
            var sql = @"DELETE FROM MI_WEXPINV
                         WHERE WH_NO = :wh_no 
                           AND MMCODE = :mmcode 
                           AND LOT_NO = :lot_no 
                           AND TRUNC(EXP_DATE, 'DD') = TO_DATE(:exp_date,'YYYY-MM-DD')";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = mmcode, LOT_NO = lot_no, EXP_DATE = exp_date }, DBWork.Transaction);
        }

        public bool CheckExists(string wh_no, string mmcode, string lot_no, string exp_date)
        {
            string sql = @"SELECT 1 FROM MI_WEXPINV 
                            WHERE WH_NO = :wh_no 
                              AND MMCODE = :mmcode 
                              AND LOT_NO = :lot_no 
                              AND TRUNC(EXP_DATE, 'DD') = TO_DATE(:exp_date,'YYYY-MM-DD')"; // TRUNC(a.EXP_DATE, 'DD') TWN_TODATE(:exp_date)
            return !(DBWork.Connection.ExecuteScalar(sql, 
                                                     new { WH_NO = wh_no,
                                                           MMCODE = mmcode,
                                                           LOT_NO = lot_no,
                                                           EXP_DATE = string.Format("{0}", DateTime.Parse(exp_date).ToString("yyyy-MM-dd")) }, 
                                                     DBWork.Transaction) == null);
        }

        public String GetUserKind(string userid)
        {
            DynamicParameters p = new DynamicParameters();
            string sql = @"Select USER_KIND(:userid)  from dual";
            p.Add(":userid", userid);

            return DBWork.Connection.ExecuteScalar(sql, p, DBWork.Transaction).ToString();
        }

        public IEnumerable<ComboItemModel> GetWhnoCombo(string wh_userId)
        {
            string sql = @"SELECT WH_NO AS VALUE, WH_NO || ' ' || WH_NAME AS TEXT
                             FROM MI_WHMAST a 
                            WHERE EXISTS (
                                     SELECT *
                                       FROM MI_WHID b
                                      WHERE a.WH_NO = b.WH_NO
                                        AND WH_USERID = :WH_USERID
                            )
                            ORDER BY VALUE";
            return DBWork.Connection.Query<ComboItemModel>(sql, new { WH_USERID = wh_userId }, DBWork.Transaction);
        }
        public IEnumerable<COMBO_MODEL> Getst_LOT_NO(string mmcode)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"SELECT LOT_NO as  VALUE, LOT_NO as TEXT FROM PH_LOTNO where mmcode = :mmcode";
            p.Add(":mmcode", mmcode);

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> Getst_EXP_DATE(string mmcode,string lot_no)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"SELECT TWN_DATE(EXP_DATE) as  VALUE, TWN_DATE(EXP_DATE) as TEXT FROM PH_LOTNO where 1=1  ";
            if(mmcode != "")
            {
                sql += "and mmcode = :mmcode ";
            }
            if (lot_no != "")
            {
                sql += "and lot_no = :lot_no ";
            }
            p.Add(":mmcode", mmcode);
            p.Add(":lot_no", lot_no);

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }

        
        public IEnumerable<ComboItemModel> GetWhnoCombo_S(string wh_userId)
        {
            string sql = @"  SELECT distinct b.WH_NO AS VALUE, b.WH_NO || ' ' || b.WH_NAME AS TEXT, b.wh_kind as EXTRA1, b.wh_grade as extra2
                             FROM (SELECT DISTINCT WH_NO 
                             FROM MI_WHID 
                             WHERE WH_USERID= :WH_USERID 
                             UNION 
                             SELECT DISTINCT WH_NO 
                             FROM MI_WHMAST 
                             WHERE INID=USER_INID(:WH_USERID) AND WH_KIND='1') a JOIN MI_WHMAST b ON a.WH_NO = b.WH_NO
                            order by VALUE";
            return DBWork.Connection.Query<ComboItemModel>(sql, new { WH_USERID = wh_userId }, DBWork.Transaction);
        }
        public IEnumerable<ComboItemModel> GetWhnoCombo_1OrElse(string wh_userId)
        {
            string sql = @"    SELECT distinct b.WH_NO AS VALUE, b.WH_NO || ' ' || b.WH_NAME AS TEXT , b.wh_kind as EXTRA1, b.wh_grade as extra2
                                 FROM (SELECT DISTINCT WH_NO 
                                         FROM MI_WHID 
                                        WHERE WH_USERID= :WH_USERID )  a JOIN MI_WHMAST b ON a.WH_NO = b.WH_NO 
                                order by VALUE";
            return DBWork.Connection.Query<ComboItemModel>(sql, new { WH_USERID = wh_userId }, DBWork.Transaction);
        }



        public IEnumerable<MI_MAST> GetMMCODECombo(string mmcode, string wh_no, int page_index, int page_size, string sorters)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"SELECT {0} A.MMCODE , A.MMNAME_C, A.MMNAME_E " +
                "from MI_MAST A, MI_WHINV B WHERE A.MMCODE=B.MMCODE ";

            if(wh_no!="" || wh_no != null)
            {
                sql += "AND B.WH_NO= :wh_no ";
                p.Add(":wh_no", wh_no);
            }

            if (mmcode != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", mmcode);
                p.Add(":MMNAME_E_I", mmcode);
                p.Add(":MMNAME_C_I", mmcode);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", mmcode));

                sql += " OR MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", mmcode));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", mmcode));

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

        public IEnumerable<MI_MAST> GetMMCODECombo_else(string mmcode, string wh_no,string userid, int page_index, int page_size, string sorters)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"SELECT {0} B.MMCODE , B.MMNAME_C, B.MMNAME_E " +
                "from MI_WHINV A, MI_MAST  B WHERE  A.MMCODE=B.MMCODE ";

            if (wh_no != "" || wh_no != null)
            {
                sql += "AND A.WH_NO= :wh_no ";
                p.Add(":wh_no", wh_no);
            }
            sql = sql + @"AND EXISTS 
          (SELECT 1 FROM MI_MAST WHERE MMCODE = A.MMCODE 
            AND MAT_CLASS IN (SELECT MAT_CLASS FROM MI_MATCLASS WHERE MAT_CLSID = USER_KIND(:userid)))";

            p.Add(":userid", userid);

            if (mmcode != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", mmcode);
                p.Add(":MMNAME_E_I", mmcode);
                p.Add(":MMNAME_C_I", mmcode);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", mmcode));

                sql += " OR MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", mmcode));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", mmcode));

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

        public bool CheckWhnoExists(string wh_no)
        {
            string sql = @"SELECT 1 FROM MI_WHMAST 
                            WHERE WH_NO = :wh_no ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = wh_no }, DBWork.Transaction) == null);
        }

        public bool CheckMmcodeExists(string mmcode)
        {
            string sql = @"SELECT 1 FROM MI_MAST 
                            WHERE MMCODE = :mmcode ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode }, DBWork.Transaction) == null);
        }
        public bool CheckMeexpmMatch(string mmcode, string lot_no, string exp_date)
        {

            string sql = @"SELECT 1 FROM ME_EXPM
                            WHERE MMCODE = :MMCODE
                              AND LOTNO = :LOTNO
                              AND TRUNC(EXPDATE, 'DD') = TO_DATE(:exp_date,'YYYY-MM-DD')";
            var req = new
            {
                MMCODE = mmcode,
                LOTNO = lot_no,
                EXPDATE = string.Format("{0}", DateTime.Parse(exp_date).ToString("yyyy-MM-dd"))
            };
            return !(DBWork.Connection.ExecuteScalar(sql, req, DBWork.Transaction) == null);
        }


        public class AA0076_MM_MATMAST
        {
            public string MMCODE { get; set; }
            public string MMNAME_C { get; set; }
            public string MMNAME_E { get; set; }
        }
        public IEnumerable<AA0076_MM_MATMAST> GetMmdataByMmcode(string mmcode)
        {
            string sql = @"select MMCODE, MMNAME_C, MMNAME_E
                             from MI_MAST
                            where MMCODE=:MMCODE";
            return DBWork.Connection.Query<AA0076_MM_MATMAST>(sql, new { MMCODE = mmcode }, DBWork.Transaction);
        }
        public bool CheckWhmmExist(string mmcode, string wh_no)
        {
            string sql = @"SELECT 1
                             FROM MI_MAST a, MI_WINVCTL b
                            WHERE 1=1
                              AND a.MMCODE = :MMCODE 
                              AND b.MMCODE = a.MMCODE
                              AND b.WH_NO = :WH_NO";

            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode, WH_NO = wh_no}, DBWork.Transaction) == null);
        }

        public DataTable GetExcel(string wh_no, string userId)
        {
            var p = new DynamicParameters();


            var sql = @"SELECT A.WH_NO as 庫房代碼, A.MMCODE as 院內碼, TO_CHAR(TO_DATE(A.EXP_DATE),'YYYY-MM-DD') as 效期, A.LOT_NO as 批號, A.INV_QTY as 效期批號庫存數量, 
                        (SELECT INV_QTY FROM MI_WHINV 
                        WHERE WH_NO = A.WH_NO AND MMCODE = A.MMCODE) as 總庫存數量 
                        FROM MI_WEXPINV A, MI_MAST B 
                        WHERE A.MMCODE = B.MMCODE AND A.WH_NO = :wh_no AND 
                        B.MAT_CLASS IN(SELECT MAT_CLASS FROM MI_MATCLASS 
                        WHERE MAT_CLSID = WHM1_TASK(:userID))";

            p.Add(":wh_no", wh_no);
            p.Add(":userID", userId);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public bool CheckPH_LOTNOExists(string MMCODE,string LOT_NO, string EXP_DATE)
        {
            string sql = @"SELECT 1 FROM PH_LOTNO WHERE MMCODE=:MMCODE AND LOT_NO=:LOT_NO AND TO_CHAR(TO_DATE(EXP_DATE),'YYYY-MM-DD') =:EXP_DATE ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = MMCODE , LOT_NO = LOT_NO, EXP_DATE = EXP_DATE }, DBWork.Transaction) == null);
        }

        public string selectWH_NAME(string WH_NO)
        {
            string sql = @"SELECT WH_NAME FROM MI_WHMAST WHERE WH_NO = :WH_NO ";

            if ((DBWork.Connection.ExecuteScalar(sql, new { WH_NO = WH_NO }, DBWork.Transaction)) == null) //因為有可能會null
            {
                return "";
            }
            else
            {
                return (DBWork.Connection.ExecuteScalar(sql, new { WH_NO = WH_NO }, DBWork.Transaction)).ToString();
            }
        }
        public string selectMMNAME_C(string MMCODE)
        {
            string sql = @"select  MMNAME_C from MI_MAST where MMCODE=:MMCODE";

            if ((DBWork.Connection.ExecuteScalar(sql, new { MMCODE = MMCODE }, DBWork.Transaction)) == null) //因為有可能會null
            {
                return "";
            }
            else
            {
                return (DBWork.Connection.ExecuteScalar(sql, new { MMCODE = MMCODE }, DBWork.Transaction)).ToString();
            }
        }
        public string selectMMNAME_E(string MMCODE)
        {
            string sql = @"select MMNAME_E from MI_MAST where MMCODE=:MMCODE";

            if ((DBWork.Connection.ExecuteScalar(sql, new { MMCODE = MMCODE }, DBWork.Transaction)) ==null) //因為有可能會null
            {
                return "";
            }
            else
            {
                return (DBWork.Connection.ExecuteScalar(sql, new { MMCODE = MMCODE }, DBWork.Transaction)).ToString();
            }
        }

        public bool CheckInv_qty(string WH_NO, string PREMMCODE, double SUMINV_QTY)
        {
            string sql = @"SELECT INV_QTY FROM MI_WHINV WHERE WH_NO = :WH_NO AND MMCODE = :MMCODE  AND INV_QTY = :INV_QTY";
            return !(DBWork.Connection.ExecuteScalar(sql, new { WH_NO = WH_NO, MMCODE = PREMMCODE, INV_QTY = SUMINV_QTY }, DBWork.Transaction) == null);
        }
        public int SelectMI_WEXPINV(string wh_no, string premmcode)
        {
            var sql = @"Select count(*) as count FROM MI_WEXPINV 
                         WHERE WH_NO = :WH_NO  
                           AND MMCODE = :MMCODE";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = premmcode }, DBWork.Transaction);
        }

        public int SelectMI_WEXPINV2(string wh_no, string mmcode, string exp_date, string lot_no, string inv_qty)
        {
            var sql = @"Select count(*) as count FROM MI_WEXPINV 
                         WHERE WH_NO = :WH_NO  AND MMCODE = :MMCODE AND TO_CHAR(TO_DATE(EXP_DATE),'YYYY-MM-DD') = :EXP_DATE AND LOT_NO = :LOT_NO AND INV_QTY = :INV_QTY";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = mmcode, EXP_DATE = exp_date, LOT_NO = lot_no, INV_QTY = inv_qty }, DBWork.Transaction);
        }

        

        public int DeleteMI_WEXPINV(string wh_no, string premmcode)
        {
            var sql = @"DELETE FROM MI_WEXPINV 
                         WHERE WH_NO = :WH_NO  
                           AND MMCODE = :MMCODE ";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = premmcode }, DBWork.Transaction);
        }

        public int InsertMI_WEXPINV(string wh_no,string mmcode,string exp_date, string lot_no, string inv_qty,string create_user,string update_user,string update_ip)
        {
            var sql = @"INSERT INTO MI_WEXPINV (WH_NO, MMCODE, EXP_DATE, LOT_NO, INV_QTY, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                        VALUES (:WH_NO, :MMCODE, TO_DATE(:EXP_DATE,'YYYY-MM-DD'), :LOT_NO, :INV_QTY, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, MMCODE = mmcode, EXP_DATE = exp_date, LOT_NO = lot_no, INV_QTY = inv_qty
                                                      , CREATE_USER = create_user, UPDATE_USER = update_user, UPDATE_IP = update_ip}, DBWork.Transaction);
        }

        public bool CheckPH1SExpExists(string MMCODE, string LOT_NO, string EXP_DATE)
        {
            string sql = @"select 1 from MI_WEXPINV 
                            where wh_no = 'PH1S' 
                              and MMCODE=:MMCODE 
                              AND LOT_NO=:LOT_NO 
                              AND TO_CHAR(TO_DATE(EXP_DATE),'YYYY-MM-DD') =:EXP_DATE ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = MMCODE, LOT_NO = LOT_NO, EXP_DATE = EXP_DATE }, DBWork.Transaction) == null);
        }
    }
}