using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using WebApp.Models.C;
using System.Collections.Generic;

namespace WebApp.Repository.C
{
    public class CE0040Repository : JCLib.Mvc.BaseRepository
    {
        public CE0040Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //Master
        public IEnumerable<CE0040> GetAllM(string chk_ym, string wh_no, bool showNotFinish, bool notStarted, bool started, string menuLink, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @" SELECT A.CHK_YM CHK_YM, 
                                A.CHK_NO CHK_NO, 
                                A.CHK_WH_NO CHK_WH_NO,
                                A.CHK_WH_NO || ' ' || WH_NAME(A.CHK_WH_NO) WH_NAME,
                                (SELECT MAT_CLASS
                                 FROM MI_MATCLASS 
                                 WHERE MAT_CLASS = A.CHK_CLASS) MAT_CLASS, 
                                (SELECT MAT_CLASS || ' ' || MAT_CLSNAME 
                                 FROM MI_MATCLASS 
                                 WHERE MAT_CLASS = A.CHK_CLASS) MAT_CLSNAME, 
                                (SELECT F.DATA_VALUE || ' ' || F.DATA_DESC  
                                 FROM PARAM_D F  
                                 WHERE F.GRP_CODE = 'CHK_MAST'   
                                 AND F.DATA_NAME = 'CHK_STATUS'  
                                 AND F.DATA_VALUE = A.CHK_STATUS) CHK_STATUS,  
                                A.CHK_TOTAL CHK_TOTAL,   
                                A.CHK_NUM CHK_NUM
                         FROM CHK_MAST A
                         WHERE 1 = 1 
                         AND CHK_YM = :SET_YM
                         AND CHK_WH_NO = NVL(TRIM(:WH_NO), CHK_WH_NO) ";

            if (showNotFinish) {
                sql += " and a.chk_num <> a.chk_total";
            }

            if (notStarted)
            {
                sql += " and a.chk_num = 0  and a.chk_total > 0";
            }

            if (started)
            {
                sql += " and a.chk_num > 0";
            }

            //增加物料類別為「全部藥品」，只顯示藥品庫房盤點單
            if (menuLink == "FA0085")
            {
                sql += " AND  CHK_WH_KIND ='0'  AND A.CHK_CLASS = '01'";
            }

            p.Add(":SET_YM", chk_ym);
            p.Add(":WH_NO", wh_no);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CE0040>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        //Detail
        public IEnumerable<CE0040> GetAllD(string chk_no, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @" SELECT A.CHK_NO CHK_NO, 
                                A.MMCODE MMCODE, 
                                B.MMNAME_C MMNAME_C,  
                                B.MMNAME_E MMNAME_E,   
                                A.STORE_QTYC STORE_QTYC,   
                                A.CHK_QTY CHK_QTY,  
                                (CASE WHEN C.CHK_WH_GRADE = '1' THEN 0  
                                      WHEN C.CHK_WH_KIND = '0' AND C.CHK_WH_GRADE= '2' AND ALTERED_USE_QTY IS NOT NULL THEN ALTERED_USE_QTY 
                                      WHEN A.CHK_QTY <= A.STORE_QTYC THEN A.STORE_QTYC - A.CHK_QTY ELSE 0 END) USE_QTY, 
                                A.INVENTORY INVENTORY, 
                                INVENTORY * A.DISC_CPRICE DIFF_AMOUNT, 
                                TO_CHAR(A.CHK_TIME,'YYYY/MM/DD HH24:MI:SS') CHK_TIME
                         FROM CHK_MAST C, CHK_DETAIL A, MI_MAST_MON B
                         WHERE C.CHK_NO= :CHK_NO_TEXT 
                         AND A.CHK_NO = C.CHK_NO
                         AND A.MMCODE = B.MMCODE
                         AND B.DATA_YM = C.CHK_YM ";

            p.Add(":CHK_NO_TEXT", chk_no);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CE0040>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        //匯出
        public DataTable GetExcel(string chk_no)
        {
            DynamicParameters p = new DynamicParameters();

            var sql = @"  SELECT A.CHK_NO 盤點單號,
                                 A.MMCODE 院內碼, 
                                 B.MMNAME_C 中文品名, 
                                 B.MMNAME_E 英文品名, 
                                 (CASE WHEN (A.CHK_TIME IS NULL OR A.STORE_QTY_UPDATE_TIME IS NULL) THEN A.STORE_QTYC 
                                       ELSE A.STORE_QTY_N END) AS 電腦量,  
                                 A.CHK_QTY 盤點量,  
                                 A.CHK_TIME 盤點時間
                          FROM CHK_DETAIL A, MI_MAST B
                          WHERE A.CHK_NO = :CHK_NO_TEXT
                          AND A.MMCODE = B.MMCODE 
                          ORDER BY A.MMCODE ";

            p.Add(":CHK_NO_TEXT", chk_no);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        //盤點年月(combo)
        public IEnumerable<COMBO_MODEL> GetSetymCombo()
        {
            string sql = @" SELECT SET_YM VALUE
                            FROM MI_MNSET 
                            WHERE SET_STATUS = 'C' 
                            ORDER BY SET_YM DESC ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        //庫房代碼(combo)
        public IEnumerable<COMBO_MODEL> GetWhNoCombo(string id)
        {
            string sql = @" SELECT WH_NO VALUE, WH_NO || ' ' || WH_NAME TEXT
                            FROM MI_WHMAST A 
                            WHERE 1=1 
                            AND NVL(A.CANCEL_ID,'N') = 'N' 
                            ORDER BY WH_NO ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id });
        }
        //盤點單開立完成時間
        public IEnumerable<string> GetSetAtime(string ym)
        {
            var p = new DynamicParameters();

            string sql = @" SELECT TWN_TIME(SET_ATIME) VALUE
                            FROM CHK_MNSET
                            WHERE CHK_YM = :SET_YM";

            p.Add(":SET_YM", ym);
            return DBWork.Connection.Query<string>(sql, p, DBWork.Transaction);
        }
        //盤點單開立完成時間
        public IEnumerable<string> GetChkEndtime(string ym)
        {
            var p = new DynamicParameters();

            string sql = @" SELECT TWN_DATE(CHK_ENDTIME) 
                            FROM CHK_MNSET 
                            WHERE CHK_YM = :SET_YM";

            p.Add(":SET_YM", ym);
            return DBWork.Connection.Query<string>(sql, p, DBWork.Transaction);
        }
        //全院盤點結束時間
        public IEnumerable<string> GetChkClosetime(string ym)
        {
            var p = new DynamicParameters();

            string sql = @" SELECT TWN_TIME(CHK_CLOSETIME) 
                            FROM CHK_MNSET  
                            WHERE CHK_YM = :SET_YM";

            p.Add(":SET_YM", ym);
            return DBWork.Connection.Query<string>(sql, p, DBWork.Transaction);
        }
        //檢查是否有庫房未盤點
        public IEnumerable<string> CheckChkClosetime(string ym)
        {
            var p = new DynamicParameters();

            string sql = @" SELECT 1 
                            FROM CHK_DETAIL 
                            WHERE CHK_NO IN (SELECT CHK_NO 
                                             FROM CHK_MAST 
                                             WHERE CHK_YM = :SET_YM)
                            AND CHK_TIME IS NULL ";

            p.Add(":SET_YM", ym);
            return DBWork.Connection.Query<string>(sql, p, DBWork.Transaction);
        }
        //尚未完成盤點的項目
        public IEnumerable<CHK_MAST> ShowChkData(string ym)
        {
            var p = new DynamicParameters();

            string sql = @" SELECT A.CHK_WH_NO, 
                                   B.MMCODE,  
                                   C.MMNAME_C || ' ' || C.MMNAME_E AS MMNAME
                            FROM CHK_MAST A, CHK_DETAIL B, MI_MAST C
                            WHERE A.CHK_YM = :SET_YM 
                            AND A.CHK_NO = B.CHK_NO 
                            AND B.CHK_TIME IS NULL
                            AND B.MMCODE = C.MMCODE
                            ORDER BY A.CHK_WH_NO, B.MMCODE ";

            p.Add(":SET_YM", ym);
            return DBWork.Connection.Query<CHK_MAST>(sql, p, DBWork.Transaction);
        }


        //設定盤點結束時間
        public int SetChkEndTime(CE0040 CE0040)
        {
            string sql = @"  UPDATE CHK_MNSET 
                                SET CHK_ENDTIME = TO_DATE(:CHK_ENDTIME,'YYYY-MM-DD'),
                                    UPDATE_TIME	= SYSDATE,
                                    UPDATE_USER	= :UPDATE_USER,
                                    UPDATE_IP	= :UPDATE_IP
                              WHERE CHK_YM = :CHK_YM";

            return DBWork.Connection.Execute(sql, CE0040, DBWork.Transaction);
        }
        //取得全部盤點單(結束全院盤點)
        public IEnumerable<CE0040> GetChkMast(string set_ym)
        {
            var p = new DynamicParameters();

            string sql = @" SELECT CHK_NO CHK_NO,
                                   CHK_WH_NO CHK_WH_NO, 
                                   CHK_WH_KIND CHK_WH_KIND, 
                                   CHK_WH_GRADE CHK_WH_GRADE, 
                                   WH_NAME(CHK_WH_NO) WH_NAME 
                            FROM CHK_MAST 
                            WHERE CHK_YM = :SET_YM";

            p.Add(":SET_YM", set_ym);
            return DBWork.Connection.Query<CE0040>(sql, p, DBWork.Transaction);
        }
        //更新CHK_DETAIL狀態(結束全院盤點)
        public int UpdateChkDetail(CE0040 item)
        {
            string sql = @"   UPDATE CHK_DETAIL 
                              SET STATUS_INI = '3', 
                                  UPDATE_TIME = SYSDATE,
                                  UPDATE_USER = :UPDATE_USER,
                                  UPDATE_IP	= :UPDATE_IP
                            WHERE CHK_NO = :CHK_NO";
            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);
        }
        //新增至CHK_DETAILTOT(結束全院盤點)
        public int AddChkDetailtot(CE0040 item)
        {
            string sql = @"    INSERT INTO CHK_DETAILTOT(CHK_NO, MMCODE, MMNAME_C, MMNAME_E, BASE_UNIT, M_CONTPRICE, WH_NO, STORE_LOC,
                                                         MAT_CLASS, M_STOREID, STORE_QTY, STORE_QTYC, GAP_T, 
                                                         GAP_C, 
                                                         PRO_LOS_QTY, 
                                                         MISS_PER, APL_OUTQTY, CHK_QTY1, STATUS_TOT, CREATE_DATE, CREATE_USER, 
                                                         UPDATE_TIME, UPDATE_USER, UPDATE_IP, STORE_QTY1, CONSUME_QTY, CONSUME_AMOUNT)
                               SELECT CD.CHK_NO, CD.MMCODE, CD.MMNAME_C, CD.MMNAME_E, CD.BASE_UNIT, CD.M_CONTPRICE, CD.WH_NO, CD.STORE_LOC, 
                                      CD.MAT_CLASS, CD.M_STOREID, CD.STORE_QTYC, CD.STORE_QTYC, (CD.CHK_QTY - CD.STORE_QTYC),
                                      (CD.CHK_QTY - CD.STORE_QTYC), 
                                      (CASE WHEN :CHK_WH_GRADE = 1 THEN (CD.CHK_QTY - CD.STORE_QTYC)
                                            WHEN :CHK_WH_GRADE = 2 AND :CHK_WH_KIND = 0 THEN (CD.CHK_QTY - CD.STORE_QTYC) 
                                            WHEN :WH_NAME ='供應中心' THEN (CD.CHK_QTY - CD.STORE_QTYC) ELSE 0 END),
                                      0, CD.APL_OUTQTY, CD.CHK_QTY, '1', SYSDATE, :CREATE_USER, 
                                      SYSDATE, :UPDATE_USER, :UPDATE_IP, CD.STORE_QTYC, 0, 0
                               FROM CHK_DETAIL CD 
                               WHERE CD.CHK_NO = :CHK_NO ";
            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);
        }
        //取得明細與現存量(結束全院盤點)
        public IEnumerable<CE0040> GetChkDetail(string chk_no)
        {
            var p = new DynamicParameters();

            string sql = @" SELECT A.CHK_NO, A.MMCODE, A.WH_NO, A.STORE_QTYC, A.ALTERED_USE_QTY, A.USE_QTY, A.USE_QTY_AF_CHK,
                                   B.INV_QTY, A.INVENTORY, A.CHK_QTY, a.store_qtyc
                            FROM CHK_DETAIL A, MI_WHINV B 
                            WHERE A.CHK_NO = :CHK_NO_TEXT 
                            AND A.WH_NO = B.WH_NO 
                            AND A.MMCODE = B.MMCODE ";

            p.Add(":CHK_NO_TEXT", chk_no);
            return DBWork.Connection.Query<CE0040>(sql, p, DBWork.Transaction);
        }
        //修改MI_WINVMON(結束全院盤點)
        public int UpdateMiWinvmon(CE0040 item)
        {
            string sql = @"  UPDATE MI_WINVMON
                                SET USE_QTY = (CASE WHEN :CHK_WH_GRADE = 1 THEN 0 
                                                    WHEN :CHK_WH_GRADE = 2 AND :CHK_WH_KIND = 0 THEN NVL(TO_NUMBER(:ALTERED_USE_QTY), TO_NUMBER(:USE_QTY))
                                                    WHEN :WH_NAME = '供應中心' THEN 0 
                                                    ELSE TO_NUMBER(:USE_QTY_AF_CHK) END),
                                          INVENTORYQTY = :INVENTORY, 
                                          INV_QTY = nvl(to_number(INV_QTY), 0) + (nvl(to_number(:CHK_QTY), 0) - nvl(to_number(:STORE_QTYC), 0))
                              WHERE DATA_YM = :SET_YM
                              AND WH_NO = :WH_NO
                              AND MMCODE = :MMCODE ";
            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);
        }
        //修改MI_WHINV(結束全院盤點)
        public int UpdateMiWhinv(CE0040 item)
        {
            string sql = @"  UPDATE MI_WHINV 
                                SET INV_QTY = nvl(to_number(INV_QTY), 0) + (nvl(to_number(:CHK_QTY), 0) - nvl(to_number(:STORE_QTYC), 0))
                                WHERE WH_NO = :WH_NO 
                                AND MMCODE = :MMCODE ";
            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);
        }
        //新增MI_WHTRNS(結束全院盤點)
        public int AddMiWhtrns(CE0040 item)
        {
            string sql = @"  INSERT INTO MI_WHTRNS (WH_NO, TR_DATE, TR_SNO, MMCODE, TR_INV_QTY, TR_ONWAY_QTY, TR_DOCNO, TR_IO, TR_MCODE, 
                                                    BF_TR_INVQTY, AF_TR_INVQTY)
                             SELECT A.WH_NO, SYSDATE, WHTRNS_SEQ.NEXTVAL, A.MMCODE, (A.CHK_QTY - A.STORE_QTYC), 0, A.CHK_NO, 'I', 'CHIO', 
                                    :INV_QTY, :INV_QTY + (A.CHK_QTY - A.STORE_QTYC)
                             FROM CHK_DETAIL A 
                             WHERE A.CHK_NO = :CHK_NO  
                             AND A.MMCODE = :MMCODE";
            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);
        }
        //更新CHK_MAST狀態(結束全院盤點)
        public int UpdateChkMast(CE0040 item)
        {
            string sql = @"  UPDATE CHK_MAST 
                                SET CHK_STATUS = 'P',
                                    UPDATE_TIME = SYSDATE,
                                    UPDATE_USER = :UPDATE_USER,
                                    UPDATE_IP = :UPDATE_IP
                       WHERE CHK_NO = :CHK_NO ";

            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);
        }
        //更新CHK_MNSET closetime(結束全院盤點)
        public int UpdateChkMnset(string set_ym, string update_user, string update_ip)
        {
            var p = new DynamicParameters();

            string sql = @"  UPDATE CHK_MNSET 
                                SET CHK_CLOSETIME = SYSDATE,
                                    UPDATE_TIME = SYSDATE,
                                    UPDATE_USER = :UPDATE_USER,
                                    UPDATE_IP = :UPDATE_IP
                              WHERE CHK_YM = :SET_YM ";

            p.Add(":SET_YM", set_ym);
            p.Add(":UPDATE_USER", update_user);
            p.Add(":UPDATE_IP", update_ip);

            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }
    }
}