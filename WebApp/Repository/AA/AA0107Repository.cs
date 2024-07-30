using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.AA
{

    public class AA0107ReportMODEL : JCLib.Mvc.BaseModel
    {
        public string F1 { get; set; }
        public string F2 { get; set; }

    }
    public class AA0107Repository : JCLib.Mvc.BaseRepository
    {
        public AA0107Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        
        public IEnumerable<AA0107> GetAllM(string chk_ym, string type, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT  distinct a.INID, c.INID_NAME
                FROM (
               SELECT   B.INID,
                         A.CHK_NO,
                         A.CHK_WH_NO wh_no,
                         A.CHK_YM,
                         C.MMCODE,
                        (CASE
                            WHEN C.STATUS_TOT = '1' THEN C.CHK_QTY1
                            WHEN C.STATUS_TOT = '2' THEN C.CHK_QTY2
                            WHEN C.STATUS_TOT = '3' THEN C.CHK_QTY3
                            ELSE 0
                         END)
                           AS CHK_QTY
                 FROM   CHK_MAST A,  MI_WHMAST B, CHK_DETAILTOT C
                WHERE  A.CHK_YM = :CHK_YM
                   AND A.CHK_WH_NO = B.WH_NO
                   AND A.CHK_NO = C.CHK_NO ) a, MI_WINVMON b, UR_INID c
              Where a.wh_no = b.wh_no
                And a.mmcode = b.mmcode
                And a.inid = c.inid
                and b.data_ym = :CHK_YM
                And b.USE_QTY <> 0  
             ";

            p.Add("CHK_YM", chk_ym);

            if (type == "0")
            {
                sql += " AND (a.CHK_QTY / b.USE_QTY) < 0 ";
            }
            else
            {
                sql += " AND (a.CHK_QTY / b.USE_QTY) > 1 ";
            }
            //sql += " order by inid ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<AA0107>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<COMBO_MODEL> GetChkymCombo()
        {
            string sql = @"select distinct CHK_YM as VALUE, CHK_YM as TEXT ,
                        CHK_YM as COMBITEM from CHK_MAST order by CHK_YM ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<AA0107ReportMODEL> GetPrintData(string chk_ym, string type)
        {
            var p = new DynamicParameters();


            var sql = @"SELECT  distinct a.INID AS F1, c.INID_NAME AS F2 
                FROM (
               SELECT   B.INID,
                         A.CHK_NO,
                         A.CHK_WH_NO wh_no,
                         A.CHK_YM,
                         C.MMCODE,
                        (CASE
                            WHEN C.STATUS_TOT = '1' THEN C.CHK_QTY1
                            WHEN C.STATUS_TOT = '2' THEN C.CHK_QTY2
                            WHEN C.STATUS_TOT = '3' THEN C.CHK_QTY3
                            ELSE 0
                         END)
                           AS CHK_QTY
                 FROM   CHK_MAST A,  MI_WHMAST B, CHK_DETAILTOT C
                WHERE  A.CHK_YM = :chk_ym
                   AND  A.CHK_WH_NO = B.WH_NO
                   AND   A.CHK_NO = C.CHK_NO ) a, MI_WINVMON b, UR_INID c
              Where a.wh_no = b.wh_no
                And a.mmcode = b.mmcode
                And a.inid = c.inid
                AND b.data_ym = :CHK_YM
            And b.USE_QTY <> 0  
             ";

            p.Add("chk_ym", chk_ym);

            if (type == "0")
            {
                sql += " AND (a.CHK_QTY / b.USE_QTY) < 0 ";
            }
            else
            {
                sql += " AND (a.CHK_QTY / b.USE_QTY) > 1 ";
            }
            sql += " order by a.inid ";

            return DBWork.Connection.Query<AA0107ReportMODEL>(sql, p, DBWork.Transaction);
        }

    }
}
