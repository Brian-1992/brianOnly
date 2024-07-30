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
    
    public class FA0079Repository : JCLib.Mvc.BaseRepository
    {
        public FA0079Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }


        public IEnumerable<FA0079MatserMODEL> GetAllM(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p9, string yyymm, string isab, string user, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = "";
        
            sql += BuildGridSql(ref p, p0, p1, p2, p3, p4, p5, p6, p7, p8, p9,yyymm,isab,user);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<FA0079MatserMODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }//GetAllM

        public IEnumerable<FA0079DetailMODEL> GetAllD(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p9,  string yyymm, string isab, string user, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = "";

            sql = @"select NVL(SUM(ROUND(F10 * F4)) ,0 ) F1 ,
                                NVL(SUM(ROUND(F15 * F6)) ,0 ) F2 ,
                                NVL(SUM(ROUND(F9 * F6)) ,0 ) F3 ,
                                NVL(SUM(ROUND(case when F25='買斷' then F9*F6 else 0 end)) ,0 ) F4 ,
                                NVL(SUM(ROUND(case when F25='寄庫' then F9*F6 else 0 end)) ,0 ) F5 ,
                                NVL(SUM(ROUND(F11 * F6)) ,0 ) F6 ,
                                NVL(SUM(ROUND(0 * F6)) ,0 ) F7 ,
                                NVL(SUM(ROUND(F8 * F6)) ,0 ) F8 ,
                                NVL(SUM(ROUND(0 * F4)) ,0 ) F9 ,
                                NVL(SUM(ROUND(F10 * (F4 - F5))) ,0 ) F10 ,
                                NVL(SUM(ROUND(F12 * F6)) ,0 ) F11 ,
                                NVL(SUM(ROUND(F15 * F6)) ,0 ) F12 ,
                                NVL(SUM(ROUND(F8 * F6)) ,0 ) F13 ,
                                NVL(SUM(ROUND(0 * F6)) ,0 ) F14 ,
                                NVL(SUM(ROUND(0 * F6)) ,0 ) F15 ,
                                NVL(SUM(ROUND(0 * F6)) ,0 ) F16 ,
                                NVL(SUM(ROUND(F14 * F6)) ,0 ) F17 ,
                                NVL(SUM(ROUND(F38 * F6)) ,0 ) F18 ,
                                NVL(SUM(ROUND(0 * F6)) ,0 ) F19
                        from (";
            sql += BuildGridSql(ref p, p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, yyymm, isab, user);

            sql += ")";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<FA0079DetailMODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        private string BuildGridSql(
            ref DynamicParameters p,
            string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p9,
            string yyymm, string isab,string user)
        {
            var sql = "";
            if (p0 == yyymm)
            {
                sql = @"  SELECT * FROM (SELECT
                            (select WH_NO||' '||WH_NAME from MI_WHMAST where WH_NO=A.WH_NO) F1,         --庫房代碼
                            D.MMCODE F2,        --藥材代碼
                            D.MMNAME_C F3,      --藥材名稱
                            B.CONT_PRICE F4,    --上月單價
                            B.DISC_CPRICE F5,   --上月優惠單價
                            D.M_CONTPRICE F6,   --目前單價
                            D.DISC_CPRICE F7,   --目前優惠單價
                            A.INV_QTY F8,   --實存量
                            A.INV_QTY F9,       --應存量
                            NVL ((SELECT SUM(INV_QTY)/COUNT(*) FROM MI_WINVMON WHERE  DATA_YM = TWN_PYM (TWN_YYYMM(SYSDATE)) AND WH_NO = A.WH_NO AND MMCODE = A.MMCODE GROUP BY MMCODE),0) F10,  --上月結存
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
                            END) F11,               --進貨量
                            A.REJ_OUTQTY F12,       --退貨量
                            (CASE C.WH_GRADE
                              WHEN '1'
                              THEN
                                 A.APL_OUTQTY
                              ELSE
                            0
                            END) F13,               --撥發量
                            A.BAK_INQTY F14,        --退料量
                            A.USE_QTY F15,          --消耗量
                            A.EXG_OUTQTY F16,       --出貨量
                            D.UNITRATE  F17,       --轉換量比
                            D.BASE_UNIT F18,        --單位
                            D.M_CONTPRICE * (A.INV_QTY+A.INVENTORYQTY) F19,     --庫存成本
                            D.M_CONTPRICE * A.INV_QTY F20,                      --應有庫存成本
                            D.M_AGENNO F21,         --廠商代碼
                            E.EASYNAME F22,         --廠商簡稱
                            E.AGEN_NAMEC F23,       --廠商名稱
                            DECODE(D.E_RESTRICTCODE,'N','否','是') F24,           --管制品
                            DECODE(D.E_SOURCECODE,'P','買斷','C','寄庫','') F25,  --買斷寄庫
                            (select DATA_VALUE || ':' || DATA_DESC from PARAM_D 
                            where GRP_CODE ='MI_MAST' 
                            and DATA_NAME = 'MAT_CLASS_SUB' and DATA_VALUE = D.MAT_CLASS_SUB) F26,   --藥材類別
                            DECODE(D.M_CONTID,'0','合約','2','非合約','') F27,   --合約方式
                            D.CASENO F28,                                       --案號
                            TWN_YYYMM(D.E_CODATE) F29,                          --合約到期日
                            DECODE(D.TOUCHCASE,'0','無合約','1','院內選項 ','2','非院內選項 ','3','院內自辦合約','') F30,  --合約類別
                            D.M_DISCPERC F31,       --優惠比
                            A.TRN_INQTY F32,        --調撥入庫
                            A.TRN_OUTQTY F33,       --調撥出庫
                            A.BAK_OUTQTY F34,       --繳回出庫總量
                            A.DIS_OUTQTY F35,       --報廢總量
                            A.EXG_INQTY F36,        --換貨入庫總量
                            A.EXG_OUTQTY F37,       --換貨出庫總量
                            A.INVENTORYQTY F38,     --盤點差異量
                            GET_STORELOC(A.WH_NO,D.MMCODE) F39,     --儲位
                            INV_QTY(C.PWH_NO, A.MMCODE) F40         --上級庫存量
                            FROM MI_WHINV A, MI_WHCOST B, MI_WHMAST C, MI_MAST D, PH_VENDER E
                                WHERE TWN_YYYMM(SYSDATE) = B.DATA_YM(+)
                                      AND A.MMCODE = B.MMCODE(+)
                                      AND A.WH_NO = C.WH_NO
                                      AND A.MMCODE = D.MMCODE
                                      AND D.M_AGENNO = E.AGEN_NO(+)
                            ";
            }
            else
            {
                sql = @" SELECT * FROM (SELECT
                            (select WH_NO||' '||WH_NAME from MI_WHMAST where WH_NO=A.WH_NO) F1,  
                            D.MMCODE F2,  
                            D.MMNAME_C F3,  
                            B.CONT_PRICE F4,  
                            B.DISC_CPRICE F5,  
                            D.M_CONTPRICE F6,  
                            D.DISC_CPRICE F7,  
                            A.INV_QTY F8,  
                            A.INV_QTY F9,  
                            NVL ((SELECT SUM(INV_QTY)/COUNT(*) FROM MI_WINVMON WHERE  DATA_YM = TWN_PYM (A.DATA_YM) AND WH_NO = A.WH_NO AND MMCODE = A.MMCODE GROUP BY MMCODE),0) F10,  
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
                            END) F11,  
                            A.REJ_OUTQTY F12,  
                            (CASE C.WH_GRADE
                              WHEN '1'
                              THEN
                                 A.APL_OUTQTY
                              ELSE
                            0
                            END) F13,  
                            A.BAK_INQTY F14,  
                            A.USE_QTY F15,  
                            A.EXG_OUTQTY F16,  
                            D.UNITRATE  F17,  
                            D.BASE_UNIT F18,  
                            D.M_CONTPRICE * (A.INV_QTY+A.INVENTORYQTY) F19,  
                            D.M_CONTPRICE * A.INV_QTY F20,  
                            D.M_AGENNO F21,  
                            E.EASYNAME F22,  
                            E.AGEN_NAMEC F23,  
                            DECODE(D.E_RESTRICTCODE,'N','否','是') F24,  
                            DECODE(D.E_SOURCECODE,'P','買斷','C','寄庫','') F25,  
                            (select DATA_VALUE || ':' || DATA_DESC from PARAM_D 
                            where GRP_CODE ='MI_MAST' 
                            and DATA_NAME = 'MAT_CLASS_SUB' and DATA_VALUE = D.MAT_CLASS_SUB) F26,  
                            DECODE(D.M_CONTID,'0','合約','2','非合約','') F27,  
                            D.CASENO F28,  
                            TWN_YYYMM(D.E_CODATE) F29,  
                            DECODE(D.TOUCHCASE,'0','無合約','1','院內選項 ','2','非院內選項 ','3','院內自辦合約','') F30,  
                            D.M_DISCPERC F31,
                            A.TRN_INQTY F32,
                            A.TRN_OUTQTY F33,
                            A.BAK_OUTQTY F34,
                            A.DIS_OUTQTY F35,
                            A.EXG_INQTY F36,
                            A.EXG_OUTQTY F37,
                            A.INVENTORYQTY F38,
                            GET_STORELOC(A.WH_NO,D.MMCODE) F39,     --儲位
                            INV_QTY(C.PWH_NO, A.MMCODE) F40         --上級庫存量
                            FROM MI_WINVMON A, MI_WHCOST B, MI_WHMAST C, MI_MAST D, PH_VENDER E
                            WHERE A.DATA_YM = B.DATA_YM(+)
                              AND A.MMCODE = B.MMCODE(+)
                              AND A.WH_NO = C.WH_NO
                              AND A.MMCODE = D.MMCODE
                              AND D.M_AGENNO = E.AGEN_NO(+)
                             and A.DATA_YM=:p0
                            ";
                p.Add(":p0", p0);
            }

            if (p1 != "")
            {
                sql += " AND A.WH_NO = :p1 ";
                p.Add(":p1", p1);
            }
            else
            {
                if (isab == "Y")
                {
                    sql += @" and ((select count(*) from MI_WHID where WH_NO=C.WH_NO and WH_USERID = :userid ) > 0 
                                or (select count(*) from UR_UIR where RLNO in ('MAT_14', 'MED_14', 'MMSpl_14') and TUSER = :userid) > 0)";
                    p.Add(":userid", user);
                }
            }
            if (p2 != "")
            {
                if (p2 == "01" || p2 == "02")
                {
                    sql += " AND D.MAT_CLASS = :p2 ";
                }
                else
                {
                    sql += " AND D.MAT_CLASS_SUB = :p2 ";
                }
                p.Add(":p2", p2);
            }
            if (p3 != "")
            {
                sql += " AND A.MMCODE = :p3 ";
                p.Add(":p3", p3);
            }
            if (p4 != "")
            {
                sql += " AND D.E_SOURCECODE = :p4 ";
                p.Add(":p4", p4);
            }
            if (p5 != "")
            {
                sql += " AND D.M_CONTID = :p5 ";
                p.Add(":p5", p5);
            }
            if (p6 != "")
            {
                sql += " AND D.ORDERKIND = :p6 ";
                p.Add(":p6", p6);
            }
            if (p7 != "")
            {
                sql += " AND D.TOUCHCASE = :p7 ";
                p.Add(":p7", p7);
            }
            if (p8 != "")
            {
                sql += " AND D.E_RESTRICTCODE = :p8 ";
                p.Add(":p8", p8);
            }

            if (p9.Trim() != "")
            {
                string[] tmp = p9.Split(',');
                for (int i = 0; i < tmp.Length; i++)
                {
                    if (tmp[i] == "2")
                        sql += @" AND (A.APL_INQTY != 0 OR A.TRN_INQTY != 0 OR A.TRN_INQTY != 0 OR A.ADJ_INQTY != 0 OR
                                       A.BAK_INQTY != 0 OR A.EXG_INQTY != 0 OR A.MIL_INQTY != 0 OR A.INVENTORYQTY != 0 OR 
                                       A.APL_OUTQTY != 0 OR  A.TRN_OUTQTY != 0 OR A.ADJ_OUTQTY != 0 OR A.BAK_OUTQTY != 0 OR 
                                       A.REJ_OUTQTY != 0 OR A.DIS_OUTQTY != 0 OR A.EXG_OUTQTY != 0 OR A.MIL_OUTQTY != 0) ";
                }
            }

            sql += ") WHERE 1=1 ";
            if (p9.Trim() != "")
            {
                string[] tmp = p9.Split(',');
                for (int i = 0; i < tmp.Length; i++)
                {
                    if (tmp[i] == "1")
                        sql += "AND NOT (F8 = 0 AND F9 = 0 AND F10 = 0) ";
                    else if (tmp[i] == "2")
                        sql += @" AND (F12 != 0 OR F13 != 0 OR F14 != 0 OR F15 != 0 OR F16 != 0) ";
                }
            }
            return sql;
        }

        public IEnumerable<FA0079ReportMODEL> GetPrintData(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p9, string yyymm, string isab, string user)
        {
            var p = new DynamicParameters();
            var sql = "";

            sql += BuildGridSql(ref p, p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, yyymm, isab, user);

            return DBWork.Connection.Query<FA0079ReportMODEL>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<FA0079ReportMODELD> GetPrintData2(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p9, string yyymm, string isab, string user)
        {
            var p = new DynamicParameters();
            var sql = "";

            sql = @"select NVL(SUM(ROUND(F10 * F4)) ,0 ) F1 ,
                                NVL(SUM(ROUND(F15 * F6)) ,0 ) F2 ,
                                NVL(SUM(ROUND(F9 * F6)) ,0 ) F3 ,
                                NVL(SUM(ROUND(case when F25='買斷' then F9*F6 else 0 end)) ,0 ) F4 ,
                                NVL(SUM(ROUND(case when F25='寄庫' then F9*F6 else 0 end)) ,0 ) F5 ,
                                NVL(SUM(ROUND(F11 * F6)) ,0 ) F6 ,
                                NVL(SUM(ROUND(0 * F6)) ,0 ) F7 ,
                                NVL(SUM(ROUND(F8 * F6)) ,0 ) F8 ,
                                NVL(SUM(ROUND(0 * F4)) ,0 ) F9 ,
                                NVL(SUM(ROUND(F10 * (F4 - F5))) ,0 ) F10 ,
                                NVL(SUM(ROUND(F12 * F6)) ,0 ) F11 ,
                                NVL(SUM(ROUND(F15 * F6)) ,0 ) F12 ,
                                NVL(SUM(ROUND(F8 * F6)) ,0 ) F13 ,
                                NVL(SUM(ROUND(0 * F6)) ,0 ) F14 ,
                                NVL(SUM(ROUND(0 * F6)) ,0 ) F15 ,
                                NVL(SUM(ROUND(0 * F6)) ,0 ) F16 ,
                                NVL(SUM(ROUND(F14 * F6)) ,0 ) F17 ,
                                NVL(SUM(ROUND(F38 * F6)) ,0 ) F18 ,
                                NVL(SUM(ROUND(0 * F6)) ,0 ) F19
                        from (";
            sql += BuildGridSql(ref p, p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, yyymm, isab, user);

            sql += ")";

            return DBWork.Connection.Query<FA0079ReportMODELD>(sql, p, DBWork.Transaction);

        }

        public IEnumerable<MI_WHMAST> GetWhmastCombo(string p0, string isab, string user, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.WH_NO, A.WH_NAME, A.WH_KIND, A.WH_GRADE ";
            if (isab == "Y")
            {
                sql += @" FROM MI_WHMAST A,MI_WHID B WHERE A.WH_NO= B.WH_NO 
                            AND (B.WH_USERID = :userid
                                or (select count(*) from UR_UIR where RLNO in ('MAT_14', 'MED_14', 'MMSpl_14') and TUSER = :userid) > 0) ";
                p.Add(":userid", user);

            }
            else
            {
                sql += " FROM MI_WHMAST A WHERE 1=1 ";
            }
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
            string sql = @" select MAT_CLASS as VALUE,
                            '全部' || MAT_CLSNAME as TEXT
                            from MI_MATCLASS
                            where MAT_CLSID in ('1', '2')
                            union
                            select DATA_VALUE as VALUE, DATA_DESC as TEXT
                            from PARAM_D
                            where GRP_CODE ='MI_MAST' 
                            and DATA_NAME = 'MAT_CLASS_SUB'
                            and trim(DATA_DESC) is not null
                            order by VALUE ";

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
                            SELECT '0' VALUE, '合約品項' TEXT FROM DUAL
                            UNION
                            SELECT '2' VALUE, '非合約' TEXT FROM DUAL
                            ) ORDER BY VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetOrderCodeCombo()
        {
            string sql = @" SELECT VALUE, TEXT FROM (
                            SELECT '0' VALUE, '無' TEXT FROM DUAL
                            UNION
                            SELECT '1' VALUE, '常備品項' TEXT FROM DUAL
                            UNION
                            SELECT '2' VALUE, '小額採購' TEXT FROM DUAL
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

        
        public DataTable GetExcel(string p0, string p1, string p2, string p3, string p4, string p5, string p6, string p7, string p8, string p9, string yyymm, string isab, string user)
        {
            var p = new DynamicParameters();
            var sql = @"select F1 庫房代碼,  
                                               F2 as 藥材代碼, F3 as 藥材名稱,F4 as 上月單價,F5 as 上月優惠單價,
                                               F6 as 目前單價, F7 as 目前優惠單價, F8 as 實存量, F9 as 應存量,
                                               F10 as 上月結存, F11 as 進貨量, F12 as 退貨量, F13 as 撥發量,
                                               F14 as 退料量, F15 as 消耗量, F16 as 出貨量, F17 as 轉換量比,
                                               F18 as 單位, F19 as 庫存成本, F20 as 應有庫存成本, F21 as 廠商代碼,
                                               F22 as 廠商簡稱, F23 as 廠商名稱, F24 as 管制品, F25 as 買斷寄庫,
                                               F26 as 藥材類別, F27 as 合約方式, F28 as 案號, F29 as 合約到期日,
                                               F30 as 合約類別, F31 as 優惠比, F32 as 調撥入庫, F33 as 調撥出庫,
                                               F34 as 繳回出庫總量, F35 as 報廢總量, F36 as 換貨入庫總量,
                                               F37 as 換貨出庫總量, F38 as 盤點差異量, F39 as 儲位,  F40 as 上級庫存量
                                    from (";
            sql += BuildGridSql(ref p, p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, yyymm, isab, user);

            sql += ") T";

            DataTable dt = new DataTable();
            try
            {
                using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                    dt.Load(rdr);
            }
            catch (TimeoutException ex)
            {
                dt = null;
            }
            return dt;
        }

        public string GetExtraDiscAmout(string p0,string yyymm)
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


        public class FA0079MatserMODEL : JCLib.Mvc.BaseModel
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
            public string F23 { get; set; }
            public string F24 { get; set; }
            public string F25 { get; set; }
            public string F26 { get; set; }
            public string F27 { get; set; }
            public string F28 { get; set; }
            public string F29 { get; set; }
            public string F30 { get; set; }
            public string F31 { get; set; }
            public string F32 { get; set; }
            public string F33 { get; set; }
            public string F34 { get; set; }
            public string F35 { get; set; }
            public string F36 { get; set; }
            public string F37 { get; set; }
            public string F38 { get; set; }
            public string F39 { get; set; }
            public string F40 { get; set; }
            public string F41 { get; set; }
        }

        public class FA0079DetailMODEL : JCLib.Mvc.BaseModel
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
        }

        public class FA0079ReportMODEL : JCLib.Mvc.BaseModel
        {
            public string F1 { get; set; }
            public string F2 { get; set; }
            public string F3 { get; set; }
            public float F4 { get; set; }
            public float F5 { get; set; }
            public float F6 { get; set; }
            public float F7 { get; set; }
            public float F8 { get; set; }
            public float F9 { get; set; }
            public float F10 { get; set; }
            public float F11 { get; set; }
            public float F12 { get; set; }
            public float F13 { get; set; }
            public float F14 { get; set; }
            public float F15 { get; set; }
            public float F16 { get; set; }
            public float F17 { get; set; }
            public string F18 { get; set; }
            public float F19 { get; set; }
            public float F20 { get; set; }
            public string F21 { get; set; }
            public string F22 { get; set; }
            public string F23 { get; set; }
            public string F24 { get; set; }
            public string F25 { get; set; }
            public string F26 { get; set; }
            public string F27 { get; set; }
            public string F28 { get; set; }
            public string F29 { get; set; }
            public string F30 { get; set; }
            public float F31 { get; set; }
            public float F32 { get; set; }
            public float F33 { get; set; }
            public float F34 { get; set; }
            public float F35 { get; set; }
            public float F36 { get; set; }
            public float F37 { get; set; }
            public float F38 { get; set; }
            public string F39 { get; set; }
            public float F40 { get; set; }
        }

        public class FA0079ReportMODELD : JCLib.Mvc.BaseModel
        {
            public float F1 { get; set; }
            public float F2 { get; set; }
            public float F3 { get; set; }
            public float F4 { get; set; }
            public float F5 { get; set; }
            public float F6 { get; set; }
            public float F7 { get; set; }
            public float F8 { get; set; }
            public float F9 { get; set; }
            public float F10 { get; set; }
            public float F11 { get; set; }
            public float F12 { get; set; }
            public float F13 { get; set; }
            public float F14 { get; set; }
            public float F15 { get; set; }
            public float F16 { get; set; }
            public float F17 { get; set; }
            public float F18 { get; set; }
            public float F19 { get; set; }
        }
    }
}
