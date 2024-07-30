using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using BarcodeLib;
using System.Drawing;
using System.Drawing.Imaging;

namespace WebApp.Repository.AA
{
    public class AA0095Report_MODEL : JCLib.Mvc.BaseModel
    {
        public string WH_NO { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_E { get; set; }
        public string EXP_DATE1 { get; set; }
        public string LOT_NO1 { get; set; }
        public string EXP_DATE2 { get; set; }
        public string LOT_NO2 { get; set; }
        public string EXP_DATE3 { get; set; }
        public string LOT_NO3 { get; set; }
        public string EXP_DATE { get; set; }
        public string EXP_QTY { get; set; }
        public string MEMO { get; set; }

        public string WH_NO_C { get; set; }

    }

    public class AA0095Repository : JCLib.Mvc.BaseRepository
    {
        public AA0095Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        /// <summary>
        /// 讀取庫別代碼ComboList
        /// </summary>
        /// <returns></returns>
        public IEnumerable<COMBO_MODEL> GetWHNO()
        {
            var p = new DynamicParameters();

            string sql = "";
            sql = @"SELECT WH_NO AS VALUE, WH_NAME AS TEXT,
                        RTrim(WH_NO || ' ' || WH_NAME) AS COMBITEM
                        FROM MI_WHMAST
                        WHERE WH_KIND = '0'";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }

        public IEnumerable<AA0095Report_MODEL> SearchReportData(string str_WHNO, string str_YearMonth, string str_MMCODE_FROM, string str_MMCODE_TO, int page_index = 0, int page_size = 0, string sorters = "")
        {
            var p = new DynamicParameters();

            string sql = @"SELECT WH_NO, A.MMCODE, B.MMNAME_E, TWN_DATE(EXP_DATE1) EXP_DATE1, LOT_NO1, 
                        TWN_DATE(EXP_DATE2) EXP_DATE2, LOT_NO2, TWN_DATE(EXP_DATE3) EXP_DATE3, LOT_NO3, 
                        TWN_DATE(EXP_DATE) EXP_DATE, EXP_QTY, MEMO,
                        (SELECT WH_NAME FROM MI_WHMAST WHERE A.WH_NO = WH_NO ) AS WH_NO_C
                        FROM ME_EXPD A, MI_MAST B 
                        WHERE A.MMCODE = B.MMCODE
                        AND WH_NO = :WH_NO
                        AND SUBSTR(TWN_DATE(EXP_DATE),0,5) = :YearMonth ";

            p.Add(":WH_NO", str_WHNO);
            p.Add(":YearMonth", str_YearMonth);

            if (!string.IsNullOrEmpty(str_MMCODE_FROM) && !string.IsNullOrEmpty(str_MMCODE_TO))
            {
                sql += "AND A.MMCODE BETWEEN :MMCODE_From AND :MMCODE_To ";
                p.Add(":MMCODE_From", str_MMCODE_FROM);
                p.Add(":MMCODE_To", str_MMCODE_TO);
            }

            sql += "ORDER BY MMCODE ASC";

            if (page_index == 0 && page_size == 0 && string.IsNullOrEmpty(sorters))
            {
                return DBWork.Connection.Query<AA0095Report_MODEL>(sql, p);
            }
            else
            {
                p.Add("OFFSET", (page_index - 1) * page_size);
                p.Add("PAGE_SIZE", page_size);

                return DBWork.Connection.Query<AA0095Report_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
            }
        }
    }
}