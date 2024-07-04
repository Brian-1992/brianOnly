using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AB
{
    public class AB0080Repository : JCLib.Mvc.BaseRepository
    {
        public AB0080Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //public IEnumerable<AA0080> GetNow(string WH_NO, string YYYYMM,  int page_index, int page_size, string sorters)
        //{
        //    var p = new DynamicParameters();

        //    var sql = @"SELECT A.DATA_YM,
        //                       A.MMCODE,
        //                       B.MMNAME_E,
        //                       B.E_RESTRICTCODE,
        //                       B.BASE_UNIT,
        //                       --缺上月結存
        //                       A.APL_INQTY,
        //                       A.APL_OUTQTY,
        //                       A.INVENTORYQTY,  --盤點差異量
        //                       A.ADJ_INQTY - A.ADJ_OUTQTY ADJ_QTY, --調帳數量
        //                       A.INV_QTY
        //                FROM MI_WINVMON A, 
        //                     MI_MAST B, 
        //                     MI_WHMAST C, 
        //                     MI_WHINV D
        //                WHERE A.MMCODE = B.MMCODE
        //                AND A.WH_NO = C.WH_NO
        //                AND A.MMCODE = D.MMCODE(+)
        //                AND A.WH_NO = D.WH_NO(+)
        //                AND B. E_RESTRICTCODE IN ('1','2','3','4')";


        //    if (WH_NO != "")
        //    {
        //        sql += " AND A.WH_NO  = :WH_NO ";
        //        p.Add(":WH_NO", string.Format("{0}", WH_NO));
        //    }
        //    if (YYYYMM != "")
        //    {
        //        sql += " AND A.DATA_YM =:YYYYMM ";
        //        p.Add(":YYYYMM", string.Format("{0}", YYYYMM));
        //    }

        //    p.Add("OFFSET", (page_index - 1) * page_size);
        //    p.Add("PAGE_SIZE", page_size);

        //    return DBWork.Connection.Query<AA0080>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        //}

        public IEnumerable<MI_WHMAST> GetWH_NoCombo(string p0,string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.WH_NO, A.WH_NAME, A.WH_KIND, A.WH_GRADE FROM MI_WHMAST A WHERE 1=1 AND WH_KIND ='0' AND  A.WH_GRADE=:p1";

            p.Add("p1",p1);

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

        public IEnumerable<COMBO_MODEL> GetWhGCombo()
        {
            string sql = @"select DATA_VALUE VALUE,DATA_VALUE||' '||DATA_DESC TEXT
                            from PARAM_D
                            where GRP_CODE='MI_WHMAST' and DATA_NAME='WH_GRADE420'
                            order by DATA_VALUE
                            ";            
            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }


        public DataTable Report(string parYYYYMM, string parYYYYMM_D, string parWHG, string parWHGAll, string parWH_NO, string parMMCODE_B, string parMMCODE_E)
        {
            var p = new DynamicParameters();

            p.Add(":YYYYMM", string.Format("{0}", parYYYYMM));
            p.Add(":YYYYMM_D", string.Format("{0}", parYYYYMM_D));


            var sql = @"SELECT A.WH_NO,A.MMCODE,MMCODE_NAME(A.MMCODE) MMNAME_E,
                               PMN_INVQTY(A.DATA_YM,A.WH_NO,A.MMCODE) LS_INV_QTY, --上月結存
                               B.PMN_AVGPRICE LS_AVG_PRICE, --上月平均單價
                               A.APL_INQTY as IN_QTY, --本期進貨
                               (CASE 
                                 WHEN C.WH_GRADE='1' THEN
                                  APL_OUTQTY+TRN_OUTQTY-TRN_INQTY+BAK_OUTQTY-BAK_INQTY+REJ_OUTQTY+DIS_OUTQTY+EXG_OUTQTY-EXG_INQTY+MIL_OUTQTY-MIL_INQTY+USE_QTY
                                 WHEN C.WH_GRADE='2' THEN
                                  TRN_OUTQTY-TRN_INQTY+BAK_OUTQTY-BAK_INQTY+REJ_OUTQTY+DIS_OUTQTY+EXG_OUTQTY-EXG_INQTY+MIL_OUTQTY-MIL_INQTY+USE_QTY
                                 ELSE
                                  APL_OUTQTY+TRN_OUTQTY-TRN_INQTY+BAK_OUTQTY-BAK_INQTY+REJ_OUTQTY+DIS_OUTQTY+EXG_OUTQTY-EXG_INQTY+MIL_OUTQTY-MIL_INQTY+USE_QTY
                                END
                               ) OUT_QTY, --消耗(撥出)
                               A.INV_QTY,  --本期結存
                               B.AVG_PRICE, --移動平均加權價
                               A.INVENTORYQTY, --盤點 (盤點差異量)
                               (ADJ_INQTY-ADJ_OUTQTY) ADJ_QTY,  --調帳
                               (CASE WHEN C.WH_GRADE='2' THEN APL_OUTQTY ELSE 0 END) OUT_QTY2, --撥發三級庫
                               B.DISC_UPRICE, --進價
                               B.CONT_PRICE --合約價
                          FROM MI_WINVMON A,MI_WHCOST B,MI_WHMAST C
                         WHERE A.DATA_YM=:YYYYMM AND A.DATA_YM=B.DATA_YM
                           AND A.MMCODE=B.MMCODE AND A.WH_NO=C.WH_NO AND C.WH_KIND='0'
                        ";            


            if (parMMCODE_B != "")
            {
                sql += " and a.MMCODE >= :MMCODE_B ";
                p.Add(":MMCODE_B", string.Format("{0}", parMMCODE_B));
            }
            if (parMMCODE_E != "")
            {
                sql += " and a.MMCODE <= :MMCODE_E ";
                p.Add(":MMCODE_E", string.Format("{0}", parMMCODE_E));
            }
            if (bool.Parse(parWHGAll))
            {
                sql += " and c.WH_GRADE =:WH_GRADE ";
                p.Add(":WH_GRADE", parWHG);
            }
            else
            {
                sql += " and a.WH_NO = :WH_NO ";
                p.Add(":WH_NO", parWH_NO);
            }

            //sql += "  order by WH_NO,MMCODE ";


            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }


        public DataTable GetReportSum(string parYYYYMM, string parYYYYMM_D, string parWHG, string parWHGAll, string parWH_NO, string parMMCODE_B, string parMMCODE_E)
        {
            var p = new DynamicParameters();

            p.Add(":YYYYMM", string.Format("{0}", parYYYYMM));
            p.Add(":YYYYMM_D", string.Format("{0}", parYYYYMM_D));


            var sql = @"select 
                        sum((select INV_QTY from MI_WINVMON where DATA_YM=:YYYYMM_D and WH_NO=a.WH_NO and MMCODE=a.MMCODE)*(select DISC_UPRICE from MI_MAST where MMCODE=a.MMCODE)) as LS_INV_QTYxDISC_UPRICE, --上月結存                      
                        sum((APL_INQTY + TRN_INQTY + ADJ_INQTY + BAK_INQTY + EXG_INQTY + MIL_INQTY)*(select M_CONTPRICE from MI_MAST where MMCODE=a.MMCODE)) as IN_QTYxM_CONTPRICE, --本期進貨 
                        sum((APL_OUTQTY + TRN_OUTQTY + ADJ_OUTQTY + BAK_OUTQTY + EXG_OUTQTY + MIL_OUTQTY + REJ_OUTQTY + DIS_OUTQTY + USE_QTY)*(select DISC_UPRICE from MI_MAST where MMCODE=a.MMCODE)) as OUT_QTYxDISC_UPRICE, --消耗(撥出)
                        sum(a.INV_QTY*(select DISC_UPRICE from MI_MAST where MMCODE=a.MMCODE))  INV_QTYxDISC_UPRICE --本期結存
                        from MI_WINVMON a, MI_MAST b
                        where 
                        DATA_YM=:YYYYMM 
                        and a.MMCODE = b.MMCODE 
                        AND B.MAT_CLASS = '01' ";


            if (parMMCODE_B != "")
            {
                sql += " and a.MMCODE >= :MMCODE_B ";
                p.Add(":MMCODE_B", string.Format("{0}", parMMCODE_B));
            }
            if (parMMCODE_E != "")
            {
                sql += " and a.MMCODE <= :MMCODE_E ";
                p.Add(":MMCODE_E", string.Format("{0}", parMMCODE_E));
            }
            if (bool.Parse(parWHGAll))
            {
                sql += " and WH_NO in (SELECT  WH_NO FROM MI_WHMAST WHERE WH_GRADE =:WH_GRADE) ";
                p.Add(":WH_GRADE", parWHG);
            }
            else
            {
                sql += " and WH_NO = :WH_NO ";
                p.Add(":WH_NO", parWH_NO);
            }


            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public DataTable GetSetTime(string parYYYMM)
        {
            var p = new DynamicParameters();


            var sql = @"select SET_BTIME,SET_ETIME from   MI_MNSET where SET_YM=:SET_YM";

                p.Add(":SET_YM", string.Format("{0}", parYYYMM));
   


            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

    }
}