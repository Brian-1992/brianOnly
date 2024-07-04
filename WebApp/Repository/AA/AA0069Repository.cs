using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using TSGH.Models;
namespace WebApp.Repository.AA
{
    public class AA0069_MODEL : JCLib.Mvc.BaseModel
    {
        public string TR_SNO { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_E { get; set; } 
        public string WH_NO { get; set; }
        public string MMNAME_C { get; set; }
        public string TR_DATE { get; set; }
        public string TR_DOCNO { get; set; }
        public string TR_INV_QTY { get; set; }
        public string TR_ONWAY_QTY { get; set; }
        public string DOCTYPE_NAME { get; set; }
        public string MCODE_NAME { get; set; }
        public string BASE_UNIT { get; set; }
        public string M_CONTPRICE { get; set; }
        public string E_SUPSTATUS { get; set; }
        public string BF_TR_INVQTY { get; set; }
        public string AF_TR_INVQTY { get; set; }
        public string FRWH_N { get; set; }
        public string TOWH_N { get; set; }


        public string LOTNO_EXP_QTY { get; set; }

    }
    public class AA0069Repository : JCLib.Mvc.BaseRepository
    {
        public AA0069Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

       
        public IEnumerable<ComboModel> GetMatClass(string userId)
        {
            string sql = @"SELECT DISTINCT b.MAT_CLASS as VALUE, " +
                "b.MAT_CLASS || ' ' || b.MAT_CLSNAME as COMBITEM " +
                "from MI_WHID a " +
                "JOIN MI_MATCLASS b on a.TASK_ID=b.MAT_CLSID " +
                "where MAT_CLSID=WHM1_TASK(:WH_USERID)";
            return DBWork.Connection.Query<ComboModel>(sql, new { WH_USERID = userId }, DBWork.Transaction);
        }
        public IEnumerable<COMBO_MODEL> GetMatclassCombo(string id)
        {
            string sql = @"select MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
                        FROM MI_MATCLASS WHERE MAT_CLSID=WHM1_TASK(:TUSER)   
                        ORDER BY MAT_CLASS";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { TUSER = id });
        }
        public IEnumerable<ComboModel> GetWhnoCombo(string category, string level)
        {
            string sql = @"select distinct wh_no as VALUE , wh_no ||' '|| wh_name as COMBITEM from mi_whmast where wh_kind=:wh_kind and wh_grade=:wh_grade ORDER by VALUE";

            return DBWork.Connection.Query<ComboModel>(sql, new { wh_kind = category, wh_grade = level }, DBWork.Transaction);
        }


        public IEnumerable<AA0069_MODEL> GetAll(string bgdate,string endate, string type, string mmcode, string grade,string whcode, string kind, string show,string doctype,string mcode, string tr_docno, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select a.TR_SNO, WH_NO,a.MMCODE,b.MMNAME_C,b.MMNAME_E,TWN_TIME(a.TR_DATE) TR_DATE,
                        TR_DOCNO,TR_INV_QTY,TR_ONWAY_QTY,case when TR_IO='I' then '入庫' when TR_IO='O' then '出庫' else '' end as TR_IO, 
                        (select DOCTYPE_NAME from MI_DOCTYPE where MI_DOCTYPE.DOCTYPE=a.TR_DOCTYPE) as DOCTYPE_NAME ,
                        (select MCODE_NAME from MI_MCODE where MI_MCODE.MCODE=a.TR_MCODE) as MCODE_NAME, 
                         b.BASE_UNIT,b.M_CONTPRICE,
                        a.BF_TR_INVQTY,a.AF_TR_INVQTY,
                        (SELECT X.WH_NO || ' ' || X.WH_NAME from MI_WHMAST X,ME_DOCM Y where X.WH_NO=Y.FRWH AND Y.DOCNO = A.TR_DOCNO) FRWH_N,
                        (SELECT X.WH_NO || ' ' || X.WH_NAME from MI_WHMAST X,ME_DOCM Y where X.WH_NO=Y.TOWH AND Y.DOCNO = A.TR_DOCNO) TOWH_N,
                        DOCEXP_LOT(a.tr_docno, a.tr_docseq) as LOTNO_EXP_QTY
                        from mi_whtrns a, mi_mast b where a.mmcode=b.mmcode ";

            //if (bgdate != "null" && bgdate != "NaN" && bgdate != "")
            //{
            //    sql += " AND to_char(a.TR_DATE,'yyyy-mm-dd') >= Substr(:p1, 1, 10) ";
            //    p.Add(":p1", bgdate);
            //}
            //if (endate != "null" && endate != "NaN" && endate != "")
            //{
            //    sql += " AND to_char(a.TR_DATE,'yyyy-mm-dd') <= Substr(:p2, 1, 10) ";
            //    p.Add(":p2", endate);
            //}

            if (bgdate != "" & endate != "")
            {
                sql += " AND TWN_DATE(A.TR_DATE) BETWEEN :d0 AND :d1 ";
                p.Add(":d0", string.Format("{0}", bgdate));
                p.Add(":d1", string.Format("{0}", endate));
            }
            if (bgdate != "" & endate == "")
            {
                sql += " AND TWN_DATE(A.TR_DATE) >= :d0 ";
                p.Add(":d0", string.Format("{0}", bgdate));
            }
            if (bgdate == "" & endate != "")
            {
                sql += " AND TWN_DATE(A.TR_DATE) <= :d1 ";
                p.Add(":d1", string.Format("{0}", endate));
            }
            if (type != "" && type != "null" && type != null)
            {
                sql += " AND b.MAT_CLASS  = :p3 ";
                p.Add(":p3", type);
            }

            if (mmcode != "" && mmcode != "null" && mmcode != null)
            {
                sql += " AND A.MMCODE  = :p4 ";
                p.Add(":p4", mmcode);
            }

            //if (mmcode1 != "" && mmcode1 != "null")
            //{
            //    sql += " AND A.MMCODE <=   = :p2 ";
            //    p.Add(":p2", mmcode1);
            //}

            if (whcode != "" && whcode != "null" && whcode !=null)
            {
                sql += " AND a.WH_NO = :p5 ";
                p.Add(":p5", whcode);
            }

            if (show != "" && show != "null" && show != null)
            {
                sql += " AND b.M_STOREID  = :p6 ";
                p.Add(":p6", show);
            }

            if (doctype != "" && doctype != "null" && doctype != null)
            {
                sql += " AND a.TR_DOCTYPE  = :p9 ";
                p.Add(":p9", doctype);
            }
            
            if (mcode != "" && mcode != "null" && mcode != null)
            {
                sql += " AND a.TR_MCODE  = :p10 ";
                p.Add(":p10", mcode);
            }

            if (tr_docno != "" && tr_docno != "null" && tr_docno != null)
            {
                sql += " AND a.TR_DOCNO like :p11 ";
                p.Add(":p11", string.Format("%{0}%", tr_docno));
            }
         
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0069_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCODECombo(string mmcode, string task_id,  int page_index, int page_size, string sorters)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"SELECT DISTINCT {0} MMCODE , MMNAME_C, MMNAME_E from MI_MAST A WHERE 1=1 ";


            if (task_id != "" && task_id != null)  //物料分類
            {
                //string[] tmp = task_id.Split(',');
                sql += "AND MAT_CLASS = :mat_class ";
                p.Add(":mat_class", task_id);
            }
            else
            {
                //表示一般物品人員選單
                sql += "AND MAT_CLASS BETWEEN '03' and '08'";
            }


            if (mmcode != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", mmcode);
                p.Add(":MMNAME_E_I", mmcode);
                p.Add(":MMNAME_C_I", mmcode);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", mmcode));

                sql += " OR MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", mmcode));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", mmcode));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public string GetUserKind(string id)
        {
            string sql = @"SELECT USER_KIND(:ID) FROM DUAL";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { ID = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetTaskid(string id)
        {
            string sql = @"SELECT WHM1_TASK(:WH_USERID) FROM DUAL ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { WH_USERID = id }, DBWork.Transaction).ToString();
            return rtn;
        }

        public IEnumerable<COMBO_MODEL> GetWhComboA()
        {
            string sql = @"SELECT DISTINCT A.WH_NO as VALUE, A.WH_NAME as TEXT,
                        A.WH_NO || ' ' || A.WH_NAME as COMBITEM 
                        FROM MI_WHMAST A
                        WHERE A.WH_KIND IN ('1','2') 
                        ORDER BY WH_NO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetWhCombo1()
        {
            string sql = @"SELECT DISTINCT A.WH_NO as VALUE, A.WH_NAME as TEXT,
                        A.WH_NO || ' ' || A.WH_NAME as COMBITEM 
                        FROM MI_WHMAST A
                        WHERE A.WH_KIND = '0'
                        ORDER BY WH_NO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetWhCombo2()
        {
            string sql = @"SELECT DISTINCT A.WH_NO as VALUE, A.WH_NAME as TEXT,
                        A.WH_NO || ' ' || A.WH_NAME as COMBITEM 
                        FROM MI_WHMAST A
                        WHERE A.WH_KIND = '1'  
                        ORDER BY WH_NO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetMatclassComboA()
        {
            string sql = @"select MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
                        FROM MI_MATCLASS WHERE MAT_CLSID IN ('1','2','3')    
                        ORDER BY MAT_CLASS";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetMatclassCombo1()
        {
            string sql = @"select MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
                        FROM MI_MATCLASS WHERE MAT_CLSID = '1'    
                        ORDER BY MAT_CLASS";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetMatclassCombo2()
        {
            string sql = @"select MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
                        FROM MI_MATCLASS WHERE MAT_CLSID IN ('2','3')    
                        ORDER BY MAT_CLASS";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetDoctypeCombo()
        {
            string sql = @"SELECT DISTINCT A.TR_DOCTYPE AS VALUE, B.DOCTYPE_NAME AS TEXT, 
                       A.TR_DOCTYPE || ' '|| B.DOCTYPE_NAME AS COMBITEM 
                        FROM MI_WHTRNS A,MI_DOCTYPE B 
                        WHERE A.TR_DOCTYPE=B.DOCTYPE 
                        ORDER BY B.DOCTYPE_NAME ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetMcodeCombo()
        {
            string sql = @"SELECT DISTINCT A.TR_MCODE AS VALUE, B.MCODE_NAME AS TEXT, 
                       A.TR_MCODE || ' '|| B.MCODE_NAME AS COMBITEM 
                        FROM MI_WHTRNS A,MI_MCODE B 
                        WHERE A.TR_MCODE=B.MCODE 
                        ORDER BY B.MCODE_NAME ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
    }
}