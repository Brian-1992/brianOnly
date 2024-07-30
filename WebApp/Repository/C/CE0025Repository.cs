using System;
using System.Collections.Generic;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Text;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using TSGH.Models;

namespace WebApp.Repository.C
{
        public class CE0025ReportMODEL : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }
        public string MMNAME_E { get; set; }
        public string MMNAME_C { get; set; }
        public string BASE_UNIT { get; set; }
        public string STORE_QTY { get; set; }
        public string CHK_QTY { get; set; }
        public string GAP_T { get; set; }
        public string MEMO { get; set; }
    }

    public class CE0025Repository : JCLib.Mvc.BaseRepository
    {
        public CE0025Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        
        public IEnumerable<CE0025ReportMODEL> GetQueryData(string pFromWhere, string pChkYM, int page_index, int page_size, string sorters)
        {
            DynamicParameters sqlParam = new DynamicParameters();
            string sqlStr = GetSqlstr(pChkYM, out sqlParam);

            sqlParam.Add("OFFSET", (page_index - 1) * page_size);
            sqlParam.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CE0025ReportMODEL>((pFromWhere == "rdlc" ? sqlStr : GetPagingStatement(sqlStr, sorters)), sqlParam, DBWork.Transaction);
        }

        public String GetSqlstr(string pChkYM, out DynamicParameters pSqlParam)
        {
            pSqlParam = new DynamicParameters();
            String sqlStr = @"with chk_nos as (
                                   select chk_no from CHK_MAST a
                                    where substr(a.chk_ym, 1, 5) = :CHK_YM 
                                      and a.chk_wh_grade = '2' 
                                      and a.chk_wh_kind = '0' 
                                      and a.chk_level = '1' 
                               ),
                                   memo_data as (
                                   select a.wh_no, a.mmcode, a.memo
                                     from CHK_G2_DETAILTOT a, chk_nos b
                                    where a.chk_no = b.chk_no
                                      and a.memo is not null
                               )
                               select b.mmcode, 
                                      (select mmname_c from MI_MAST where mmcode = b.mmcode) as mmname_c, 
                                      (select mmname_e from MI_MAST where mmcode = b.mmcode) as mmname_e,
                                      (select base_unit from MI_MAST where mmcode = b.mmcode) as base_unit,
                                      sum(b.STORE_QTY) as store_qty, 
                                      sum(
                                         (CASE 
                                              WHEN B.STATUS_TOT = '1' THEN B.CHK_QTY1 
                                              WHEN B.STATUS_TOT = '2' THEN B.CHK_QTY2 
                                              WHEN B.STATUS_TOT = '3' THEN B.CHK_QTY3 
                                                ELSE 0 
                                          END)
                                      ) as chk_qty, 
                                      sum(b.GAP_T) as gap_t ,
                                      (select listagg(wh_no || ':' || memo, '<br>') 
                                               within group (order by mmcode)
                                         from memo_data
                                        where mmcode = b.mmcode) as memo
                                from chk_nos A, CHK_DETAILTOT B
                                     WHERE A.CHK_NO = B.CHK_NO
                               group by mmcode
                            order by mmcode";
          
            pSqlParam.Add("CHK_YM", pChkYM);

            return sqlStr.ToString();
        }
    }
}