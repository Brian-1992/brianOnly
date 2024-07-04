using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using TSGH.Models;

namespace WebApp.Repository.AB
{
    public class AB0129Repository : JCLib.Mvc.BaseRepository
    {
        public AB0129Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AB0129> GetAll(string wh_no, string mmcode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = "";
            //1130105 發現除完後會有小數溢位故取至小數後4位
            sql += @"SELECT :WH_NO_INPUT MMCODE,
                            :MMCODE_INPUT MMNAME_C,
                            NVL(round((SELECT NVL(SUM(CASE WHEN TR_MCODE = 'USEI' THEN TR_INV_QTY ELSE -TR_INV_QTY END),0)/10 USE_QTY
                                 FROM MI_WHTRNS
                                 WHERE WH_NO = :WH_NO_INPUT
                                 AND MMCODE = :MMCODE_INPUT
                                 AND TR_MCODE IN ('USEI','USEO')
                                 AND TR_DATE BETWEEN SYSDATE AND SYSDATE - 10
                                 GROUP BY WH_NO,MMCODE),4),0) USE_QTY_10,
                            NVL(round((SELECT NVL(SUM(CASE WHEN TR_MCODE = 'USEI' THEN TR_INV_QTY ELSE -TR_INV_QTY END),0)/14 USE_QTY
                                 FROM MI_WHTRNS
                                 WHERE WH_NO = :WH_NO_INPUT
                                 AND MMCODE = :MMCODE_INPUT
                                 AND TR_MCODE IN ('USEI','USEO')
                                 AND TR_DATE BETWEEN SYSDATE AND SYSDATE - 14
                                 GROUP BY WH_NO,MMCODE),4),0) USE_QTY_14,
                            NVL(round((SELECT NVL(SUM(CASE WHEN TR_MCODE = 'USEI' THEN TR_INV_QTY ELSE -TR_INV_QTY END),0)/90 USE_QTY
                                 FROM MI_WHTRNS
                                 WHERE WH_NO = :WH_NO_INPUT
                                 AND MMCODE = :MMCODE_INPUT
                                 AND TR_MCODE IN ('USEI','USEO')
                                 AND TR_DATE BETWEEN SYSDATE AND SYSDATE - 90
                                 GROUP BY WH_NO,MMCODE),4),0) USE_QTY_90,
                            NVL(round((SELECT USE_QTY
                                 FROM MI_WINVMON
                                 WHERE WH_NO = :WH_NO_INPUT
                                 AND MMCODE = :MMCODE_INPUT
                                 AND DATA_YM = TWN_YYYMM(add_months( sysdate, -1 ))),4),0) USE_QTY_1M,
                            NVL(round((SELECT USE_QTY
                                 FROM MI_WINVMON
                                 WHERE WH_NO = :WH_NO_INPUT
                                 AND MMCODE = :MMCODE_INPUT
                                 AND DATA_YM = TWN_YYYMM(add_months( sysdate, -2 ))),4),0) USE_QTY_2M,
                            NVL(round((SELECT USE_QTY
                                 FROM MI_WINVMON
                                 WHERE WH_NO = :WH_NO_INPUT
                                 AND MMCODE = :MMCODE_INPUT
                                 AND DATA_YM = TWN_YYYMM(add_months( sysdate, -3 ))),4),0) USE_QTY_3M,
                            NVL(round((SELECT USE_QTY
                                 FROM MI_WINVMON
                                 WHERE WH_NO = :WH_NO_INPUT
                                 AND MMCODE = :MMCODE_INPUT
                                 AND DATA_YM = TWN_YYYMM(add_months( sysdate, -4 ))),4),0) USE_QTY_4M,
                            NVL(round((SELECT USE_QTY
                                 FROM MI_WINVMON
                                 WHERE WH_NO = :WH_NO_INPUT
                                 AND MMCODE = :MMCODE_INPUT
                                 AND DATA_YM = TWN_YYYMM(add_months( sysdate, -5 ))),4),0) USE_QTY_5M,
                            NVL(round((SELECT USE_QTY
                                 FROM MI_WINVMON
                                 WHERE WH_NO = :WH_NO_INPUT
                                 AND MMCODE = :MMCODE_INPUT
                                 AND DATA_YM = TWN_YYYMM(add_months( sysdate, -6 ))),4),0) USE_QTY_6M,
                            NVL(round((SELECT SUM(USE_QTY) / 3
                                 FROM MI_WINVMON
                                 WHERE WH_NO = :WH_NO_INPUT
                                 AND MMCODE = :MMCODE_INPUT
                                 AND DATA_YM >= TWN_YYYMM(add_months( sysdate, -3 ))),4),0) USE_QTY_3MA,
                            NVL(round((SELECT SUM(USE_QTY) / 6
                                 FROM MI_WINVMON
                                 WHERE WH_NO = :WH_NO_INPUT
                                 AND MMCODE = :MMCODE_INPUT
                                 AND DATA_YM >= TWN_YYYMM(add_months( sysdate, -6 ))),4),0) USE_QTY_6MA
                          FROM DUAL";


            p.Add(":WH_NO_INPUT", string.Format("{0}", wh_no));
            p.Add(":MMCODE_INPUT", string.Format("{0}", mmcode));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0129>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //庫房代碼
        public IEnumerable<MI_WHMAST> GetWH_NOComboOne(int page_index, int page_size, string sorters)
        {
            string sql = @"SELECT WH_NO, WH_NAME 
                           FROM MI_WHMAST 
                           WHERE WH_KIND IN ('0', '1') 
                           AND WH_GRADE <= '4' 
                           ORDER BY 1";
            return DBWork.Connection.Query<MI_WHMAST>(sql, new { }, DBWork.Transaction);
        }
        //院內碼
        public IEnumerable<MI_MAST> GetMMCODECombo(string mmcode, string wh_no, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"SELECT {0}
                                  A.MMCODE,
                                  A.MMNAME_C,
                                  (SELECT MM.MAT_CLSNAME FROM MI_MATCLASS MM WHERE MM.MAT_CLASS = A.MAT_CLASS ) MAT_CLSNAME,
                                  BASE_UNIT,
                                  UNITRATE || ' ' || BASE_UNIT || '/' || M_PURUN UNITRATE
                           FROM MI_MAST A JOIN MI_WINVCTL B ON (A.MMCODE = B.MMCODE) WHERE B.WH_NO = :WH_NO ";

            if (mmcode != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(MMNAME_E), UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,");
                p.Add(":MMCODE_I", mmcode);
                p.Add(":MMNAME_E_I", mmcode);
                p.Add(":MMNAME_C_I", mmcode);

                sql += " AND (UPPER(A.MMCODE) LIKE UPPER(:MMCODE) ";
                p.Add(":MMCODE", string.Format("{0}%", mmcode));

                sql += " OR UPPER(MMNAME_E) LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", mmcode));

                sql += " OR UPPER(MMNAME_C) LIKE UPPER(:MMNAME_C)) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", mmcode));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY MMCODE ";
            }

            p.Add(":WH_NO", wh_no);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //月份
        public IEnumerable<string> GetNowMonth()
        {
            string sql = @"SELECT TWN_YYYMM(SYSDATE) FROM DUAL";
            return DBWork.Connection.Query<string>(sql, new { }, DBWork.Transaction);
        }
        //最後更新日期
        public IEnumerable<string> GetLastUpdateDate()
        {
            string sql = @"SELECT TWN_DATE(MAX(SET_ETIME)) FROM MI_MNSET WHERE SET_STATUS = 'C'";
            return DBWork.Connection.Query<string>(sql, new { }, DBWork.Transaction);
        }
    }

}