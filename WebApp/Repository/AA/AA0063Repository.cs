using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{
    public class AA0063Repository : JCLib.Mvc.BaseRepository
    {
        public AA0063Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //查詢Master
        public IEnumerable<AA0063M> GetAll(string MAT_CLASS, string MMCODE, bool clsALL ,string FLOWID, string PR_TIME_B, string PR_TIME_E, string DIS_TIME_B, string DIS_TIME_E, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT 
                            A.DOCNO,
                            (case when A.DOCTYPE in ('MR1','MR2') then '庫備' when A.DOCTYPE in ('MR3','MR4') then '非庫備' end) as M_STOREID,
                            (SELECT APPDEPT || ' ' || INID_NAME(APPDEPT) TEXT FROM ME_DOCM WHERE DOCNO = A.DOCNO) AS INID_NAME,
                            (CASE A.FLOWID WHEN '1' THEN '1-申請中' WHEN '2' THEN '2-核撥中' WHEN '3' THEN '3-揀料中' WHEN '4' THEN '4-點收中' WHEN '5' THEN '5-已點收' WHEN '6' THEN '6-已核撥確認'  when '11' then '11-核可中' ELSE FLOWID END) AS FLOWID,
                            (SELECT MAT_CLASS || ' ' || MAT_CLSNAME TEXT FROM MI_MATCLASS WHERE MAT_CLASS = A.MAT_CLASS) AS MAT_CLASS,
                            (SELECT WH_NO || ' ' || WH_NAME TEXT FROM MI_WHMAST WHERE WH_NO = A.FRWH) AS FRWH,
                            TWN_DATE(A.APPTIME) AS APPTIME,
                            TWN_DATE(B.DIS_TIME) AS DIS_TIME,
                            (case when A.FLOWID in ('1', '2') then null 
                                  when A.DOCTYPE in ('MR1','MR2','MR5','MR6') and A.FLOWID in ('3', '4', '5') then B.EXPT_DISTQTY 
                                  when A.DOCTYPE in ('MR3','MR4') and A.FLOWID in ('3', '4', '5', '51') then B.APVQTY
                                  when A.FLOWID = '6' then B.APVQTY 
                                  else null 
                              end) as dis_qty,
                            A.APPLY_NOTE,
                            B.MMCODE,
                            (SELECT MMNAME_C TEXT FROM MI_MAST WHERE MMCODE = B.MMCODE) AS MMNAME_C,
                            (SELECT MMNAME_E TEXT FROM MI_MAST WHERE MMCODE = B.MMCODE) AS MMNAME_E,
                            B.APPQTY,
                            (SELECT BASE_UNIT TEXT FROM MI_MAST WHERE MMCODE = B.MMCODE) AS BASE_UNIT,
                            (SELECT uprice TEXT FROM MI_MAST WHERE MMCODE = B.MMCODE) AS M_CONTPRICE,
                             (A.TOWH || ' ' || WH_NAME(A.TOWH)) as TOWH,
                             B.SEQ As SEQ
                        FROM
                            ME_DOCM A, ME_DOCD B
                        WHERE
                            1=1 AND A.DOCNO = B.DOCNO AND A.DOCTYPE in ('MR1','MR2','MR3','MR4','MR5','MR6') ";

            if (!string.IsNullOrWhiteSpace(MAT_CLASS))
            {
                if (clsALL == true)
                {
                    sql += @" AND A.MAT_CLASS IN ("+MAT_CLASS+") ";
                }
                else
                {
                    sql += @" AND A.MAT_CLASS = :p0 ";
                    p.Add(":p0", string.Format("{0}", MAT_CLASS));
                }
            }

            if (!string.IsNullOrWhiteSpace(MMCODE))
            {
                sql += @" AND B.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("{0}", MMCODE));
            }

            if (!string.IsNullOrWhiteSpace(FLOWID))
            {
                sql += @" AND A.FLOWID LIKE :p4 ";
                p.Add(":p4", string.Format("{0}", FLOWID));
            }

            if (!string.IsNullOrWhiteSpace(PR_TIME_B))
            {
                sql += " AND TWN_DATE(APPTIME) >= :PR_TIME_B ";
                p.Add(":PR_TIME_B", string.Format("{0}", PR_TIME_B));
            }

            if (!string.IsNullOrWhiteSpace(PR_TIME_E))
            {
                sql += " AND TWN_DATE(APPTIME) <= :PR_TIME_E ";
                p.Add(":PR_TIME_E", string.Format("{0}", PR_TIME_E));
            }

            if (!string.IsNullOrWhiteSpace(DIS_TIME_B))
            {
                sql += " AND TWN_DATE(DIS_TIME) >= :DIS_TIME_B ";
                p.Add(":DIS_TIME_B", string.Format("{0}", DIS_TIME_B));
            }

            if (!string.IsNullOrWhiteSpace(DIS_TIME_E))
            {
                sql += " AND TWN_DATE(DIS_TIME) <= :DIS_TIME_E ";
                p.Add(":DIS_TIME_E", string.Format("{0}", DIS_TIME_E));
            }

            //sql += @" ORDER BY APPTIME, A.DOCNO ";
            sql += @" ORDER BY APPTIME, A.DOCNO, B.SEQ ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0063M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //列印
        public IEnumerable<AA0063M> Print(string MAT_CLASS, string MMCODE, bool clsALL, string FLOWID, string PR_TIME_B, string PR_TIME_E, string DIS_TIME_B, string DIS_TIME_E)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT 
                            A.DOCNO,
                            (case when A.DOCTYPE in ('MR1','MR2') then '庫備' when A.DOCTYPE in ('MR3','MR4') then '非庫備' end) as M_STOREID,
                            (SELECT APPDEPT || ' ' || INID_NAME(APPDEPT) TEXT FROM ME_DOCM WHERE DOCNO = A.DOCNO) AS INID_NAME,
                            (CASE A.FLOWID WHEN '1' THEN '1-申請中' WHEN '2' THEN '2-核撥中' WHEN '3' THEN '3-揀料中' WHEN '4' THEN '4-點收中' WHEN '5' THEN '5-已點收' WHEN '6' THEN '6-已核撥確認'  when '11' then '11-核可中' ELSE FLOWID END) AS FLOWID,
                            (SELECT MAT_CLASS || ' ' || MAT_CLSNAME TEXT FROM MI_MATCLASS WHERE MAT_CLASS = A.MAT_CLASS) AS MAT_CLASS,
                            (CASE WHEN A.DOCTYPE IN ('MR1','MR2') THEN '庫備' WHEN A.DOCTYPE IN ('MR3','MR4') THEN '非庫備' END) AS M_STOREID,
                            (SELECT WH_NO || ' ' || WH_NAME TEXT FROM MI_WHMAST WHERE WH_NO = A.FRWH) AS FRWH,
                            TWN_DATE(A.APPTIME) AS APPTIME,
                            A.APPLY_NOTE,
                            B.MMCODE,
                            (SELECT MMNAME_C TEXT FROM MI_MAST WHERE MMCODE = B.MMCODE) AS MMNAME_C,
                            (SELECT MMNAME_E TEXT FROM MI_MAST WHERE MMCODE = B.MMCODE) AS MMNAME_E,
                            B.APPQTY,
                            (CASE WHEN A.FLOWID IN ('1', '2') THEN NULL 
                                WHEN A.DOCTYPE IN ('MR1','MR2','MR5','MR6') AND A.FLOWID IN ('3', '4', '5') THEN B.EXPT_DISTQTY
                                WHEN A.DOCTYPE IN ('MR3','MR4') AND A.FLOWID IN ('3', '4', '5', '51') THEN B.APVQTY 
                                WHEN A.FLOWID = '6' THEN B.APVQTY 
                                ELSE NULL 
                            END) AS dis_qty,
                            (SELECT BASE_UNIT TEXT FROM MI_MAST WHERE MMCODE = B.MMCODE) AS BASE_UNIT,
                            (SELECT uprice TEXT FROM MI_MAST WHERE MMCODE = B.MMCODE) AS M_CONTPRICE,
                            (A.TOWH || ' ' || WH_NAME(A.TOWH)) as TOWH
                        FROM
                            ME_DOCM A, ME_DOCD B
                        WHERE
                            1=1 AND A.DOCNO = B.DOCNO AND A.DOCTYPE in ('MR1','MR2','MR3','MR4','MR5','MR6') ";

            if (!string.IsNullOrWhiteSpace(MAT_CLASS))
            {
                if (clsALL == true)
                {
                    sql += @" AND A.MAT_CLASS IN (" + MAT_CLASS + ") ";
                }
                else
                {
                    sql += @" AND A.MAT_CLASS = :p0 ";
                    p.Add(":p0", string.Format("{0}", MAT_CLASS));
                }
            }

            if (!string.IsNullOrWhiteSpace(MMCODE))
            {
                sql += @" AND B.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("{0}", MMCODE));
            }

            if (!string.IsNullOrWhiteSpace(FLOWID))
            {
                sql += @" AND A.FLOWID LIKE :p4 ";
                p.Add(":p4", string.Format("{0}", FLOWID));
            }

            if (!string.IsNullOrWhiteSpace(PR_TIME_B))
            {
                sql += " AND TWN_DATE(APPTIME) >= :PR_TIME_B ";
                p.Add(":PR_TIME_B", string.Format("{0}", PR_TIME_B));
            }

            if (!string.IsNullOrWhiteSpace(PR_TIME_E))
            {
                sql += " AND TWN_DATE(APPTIME) <= :PR_TIME_E ";
                p.Add(":PR_TIME_E", string.Format("{0}", PR_TIME_E));
            }

            if (!string.IsNullOrWhiteSpace(DIS_TIME_B))
            {
                sql += " AND TWN_DATE(DIS_TIME) >= :DIS_TIME_B ";
                p.Add(":DIS_TIME_B", string.Format("{0}", DIS_TIME_B));
            }

            if (!string.IsNullOrWhiteSpace(DIS_TIME_E))
            {
                sql += " AND TWN_DATE(DIS_TIME) <= :DIS_TIME_E ";
                p.Add(":DIS_TIME_E", string.Format("{0}", DIS_TIME_E));
            }

            //sql += @" ORDER BY APPTIME, A.DOCNO ";
            sql += @" ORDER BY APPTIME, A.DOCNO, B.SEQ ";

            return DBWork.Connection.Query<AA0063M>(sql, p, DBWork.Transaction);
        }

        //匯出Excel
         public DataTable GetExcel(string MAT_CLASS, string MMCODE, bool clsALL, string FLOWID, string PR_TIME_B, string PR_TIME_E, string DIS_TIME_B, string DIS_TIME_E)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT 
                            A.DOCNO AS 申請單號,
                             (A.TOWH || ' ' || WH_NAME(A.TOWH)) AS 入庫庫房,
                            (CASE A.FLOWID WHEN '1' THEN '1-申請中' WHEN '2' THEN '2-核撥中' WHEN '3' THEN '3-揀料中' WHEN '4' THEN '4-點收中' WHEN '5' THEN '5-已點收' WHEN '51' THEN '51-已點收確認' WHEN '6' THEN '6-已核撥確認' when '11' then '11-核可中' ELSE FLOWID END) AS 申請單狀態,
                            (SELECT MAT_CLASS || ' ' || MAT_CLSNAME TEXT FROM MI_MATCLASS WHERE MAT_CLASS = A.MAT_CLASS) AS 物料分類,
                            (CASE WHEN A.DOCTYPE IN ('MR1','MR2') THEN '庫備'
                                WHEN A.DOCTYPE IN ('MR3','MR4') THEN '非庫備'
                            end) AS 庫備否,
                            TWN_DATE(A.APPTIME) AS 申請日期,
                            TWN_DATE(B.DIS_TIME) AS 核撥日期,
                            A.APPLY_NOTE AS 申請單備註,
                            B.MMCODE AS 院內碼,
                            (SELECT MMNAME_C TEXT FROM MI_MAST WHERE MMCODE = B.MMCODE) AS 中文品名,
                            (SELECT MMNAME_E TEXT FROM MI_MAST WHERE MMCODE = B.MMCODE) AS 英文品名,
                            B.APPQTY AS 申請數量,
                            (case when A.FLOWID in ('1', '2') then null 
                                when A.DOCTYPE in ('MR1','MR2','MR5','MR6') and A.FLOWID in ('3', '4', '5') then B.EXPT_DISTQTY 
                                when A.DOCTYPE in ('MR3','MR4') and A.FLOWID in ('3', '4', '5', '51') then B.APVQTY
                                when A.FLOWID = '6' then B.APVQTY else null end) as 核撥數量,
                            (SELECT BASE_UNIT TEXT FROM MI_MAST WHERE MMCODE = B.MMCODE) AS 單位,
                            (SELECT uprice TEXT FROM MI_MAST WHERE MMCODE = B.MMCODE) AS 合約單價
                        FROM
                            ME_DOCM A, ME_DOCD B
                        WHERE
                            1=1 AND A.DOCNO = B.DOCNO AND A.DOCTYPE in ('MR1','MR2','MR3','MR4','MR5','MR6') ";

            if (!string.IsNullOrWhiteSpace(MAT_CLASS))
            {
                if (clsALL == true)
                {
                    sql += @" AND A.MAT_CLASS IN (" + MAT_CLASS + ") ";
                }
                else
                {
                    sql += @" AND A.MAT_CLASS = :p0 ";
                    p.Add(":p0", string.Format("{0}", MAT_CLASS));
                }
            }

            if (!string.IsNullOrWhiteSpace(MMCODE))
            {
                sql += @" AND B.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("{0}", MMCODE));
            }

            if (!string.IsNullOrWhiteSpace(FLOWID))
            {
                sql += @" AND A.FLOWID LIKE :p4 ";
                p.Add(":p4", string.Format("{0}", FLOWID));
            }

            if (!string.IsNullOrWhiteSpace(PR_TIME_B))
            {
                sql += " AND TWN_DATE(APPTIME) >= :PR_TIME_B ";
                p.Add(":PR_TIME_B", string.Format("{0}", PR_TIME_B));
            }

            if (!string.IsNullOrWhiteSpace(PR_TIME_E))
            {
                sql += " AND TWN_DATE(APPTIME) <= :PR_TIME_E ";
                p.Add(":PR_TIME_E", string.Format("{0}", PR_TIME_E));
            }

            if (!string.IsNullOrWhiteSpace(DIS_TIME_B))
            {
                sql += " AND TWN_DATE(DIS_TIME) >= :DIS_TIME_B ";
                p.Add(":DIS_TIME_B", string.Format("{0}", DIS_TIME_B));
            }

            if (!string.IsNullOrWhiteSpace(DIS_TIME_E))
            {
                sql += " AND TWN_DATE(DIS_TIME) <= :DIS_TIME_E ";
                p.Add(":DIS_TIME_E", string.Format("{0}", DIS_TIME_E));
            }

            //sql += @" ORDER BY APPTIME, A.DOCNO ";
            sql += @" ORDER BY APPTIME, A.DOCNO, B.SEQ ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }


        public IEnumerable<ComboItemModel> GetCLSNAME()
        {

            string sql = @"  SELECT MAT_CLASS VALUE,
                                    MAT_CLASS || ' ' || MAT_CLSNAME TEXT
                             FROM MI_MATCLASS
                             WHERE MAT_CLSID IN ('2','3') 
                             ORDER BY MAT_CLASS";
            

            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetFLOWID()
        {
            string sql = @"  SELECT FLOWID
                             FROM ME_FLOW
                             ORDER BY FLOWID";
            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }


        

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} 
                            A.MMCODE, 
                            A.MMNAME_C, 
                            A.MMNAME_E, 
                            A.MAT_CLASS, 
                            A.BASE_UNIT 
                        FROM 
                            MI_MAST A 
                        WHERE 1=1 
                            AND (SELECT COUNT(*) FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO = WHNO_MM1 ) > 0 ";

            if (p1 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(A.MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(A.MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p1);
                p.Add(":MMNAME_E_I", p1);
                p.Add(":MMNAME_C_I", p1);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", p1));

                sql += " OR A.MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p1));

                sql += " OR A.MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p1));

                if (p0 != "")
                {
                    sql += " AND A.MAT_CLASS = :MAT_CLASS ";
                    p.Add(":MAT_CLASS", string.Format("{0}", p0));
                }

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, MMCODE", sql);
            }
            else
            {
                sql = string.Format(sql, "");

                if (p0 != "")
                {
                    sql += " AND A.MAT_CLASS = :MAT_CLASS ";
                    p.Add(":MAT_CLASS", string.Format("{0}", p0));
                }

                sql += " ORDER BY A.MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }


        public IEnumerable<MI_MAST> GetMmcode(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT 
                                A.MMCODE, 
                                A.MMNAME_C, 
                                A.MMNAME_E, 
                                A.M_CONTPRICE, 
                                A.BASE_UNIT,
                                A.MAT_CLASS
                            FROM 
                                MI_MAST A 
                            WHERE 1=1 
                                AND (SELECT COUNT(*) FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO = WHNO_MM1 ) > 0 ";

            if (query.MMCODE != "")
            {
                sql += " AND A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", query.MMCODE));
            }

            if (query.MMNAME_C != "")
            {
                sql += " AND A.MMNAME_C LIKE :MMNAME_C ";
                p.Add(":MMNAME_C", string.Format("%{0}%", query.MMNAME_C));
            }

            if (query.MMNAME_E != "")
            {
                sql += " AND A.MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", query.MMNAME_E));
            }

            if (query.MAT_CLASS != "")
            {
                sql += " AND A.MAT_CLASS = :MAT_CLASS ";
                p.Add(":MAT_CLASS", string.Format("{0}", query.MAT_CLASS));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_WHMAST> GetWH_NoCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.WH_NO, A.WH_NAME, A.WH_KIND, A.WH_GRADE 
                        FROM MI_WHMAST A 
                        WHERE 1=1 AND (SELECT COUNT(*) FROM ME_DOCM WHERE FRWH = A.WH_NO) > 0 ";
            
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

        public IEnumerable<ComboItemModel> GetWh_no()
        {
            var p = new DynamicParameters();

            string sql = @" SELECT 
                                WH_NO VALUE, WH_NO || ' ' || WH_NAME TEXT
                            FROM 
                                MI_WHMAST 
                            WHERE 
                                WH_NO = WHNO_MM1";

            return DBWork.Connection.Query<ComboItemModel>(sql, new { inid = DBWork.UserInfo.Inid }, DBWork.Transaction);
        }

        public string getDeptName()
        {
            string sql = @" SELECT  INID_NAME AS USER_DEPTNAME
                            FROM    UR_INID
                            WHERE   INID = (select INID from UR_ID where TUSER = (:userID)) ";

            var str = DBWork.Connection.ExecuteScalar(sql, new { userID = DBWork.ProcUser }, DBWork.Transaction);
            return str == null ? "" : str.ToString();

            //return DBWork.Connection.ExecuteScalar(sql, new { userID = DBWork.ProcUser }, DBWork.Transaction).ToString();
        }


        public class MI_MAST_QUERY_PARAMS
        {
            public string MMCODE;
            public string MMNAME_C;
            public string MMNAME_E;
            public string MAT_CLASS;

            public string WH_NO;
            public string IS_INV;  // 需判斷庫存量>0
            public string E_IFPUBLIC;  // 是否公藥
        }

        public class MI_WHMAST_QUERY_PARAMS
        {
            public string MMCODE;
            public string MMNAME_C;
            public string MMNAME_E;
            public string MAT_CLASS;

            public string WH_NO;
            public string WH_NAME;
            public string IS_INV;  // 需判斷庫存量>0
            public string E_IFPUBLIC;  // 是否公藥
        }

    }
}
