using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{
    public class AA0071Repository : JCLib.Mvc.BaseRepository
    {
        public AA0071Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MI_MAST> GetMMCODECombo(string mmcode, string wh_no, string mat_class, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"SELECT {0} A.MMCODE , A.MMNAME_C, A.MMNAME_E " +
                "from MI_MAST A JOIN MI_WINVCTL B ON (A.MMCODE = B.MMCODE) WHERE B.WH_NO = :WH_NO ";

            if(mat_class !="" && mat_class != null)
            {
                sql += " AND A.MAT_CLASS = :mat_class ";
                p.Add(":mat_class", mat_class);
            }

            if (mmcode != "" && mmcode != null)
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

            p.Add(":WH_NO", wh_no);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA0071> GetAll(string apptime1, string apptime2, string matclass, string mmcode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select A.DOCNO, TWN_DATE(A.APPTIME) APPTIME, 
                               A.DOCTYPE||(select DATA_DESC from PARAM_D where GRP_CODE='ADJX_CODE' and DATA_VALUE=A.DOCTYPE) DOCTYPE_N,
                              (select MAT_CLASS||'-'||MAT_CLSNAME from MI_MATCLASS where A.MAT_CLASS = MAT_CLASS) MAT_CLASS_N,
                               B.MMCODE, C.mmname_C ||C.mmname_E MMNAME, B.APPQTY, C.BASE_UNIT, C.M_CONTPRICE
                          from ME_DOCM A, ME_DOCD B, MI_MAST C 
                         where A.DOCNO = B.DOCNO and B.MMCODE = C.MMCODE and A.FLOWID = '3' and A.DOCTYPE like 'AJ%' ";
            
            if (apptime1 != "" & apptime2 != "") //日期區間
            {
                sql += " AND TWN_DATE(A.APPTIME) BETWEEN :d0 AND :d1 ";
                p.Add(":d0", string.Format("{0}", apptime1));
                p.Add(":d1", string.Format("{0}", apptime2));
            }
            if (apptime1 != "" & apptime2 == "")
            {
                sql += " AND TWN_DATE(A.APPTIME) >= :d0 ";
                p.Add(":d0", string.Format("{0}", apptime1));
            }
            if (apptime1 == "" & apptime2 != "")
            {
                sql += " AND TWN_DATE(A.APPTIME) <= :d1 ";
                p.Add(":d1", string.Format("{0}", apptime2));
            }
            if (matclass != "" && matclass != null) //物料分類
            {
                sql += " AND A.MAT_CLASS = :p1 ";
                p.Add(":p1", string.Format("{0}", matclass));
            }
            if (mmcode != "" && mmcode != null) //院內碼
            {
                sql += " AND B.MMCODE LIKE :p2 ";
                p.Add(":p2", string.Format("%{0}%", mmcode));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0071>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        internal DataTable GetExcel(string apptime1, string apptime2, string matclass, string mmcode)
        {
            var dt = new DataTable();
            var p = new DynamicParameters();
            var sql = @"select A.DOCNO as 單據號碼, TWN_DATE(A.APPTIME) as 申請日期, 
                               A.DOCTYPE||(SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='ADJX_CODE' AND DATA_VALUE=A.DOCTYPE) as 異動代碼,
                              (select MAT_CLASS||'-'||MAT_CLSNAME from MI_MATCLASS WHERE A.MAT_CLASS = MAT_CLASS) as 物料分類,
                               B.MMCODE as 院內碼, C.mmname_C ||C.mmname_E as 品名, B.APPQTY as 調帳數量, C.base_unit as 單位, C.M_CONTPRICE as 合約單價 
                          from ME_DOCM A, ME_DOCD B, MI_MAST C 
                         where A.DOCNO= B.DOCNO and B.MMCODE = C.MMCODE and A.FLOWID = '3' and A.DOCTYPE like 'AJ%'";

            if (apptime1 != "" & apptime2 != "") //日期區間
            {
                sql += " AND TWN_DATE(A.APPTIME) BETWEEN :d0 AND :d1 ";
                p.Add(":d0", string.Format("{0}", apptime1));
                p.Add(":d1", string.Format("{0}", apptime2));
            }
            if (apptime1 != "" & apptime2 == "")
            {
                sql += " AND TWN_DATE(A.APPTIME) >= :d0 ";
                p.Add(":d0", string.Format("{0}", apptime1));
            }
            if (apptime1 == "" & apptime2 != "")
            {
                sql += " AND TWN_DATE(A.APPTIME) <= :d1 ";
                p.Add(":d1", string.Format("{0}", apptime2));
            }
            if (matclass != "") //物料分類
            {
                sql += " AND A.MAT_CLASS = :p1 ";
                p.Add(":p1", string.Format("{0}", matclass));
            }
            if (mmcode != "") //院內碼
            {
                sql += " AND B.MMCODE LIKE :p2 ";
                p.Add(":p2", string.Format("%{0}%", mmcode));
            }
            sql += " ORDER BY A.APPTIME ";
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
            {
                dt.Load(rdr);
            }
            return dt;
        }

        public IEnumerable<AA0071> GetPrintData(string apptime1, string apptime2, string matclass, string mmcode)
        {
            var p = new DynamicParameters();

            var sql = @"select A.DOCNO, TWN_DATE(A.APPTIME) APPTIME, 
                               A.DOCTYPE||(select DATA_DESC from PARAM_D where GRP_CODE='ADJX_CODE' and DATA_VALUE=A.DOCTYPE) DOCTYPE_N,
                              (select MAT_CLASS||'-'||MAT_CLSNAME from MI_MATCLASS where A.MAT_CLASS = MAT_CLASS) MAT_CLASS_N,
                               B.MMCODE, C.mmname_C ||C.mmname_E MMNAME, B.APPQTY, C.BASE_UNIT, C.M_CONTPRICE
                          from ME_DOCM A, ME_DOCD B, MI_MAST C 
                         where A.DOCNO = B.DOCNO and B.MMCODE = C.MMCODE and A.FLOWID = '3' and A.DOCTYPE like 'AJ%' ";

            if (apptime1 != "" & apptime2 != "") //日期區間
            {
                sql += " AND TWN_DATE(A.APPTIME) BETWEEN :d0 AND :d1 ";
                p.Add(":d0", apptime1);
                p.Add(":d1", apptime2);
            }
            if (apptime1 != "" & apptime2 == "")
            {
                sql += " AND TWN_DATE(A.APPTIME) >= :d0 ";
                p.Add(":d0", apptime1);
            }
            if (apptime1 == "" & apptime2 != "")
            {
                sql += " AND TWN_DATE(A.APPTIME) <= :d1 ";
                p.Add(":d1", apptime2);
            }
            if (matclass != "" && matclass != null) //物料分類
            {
                sql += " AND A.MAT_CLASS = :p1 ";
                p.Add(":p1", string.Format("{0}", matclass));
            }
            if (mmcode != "" && mmcode != null) //院內碼
            {
                sql += " AND B.MMCODE LIKE :p2 ";
                p.Add(":p2", string.Format("%{0}%", mmcode));
            }

            sql += @" ORDER BY A.APPTIME, A.DOCNO ";

            return DBWork.Connection.Query<AA0071>(sql, p, DBWork.Transaction);
        }
    }
}