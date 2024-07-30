using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AB
{
    public class AA0071Repository : JCLib.Mvc.BaseRepository
    {
        public AA0071Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0080> GetNow(string WH_NO, string YYYYMM,  int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.DATA_YM,
                               A.MMCODE,
                               B.MMNAME_E,
                               B.E_RESTRICTCODE,
                               B.BASE_UNIT,
                               --缺上月結存
                               A.APL_INQTY,
                               A.APL_OUTQTY,
                               A.INVENTORYQTY,  --盤點差異量
                               A.ADJ_INQTY - A.ADJ_OUTQTY ADJ_QTY, --調帳數量
                               A.INV_QTY
                        FROM MI_WINVMON A, 
                             MI_MAST B, 
                             MI_WHMAST C, 
                             MI_WHINV D
                        WHERE A.MMCODE = B.MMCODE
                        AND A.WH_NO = C.WH_NO
                        AND A.MMCODE = D.MMCODE(+)
                        AND A.WH_NO = D.WH_NO(+)
                        AND B. E_RESTRICTCODE IN ('1','2','3','4')";


            if (WH_NO != "")
            {
                sql += " AND A.WH_NO  = :WH_NO ";
                p.Add(":WH_NO", string.Format("{0}", WH_NO));
            }
            if (YYYYMM != "")
            {
                sql += " AND A.DATA_YM =:YYYYMM ";
                p.Add(":YYYYMM", string.Format("{0}", YYYYMM));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0080>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_WHMAST> GetWH_NoCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.WH_NO, A.WH_NAME, A.WH_KIND, A.WH_GRADE FROM MI_WHMAST A WHERE 1=1 AND WH_KIND ='0'  ";


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

        //public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        //{
        //    var p = new DynamicParameters();

        //    var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT FROM MI_MAST A WHERE 1=1 ";


        //    if (p0 != "")
        //    {
        //        sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(A.MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(A.MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
        //        p.Add(":MMCODE_I", p0);
        //        p.Add(":MMNAME_E_I", p0);
        //        p.Add(":MMNAME_C_I", p0);

        //        sql += " AND (A.MMCODE LIKE :MMCODE ";
        //        p.Add(":MMCODE", string.Format("%{0}%", p0));

        //        sql += " OR A.MMNAME_E LIKE :MMNAME_E ";
        //        p.Add(":MMNAME_E", string.Format("%{0}%", p0));

        //        sql += " OR A.MMNAME_C LIKE :MMNAME_C) ";
        //        p.Add(":MMNAME_C", string.Format("%{0}%", p0));

        //        sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, MMCODE", sql);
        //    }
        //    else
        //    {
        //        sql = string.Format(sql, "");
        //        sql += " ORDER BY A.MMCODE ";
        //    }

        //    p.Add("OFFSET", (page_index - 1) * page_size);
        //    p.Add("PAGE_SIZE", page_size);

        //    return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        //}

        public IEnumerable<AA0080> Report(string WH_NO, string YYYYMM)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.DATA_YM,
                               A.MMCODE,
                               B.MMNAME_E,
                               B.E_RESTRICTCODE,
                               B.BASE_UNIT,
                               --缺上月結存
                               A.APL_INQTY,
                               A.APL_OUTQTY,
                               A.INVENTORYQTY,  --盤點差異量
                               A.ADJ_INQTY - A.ADJ_OUTQTY ADJ_QTY, --調帳數量
                               A.INV_QTY
                        FROM MI_WINVMON A, 
                             MI_MAST B, 
                             MI_WHMAST C, 
                             MI_WHINV D
                        WHERE A.MMCODE = B.MMCODE
                        AND A.WH_NO = C.WH_NO
                        AND A.MMCODE = D.MMCODE(+)
                        AND A.WH_NO = D.WH_NO(+)
                        AND B. E_RESTRICTCODE IN ('1','2','3','4')";


            if (WH_NO != "")
            {
                sql += " AND A.WH_NO  = :WH_NO ";
                p.Add(":WH_NO", string.Format("{0}", WH_NO));
            }
            if (YYYYMM != "")
            {
                sql += " AND A.DATA_YM =:YYYYMM ";
                p.Add(":YYYYMM", string.Format("{0}", YYYYMM));
            }

            return DBWork.Connection.Query<AA0080>(sql, p, DBWork.Transaction);
        }
    }
}