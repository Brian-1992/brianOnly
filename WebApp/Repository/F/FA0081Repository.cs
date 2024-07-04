using System;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using System.Text;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

namespace WebApp.Repository.F
{

    public class FA0081Repository : JCLib.Mvc.BaseRepository
    {
        public FA0081Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }


        public IEnumerable<FA0081MatserMODEL> GetAllM(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p9, string p10, string p11, string p12, string p13, string user, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = "";
            
            sql = @"  SELECT
                                DATA_YM F1,  
                                MMCODE F2,  
                                MMNAME_C F3,  
                                BASE_UNIT F4,  
                                M_CONTPRICE F5,  
                                PMN_INVQTY F6,  
                                IN_QTY F7,
                                ( PMN_INVQTY + IN_QTY - REJ_OUTQTY ) F8,
                                REJ_OUTQTY F9,
                                ( INV_QTY + INVENTORYQTY ) F10,
                                INV_QTY F11,
                                M_CONTPRICE * PMN_INVQTY F12,
                                (select DATA_VALUE || ':' || DATA_DESC from PARAM_D 
                                where GRP_CODE ='MI_MAST' 
                                and DATA_NAME = 'MAT_CLASS_SUB' and DATA_VALUE = TB.MAT_CLASS_SUB) F13,  
                                DECODE(E_SOURCECODE,'P','買斷','C','寄庫','') F14,  
                                DECODE(M_CONTID,'0','合約','2','非合約','') F15,  
                                DECODE(WARBAK,'0','否','1','是','否') F16,  
                                MIL_QTY F17,
                                M_CONTPRICE * MIL_QTY F18, 
                                DECODE(ORDERKIND,'1','常備品項','2','小額採購','無') F19,  
                                DECODE(CANCEL_ID, 'Y','是', 'N','否','否') F20,
                                DECODE(SPDRUG,'0','否','1','是','否') F21, 
                                DECODE(FASTDRUG,'0','否','1','是','') F22
                        FROM (
                                SELECT 
                                    A.DATA_YM,   
                                    D.MMCODE,  
                                    D.MMNAME_C,  
                                    D.BASE_UNIT,  
                                    D.M_CONTPRICE,  
                                    G.PMN_INVQTY ,  
                                    (CASE C.WH_GRADE
                                          WHEN '1'
                                          THEN
                                             A.APL_INQTY
                                          ELSE
                                             (  A.APL_INQTY
                                              + A.TRN_INQTY
                                              + A.ADJ_INQTY
                                              + A.BAK_INQTY
                                              + A.EXG_INQTY
                                              + A.MIL_INQTY
                                              + A.INVENTORYQTY
                                              - A.APL_OUTQTY
                                              - A.TRN_OUTQTY
                                              - A.ADJ_OUTQTY
                                              - A.BAK_OUTQTY
                                              - A.REJ_OUTQTY
                                              - A.DIS_OUTQTY
                                              - A.EXG_OUTQTY
                                              - A.MIL_OUTQTY)
                                          END) IN_QTY,   
                                    A.REJ_OUTQTY, 
                                    A.INVENTORYQTY, 
                                    CASE WHEN D.WARBAK = '1' THEN A.INV_QTY+A.MIL_INQTY-A.MIL_OUTQTY ELSE 0 END MIL_QTY,
                                    D.MAT_CLASS_SUB,
                                    D.E_SOURCECODE,
                                    D.M_CONTID,
                                    D.WARBAK,
                                    A.INV_QTY,
                                    A.MIL_INQTY,
                                    A.MIL_OUTQTY,
                                    D.ORDERKIND,
                                    D.CANCEL_ID,
                                    D.SPDRUG,
                                    D. FASTDRUG    
                                FROM MI_WINVMON A, MI_WHMAST C, MI_MAST D, MI_WHCOST G
                                WHERE A.WH_NO = C.WH_NO
                                AND A.MMCODE = D.MMCODE
                                AND A.DATA_YM = G.DATA_YM
                                AND A.MMCODE = G.MMCODE
                                AND IDLE_MMCODE(A.DATA_YM, A.MMCODE) = 'Y'
                                and c.wh_grade='1'
                            ";


            if (p0 != "" && p1 != "") // 查詢月份 P0 P1
            {
                sql += " AND A.DATA_YM >= :p0 and A.DATA_YM <= :p1 ";
                p.Add(":p0", p0);
                p.Add(":p1", p1);
            }

            if (p2 != "") //藥材代碼 P2
            {
                sql += " AND D.MMCODE = :p2 ";
                p.Add(":p2", p2);
            }

            if (p3 != "") //藥材類別 P3
            {
                sql += " AND D.MAT_CLASS_SUB = :p3 ";
                p.Add(":p3", p3);
            }

            if (p4 != "") //單位 P4
            {
                sql += " AND C.WH_NO = :p4 ";
                p.Add(":p4", p4);
            }

            if (p5 != "") //類別 P5
            {
                sql += " AND D.MAT_CLASS = :p5 ";
                p.Add(":p5", p5);
            }

            if (p6 != "") //買斷寄庫 P6
            {
                sql += " AND D.E_SOURCECODE = :p6 ";
                p.Add(":p6", p6);
            }

            if (p7 != "") //是否合約 P7
            {
                sql += " AND D.M_CONTID = :p7 ";
                p.Add(":p7", p7);
            }

            if (p8 != "") //是否備戰 P8
            {
                sql += " AND D.WARBAK = :p8 ";
                p.Add(":p8", p8);
            }

            if (p9 != "") //常備品 P9
            {
                sql += " AND D.ORDERKIND = :p9 ";
                p.Add(":p9", p9);
            }

            if (p10 != "") //刪除品項 P10
            {
                sql += " AND D.CANCEL_ID = :p10 ";
                p.Add(":p10", p10);
            }

            if (p11 != "") //特殊品項 P11
            {
                sql += " AND D.SPDRUG = :p11 ";
                p.Add(":p11", p11);
            }

            if (p12 != "") //急救品項 P12
            {
                sql += " AND D.FASTDRUG = :p12 ";
                p.Add(":p12", p12);
            }

            if (p13 != "false") //買斷與備戰 P13
            {
                sql += " AND D.E_SOURCECODE = 'P' and D. WARBAK = '1' ";
            }

            sql += ") TB";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<FA0081MatserMODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }//GetAllM
        
        public IEnumerable<FA0081ReportMODEL> GetPrintData(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p9, string p10, string p11, string p12, string p13, string user)
        {
            var p = new DynamicParameters();
            var sql = "";

            sql = @"  SELECT
                                DATA_YM F1,  
                                MMCODE F2,  
                                MMNAME_C F3,  
                                BASE_UNIT F4,  
                                M_CONTPRICE F5,  
                                PMN_INVQTY F6,  
                                IN_QTY F7,
                                ( PMN_INVQTY + IN_QTY - REJ_OUTQTY ) F8,
                                REJ_OUTQTY F9,
                                ( INV_QTY + INVENTORYQTY ) F10,
                                INV_QTY F11,
                                M_CONTPRICE * PMN_INVQTY F12,
                                (select DATA_VALUE || ':' || DATA_DESC from PARAM_D 
                                where GRP_CODE ='MI_MAST' 
                                and DATA_NAME = 'MAT_CLASS_SUB' and DATA_VALUE = TB.MAT_CLASS_SUB) F13,  
                                DECODE(E_SOURCECODE,'P','買斷','C','寄庫','') F14,  
                                DECODE(M_CONTID,'0','合約','2','非合約','') F15,  
                                DECODE(WARBAK,'0','否','1','是','否') F16,  
                                MIL_QTY F17,
                                M_CONTPRICE * MIL_QTY F18, 
                                DECODE(ORDERKIND,'1','常備品項','2','小額採購','無') F19,  
                                DECODE(CANCEL_ID, 'Y','是', 'N','否','否') F20,
                                DECODE(SPDRUG,'0','否','1','是','否') F21, 
                                DECODE(FASTDRUG,'0','否','1','是','') F22
                        FROM (
                                SELECT 
                                    A.DATA_YM,   
                                    D.MMCODE,  
                                    D.MMNAME_C,  
                                    D.BASE_UNIT,  
                                    D.M_CONTPRICE,  
                                    G.PMN_INVQTY ,  
                                    (CASE C.WH_GRADE
                                          WHEN '1'
                                          THEN
                                             A.APL_INQTY
                                          ELSE
                                             (  A.APL_INQTY
                                              + A.TRN_INQTY
                                              + A.ADJ_INQTY
                                              + A.BAK_INQTY
                                              + A.EXG_INQTY
                                              + A.MIL_INQTY
                                              + A.INVENTORYQTY
                                              - A.APL_OUTQTY
                                              - A.TRN_OUTQTY
                                              - A.ADJ_OUTQTY
                                              - A.BAK_OUTQTY
                                              - A.REJ_OUTQTY
                                              - A.DIS_OUTQTY
                                              - A.EXG_OUTQTY
                                              - A.MIL_OUTQTY)
                                          END) IN_QTY,   
                                    A.REJ_OUTQTY, 
                                    A.INVENTORYQTY, 
                                    CASE WHEN D.WARBAK = '1' THEN A.INV_QTY+A.MIL_INQTY-A.MIL_OUTQTY ELSE 0 END MIL_QTY,
                                    D.MAT_CLASS_SUB,
                                    D.E_SOURCECODE,
                                    D.M_CONTID,
                                    D.WARBAK,
                                    A.INV_QTY,
                                    A.MIL_INQTY,
                                    A.MIL_OUTQTY,
                                    D.ORDERKIND,
                                    D.CANCEL_ID,
                                    D.SPDRUG,
                                    D. FASTDRUG    
                                FROM MI_WINVMON A, MI_WHMAST C, MI_MAST D, MI_WHCOST G
                                WHERE A.WH_NO = C.WH_NO
                                AND A.MMCODE = D.MMCODE
                                AND A.DATA_YM = G.DATA_YM
                                AND A.MMCODE = G.MMCODE
                                AND IDLE_MMCODE(A.DATA_YM, A.MMCODE) = 'Y'
                                AND C.WH_GRADE='1'
                            ";


            if (p0 != "" && p1 != "") // 查詢月份 P0 P1
            {
                sql += " AND A.DATA_YM >= :p0 and A.DATA_YM <= :p1 ";
                p.Add(":p0", p0);
                p.Add(":p1", p1);
            }

            if (p2 != "") //藥材代碼 P2
            {
                sql += " AND D.MMCODE = :p2 ";
                p.Add(":p2", p2);
            }

            if (p3 != "") //藥材類別 P3
            {
                sql += " AND D.MAT_CLASS_SUB = :p3 ";
                p.Add(":p3", p3);
            }

            if (p4 != "") //單位 P4
            {
                sql += " AND C.WH_NO = :p4 ";
                p.Add(":p4", p4);
            }

            if (p5 != "") //類別 P5
            {
                sql += " AND D.MAT_CLASS = :p5 ";
                p.Add(":p5", p5);
            }

            if (p6 != "") //買斷寄庫 P6
            {
                sql += " AND D.E_SOURCECODE = :p6 ";
                p.Add(":p6", p6);
            }

            if (p7 != "") //是否合約 P7
            {
                sql += " AND D.M_CONTID = :p7 ";
                p.Add(":p7", p7);
            }

            if (p8 != "") //是否備戰 P8
            {
                sql += " AND D.WARBAK = :p8 ";
                p.Add(":p8", p8);
            }

            if (p9 != "") //常備品 P9
            {
                sql += " AND D.ORDERKIND = :p9 ";
                p.Add(":p9", p9);
            }

            if (p10 != "") //刪除品項 P10
            {
                sql += " AND D.CANCEL_ID = :p10 ";
                p.Add(":p10", p10);
            }

            if (p11 != "") //特殊品項 P11
            {
                sql += " AND D.SPDRUG = :p11 ";
                p.Add(":p11", p11);
            }

            if (p12 != "") //急救品項 P12
            {
                sql += " AND D.FASTDRUG = :p12 ";
                p.Add(":p12", p12);
            }

            if (p13 != "false") //買斷與備戰 P13
            {
                sql += " AND D.E_SOURCECODE = 'P' and D. WARBAK = '1' ";
            }

            sql += ") TB";

            return DBWork.Connection.Query<FA0081ReportMODEL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<MI_WHMAST> GetWhmastCombo(string p0, string isab, string user, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.WH_NO, A.WH_NAME, A.WH_KIND, A.WH_GRADE ";
            sql += " FROM MI_WHMAST A WHERE 1=1 ";
            sql += "      AND A.WH_KIND = '1' AND A.WH_NO <> WHNO_MM1 ";
            
            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.WH_NO, :WH_NO_I), 1000) + NVL(INSTR(A.WH_NAME, :WH_NAME_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大                
                p.Add(":WH_NO_I", p0);
                p.Add(":WH_NAME_I", p0);

                sql += " AND (A.WH_NO LIKE :WH_NO ";
                p.Add(":WH_NO", string.Format("%{0}%", p0));

                sql += " OR A.WH_NAME LIKE :WH_NAME) ";
                p.Add(":WH_NAME", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, WH_NO", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.WH_NO ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_WHMAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);

        }

        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E  
                        FROM MI_MAST A WHERE 1 = 1 ";
            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(MMNAME_E), UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (UPPER(A.MMCODE) LIKE UPPER(:MMCODE) ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

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

        public IEnumerable<COMBO_MODEL> GetMatClassSubCombo()
        {
            string sql = @" select DATA_VALUE as VALUE, DATA_DESC as TEXT
                            from PARAM_D
                            where GRP_CODE ='MI_MAST' 
                            and DATA_NAME = 'MAT_CLASS_SUB'
                            and trim(DATA_DESC) is not null
                            order by VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetMatClassCombo()
        {
            string sql = @" SELECT VALUE, TEXT FROM (
                            SELECT '01' VALUE, '藥品' TEXT FROM DUAL
                            UNION
                            SELECT '02' VALUE, '衛材' TEXT FROM DUAL
                            ) ORDER BY VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetESourceCodeCombo()
        {
            string sql = @" SELECT VALUE, TEXT FROM (
                            SELECT 'P' VALUE, '買斷' TEXT FROM DUAL
                            UNION
                            SELECT 'C' VALUE, '寄售' TEXT FROM DUAL
                            ) ORDER BY VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetMContidCombo()
        {
            string sql = @" SELECT VALUE, TEXT FROM (
                            SELECT '0' VALUE, '合約' TEXT FROM DUAL
                            UNION
                            SELECT '2' VALUE, '非合約' TEXT FROM DUAL
                            ) ORDER BY VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetWarbakCombo()
        {
            string sql = @" SELECT VALUE, TEXT FROM (
                            SELECT '0' VALUE, '非戰備' TEXT FROM DUAL
                            UNION
                            SELECT '1' VALUE, '戰備' TEXT FROM DUAL
                            ) ORDER BY VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetOrderCodeCombo()
        {
            string sql = @" SELECT VALUE, TEXT FROM (
                            SELECT '1' VALUE, '常備品項' TEXT FROM DUAL
                            UNION
                            SELECT '2' VALUE, '小額採購' TEXT FROM DUAL
                            ) ORDER BY VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetCancelIdCombo()
        {
            string sql = @" SELECT VALUE, TEXT FROM (
                            SELECT 'Y' VALUE, '是' TEXT FROM DUAL
                            UNION
                            SELECT 'N' VALUE, '否' TEXT FROM DUAL
                            ) ORDER BY VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetSpdrugCombo()
        {
            string sql = @" SELECT VALUE, TEXT FROM (
                            SELECT '0' VALUE, '非特殊品項' TEXT FROM DUAL
                            UNION
                            SELECT '1' VALUE, '特殊品項' TEXT FROM DUAL
                            ) ORDER BY VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetFastdrugCombo()
        {
            string sql = @" SELECT VALUE, TEXT FROM (
                            SELECT '0' VALUE, '非急救品項' TEXT FROM DUAL
                            UNION
                            SELECT '1' VALUE, '急救品項' TEXT FROM DUAL
                            ) ORDER BY VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetTouchCaseCombo()
        {
            string sql = @" SELECT VALUE, TEXT FROM (
                            SELECT '0' VALUE, '無合約' TEXT FROM DUAL
                            UNION
                            SELECT '1' VALUE, '院內選項' TEXT FROM DUAL
                            UNION
                            SELECT '2' VALUE, '非院內選項' TEXT FROM DUAL
                            UNION
                            SELECT '3' VALUE, '院內自辦合約' TEXT FROM DUAL
                            ) ORDER BY VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetERestrictcodeCombo()
        {
            string sql = @" SELECT VALUE, TEXT FROM (
                            SELECT 'N' VALUE, '非管制用藥' TEXT FROM DUAL
                            UNION
                            SELECT '0' VALUE, '其它列管藥品' TEXT FROM DUAL
                            UNION
                            SELECT '1' VALUE, '第一級管制用藥' TEXT FROM DUAL
                            UNION
                            SELECT '2' VALUE, '第二級管制用藥' TEXT FROM DUAL
                            UNION
                            SELECT '3' VALUE, '第三級管制用藥' TEXT FROM DUAL
                            UNION
                            SELECT '4' VALUE, '第四級管制用藥' TEXT FROM DUAL
                            ) ORDER BY VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }


        public DataTable GetExcel(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p9, string p10, string p11, string p12, string p13, string user)
        {
            var p = new DynamicParameters();
            var sql = "";

            sql = @"  SELECT
                                DATA_YM 月結月份,  
                                MMCODE 藥材代碼,  
                                MMNAME_C 藥材名稱,  
                                BASE_UNIT 單位,  
                                M_CONTPRICE 單價,  
                                PMN_INVQTY 上月結存,  
                                IN_QTY 本月進貨,
                                ( PMN_INVQTY + IN_QTY - REJ_OUTQTY ) 應有結存量,
                                REJ_OUTQTY 退貨量,
                                ( INV_QTY + INVENTORYQTY ) 盤存量,
                                INV_QTY 本月結存,
                                M_CONTPRICE * PMN_INVQTY 結存金額,
                                (select DATA_VALUE || ':' || DATA_DESC from PARAM_D 
                                where GRP_CODE ='MI_MAST' 
                                and DATA_NAME = 'MAT_CLASS_SUB' and DATA_VALUE = TB.MAT_CLASS_SUB) 藥材類別,  
                                DECODE(E_SOURCECODE,'P','買斷','C','寄庫','') 付款方式,  
                                DECODE(M_CONTID,'0','合約','2','非合約','') 合約方式,  
                                DECODE(WARBAK,'0','否','1','是','否') 是否戰備,  
                                MIL_QTY 戰備存量,
                                M_CONTPRICE * MIL_QTY 戰備金額, 
                                DECODE(ORDERKIND,'1','常備品項','2','小額採購','無') 採購類別,  
                                DECODE(CANCEL_ID, 'Y','是', 'N','否','否') 刪除品項,
                                DECODE(SPDRUG,'0','否','1','是','否') 特殊品項, 
                                DECODE(FASTDRUG,'0','否','1','是','') 急救品項
                        FROM (
                                SELECT 
                                    A.DATA_YM,   
                                    D.MMCODE,  
                                    D.MMNAME_C,  
                                    D.BASE_UNIT,  
                                    D.M_CONTPRICE,  
                                    G.PMN_INVQTY ,  
                                    (CASE C.WH_GRADE
                                          WHEN '1'
                                          THEN
                                             A.APL_INQTY
                                          ELSE
                                             (  A.APL_INQTY
                                              + A.TRN_INQTY
                                              + A.ADJ_INQTY
                                              + A.BAK_INQTY
                                              + A.EXG_INQTY
                                              + A.MIL_INQTY
                                              + A.INVENTORYQTY
                                              - A.APL_OUTQTY
                                              - A.TRN_OUTQTY
                                              - A.ADJ_OUTQTY
                                              - A.BAK_OUTQTY
                                              - A.REJ_OUTQTY
                                              - A.DIS_OUTQTY
                                              - A.EXG_OUTQTY
                                              - A.MIL_OUTQTY)
                                          END) IN_QTY,   
                                    A.REJ_OUTQTY, 
                                    A.INVENTORYQTY, 
                                    CASE WHEN D.WARBAK = '1' THEN A.INV_QTY+A.MIL_INQTY-A.MIL_OUTQTY ELSE 0 END MIL_QTY,
                                    D.MAT_CLASS_SUB,
                                    D.E_SOURCECODE,
                                    D.M_CONTID,
                                    D.WARBAK,
                                    A.INV_QTY,
                                    A.MIL_INQTY,
                                    A.MIL_OUTQTY,
                                    D.ORDERKIND,
                                    D.CANCEL_ID,
                                    D.SPDRUG,
                                    D. FASTDRUG    
                                FROM MI_WINVMON A, MI_WHMAST C, MI_MAST D, MI_WHCOST G
                                WHERE A.WH_NO = C.WH_NO
                                AND A.MMCODE = D.MMCODE
                                AND A.DATA_YM = G.DATA_YM
                                AND A.MMCODE = G.MMCODE
                                AND IDLE_MMCODE(A.DATA_YM, A.MMCODE) = 'Y'
                            ";


            if (p0 != "" && p1 != "") // 查詢月份 P0 P1
            {
                sql += " AND A.DATA_YM >= :p0 and A.DATA_YM <= :p1 ";
                p.Add(":p0", p0);
                p.Add(":p1", p1);
            }

            if (p2 != "") //藥材代碼 P2
            {
                sql += " AND D.MMCODE = :p2 ";
                p.Add(":p2", p2);
            }

            if (p3 != "") //藥材類別 P3
            {
                sql += " AND D.MAT_CLASS_SUB = :p3 ";
                p.Add(":p3", p3);
            }

            if (p4 != "") //單位 P4
            {
                sql += " AND C.WH_NO = :p4 ";
                p.Add(":p4", p4);
            }

            if (p5 != "") //類別 P5
            {
                sql += " AND D.MAT_CLASS = :p5 ";
                p.Add(":p5", p5);
            }

            if (p6 != "") //買斷寄庫 P6
            {
                sql += " AND D.E_SOURCECODE = :p6 ";
                p.Add(":p6", p6);
            }

            if (p7 != "") //是否合約 P7
            {
                sql += " AND D.M_CONTID = :p7 ";
                p.Add(":p7", p7);
            }

            if (p8 != "") //是否備戰 P8
            {
                sql += " AND D.WARBAK = :p8 ";
                p.Add(":p8", p8);
            }

            if (p9 != "") //常備品 P9
            {
                sql += " AND D.ORDERKIND = :p9 ";
                p.Add(":p9", p9);
            }

            if (p10 != "") //刪除品項 P10
            {
                sql += " AND D.CANCEL_ID = :p10 ";
                p.Add(":p10", p10);
            }

            if (p11 != "") //特殊品項 P11
            {
                sql += " AND D.SPDRUG = :p11 ";
                p.Add(":p11", p11);
            }

            if (p12 != "") //急救品項 P12
            {
                sql += " AND D.FASTDRUG = :p12 ";
                p.Add(":p12", p12);
            }

            if (p13 != "false") //買斷與備戰 P13
            {
                sql += " AND D.E_SOURCECODE = 'P' and D. WARBAK = '1' ";
            }

            sql += ") TB";


            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public string GetExtraDiscAmout(string p0, string yyymm)
        {
            var sql = "";
            if (p0 == yyymm)
            {
                sql = @"  SELECT NVL(SUM(NVL(EXTRA_DISC_AMOUNT,0)),0) EXTRA_DISC_AMOUNT
                            FROM PH_INVOICE
                           WHERE TRANSNO >  (RPAD(:P0,13,'0')+19110000000000) 
                             AND TRANSNO <  (RPAD(:P0,13,'0')+19110100000000) 
                         ";
            }
            else
            {
                sql = @"  SELECT NVL(SUM(NVL(EXTRA_DISC_AMOUNT,0)),0) EXTRA_DISC_AMOUNT
                            FROM PH_INVOICE
                           WHERE TRANSNO >  (RPAD(:P0,13,'0')+19110000000000) 
                             AND TRANSNO <  (RPAD(:P0,13,'0')+19110100000000)
                         ";
            }
            return DBWork.Connection.ExecuteScalar(sql, new { P0 = p0 }, DBWork.Transaction).ToString();
        }


        public class FA0081MatserMODEL : JCLib.Mvc.BaseModel
        {
            public string F1 { get; set; }
            public string F2 { get; set; }
            public string F3 { get; set; }
            public string F4 { get; set; }
            public string F5 { get; set; }
            public string F6 { get; set; }
            public string F7 { get; set; }
            public string F8 { get; set; }
            public string F9 { get; set; }
            public string F10 { get; set; }
            public string F11 { get; set; }
            public string F12 { get; set; }
            public string F13 { get; set; }
            public string F14 { get; set; }
            public string F15 { get; set; }
            public string F16 { get; set; }
            public string F17 { get; set; }
            public string F18 { get; set; }
            public string F19 { get; set; }
            public string F20 { get; set; }
            public string F21 { get; set; }
            public string F22 { get; set; }
        }

        public class FA0081ReportMODEL : JCLib.Mvc.BaseModel
        {
            public string F1 { get; set; }
            public string F2 { get; set; }
            public string F3 { get; set; }
            public string F4 { get; set; }
            public string F5 { get; set; }
            public string F6 { get; set; }
            public string F7 { get; set; }
            public string F8 { get; set; }
            public string F9 { get; set; }
            public string F10 { get; set; }
            public string F11 { get; set; }
            public string F12 { get; set; }
            public string F13 { get; set; }
            public string F14 { get; set; }
            public string F15 { get; set; }
            public string F16 { get; set; }
            public string F17 { get; set; }
            public string F18 { get; set; }
            public string F19 { get; set; }
            public string F20 { get; set; }
            public string F21 { get; set; }
            public string F22 { get; set; }
        }
    }
}
