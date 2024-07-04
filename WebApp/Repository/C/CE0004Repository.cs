using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using JCLib.DB;
using JCLib.Mvc;
using Dapper;
using WebApp.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.C
{
    public class CE0004Repository : JCLib.Mvc.BaseRepository
    {
        public CE0004Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<CHK_MAST> GetAll(CHK_MAST_QUERY_PARAMS query)
        {
            var p = new DynamicParameters();
            //var sql = @"Select a.CHK_NO, a.CHK_YM, a.CHK_WH_NO
            //            , a.CHK_KEEPER || ' ' || USER_NAME(a.CHK_KEEPER) AS CHK_KEEPER
            //            , WH_NAME(a.CHK_WH_NO) AS WH_NAME
            //            , a.CHK_NUM || '/' || a.CHK_TOTAL AS QTY
            //            , a.CHK_LEVEL || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_LEVEL' AND DATA_VALUE=a.CHK_LEVEL) AS CHK_LEVEL
            //            , a.CHK_WH_GRADE || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_WH_GRADE' AND DATA_VALUE=a.CHK_WH_GRADE) AS CHK_WH_GRADE
            //            , a.CHK_WH_KIND || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_WH_KIND' AND DATA_VALUE=a.CHK_WH_KIND) AS CHK_WH_KIND
            //            , a.CHK_PERIOD || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_PERIOD' AND DATA_VALUE=a.CHK_PERIOD) AS CHK_PERIOD
            //            , CASE WHEN a.CHK_WH_KIND='0' THEN a.CHK_TYPE || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_WH_KIND_0' AND DATA_VALUE=a.CHK_TYPE)
            //            ELSE a.CHK_TYPE || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_WH_KIND_1' AND DATA_VALUE=a.CHK_TYPE) END AS CHK_TYPE
            //            , a.CHK_STATUS || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_STATUS' AND DATA_VALUE=a.CHK_STATUS) AS CHK_STATUS
            //            from CHK_MAST a, MI_WHID b
            //            where 1=1 and chk_status='2' and CHK_LEVEL='1'
            //              and b.WH_USERID = :CHK_KEEPER
            //              and a.CHK_WH_NO = b.WH_NO";
            //p.Add(":CHK_KEEPER", query.CHK_KEEPER);
            var sql = @"Select a.CHK_NO, a.CHK_YM, a.CHK_WH_NO, a.chk_level, a.chk_wh_grade, a.chk_wh_kind, a.chk_period, a.chk_type, a.chk_status, a.create_date
                        , USER_NAME(a.CHK_KEEPER) AS CHK_KEEPER
                        , WH_NAME(a.CHK_WH_NO) AS WH_NAME
                        , a.CHK_NUM || '/' || a.CHK_TOTAL AS QTY
                        , a.CHK_LEVEL || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_LEVEL' AND DATA_VALUE=a.CHK_LEVEL) AS CHK_LEVEL_NAME
                        , a.CHK_WH_GRADE || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_WH_GRADE' AND DATA_VALUE=a.CHK_WH_GRADE) AS WH_GRADE_NAME
                        , a.CHK_WH_KIND || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_WH_KIND' AND DATA_VALUE=a.CHK_WH_KIND) AS WH_KIND_NAME
                        , a.CHK_PERIOD || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_PERIOD' AND DATA_VALUE=a.CHK_PERIOD) AS CHK_PERIOD_NAME
                        , CHK_CLASS || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_CLASS' AND DATA_VALUE=CHK_CLASS) AS CHK_CLASS_NAME
                        ,(CASE WHEN CHK_WH_KIND='0' 
                                    THEN CHK_TYPE || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_WH_KIND_0' AND DATA_VALUE=CHK_TYPE)
                               when chk_wh_kind = 'E'
                                    then CHK_TYPE || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_WH_KIND_E' AND DATA_VALUE=CHK_TYPE)
                               when chk_wh_kind = 'C'
                                    then CHK_TYPE || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_WH_KIND_C' AND DATA_VALUE=CHK_TYPE)
                               ELSE CHK_TYPE || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_WH_KIND_1' AND DATA_VALUE=CHK_TYPE) 
                           END) AS CHK_TYPE_NAME
                        , a.CHK_STATUS || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_STATUS' AND DATA_VALUE=a.CHK_STATUS) AS CHK_STATUS_NAME
                        from CHK_MAST a
                        where 1=1 
                          and a.chk_status<>'0' and a.CHK_LEVEL='1'";
            p.Add(":CHK_KEEPER", query.CHK_KEEPER);

            if (query.CHK_NO != "")
            {
                sql += " AND CHK_NO=:CHK_NO";
                p.Add(":CHK_NO", query.CHK_NO);
            }

            if (query.CHK_WH_NO != "")
            {
                sql += " AND CHK_WH_NO=:CHK_WH_NO";
                p.Add(":CHK_WH_NO", query.CHK_WH_NO);
            }

            if (query.CHK_YM != "")
            {
                sql += " and CHK_YM LIKE :CHK_YM";
                p.Add(":CHK_YM", string.Format("{0}%", query.CHK_YM));
            }

            return DBWork.PagingQuery<CHK_MAST>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<CHK_DETAIL> GetAllDetail(CHK_MAST_QUERY_PARAMS query)
        {
            var p = new DynamicParameters();
            //var sql = @"Select * from CHK_DETAIL where 1=1 and status_ini='2'";
            var sql = @"Select a.chk_no, a.mmcode, a.mmname_c, a.mmname_e, a.base_unit,
                               a.m_contprice, a.wh_no, a.store_loc, a.loc_name, a.mat_class,
                               a.m_storeid, 
                               (case 
                                    when store_qty_update_time is null
                                         then store_qtyc - nvl(his_consume_qty_t, 0)
                                    else store_qty_n - nvl(his_consume_qty_t, 0)
                               end) as store_qtyc ,     
                               a.store_qtym, a.store_qtys,
                               a.chk_qty, 
                               a.chk_remark, 
                               a.chk_time, 
                               a.status_ini,
                               a.store_qtyc as ori_store_qtyc,
                               a.store_qty_n,  
                               a.store_qty_update_time,
                               (case 
                                    when store_qty_update_time is null
                                         then CHK_QTY - (STORE_QTYC- nvl(his_consume_qty_t, 0))
                                    else CHK_QTY - (store_qty_n- nvl(his_consume_qty_t, 0))
                               end) AS CHK_QTY_DIFF ,
                               a.his_consume_qty_t, 
                               a.his_consume_datatime
                          from CHK_DETAIL a where 1=1";
            if (query.CHK_NO != "")
            {
                sql += " AND CHK_NO=:CHK_NO";
                p.Add(":CHK_NO", query.CHK_NO);
            }
            return DBWork.PagingQuery<CHK_DETAIL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<CHK_DETAIL> GetGroupDetail(CHK_MAST_QUERY_PARAMS query)
        {
            var p = new DynamicParameters();
            //var sql = @"Select WH_NO, MMCODE from CHK_DETAIL
            //            where 1=1 and status_ini='2'";
            var sql = @"Select WH_NO, MMCODE, SUM(NVL (STORE_QTYS, 0)) STORE_QTYS, M_CONTPRICE
                          from CHK_DETAIL
                        where 1=1";

            if (query.CHK_NO != "")
            {
                sql += " AND CHK_NO=:CHK_NO";
                p.Add(":CHK_NO", query.CHK_NO);
            }
            sql += " GROUP BY WH_NO, MMCODE, M_CONTPRICE order by mmcode";
            return DBWork.Connection.Query<CHK_DETAIL>(sql, p, DBWork.Transaction);
        }

        public int DetailUpdate(CHK_DETAIL chk_detail)
        {
            var sql = @"UPDATE CHK_DETAIL SET CHK_QTY=:CHK_QTY, CHK_REMARK=trim(:CHK_REMARK), CHK_TIME=SYSDATE, UPDATE_TIME=SYSDATE, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP
                    WHERE CHK_NO=:CHK_NO AND MMCODE=:MMCODE and STORE_LOC=:STORE_LOC";
            return DBWork.Connection.Execute(sql, chk_detail, DBWork.Transaction);
        }

        public string GetSpecWhNo(string chk_no)
        {
            // 藥品 & 庫別級別(1:庫) = PH1X
            // 衛材 & 庫別級別(1:庫) = MM1X
            var sql = @"select CASE 
                         WHEN CHK_WH_KIND = '0' and  CHK_WH_GRADE ='1' then 'PH1X'
                         WHEN CHK_WH_KIND = '1' and  CHK_WH_GRADE ='1' THEN 'MM1X'
                        END as aaa
                     from CHK_MAST where CHK_NO =:CHK_NO";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { CHK_NO = chk_no }, DBWork.Transaction);
        }

        public string GetChkWhKind(string chk_no)
        {
            var sql = @"select CHK_WH_KIND from CHK_MAST where CHK_NO =:CHK_NO";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { CHK_NO = chk_no }, DBWork.Transaction);
        }

        //public string GetMmcode(string chk_no, string mmcode)
        //{
        //    var sql = @"select MMCODE FROM (SELECT MMCODE FROM CHK_DETAIL WHERE CHK_NO=:CHK_NO and MMCODE=:MMCODE GROUP BY WH_NO, MMCODE)";
        //    return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { CHK_NO = chk_no, MMCODE = mmcode }, DBWork.Transaction);
        //}

        public string GetSTORE_QTY(string chk_no, string mmcode, string chk_wh_grade)
        {
            //var sql = @"SELECT (STORE_QTYC - NVL((SELECT INV_QTY From MI_WHINV Where WH_NO=:SPEC_WH_NO and MMCODE=:MMCODE), 0)) as STORE_QTYC 
            //            FROM (SELECT SUM(NVL(STORE_QTYC, 0)) STORE_QTYC FROM CHK_DETAIL WHERE CHK_NO=:CHK_NO and MMCODE=:MMCODE GROUP BY MMCODE, WH_NO)";
            var sql = string.Format(@"SELECT SUM(NVL(
                                                    (case 
                                                        when store_qty_update_time is null
                                                            then store_qtyc
                                                        else store_qty_n
                                                     end), 0)) STORE_QTY 
                                        FROM CHK_DETAIL 
                                       WHERE CHK_NO=:CHK_NO and MMCODE=:MMCODE GROUP BY WH_NO, MMCODE");
                                       //,chk_wh_grade == "1" ? "STORE_QTYC" : "store_qty_n");
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { CHK_NO = chk_no, MMCODE = mmcode}, DBWork.Transaction);
        }

        public string GetSTORE_QTY_PH1S(string chk_no, string mmcode, string chk_wh_grade)
        {
            //var sql = @"SELECT (STORE_QTYC - NVL((SELECT INV_QTY From MI_WHINV Where WH_NO=:SPEC_WH_NO and MMCODE=:MMCODE), 0)) as STORE_QTYC 
            //            FROM (SELECT SUM(NVL(STORE_QTYC, 0)) STORE_QTYC FROM CHK_DETAIL WHERE CHK_NO=:CHK_NO and MMCODE=:MMCODE GROUP BY MMCODE, WH_NO)";
            var sql = string.Format(@"SELECT SUM(NVL(
                                                    (case 
                                                        when store_qty_update_time is null
                                                            then store_qtyc
                                                        else store_qty_n
                                                     end), 0)) STORE_QTY 
                                        FROM CHK_DETAIL 
                                       WHERE CHK_NO=:CHK_NO and MMCODE=:MMCODE GROUP BY WH_NO, MMCODE");
            //,chk_wh_grade == "1" ? "STORE_QTYC" : "store_qty_n");
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { CHK_NO = chk_no, MMCODE = mmcode }, DBWork.Transaction);
        }

        // for 藥品 & 衛材
        public string GetSTORE_QTYC(string chk_no, string ori_mmcode, string cond_mmcode, string spec_wh_no, string chk_wh_grade)
        {
            var sql = string.Format(@"select (store_qtyc - NVL((SELECT INV_QTY From MI_WHINV Where WH_NO=:SPEC_WH_NO and MMCODE=:COND_MMCODE), 0))
                                        from (select invqty 
                                                from MI_WHINV 
                                               where wh_no = (select chk_wh_no from CHK_MAST where chk_no = :chk_no) 
                                                 and mmcode = :mmcode) as store_qtyc ");
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { CHK_NO = chk_no, ORI_MMCODE = ori_mmcode, COND_MMCODE = cond_mmcode, SPEC_WH_NO = spec_wh_no }, DBWork.Transaction);
        }
        public string GetSTORE_QTYC_GRADE1(string chk_no, string ori_mmcode, string cond_mmcode, string spec_wh_no, string chk_wh_grade)
        {
            var sql = string.Format(@"SELECT (inv_qty)
                                        from MI_WHINV 
                                       where wh_no = (select chk_wh_no from CHK_MAST where chk_no = :CHK_NO) 
                                         and mmcode = :ORI_MMCODE");
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { CHK_NO = chk_no, ORI_MMCODE = ori_mmcode, COND_MMCODE = cond_mmcode, SPEC_WH_NO = spec_wh_no }, DBWork.Transaction);
        }

        public string GetSTORE_QTYC_PH1S(string chk_no, string ori_mmcode, string cond_mmcode, string spec_wh_no, string chk_wh_grade)
        {
            //var sql = @"SELECT (STORE_QTYC - NVL((SELECT INV_QTY From MI_WHINV Where WH_NO=:SPEC_WH_NO and MMCODE=:MMCODE), 0)) as STORE_QTYC 
            //            FROM (SELECT SUM(NVL(STORE_QTYC, 0)) STORE_QTYC FROM CHK_DETAIL WHERE CHK_NO=:CHK_NO and MMCODE=:MMCODE GROUP BY MMCODE, WH_NO)";
            var sql = string.Format(@"SELECT (STORE_QTYC) as STORE_QTYC 
                                        FROM (SELECT SUM(NVL((case 
                                                        when store_qty_update_time is null
                                                            then store_qtyc
                                                        else store_qty_n
                                                     end), 0)) STORE_QTYC FROM CHK_DETAIL WHERE CHK_NO=:CHK_NO and MMCODE=:ORI_MMCODE GROUP BY WH_NO, MMCODE)");
            //,chk_wh_grade == "1" ? "STORE_QTYC" : "store_qty_n");
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { CHK_NO = chk_no, ORI_MMCODE = ori_mmcode, COND_MMCODE = cond_mmcode, SPEC_WH_NO = spec_wh_no }, DBWork.Transaction);
        }

        // for 能設 & 通訊
        public string GetSTORE_QTYC_1(string chk_no, string mmcode)
        {
            var sql = @"SELECT (STORE_QTYC 
                    - NVL((SELECT INV_QTY From MI_WHINV Where WH_NO=SUBSTR (WH_NO, 1, 4) || 'M' and MMCODE=:MMCODE), 0)
                    - NVL((SELECT INV_QTY From MI_WHINV Where WH_NO=SUBSTR (WH_NO, 1, 4) || 'S' and MMCODE=:MMCODE), 0) ) as STORE_QTYC 
                        FROM (SELECT SUM(NVL(STORE_QTYC, 0)) STORE_QTYC FROM CHK_DETAIL WHERE CHK_NO=:CHK_NO and MMCODE=:MMCODE GROUP BY WH_NO, MMCODE)";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { CHK_NO = chk_no, MMCODE = mmcode }, DBWork.Transaction);
        }

        // for 藥品 & 衛材
        public string GetSTORE_QTYM(string chk_no, string ori_mmcode, string cond_mmcode, string spec_wh_no)
        {
            //var sql = @"SELECT (STORE_QTYM - NVL((SELECT INV_QTY From MI_WHINV Where WH_NO=:SPEC_WH_NO and MMCODE=:MMCODE), 0)) as STORE_QTYM 
            //            FROM (SELECT SUM(NVL(STORE_QTYM, 0)) STORE_QTYM FROM CHK_DETAIL WHERE CHK_NO=:CHK_NO and MMCODE=:MMCODE GROUP BY MMCODE, WH_NO)";
            // var sql = string.Format(@"SELECT (NVL((SELECT INV_QTY From MI_WHINV Where WH_NO=:SPEC_WH_NO and MMCODE=:COND_MMCODE), 0)) as STORE_QTYM 
            //            FROM (SELECT SUM(NVL(STORE_QTYM, 0)) STORE_QTYM FROM CHK_DETAIL WHERE CHK_NO=:CHK_NO and MMCODE=:ORI_MMCODE GROUP BY MMCODE, WH_NO)");
            string sql = @"select nvl(inv_qty, 0) from MI_WHINV where wh_no = :SPEC_WH_NO and mmcode = :COND_MMCODE";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { CHK_NO = chk_no, ORI_MMCODE = ori_mmcode, COND_MMCODE = cond_mmcode, SPEC_WH_NO = spec_wh_no }, DBWork.Transaction);
        }

        // for 能設 & 通訊
        public string GetSTORE_QTYM_1(string chk_no, string mmcode)
        {
            //var sql = @"SELECT (NVL((SELECT INV_QTY From MI_WHINV Where WH_NO=SUBSTR (WH_NO, 1, 4) || 'M' and MMCODE=:MMCODE), 0)) as STORE_QTYM 
            //            FROM (SELECT SUM(NVL(STORE_QTYC, 0)) STORE_QTYC FROM CHK_DETAIL WHERE CHK_NO=:CHK_NO and MMCODE=:MMCODE GROUP BY WH_NO, MMCODE)";

            string sql = @"select nvl(inv_qty, 0) from MI_WHINV where wh_no = SUBSTR ((select chk_wh_no from CHK_MAST where chk_no = :chk_no), 1, 4) || 'M' and MMCODE=:MMCODE";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { CHK_NO = chk_no, MMCODE = mmcode}, DBWork.Transaction);
        }

        // for 能設 & 通訊
        public string GetSTORE_QTYS_1(string chk_no, string mmcode)
        {
            //var sql = @"SELECT (NVL((SELECT INV_QTY From MI_WHINV Where WH_NO=SUBSTR (WH_NO, 1, 4) || 'S' and MMCODE=:MMCODE), 0)) as STORE_QTYS
            //            FROM (SELECT SUM(NVL(STORE_QTYC, 0)) STORE_QTYC FROM CHK_DETAIL WHERE CHK_NO=:CHK_NO and MMCODE=:MMCODE GROUP BY WH_NO, MMCODE)";
            string sql = @"select nvl(inv_qty, 0) from MI_WHINV where wh_no = SUBSTR ((select chk_wh_no from CHK_MAST where chk_no = :chk_no), 1, 4) || 'S' and MMCODE=:MMCODE";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { CHK_NO = chk_no, MMCODE = mmcode }, DBWork.Transaction);
        }

        public string GetCHK_QTY(string chk_no, string mmcode)
        {
            var sql = @"SELECT CHK_QTY FROM (SELECT SUM(NVL(CHK_QTY, 0)) CHK_QTY FROM CHK_DETAIL WHERE CHK_NO=:CHK_NO and MMCODE=:MMCODE GROUP BY MMCODE, WH_NO)";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { CHK_NO = chk_no, MMCODE = mmcode }, DBWork.Transaction);
        }

        public string GetLAST_QTYC(string chk_ym, string mmcode, string wh_no)
        {
            var sql = @"SELECT NVL((select sum(USE_QTY) from MI_WINVMON
                            where DATA_YM >=  to_char(add_months(to_date(:CHK_YM,'yyymm'),-3),'yyymm') and wh_no=:WH_NO and mmcode=:MMCODE
                            group by WH_NO, MMCODE), 0) LAST_QTYC FROM DUAL";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { CHK_YM = chk_ym, MMCODE = mmcode, WH_NO = wh_no }, DBWork.Transaction);
        }

        //  for 藥品 & 衛材
        public string GetLAST_QTYM(string chk_ym, string mmcode, string spec_wh_no)
        {
            var sql = "";
            //if (spec_wh_no == "PH1X")
            //    sql = @"SELECT NVL((select sum(USE_QTY) from MI_WINVMON
            //                where DATA_YM >=  to_char(add_months(to_date(:CHK_YM,'yyymm'),-3),'yyymm') and WH_NO=:WH_NO and mmcode='004' || SUBSTR (:MMCODE, 4, 10)
            //                group by WH_NO, MMCODE), 0) LAST_QTYM FROM DUAL";
            //else
            //    sql = @"SELECT NVL((select sum(USE_QTY) from MI_WINVMON
            //                where DATA_YM >=  to_char(add_months(to_date(:CHK_YM,'yyymm'),-3),'yyymm') and WH_NO=:WH_NO and mmcode=:MMCODE
            //                group by WH_NO, MMCODE), 0) LAST_QTYM FROM DUAL";
            sql = @"SELECT NVL((select sum(USE_QTY) from MI_WINVMON
                            where DATA_YM >= to_char(add_months(to_date(:CHK_YM, 'yyymm'), -3), 'yyymm') and WH_NO =:WH_NO and mmcode =:MMCODE
                                group by WH_NO, MMCODE), 0) LAST_QTYM FROM DUAL";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { CHK_YM = chk_ym, MMCODE = mmcode, WH_NO = spec_wh_no }, DBWork.Transaction);
        }

        //  for 能設 & 通訊
        public string GetLAST_QTYM_1(string chk_ym, string mmcode)
        {
            var sql = @"SELECT NVL((select sum(USE_QTY) from MI_WINVMON
                            where DATA_YM >= to_char(add_months(to_date(:CHK_YM, 'yyymm'), -3), 'yyymm') and WH_NO=SUBSTR(WH_NO, 1, 4) || 'M' and mmcode =:MMCODE
                                group by WH_NO, MMCODE), 0) LAST_QTYM FROM DUAL";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { CHK_YM = chk_ym, MMCODE = mmcode }, DBWork.Transaction);
        }

        //  for 能設 & 通訊
        public string GetLAST_QTYS_1(string chk_ym, string mmcode)
        {
            var sql = @"SELECT NVL((select sum(USE_QTY) from MI_WINVMON
                            where DATA_YM >= to_char(add_months(to_date(:CHK_YM, 'yyymm'), -3), 'yyymm') and WH_NO=SUBSTR(WH_NO, 1, 4) || 'S' and mmcode =:MMCODE
                                group by WH_NO, MMCODE), 0) LAST_QTYM FROM DUAL";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { CHK_YM = chk_ym, MMCODE = mmcode }, DBWork.Transaction);
        }

        public string GetAPL_OUTQTY(string mmcode, string wh_no)
        {
            var sql = @"select APL_OUTQTY from MI_WHINV where wh_no=:WH_NO and mmcode=:MMCODE";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { MMCODE = mmcode, WH_NO = wh_no }, DBWork.Transaction);
        }

        public int InsertDetailTot(INSERT_CHK_DETAIL_PARAMS myParams)
        {
            var p = new DynamicParameters();
            p.Add(":STORE_QTY", myParams.STORE_QTY);
            p.Add(":STORE_QTYC", myParams.STORE_QTYC);
            p.Add(":STORE_QTYM", myParams.STORE_QTYM);
            p.Add(":STORE_QTYS", myParams.STORE_QTYS);
            p.Add(":LAST_QTY", myParams.LAST_QTY);
            p.Add(":LAST_QTYC", myParams.LAST_QTYC);
            p.Add(":LAST_QTYM", myParams.LAST_QTYM);
            p.Add(":LAST_QTYS", myParams.LAST_QTYS);
            p.Add(":APL_OUTQTY", myParams.APL_OUTQTY);
            p.Add(":CHK_QTY", myParams.CHK_QTY);
            p.Add(":GAP_T", myParams.GAP_T);
            p.Add(":GAP_C", myParams.GAP_C);
            p.Add(":PRO_LOS_QTY", myParams.PRO_LOS_QTY);
            p.Add(":PRO_LOS_AMOUNT", myParams.PRO_LOS_AMOUNT);
            p.Add(":MISS_PER", myParams.MISS_PER);
            p.Add(":MISS_PERC", myParams.MISS_PERC);
            p.Add(":UPDATE_USER", myParams.USER);
            p.Add(":UPDATE_IP", myParams.UPDATE_IP);
            p.Add(":CHK_NO", myParams.CHK_NO);
            p.Add(":MMCODE", myParams.MMCODE);
            p.Add(":STORE_LOC", myParams.STORE_LOC);
            p.Add(":CONSUME_QTY", myParams.CONSUME_QTY);
            p.Add(":CONSUME_AMOUNT", myParams.CONSUME_AMOUNT);
            p.Add(":HIS_CONSUME_QTY_T1", myParams.HIS_CONSUME_QTY_T);
            p.Add(":HIS_CONSUME_DATATIME1", myParams.HIS_CONSUME_DATATIME);
            p.Add(":STORE_QTY_N", myParams.STORE_QTY_N);

            var sql = @"INSERT INTO CHK_DETAILTOT (chk_no, mmcode, mmname_c, mmname_e, base_unit, m_contprice, wh_no,
                                                   store_loc, mat_class, m_storeid, store_qty, store_qtyc, store_qtym, store_qtys,
                                                   last_qty, last_qtyc, last_qtym, last_qtys, gap_t, gap_c, gap_m, gap_s,
                                                   pro_los_qty, pro_los_amount, miss_per, miss_perc, miss_perm, miss_pers,
                                                   apl_outqty, chk_remark, chk_qty1, chk_qty2, chk_qty3, status_tot,
                                                   create_date, create_user,update_time, update_user, update_ip,
                                                   store_qty1, store_qty2, store_qty3, store_qty_time1, consume_qty, consume_amount,
                                                   his_consume_qty_t1, his_consume_datatime1)
                        (select distinct
                                a.CHK_NO ,
                                a.MMCODE  ,
                                b.MMNAME_C ,
                                b.MMNAME_E ,
                                b.BASE_UNIT ,
                                b.M_CONTPRICE ,
                                a.WH_NO  ,
                                '' STORE_LOC,
                                b.MAT_CLASS  ,
                                b.M_STOREID ,
                                :STORE_QTY  ,
                                :STORE_QTYC  ,
                                :STORE_QTYM  ,
                                :STORE_QTYS  ,
                                :LAST_QTY,
                                :LAST_QTYC,                  
                                :LAST_QTYM,
                                :LAST_QTYS   ,                  
                                :GAP_T,       
                                :GAP_C   ,
                                0 GAP_M   ,
                                0 GAP_S   ,
                                :PRO_LOS_QTY ,
                                :PRO_LOS_AMOUNT ,
                                :MISS_PER,
                                :MISS_PERC ,
                                0 MISS_PERM  ,
                                0 MISS_PERS  ,
                                :APL_OUTQTY,                         
                                0 CHK_REMARK ,
                                :CHK_QTY ,                          
                                0 CHK_QTY2  ,
                                0 CHK_QTY3  ,
                                '1' STATUS_TOT  , 
                                SYSDATE,
                                :UPDATE_USER,
                                SYSDATE,
                                :UPDATE_USER ,
                                :UPDATE_IP  ,
                                :STORE_QTY_N as store_qty1,
                                0 as store_qty2, 0 as store_qty3,
                                (select store_qty_update_time from CHK_DETAIL 
                                  where chk_no = a.chk_no and mmcode = b.mmcode and rownum = 1) as store_qty_time1,
                                :CONSUME_QTY, :CONSUME_AMOUNT,
                                :HIS_CONSUME_QTY_T1, :HIS_CONSUME_DATATIME1
                           from CHK_DETAIL a, MI_MAST b
                          where a.CHK_NO=:CHK_NO AND a.MMCODE=:MMCODE AND a.MMCODE = b.MMCODE)";
            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public int UpdateStatus(string chk_no, string user, string ip)
        {
            var sql = @"UPDATE CHK_MAST set CHK_STATUS='3', UPDATE_TIME=SYSDATE, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP where CHK_NO=:CHK_NO";
            DBWork.Connection.Execute(sql, new { CHK_NO = chk_no, UPDATE_USER = user, UPDATE_IP = ip }, DBWork.Transaction);
            sql = @"UPDATE CHK_DETAIL set STATUS_INI='3', UPDATE_TIME=SYSDATE, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP where CHK_NO=:CHK_NO";
            return DBWork.Connection.Execute(sql, new { CHK_NO = chk_no, UPDATE_USER = user, UPDATE_IP = ip }, DBWork.Transaction);
        }

        public SP_MODEL PostInvt(string chkno, string updusr, string updip)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_CHKNO", value: chkno, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 21);
            p.Add("I_USERID", value: updusr, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 8);
            p.Add("I_UPDIP", value: updip, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 1);
            p.Add("O_RETMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 600);

            DBWork.Connection.Query("INV_SET.POST_INVT", p, commandType: CommandType.StoredProcedure);

            string errmsg = "";
            if (!p.Get<OracleString>("O_RETMSG").IsNull)        // 因store procedure呼叫正常, O_ERRMSG會回傳空字串, 會導致取值是NULL, 所以要先判定
                errmsg = p.Get<OracleString>("O_RETMSG").Value;

            SP_MODEL sp = new SP_MODEL
            {
                O_RETID = p.Get<OracleString>("O_RETID").Value,
                O_ERRMSG = errmsg
            };
            return sp;
        }

        public class CHK_MAST_QUERY_PARAMS
        {
            public string CHK_NO;
            public string CHK_WH_NO;
            public string CHK_YM;
            public string CHK_KEEPER;
        }

        public class INSERT_CHK_DETAIL_PARAMS
        {
            public string CHK_NO;
            public string MMCODE;
            public string STORE_LOC;
            public string STORE_QTY;
            public string STORE_QTYC;
            public string STORE_QTYM;
            public string STORE_QTYS;
            public string LAST_QTY;
            public string LAST_QTYC;
            public string LAST_QTYM;
            public string LAST_QTYS;
            public string APL_OUTQTY;
            public string CHK_QTY;
            public string GAP_T;
            public string GAP_C;
            public string PRO_LOS_QTY;
            public string PRO_LOS_AMOUNT;
            public string MISS_PER;
            public string MISS_PERC;

            public string USER;
            public string UPDATE_IP;

            public string CONSUME_QTY { get; set; } 
            public string CONSUME_AMOUNT { get; set; }
            public string CHK_NO1 { get; set; }

            public string HIS_CONSUME_QTY_T { get; set; }
            public string HIS_CONSUME_DATATIME { get; set; }

            public string STORE_QTY_N { get; set; }
        }

        public IEnumerable<CHK_DETAIL> GetCurrentStatus(string chk_no) {
            string sql = @"select distinct a.chk_uid,
                                 (select una from UR_ID where tuser = a.chk_uid) as chk_uid_name,
                                  (select count(*) from CHK_DETAIL 
                                    where chk_no = a.chk_no and chk_uid = a.chk_uid 
                                      and chk_time is null) as undone_num,
                                  (select count(*) from CHK_DETAIL 
                                    where chk_no = a.chk_no and chk_uid = a.chk_uid 
                                      and chk_time is not null) as done_num,
                                  (case
                                    when (select count(*) from CHK_DETAIL 
                                           where chk_no = a.chk_no and chk_uid = a.chk_uid 
                                             and status_ini = '1') > 0
                                    then '未完成'
                                    else '已完成'
                                  end) as DONE_STATUS
                             from CHK_DETAIL a
                            where a.chk_no = :chk_no
                                  ";
            return DBWork.Connection.Query<CHK_DETAIL>(sql,new { chk_no = chk_no}, DBWork.Transaction);
        }


        public CHK_MAST GetChkMast(string chk_no) {
            string sql = @"select a.CHK_NO, a.CHK_YM, a.CHK_WH_NO, a.chk_level, a.chk_wh_grade, a.chk_wh_kind, a.chk_period, a.chk_type, a.chk_status, a.create_date
                                  , USER_NAME(a.CHK_KEEPER) AS CHK_KEEPER
                                  , WH_NAME(a.CHK_WH_NO) AS WH_NAME
                                  , a.CHK_NUM || '/' || a.CHK_TOTAL AS QTY
                                  , a.CHK_LEVEL || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_LEVEL' AND DATA_VALUE=a.CHK_LEVEL) AS CHK_LEVEL_NAME
                                  , a.CHK_WH_GRADE || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_WH_GRADE' AND DATA_VALUE=a.CHK_WH_GRADE) AS WH_GRADE_NAME
                                  , a.CHK_WH_KIND || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_WH_KIND' AND DATA_VALUE=a.CHK_WH_KIND) AS WH_KIND_NAME
                                  , a.CHK_PERIOD || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_PERIOD' AND DATA_VALUE=a.CHK_PERIOD) AS CHK_PERIOD_NAME
                                  , CHK_CLASS || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_CLASS' AND DATA_VALUE=CHK_CLASS) AS CHK_CLASS_NAME
                                  ,(CASE WHEN CHK_WH_KIND='0' 
                                              THEN CHK_TYPE || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_WH_KIND_0' AND DATA_VALUE=CHK_TYPE)
                                         when chk_wh_kind = 'E'
                                              then CHK_TYPE || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_WH_KIND_E' AND DATA_VALUE=CHK_TYPE)
                                         when chk_wh_kind = 'C'
                                              then CHK_TYPE || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_WH_KIND_C' AND DATA_VALUE=CHK_TYPE)
                                         ELSE CHK_TYPE || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_WH_KIND_1' AND DATA_VALUE=CHK_TYPE) 
                                     END) AS CHK_TYPE_NAME
                                  , a.CHK_STATUS || ' ' || (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='CHK_MAST' AND DATA_NAME='CHK_STATUS' AND DATA_VALUE=a.CHK_STATUS) AS CHK_STATUS_NAME 
                             from CHK_MAST a where a.chk_no = :chk_no";
            return DBWork.Connection.QueryFirst<CHK_MAST>(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }
        public MI_MAST GetMiMast(string mmcode)
        {
            string sql = @"select * from MI_MAST where mmcode = :mmcode";
            return DBWork.Connection.QueryFirst<MI_MAST>(sql, new { mmcode = mmcode }, DBWork.Transaction);
        }

        #region 2020-08-20 新增: 檢查科室病房藥品日盤單是否有填盤差原因
        public bool CheckChkRemark(string chk_no) {
            string sql = @"select 1 from CHK_DETAIL
                            WHERE chk_no = :chk_no 
                              and (case
                                        when STORE_QTY_UPDATE_TIME is not null
                                            then STORE_QTY_N - nvl(his_consume_qty_t, 0)
                                        else STORE_QTYC - nvl(his_consume_qty_t, 0)
                                    end) <> CHK_QTY
                              and chk_remark is null
                              "; // TRUNC(a.EXP_DATE, 'DD') TWN_TODATE(:exp_date)
           
            return !(DBWork.Connection.ExecuteScalar(sql,
                                                     new
                                                     {
                                                         chk_no = chk_no
                                                     },
                                                     DBWork.Transaction) == null);
        }
        #endregion

        #region 2020-08-31 新增: 取得待扣醫令數量
        public string GetHisConsumeQtyT(string chk_no, string mmcode) {
            string sql = @"select nvl(his_consume_qty_t , 0)
                             from CHK_DETAIL
                            where chk_no = :chk_no
                              and mmcode = :mmcode";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { chk_no = chk_no, mmcode = mmcode }, DBWork.Transaction);
        }

        public string GetHisConsumeDatatime(string chk_no, string mmcode)
        {
            string sql = @"select nvl(his_consume_datatime , '')
                             from CHK_DETAIL
                            where chk_no = :chk_no
                              and mmcode = :mmcode";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { chk_no = chk_no, mmcode = mmcode }, DBWork.Transaction);
        }
        #endregion

        #region 2020-12-03 新增: 判斷盤點單是否在本月結年月內開單
        public string CheckCurrentYm(string chk_no)
        {
            string sql = @"select 'Y' 
                             from MI_MNSET
                            where set_status = 'N'
                              and ((select twn_date(create_date) 
                                      from CHK_MAST
                                     where chk_no = :chk_no) between twn_date(set_btime) and twn_date(set_ctime))";

            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }
        #endregion

        public string GetUndoneDetailCount(string chk_no)
        {
            string sql = @"select count(*) from CHK_DETAIL
                            where chk_no = :chk_no
                              and (chk_time is null 
                                    or chk_qty is null
                                  )";
            return DBWork.Connection.QueryFirst<string>(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }
    }
}