using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.F
{
    public class FA0046Repository : JCLib.Mvc.BaseRepository
    {
        public FA0046Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<FA0046> GetAll(string WH_NO, string YYYMM, string P2, string RG, int page_index, int page_size, string sorters)
            {
           var p = new DynamicParameters();
            var sql = string.Format(@" SELECT  a.MMCODE,  
                                                a.MMNAME_C, 
                                                a.MMNAME_E,
                                                a.BASE_UNIT, 
                                                c.PMN_AVGPRICE, 
                                                c.PMN_INVQTY,
                                                (c.PMN_AVGPRICE * c.PMN_INVQTY) as pmn_amount,
                                                c.CONT_PRICE,
                                                c.MN_INQTY,
                                                (c.CONT_PRICE * c.MN_INQTY) as in_amount,
                                                c.AVG_PRICE,
                                                b.use_qty, 
                                                (b.use_qty * c.AVG_PRICE) as use_amount,   
                                                a.STORE_QTY,    
                                                (a.STORE_QTY * c.AVG_PRICE) as store_amount,
                                                (a.chk_qty - a.STORE_QTY) as diff_qty,  
                                                ((a.chk_qty - a.STORE_QTY) * c.AVG_PRICE) as diff_amount,
                                                (select turnover from MI_WINVMON
                                                        Where wh_no = a.wh_no
                                                          And  data_ym = to_char(add_months(to_date(SUBSTR(a.CHK_YM,1,5),'yyymm'),-1),'yyymm')
                                         --  /* 取上一個月的資料
                                                   And  mmcode = a.mmcode) as turnover
                                            FROM (
                                                SELECT   
                                                      CHK_TYPE,
                                                      A.CHK_NO,
                                                      A.CHK_WH_NO wh_no,
                                                      B.WH_NAME,
                                                      A.CHK_YM,
                                                      C.MMCODE,
                                                      C.MMNAME_C,
                                                      C.MMNAME_E,
                                                      C.BASE_UNIT,                       
                                                      C.STORE_QTY,
                                                      C.M_CONTPRICE,
                                                      C. M_STOREID,
                                                      C. MAT_CLASS,
                                                     (CASE
                                                        WHEN C.STATUS_TOT = '1' THEN C.CHK_QTY1
                                                        WHEN C.STATUS_TOT = '2' THEN C.CHK_QTY2
                                                        WHEN C.STATUS_TOT = '3' THEN C.CHK_QTY3
                                                        ELSE 0
                                                      END)    AS CHK_QTY
                                                 FROM   CHK_MAST A,  MI_WHMAST B, CHK_DETAILTOT C
                                                WHERE  A.CHK_LEVEL ='1'  and CHK_STATUS ='3'
                                             AND  SUBSTR(A.CHK_YM,1,5) = :CHK_YM                 -- 查詢輸入的年月值
                                             {0}
                                               AND  A.CHK_WH_NO  = :CHK_WH_NO              -- 查詢輸入的庫房值
                                               AND  A.CHK_WH_NO = B.WH_NO
                                                 AND   A.CHK_NO = C.CHK_NO 
                                                                         ) a, MI_WHINV b, MI_WHCOST c
                                          Where a.wh_no = b.wh_no
                                            And a.mmcode = b.mmcode  
                                            And SUBSTR(a.CHK_YM,1,5) = c.DATA_YM
                                        And a.mmcode = c.mmcode

                And  CHK_TYPE =:CHK_TYPE   
                               
                               order by a.mmcode  

                         ", P2 != "" ? "AND  A.CHK_CLASS = :CHK_CLASS" : "");

            if (P2 != "")
            {
                p.Add(":CHK_CLASS", string.Format("{0}", P2));
            }


            p.Add(":CHK_YM", string.Format("{0}", YYYMM));
            p.Add(":CHK_WH_NO", string.Format("{0}", WH_NO));
            p.Add(":CHK_TYPE", string.Format("{0}", RG));
            

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<FA0046>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }



        public IEnumerable<ComboItemModel> GetMATCombo()
        {
            string sql = @"Select MAT_CLASS||' '||MAT_CLSNAME TEXT, MAT_CLASS VALUE from MI_MATCLASS
                            ORDER BY VALUE";


            return DBWork.Connection.Query<ComboItemModel>(sql, new { userID = DBWork.ProcUser }, DBWork.Transaction);
        }
        public DataTable GetExcel(string WH_NO, string YYYMM, string P2, string RG)
        {
            var p = new DynamicParameters();

            var sql = string.Format(@" SELECT  a.MMCODE 院內碼,  
                                                a.MMNAME_C 中文品名, 
                                                a.MMNAME_E 英文品名,
                                                a.BASE_UNIT 單位, 
                                                c.PMN_AVGPRICE 上期結存單價, 
                                                c.PMN_INVQTY 上月結存,
                                                (c.PMN_AVGPRICE * c.PMN_INVQTY) as 上月金額 ,
                                                c.CONT_PRICE 合約單價,
                                                c.MN_INQTY 本月進貨,
                                                (c.CONT_PRICE * c.MN_INQTY) as 進貨金額 ,
                                                c.AVG_PRICE 庫存成本單價,
                                                b.use_qty 本月消耗, 
                                                (b.use_qty * c.AVG_PRICE) as 消耗金額 ,  
                                                c.AVG_PRICE 期未單價,
                                                a.STORE_QTY 本月結存,    
                                                (a.STORE_QTY * c.AVG_PRICE) as 結存金額 ,
                                                (a.chk_qty - a.STORE_QTY) as 本月盤盈虧 ,  
                                                ((a.chk_qty - a.STORE_QTY) * c.AVG_PRICE) as 盤盈虧金額 ,
                                                (select turnover from MI_WINVMON
                                                        Where wh_no = a.wh_no
                                                          And  data_ym = to_char(add_months(to_date(a.CHK_YM,'yyymm'),-1),'yyymm')
                                         --  /* 取上一個月的資料
                                                   And  mmcode = a.mmcode) as 周轉率
                                            FROM (
                                                SELECT   
                                                      CHK_TYPE,
                                                      A.CHK_NO,
                                                      A.CHK_WH_NO wh_no,
                                                      B.WH_NAME,
                                                      A.CHK_YM,
                                                      C.MMCODE,
                                                      C.MMNAME_C,
                                                      C.MMNAME_E,
                                                      C.BASE_UNIT,                       
                                                      C.STORE_QTY,
                                                      C.M_CONTPRICE,
                                                      C. M_STOREID,
                                                      C. MAT_CLASS,
                                                     (CASE
                                                        WHEN C.STATUS_TOT = '1' THEN C.CHK_QTY1
                                                        WHEN C.STATUS_TOT = '2' THEN C.CHK_QTY2
                                                        WHEN C.STATUS_TOT = '3' THEN C.CHK_QTY3
                                                        ELSE 0
                                                      END)    AS CHK_QTY
                                                 FROM   CHK_MAST A,  MI_WHMAST B, CHK_DETAILTOT C
                                                WHERE  A.CHK_LEVEL ='1'  and CHK_STATUS ='3'
                                             AND  A.CHK_YM = :CHK_YM                 -- 查詢輸入的年月值
                                             {0}
                                               AND  A.CHK_WH_NO  = :CHK_WH_NO              -- 查詢輸入的庫房值
                                               AND  A.CHK_WH_NO = B.WH_NO
                                                 AND   A.CHK_NO = C.CHK_NO 
                                                                         ) a, MI_WHINV b, MI_WHCOST c
                                          Where a.wh_no = b.wh_no
                                            And a.mmcode = b.mmcode  
                                            And a.CHK_YM = c.DATA_YM
                                        And a.mmcode = c.mmcode

                And  CHK_TYPE =:CHK_TYPE   
                               
                               order by a.mmcode  

                         ", P2 != "" ? "AND  A.CHK_CLASS = :CHK_CLASS" : "");

            if (P2 != "")
            {
                p.Add(":CHK_CLASS", string.Format("{0}", P2));
            }


            p.Add(":CHK_YM", string.Format("{0}", YYYMM));
            p.Add(":CHK_WH_NO", string.Format("{0}", WH_NO));
            p.Add(":CHK_TYPE", string.Format("{0}", RG));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }


        public DataTable Report(string WH_NO, string YYYMM, string P2, string RG)
        {
            var p = new DynamicParameters();

            var sql = string.Format(@" SELECT  a.MMCODE,  
                                                a.MMNAME_C, 
                                                a.MMNAME_E,
                                                a.BASE_UNIT, 
                                                c.PMN_AVGPRICE, 
                                                c.PMN_INVQTY,
                                                (c.PMN_AVGPRICE * c.PMN_INVQTY) as pmn_amount,
                                                c.CONT_PRICE,
                                                c.MN_INQTY,
                                                (c.CONT_PRICE * c.MN_INQTY) as in_amount,
                                                c.AVG_PRICE,
                                                b.use_qty, 
                                                (b.use_qty * c.AVG_PRICE) as use_amount,   
                                                a.STORE_QTY,    
                                                (a.STORE_QTY * c.AVG_PRICE) as store_amount,
                                                (a.chk_qty - a.STORE_QTY) as diff_qty,  
                                                ((a.chk_qty - a.STORE_QTY) * c.AVG_PRICE) as diff_amount,
                                                (select turnover from MI_WINVMON
                                                        Where wh_no = a.wh_no
                                                          And  data_ym = to_char(add_months(to_date(a.CHK_YM,'yyymm'),-1),'yyymm')
                                         --  /* 取上一個月的資料
                                                   And  mmcode = a.mmcode) as turnover
                                            FROM (
                                                SELECT   
                                                      CHK_TYPE,
                                                      A.CHK_NO,
                                                      A.CHK_WH_NO wh_no,
                                                      B.WH_NAME,
                                                      A.CHK_YM,
                                                      C.MMCODE,
                                                      C.MMNAME_C,
                                                      C.MMNAME_E,
                                                      C.BASE_UNIT,                       
                                                      C.STORE_QTY,
                                                      C.M_CONTPRICE,
                                                      C. M_STOREID,
                                                      C. MAT_CLASS,
                                                     (CASE
                                                        WHEN C.STATUS_TOT = '1' THEN C.CHK_QTY1
                                                        WHEN C.STATUS_TOT = '2' THEN C.CHK_QTY2
                                                        WHEN C.STATUS_TOT = '3' THEN C.CHK_QTY3
                                                        ELSE 0
                                                      END)    AS CHK_QTY
                                                 FROM   CHK_MAST A,  MI_WHMAST B, CHK_DETAILTOT C
                                                WHERE  A.CHK_LEVEL ='1'  and CHK_STATUS ='3'
                                             AND  A.CHK_YM = :CHK_YM                 -- 查詢輸入的年月值
                                             {0}
                                               AND  A.CHK_WH_NO  = :CHK_WH_NO              -- 查詢輸入的庫房值
                                               AND  A.CHK_WH_NO = B.WH_NO
                                                 AND   A.CHK_NO = C.CHK_NO 
                                                                         ) a, MI_WHINV b, MI_WHCOST c
                                          Where a.wh_no = b.wh_no
                                            And a.mmcode = b.mmcode  
                                            And a.CHK_YM = c.DATA_YM
                                        And a.mmcode = c.mmcode

                And  CHK_TYPE =:CHK_TYPE   
                               
                               order by a.mmcode  

                         ", P2 != "" ? "AND  A.CHK_CLASS = :CHK_CLASS" : "");

            if (P2 != "")
            {
                p.Add(":CHK_CLASS", string.Format("{0}", P2));
            }


            p.Add(":CHK_YM", string.Format("{0}", YYYMM));
            p.Add(":CHK_WH_NO", string.Format("{0}", WH_NO));
            p.Add(":CHK_TYPE", string.Format("{0}", RG));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }



        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT FROM MI_MAST A WHERE 1=1 ";


            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(A.MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(A.MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

                sql += " OR A.MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR A.MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, MMCODE", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

    }
}