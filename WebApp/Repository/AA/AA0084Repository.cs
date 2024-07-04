using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.AA
{
    public class AA0084Repository : JCLib.Mvc.BaseRepository
    {
        public AA0084Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0084M> GetAll(string startDate, string endDate, string wh_no, int page_index, int page_size, string sorters) {

            var p = new DynamicParameters();

            string sql = @" SELECT 
                                A.FRWH AS WH_NO,                 --庫房別
                                B.MMCODE,                        --院內碼
                                C.MMNAME_E,                      --藥品名稱
                                B.APPQTY,                        --數量
                                C.BASE_UNIT,                     --計量單位
                                B.AMT,                           --金額
                                '減帳' AD_FLAG,                  --加減帳
                                A.UPDATE_TIME,                   --處理日期/時間
                                A.UPDATE_USER,                   --處理人
                                '軍品調帳'as adj_type,                    --調帳類別
                                A.APPLY_NOTE,                    --備註
                                CASE WHEN A.UPDATE_TIME IS NOT NULL THEN '0' ELSE '1' END AS TIMEORDER
                            FROM 
                                ME_DOCM A, ME_DOCD B, MI_MAST C
                            WHERE 
                                A.DOCNO= B.DOCNO
                                AND B.MMCODE = C.MMCODE
                                AND A.FLOWID = '0399'
                                AND TRUNC(A.APPTIME, 'DD') BETWEEN TO_DATE(:p0,'YYYY-MM-DD') AND TO_DATE(:p1,'YYYY-MM-DD')
                                AND A.FRWH = :p2
                            UNION ALL
                            SELECT 
                                A.TOWH AS WH_NO, 
                                B.MMCODE, 
                                C.MMNAME_E, 
                                B.APPQTY, 
                                C.BASE_UNIT, 
                                B.AMT, 
                                '加帳' AD_FLAG,
                                A.UPDATE_TIME, 
                                A.UPDATE_USER, 
                                '軍品調帳' as adj_type, 
                                A.APPLY_NOTE,
                                CASE WHEN A.UPDATE_TIME IS NOT NULL THEN '0' ELSE '1' END AS TIMEORDER
                            FROM 
                                ME_DOCM A, ME_DOCD B, MI_MAST C
                            WHERE 
                                A.DOCNO= B.DOCNO
                                AND B.MMCODE = C.MMCODE
                                AND A.FLOWID = '0399'
                                AND TRUNC(A.APPTIME, 'DD') BETWEEN TO_DATE(:p0,'YYYY-MM-DD') AND TO_DATE(:p1,'YYYY-MM-DD')
                                AND A.TOWH = :p2
                            union all
                                SELECT 
                                A.FRWH AS WH_NO,                 --庫房別
                                B.MMCODE,                        --院內碼
                                mmcode_name(B.mmcode) as mmname_e,                      --藥品名稱
                                B.APPQTY,                        --數量
                                base_unit(b.mmcode) as base_unit,                     --計量單位
                                B.AMT,                           --金額
                                (case 
                                    when b.transkind in ('310', '350', '360', '370') then '減帳'
                                    when b.transkind in ('311', '351', '361', '381', '391') then '減帳'
                                    else ' '
                                end) AD_FLAG,                  --加減帳
                                A.UPDATE_TIME,                   --處理日期/時間
                                A.UPDATE_USER,                   --處理人
                                (select data_desc from PARAM_D 
                                where grp_code = 'ME_DOCD'
                                  and data_name = 'TRANSKIND' 
                                  and data_value = b.transkind) as adj_type,                      --調帳類別
                                A.APPLY_NOTE,                    --備註
                                CASE WHEN A.UPDATE_TIME IS NOT NULL THEN '0' ELSE '1' END AS TIMEORDER
                            FROM 
                                ME_DOCM A, ME_DOCD B 
                            WHERE 
                                A.DOCNO= B.DOCNO
                                AND A.FLOWID = '1799'
                                AND TRUNC(A.APPTIME, 'DD') BETWEEN TO_DATE(:p0,'YYYY-MM-DD') AND TO_DATE(:p1,'YYYY-MM-DD')
                                AND A.FRWH = :p2
                            ORDER BY 
                                TIMEORDER, UPDATE_TIME, WH_NO, MMCODE, AD_FLAG, UPDATE_USER
                            ";

            p.Add("p0", startDate);
            p.Add("p1", endDate);
            p.Add("p2", wh_no);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0084M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA0084M> Print(string startDate, string endDate, string wh_no) {

            var p = new DynamicParameters();

            string sql = @" SELECT 
                                A.FRWH AS WH_NO,                 --庫房別
                                B.MMCODE,                        --院內碼
                                C.MMNAME_E,                      --藥品名稱
                                B.APPQTY,                        --數量
                                C.BASE_UNIT,                     --計量單位
                                B.AMT,                           --金額
                                '減帳' AD_FLAG,                  --加減帳
                                A.UPDATE_TIME,                   --處理日期/時間
                                (select una from UR_ID where tuser = A.UPDATE_USER) as update_user,                   --處理人
                                '軍品調帳'as adj_type,                    --調帳類別
                                A.APPLY_NOTE,                    --備註
                                CASE WHEN A.UPDATE_TIME IS NOT NULL THEN '0' ELSE '1' END AS TIMEORDER
                            FROM 
                                ME_DOCM A, ME_DOCD B, MI_MAST C
                            WHERE 
                                A.DOCNO= B.DOCNO
                                AND B.MMCODE = C.MMCODE
                                AND A.FLOWID = '0399'
                                AND TRUNC(A.APPTIME, 'DD') BETWEEN TO_DATE(:p0,'YYYY-MM-DD') AND TO_DATE(:p1,'YYYY-MM-DD')
                                AND A.FRWH = :p2
                            UNION ALL
                            SELECT 
                                A.TOWH AS WH_NO, 
                                B.MMCODE, 
                                C.MMNAME_E, 
                                B.APPQTY, 
                                C.BASE_UNIT, 
                                B.AMT, 
                                '加帳' AD_FLAG,
                                A.UPDATE_TIME, 
                                (select una from UR_ID where tuser = A.UPDATE_USER) as update_user, 
                                '軍品調帳' as adj_type, 
                                A.APPLY_NOTE,
                                CASE WHEN A.UPDATE_TIME IS NOT NULL THEN '0' ELSE '1' END AS TIMEORDER
                            FROM 
                                ME_DOCM A, ME_DOCD B, MI_MAST C
                            WHERE 
                                A.DOCNO= B.DOCNO
                                AND B.MMCODE = C.MMCODE
                                AND A.FLOWID = '0399'
                                AND TRUNC(A.APPTIME, 'DD') BETWEEN TO_DATE(:p0,'YYYY-MM-DD') AND TO_DATE(:p1,'YYYY-MM-DD')
                                AND A.TOWH = :p2
                            union all
                                SELECT 
                                A.FRWH AS WH_NO,                 --庫房別
                                B.MMCODE,                        --院內碼
                                mmcode_name(B.mmcode) as mmname_e,                      --藥品名稱
                                B.APVQTY,                        --數量
                                base_unit(b.mmcode) as base_unit,                     --計量單位
                                B.AMT,                           --金額
                                (case 
                                    when b.transkind in ('310', '350', '360', '370') then '減帳'
                                    when b.transkind in ('311', '351', '361', '381', '391') then '加帳'
                                    else ' '
                                end) AD_FLAG,                  --加減帳
                                A.UPDATE_TIME,                   --處理日期/時間
                                (select una from UR_ID where tuser = A.UPDATE_USER) as update_user,                   --處理人
                                (select data_desc from PARAM_D 
                                where grp_code = 'ME_DOCD'
                                  and data_name = 'TRANSKIND' 
                                  and data_value = b.transkind) as adj_type,                      --調帳類別
                                b.aplyitem_note as APPLY_NOTE,                    --備註
                                CASE WHEN A.UPDATE_TIME IS NOT NULL THEN '0' ELSE '1' END AS TIMEORDER
                            FROM 
                                ME_DOCM A, ME_DOCD B 
                            WHERE 
                                A.DOCNO= B.DOCNO
                                AND A.FLOWID = '1799'
                                AND TRUNC(A.APPTIME, 'DD') BETWEEN TO_DATE(:p0,'YYYY-MM-DD') AND TO_DATE(:p1,'YYYY-MM-DD')
                                AND A.FRWH = :p2
                            ORDER BY 
                                TIMEORDER, UPDATE_TIME, WH_NO, MMCODE, AD_FLAG, UPDATE_USER
                            ";

            p.Add("p0", startDate);
            p.Add("p1", endDate);
            p.Add("p2", wh_no);

            return DBWork.Connection.Query<AA0084M>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetWh_noCombo()
        {
            var p = new DynamicParameters();

                string sql = @" SELECT WH_NO || ' ' || WH_NAME TEXT, WH_NO VALUE
                                FROM MI_WHMAST
                                WHERE WH_KIND = '0' 
                                ORDER BY VALUE ";

            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }

        public string GetWhName(string wh_no) {
            string sql = @"select wh_name(:wh_no) from dual";

            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }
    }
}