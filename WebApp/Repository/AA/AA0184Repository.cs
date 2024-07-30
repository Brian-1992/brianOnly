using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using WebApp.Models.AB;
using System.Collections.Generic;

namespace WebApp.Repository.AB
{
    public class AA0184Repository : JCLib.Mvc.BaseRepository
    {
        public AA0184Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //查詢
        public IEnumerable<AA0184> GetAll(string p0, string start_apptime, string end_apptime, string[] flowid, string start_dis_time, string end_dis_time, string towh, string strMmcode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            #region
            var sql = @"   WITH FLOWIDS AS ( SELECT DISTINCT 
                                                    DATA_VALUE AS VALUE,
                                                    DATA_DESC AS TEXT,
                                                    DATA_VALUE || ' ' || '衛材' || DATA_DESC AS COMBITEM 
                                             FROM PARAM_D
                                             WHERE GRP_CODE = 'ME_DOCM' 
                                             AND DATA_NAME = 'FLOWID_MR1' 
                                             AND DATA_VALUE IN ('1','11','2','3','4','6')
                                             UNION 
                                             SELECT FLOWID AS VALUE, 
                                                    '藥品' || FLOWNAME AS TEXT ,
                                                    FLOWID || ' ' || '藥品' || FLOWNAME AS COMBITEM
                                             FROM ME_FLOW
                                             WHERE DOCTYPE IN ('MR')
                                             AND SUBSTR(FLOWID, LENGTH(FLOWID), 1) IN ('1','2','3','9') ), 
                                  TOWHS AS ( SELECT A.WH_NO,
                                                    A.WH_NAME 
                                             FROM MI_WHMAST A
                                             WHERE 1 = 1
                                             AND NVL(A.CANCEL_ID, 'N') = 'N'
                                             AND A.WH_KIND = '1' 
                                             AND NVL(A.CANCEL_ID, 'N') = 'N'
                                             AND A.WH_GRADE NOT IN ('1','5')
                                             UNION 
                                             SELECT A.WH_NO, 
                                                    A.WH_NAME 
                                             FROM MI_WHMAST A
                                             WHERE 1 = 1
                                             AND NVL(A.CANCEL_ID, 'N') = 'N'
                                             AND A.WH_KIND = '0' 
                                             AND NVL(A.CANCEL_ID,'N') = 'N'
                                             AND A.WH_GRADE ='2')
                           SELECT A.DOCNO DOCNO, 
                                  A.MAT_CLASS MAT_CLASS, 
                                  B.COMBITEM COMBITEM,
                                  A.FRWH || ' ' || WH_NAME(A.FRWH) FRWH, 
                                  A.TOWH || ' ' || WH_NAME(A.TOWH) TOWH, 
                                  TWN_DATE(A.APPTIME) APPTIME, 
                                  C.MMCODE MMCODE, 
                                  D.MMNAME_C MMNAME_C, 
                                  D.MMNAME_E MMNAME_E, 
                                  D.BASE_UNIT BASE_UNIT, 
                                  C.APPQTY APPQTY, 
                                  C.APVQTY APVQTY, 
                                  TWN_DATE(C.DIS_TIME) DIS_TIME
                           FROM ME_DOCM A, FLOWIDS B, ME_DOCD C, MI_MAST D
                           WHERE 1 = 1
                           AND A.DOCTYPE IN ('MR', 'MR1','MR2','MR5','MR6')
                           AND A.FLOWID = B.VALUE
                           AND A.DOCNO = C.DOCNO 
                           AND C.MMCODE = D.MMCODE
                           AND A.TOWH IN (SELECT WH_NO FROM TOWHS) ";
            #endregion

            #region
            if (!String.IsNullOrEmpty(p0))
            {
                if (p0 == "all01" || p0 == "all02")
                {
                    sql += "    AND D.MAT_CLASS = :p0 ";
                    p.Add(":p0", p0.Replace("all", ""));
                }
                else
                {
                    sql += "    AND D.MAT_CLASS_SUB = :p0 ";
                    p.Add(":p0", p0);
                }
            }
            if (start_apptime.Trim() != "")
            {
                sql += "    AND TWN_DATE(A.APPTIME) >= :START_APPTIME ";
                p.Add(":START_APPTIME", start_apptime.Trim());
            }
            if (end_apptime.Trim() != "")
            {
                sql += "    AND TWN_DATE(A.APPTIME) <= :END_APPTIME ";
                p.Add(":END_APPTIME", end_apptime.Trim());
            }
            if (start_dis_time.Trim() != "")
            {
                sql += "    AND TWN_DATE(C.DIS_TIME) >= :START_DIS_TIME ";
                p.Add(":START_DIS_TIME", start_dis_time.Trim());
            }
            if (end_dis_time.Trim() != "")
            {
                sql += "    AND TWN_DATE(C.DIS_TIME) <= :END_DIS_TIME ";
                p.Add(":END_DIS_TIME", end_dis_time.Trim());
            }
            if (towh.Trim() != "")
            {
                sql += "    AND A.TOWH = :TOWH ";
                p.Add(":TOWH", towh.Trim());
            }
            if (strMmcode.Trim() != "")
            {
                sql += "    AND D.MMCODE = :MMCODE ";
                p.Add(":MMCODE", strMmcode.Trim());
            }
            if (flowid.Length > 0)
            {
                string sql_FLOWID = "";
                sql += @"AND (";
                foreach (string tmp_FLOWID in flowid)
                {
                    if (string.IsNullOrEmpty(sql_FLOWID))
                    {
                        sql_FLOWID = @"A.FLOWID = '" + tmp_FLOWID + "'";
                    }
                    else
                    {
                        sql_FLOWID += @" OR A.FLOWID = '" + tmp_FLOWID + "'";
                    }
                }
                sql += sql_FLOWID + ") ";
            }
            sql += "        ORDER BY DOCNO, MAT_CLASS, MMCODE ";
            #endregion

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0184>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        //匯出
        public DataTable GetExcel(string p0, string start_apptime, string end_apptime, string[] flowid, string start_dis_time, string end_dis_time, string towh, string strMmcode)
        {
            DynamicParameters p = new DynamicParameters();

            #region
            var sql = @"   WITH FLOWIDS AS ( SELECT DISTINCT 
                                                    DATA_VALUE AS VALUE,
                                                    DATA_DESC AS TEXT,
                                                    DATA_VALUE || ' ' || '衛材' || DATA_DESC AS COMBITEM 
                                             FROM PARAM_D
                                             WHERE GRP_CODE = 'ME_DOCM' 
                                             AND DATA_NAME = 'FLOWID_MR1' 
                                             AND DATA_VALUE IN ('1','11','2','3','4','6')
                                             UNION 
                                             SELECT FLOWID AS VALUE, 
                                                    '藥品' || FLOWNAME AS TEXT ,
                                                    FLOWID || ' ' || '藥品' || FLOWNAME AS COMBITEM
                                             FROM ME_FLOW
                                             WHERE DOCTYPE IN ('MR')
                                             AND SUBSTR(FLOWID, LENGTH(FLOWID), 1) IN ('1','2','3','9') ), 
                                  TOWHS AS ( SELECT A.WH_NO,
                                                    A.WH_NAME 
                                             FROM MI_WHMAST A
                                             WHERE 1 = 1
                                             AND NVL(A.CANCEL_ID, 'N') = 'N'
                                             AND A.WH_KIND = '1' 
                                             AND NVL(A.CANCEL_ID, 'N') = 'N'
                                             AND A.WH_GRADE NOT IN ('1','5')
                                             UNION 
                                             SELECT A.WH_NO, 
                                                    A.WH_NAME 
                                             FROM MI_WHMAST A
                                             WHERE 1 = 1
                                             AND NVL(A.CANCEL_ID, 'N') = 'N'
                                             AND A.WH_KIND = '0' 
                                             AND NVL(A.CANCEL_ID,'N') = 'N'
                                             AND A.WH_GRADE ='2')
                           SELECT A.DOCNO 申請單號, 
                                  A.MAT_CLASS 物料分類, 
                                  B.COMBITEM 狀態,
                                  A.FRWH || ' ' || WH_NAME(A.FRWH) 出庫庫房, 
                                  A.TOWH || ' ' || WH_NAME(A.TOWH) 入庫庫房, 
                                  TWN_DATE(A.APPTIME) 申請時間, 
                                  C.MMCODE 院內碼, 
                                  D.MMNAME_C 中文名稱, 
                                  D.MMNAME_E 英文名稱, 
                                  D.BASE_UNIT 單位, 
                                  C.APPQTY 申請數量, 
                                  C.APVQTY 核可數量, 
                                  TWN_DATE(C.DIS_TIME) 核撥日期
                           FROM ME_DOCM A, FLOWIDS B, ME_DOCD C, MI_MAST D
                           WHERE 1 = 1
                           AND A.DOCTYPE IN ('MR', 'MR1','MR2','MR5','MR6')
                           AND A.FLOWID = B.VALUE
                           AND A.DOCNO = C.DOCNO 
                           AND C.MMCODE = D.MMCODE
                           AND A.TOWH IN (SELECT WH_NO FROM TOWHS) ";
            #endregion

            #region
            if (!String.IsNullOrEmpty(p0))
            {
                if (p0 == "all01" || p0 == "all02")
                {
                    sql += "    AND D.MAT_CLASS = :p0 ";
                    p.Add(":p0", p0.Replace("all", ""));
                }
                else
                {
                    sql += "    AND D.MAT_CLASS_SUB = :p0 ";
                    p.Add(":p0", p0);
                }
            }
            if (start_apptime.Trim() != "")
            {
                sql += "    AND TWN_DATE(A.APPTIME) >= :START_APPTIME ";
                p.Add(":START_APPTIME", start_apptime.Trim());
            }
            if (end_apptime.Trim() != "")
            {
                sql += "    AND TWN_DATE(A.APPTIME) <= :END_APPTIME ";
                p.Add(":END_APPTIME", end_apptime.Trim());
            }
            if (start_dis_time.Trim() != "")
            {
                sql += "    AND TWN_DATE(C.DIS_TIME) >= :START_DIS_TIME ";
                p.Add(":START_DIS_TIME", start_dis_time.Trim());
            }
            if (end_dis_time.Trim() != "")
            {
                sql += "    AND TWN_DATE(C.DIS_TIME) <= :END_DIS_TIME ";
                p.Add(":END_DIS_TIME", end_dis_time.Trim());
            }
            if (towh.Trim() != "")
            {
                sql += "    AND A.TOWH = :TOWH ";
                p.Add(":TOWH", towh.Trim());
            }
            if (strMmcode.Trim() != "")
            {
                sql += "    AND D.MMCODE = :MMCODE ";
                p.Add(":MMCODE", strMmcode.Trim());
            }
            if (flowid.Length > 0)
            {
                string sql_FLOWID = "";
                sql += @"AND (";
                foreach (string tmp_FLOWID in flowid)
                {
                    if (string.IsNullOrEmpty(sql_FLOWID))
                    {
                        sql_FLOWID = @"A.FLOWID = '" + tmp_FLOWID + "'";
                    }
                    else
                    {
                        sql_FLOWID += @" OR A.FLOWID = '" + tmp_FLOWID + "'";
                    }
                }
                sql += sql_FLOWID + ") ";
            }

            sql += "        ORDER BY A.DOCNO, A.MAT_CLASS, C.MMCODE  ";
            #endregion

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        //物料代碼combo
        public IEnumerable<COMBO_MODEL> GetMatClassSubCombo()
        {
            var p = new DynamicParameters();

            string sql = @"  SELECT ROWNUM + 2 AS ORDERNO, 
                                    DATA_VALUE AS VALUE, 
                                    DATA_DESC AS TEXT 
                             FROM PARAM_D 
                             WHERE GRP_CODE = 'MI_MAST' 
                             AND DATA_NAME = 'MAT_CLASS_SUB'
                             UNION
                             SELECT 0 AS ORDERNO,
                                    '' AS VALUE, 
                                    '全部' AS TEXT 
                             FROM DUAL
                             UNION
                             SELECT 1 AS ORDERNO,'all01' AS VALUE ,  
                                    '全部藥品' AS TEXT 
                             FROM DUAL
                             UNION
                             SELECT 2 AS ORDERNO,'all02' AS VALUE ,  
                                    '全部衛材' AS TEXT 
                             FROM DUAL  ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //申請日期區間
        public IEnumerable<string> GetStartApptime()
        {
            var p = new DynamicParameters();

            string sql = @"  SELECT TWN_DATE(SET_BTIME) 
                             FROM MI_MNSET  
                             WHERE SET_STATUS = 'N'  ";

            return DBWork.Connection.Query<string>(sql, p);
        }
        //申請單狀態combo
        public IEnumerable<COMBO_MODEL> GetFlowIdCombo()
        {
            var p = new DynamicParameters();

            string sql = @"  SELECT DISTINCT 
                                    DATA_VALUE AS VALUE,  
                                    DATA_DESC AS TEXT ,  
                                    DATA_VALUE || ' ' || '衛材' || DATA_DESC AS COMBITEM 
                             FROM PARAM_D
                             WHERE GRP_CODE = 'ME_DOCM' 
                             AND DATA_NAME = 'FLOWID_MR1' 
                             AND DATA_VALUE IN ('1','11','2','3','4','6')
                             UNION 
                             SELECT FLOWID AS VALUE,  
                                    '藥品' || FLOWNAME AS TEXT, 
                                    FLOWID || ' ' || '藥品' || FLOWNAME AS COMBITEM
                             FROM ME_FLOW
                             WHERE DOCTYPE IN ('MR')
                             AND SUBSTR(FLOWID, LENGTH(FLOWID), 1) IN ('1','2','3','9') ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //入庫庫房combo
        public IEnumerable<COMBO_MODEL> GetWhNoCombo(string MatClassSub)
        {
            var p = new DynamicParameters();
            string sql = "";

            if (MatClassSub.Trim() == "")
            {
                sql = @"    SELECT A.WH_NO VALUE, 
                                   A.WH_NO || ' ' || A.WH_NAME TEXT
                            FROM MI_WHMAST A
                            WHERE 1 = 1 
                            AND NVL(A.CANCEL_ID, 'N') = 'N' 
                            AND A.WH_GRADE NOT IN ('1','5')
                            ORDER BY WH_NO ";
            }
            else if (MatClassSub.Trim() == "A" || MatClassSub.Trim() == "1")
            {
                sql = @"    SELECT A.WH_NO VALUE, 
                                   A.WH_NO || ' ' || A.WH_NAME TEXT
                            FROM MI_WHMAST A
                            WHERE 1 = 1 
                            AND A.WH_KIND = '0' 
                            AND NVL(A.CANCEL_ID, 'N') = 'N' 
                            AND A.WH_GRADE = '2'
                            ORDER BY WH_NO ";
            }
            else
            {
                sql = @"    SELECT A.WH_NO VALUE, 
                                   A.WH_NO || ' ' || A.WH_NAME TEXT
                            FROM MI_WHMAST A
                            WHERE 1 = 1 
                            AND A.WH_KIND = '1' 
                            AND NVL(A.CANCEL_ID, 'N') = 'N' 
                            AND A.WH_GRADE NOT IN ('1','5')
                            ORDER BY WH_NO ";
            }

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
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
                                      AND  nvl(cancel_id, 'N') = 'N'
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
    }
}