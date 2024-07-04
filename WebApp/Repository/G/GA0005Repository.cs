using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.G
{
    public class GA0005Repository : JCLib.Mvc.BaseRepository
    {
        public GA0005Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        // GET api/<controller>
        public IEnumerable<TC_PURCH> GetAll(string pur_no, string pur_date_b, string pur_date_e, string mmname_c, string agen_namec, string purch_st, string tc_type)
        {
            var p = new DynamicParameters();

            var sql = @" select * from
                        (select a.PUR_NO,twn_date(a.PUR_DATE) as PUR_DATE, a.PUR_DATE as PUR_DATE_O, a.TC_TYPE,a.PUR_UNM,a.PUR_NOTE, 
                        (select a.PURCH_ST || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'TC_PURCH_M' and DATA_NAME = 'PURCH_ST' and DATA_VALUE = a.PURCH_ST) as PURCH_ST, a.PURCH_ST as PURCH_ST_O,
                        b.MMCODE,b.MMNAME_C,b.AGEN_NAMEC,
                        b.PUR_QTY,b.PUR_UNIT,b.IN_PURPRICE,b.PUR_AMOUNT
                        from TC_PURCH_M a
                        left join TC_PURCH_DL b
                        on a.PUR_NO=b.PUR_NO )A
                        where 1=1 ";

            if (pur_no != "" && pur_no != null)
            {
                sql += " and PUR_NO like :PUR_NO ";
                p.Add(":PUR_NO", string.Format("%{0}%", pur_no));
            }
            if (pur_date_b != "" && pur_date_b != null)
            {
                sql += " AND PUR_DATE_O >= TO_DATE(:PUR_DATE_B,'YYYY/mm/dd') ";
                p.Add(":PUR_DATE_B", string.Format("{0}", DateTime.Parse(pur_date_b).ToString("yyyy/MM/dd")));
            }
            if (pur_date_e != "" && pur_date_e != null)
            {
                sql += " AND PUR_DATE_O <= TO_DATE(:PUR_DATE_E,'YYYY/mm/dd') ";
                p.Add(":PUR_DATE_E", string.Format("{0}", DateTime.Parse(pur_date_e).ToString("yyyy/MM/dd")));
            }
            if (mmname_c != "" && mmname_c != null)
            {
                sql += " and MMNAME_C like :MMNAME_C ";
                p.Add(":MMNAME_C", string.Format("%{0}%", mmname_c));
            }
            if (agen_namec != "" && agen_namec != null)
            {
                sql += " and AGEN_NAMEC like :AGEN_NAMEC ";
                p.Add(":AGEN_NAMEC", string.Format("%{0}%", agen_namec));
            }
            if (purch_st != "" && purch_st != null)
            {
                sql += " and PURCH_ST_O = :PURCH_ST ";
                p.Add(":PURCH_ST", purch_st);
            }
            if (tc_type == "1")
            {
                sql += " and TC_TYPE = 'A' ";
            }
            else if (tc_type == "2")
            {
                sql += " and TC_TYPE = 'B' ";
            }

            return DBWork.PagingQuery<TC_PURCH>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<TC_PURCH> GetMasterAll(string pur_no, string pur_date_b, string pur_date_e, string purch_st, string tc_type)
        {
            var p = new DynamicParameters();

            var sql = @" select PUR_NO,twn_date(PUR_DATE) as PUR_DATE,TC_TYPE,PUR_UNM,PUR_NOTE,
                        (select TC_PURCH_M.PURCH_ST || ' ' || DATA_DESC from PARAM_D where GRP_CODE = 'TC_PURCH_M' and DATA_NAME = 'PURCH_ST' and DATA_VALUE = TC_PURCH_M.PURCH_ST) as PURCH_ST 
                        from TC_PURCH_M
                        where 1=1 ";

            if (pur_no != "" && pur_no != null)
            {
                sql += " and PUR_NO like :PUR_NO ";
                p.Add(":PUR_NO", string.Format("%{0}%", pur_no));
            }
            if (pur_date_b != "" && pur_date_b != null)
            {
                sql += " AND PUR_DATE >= TO_DATE(:PUR_DATE_B,'YYYY/mm/dd') ";
                p.Add(":PUR_DATE_B", string.Format("{0}", DateTime.Parse(pur_date_b).ToString("yyyy/MM/dd")));
            }
            if (pur_date_e != "" && pur_date_e != null)
            {
                sql += " AND PUR_DATE <= TO_DATE(:PUR_DATE_E,'YYYY/mm/dd') ";
                p.Add(":PUR_DATE_E", string.Format("{0}", DateTime.Parse(pur_date_e).ToString("yyyy/MM/dd")));
            }
            if (purch_st != "" && purch_st != null)
            {
                sql += " and PURCH_ST = :PURCH_ST ";
                p.Add(":PURCH_ST", purch_st);
            }
            if (tc_type == "1")
            {
                sql += " and TC_TYPE = 'A' ";
            }
            else if (tc_type == "2")
            {
                sql += " and TC_TYPE = 'B' ";
            }

            return DBWork.PagingQuery<TC_PURCH>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<TC_PURCH> GetDetailAll(string pur_no, string mmname_c, string agen_namec)
        {
            var p = new DynamicParameters();

            var sql = @" select MMCODE,MMNAME_C,AGEN_NAMEC,
                        PUR_QTY,PUR_UNIT,IN_PURPRICE,PUR_AMOUNT
                        from TC_PURCH_DL
                        where PUR_NO = :PUR_NO ";
            p.Add(":PUR_NO", pur_no);

            if (mmname_c != "" && mmname_c != null)
            {
                sql += " and MMNAME_C like :MMNAME_C ";
                p.Add(":MMNAME_C", string.Format("%{0}%", mmname_c));
            }
            if (agen_namec != "" && agen_namec != null)
            {
                sql += " and AGEN_NAMEC like :AGEN_NAMEC ";
                p.Add(":AGEN_NAMEC", string.Format("%{0}%", agen_namec));
            }
            
            return DBWork.PagingQuery<TC_PURCH>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<string> GetAGEN_NAME(string pur_no)
        {
            string sql = @"select distinct AGEN_NAMEC from TC_PURCH_DL
                            where PUR_NO = :PUR_NO ";
            return DBWork.Connection.Query<string>(sql, new { PUR_NO = pur_no }, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetNAMECombo()
        {
            string sql = @"select distinct AGEN_NAMEC TEXT, AGEN_NAMEC VALUE  from TC_MMAGEN";
            return DBWork.Connection.Query<ComboItemModel>(sql, new { userID = DBWork.ProcUser }, DBWork.Transaction);
        }

        public DataTable GetExcel(string pur_no, string agen_namec)
        {
            var p = new DynamicParameters();

            var sql = @"select twn_yyymm(A.PUR_DATE) 資料年月 , B.MMCODE 電腦編號, B.MMNAME_C 藥品名稱,
                            B.PUR_QTY 訂購數量, B.PUR_UNIT 單位劑量, B.IN_PURPRICE　進貨單價,　B.PUR_AMOUNT　金額小計,
                            A.PUR_NOTE 訂購單備註,　twn_date(A.PUR_DATE) 訂購日期
                            from TC_PURCH_M A
                            left join TC_PURCH_DL B
                            on A.PUR_NO=B.PUR_NO 
                            where A.PUR_NO=:PUR_NO and B.AGEN_NAMEC = :AGEN_NAMEC  ";

            p.Add(":PUR_NO", pur_no);
            p.Add(":AGEN_NAMEC", agen_namec);
            
            sql += " ORDER BY A.PUR_DATE ";
            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
    }
}