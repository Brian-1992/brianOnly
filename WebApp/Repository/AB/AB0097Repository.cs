using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using TSGH.Models;
using System.Collections.Generic;
using JCLib.DB.Tool;
using System.Text;
using System.IO;
using System.Web.Mail;
using System.Net;
using System.Net.Sockets;



namespace WebApp.Repository.AB
{
    public class AB0097ReportMODEL : JCLib.Mvc.BaseModel
    {
        public string F1 { get; set; }
        public string F2 { get; set; }
        public string F3 { get; set; }
        public string F4 { get; set; }
        public string F5 { get; set; }
        public string F6 { get; set; }
        public float F7 { get; set; }
        public string F8 { get; set; }
        public float F9 { get; set; }
        public string F10 { get; set; }
    }
    public class AB0097_MODEL : JCLib.Mvc.BaseModel
    {
        public string DOCNO { get; set; } // 01.單據號碼
        public string MAT_CLASS { get; set; } // 02.物料分類
        public string MAT_CLASS_NAME { get; set; } // 03.物料分類中文
        public string FRWH { get; set; } // 04.庫房
        public string FRWH_NAME { get; set; } // 05.庫房中文名稱
        public string APPTIME { get; set; } // 06.申請日期
        public string APPTIME_START { get; set; } // 06.申請日期
        public string APPTIME_END { get; set; } // 06.申請日期
        public string APPLY_NOTE { get; set; } // 07.申請單備註
        public string MMCODE { get; set; } // 08.院內碼
        public string MMNAME_C { get; set; } // 09.品名中文
        public string MMNAME_E { get; set; } // 10.品名英文
        public string MMNAME_CE { get; set; } // 11.品名
        public string APPQTY { get; set; } // 12.申請繳回數量
        public string BASE_UNIT { get; set; } // 13.單位
        public string AVG_PRICE { get; set; } // 14.平均單價
        public string DOCTYPE { get; set; } // 15.單據類別
        public string FLOWID { get; set; } // 16.流程代碼

        public string USERID { get; set; } 
    }
    public class AB0097Repository : JCLib.Mvc.BaseRepository
    {
        FL l = new FL("WebApp.Repository.AA.AB0097Repository");

        public AB0097Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        String sBr = "\r\n";
        //AB0097
        public IEnumerable<AB0097_MODEL> GetAll(AB0097_MODEL v, int page_index, int page_size,string sorters)
        {
            int iP = 0;
            String sP = "";
            var p = new DynamicParameters();
            var sql = "";
            sql += "select " + sBr;
            sql += "A.DOCNO, " + sBr; // 單據號碼
            sql += "A.MAT_CLASS, " + sBr; // 物料分類
            //sql += "(select MAT_CLASS || ' ' || MAT_CLSNAME from MI_MATCLASS where MAT_CLASS=A.MAT_CLASS) MAT_CLASS_NAME, " + sBr; // 物料分類中文
            sql += "(select MAT_CLSNAME from MI_MATCLASS where MAT_CLASS=A.MAT_CLASS) MAT_CLASS_NAME, " + sBr; // 物料分類中文
            sql += "A.FRWH, " + sBr; // 庫房,中文名WH_NAME(FRWH)+代碼FRWH
            sql += "WH_NAME(A.FRWH) || ' ' || A.FRWH FRWH_NAME," + sBr;
            sql += "to_char(A.APPTIME,'yyyy/mm/dd') APPTIME, " + sBr;
            sql += "A.APPLY_NOTE, " + sBr;
            sql += "B.MMCODE, " + sBr;
            sql += "C.MMNAME_C, " + sBr;
            sql += "C.MMNAME_E, " + sBr;
            sql += "C.MMNAME_C || ' ' || C.MMNAME_E MMNAME_CE,  " + sBr;
            sql += "B.APPQTY, " + sBr;
            sql += "C.BASE_UNIT, " + sBr;
            sql += "(" + sBr;
            sql += "    select avg(AVG_PRICE) " + sBr;
            sql += "    from MI_WHCOST where 1=1 " + sBr;
            sql += "    and MMCODE=c.MMCODE  " + sBr;
            sql += "    and DATA_YM=to_char(TO_NUMBER(to_char(sysdate,'yyyy'))-1911) || to_char(sysdate,'mm') " + sBr;
            sql += ") AVG_PRICE,  " + sBr;
            sql += "'' endl " + sBr;
            sql += "FROM ME_DOCM A, ME_DOCD B, MI_MAST C " + sBr;
            sql += "WHERE 1=1 " + sBr;
            sql += "and A.DOCNO= B.DOCNO " + sBr;
            sql += "AND B.MMCODE = C.MMCODE " + sBr;
            sql += "and A.DOCTYPE = 'RN1' " + sBr;
            sql += "and A.FLOWID='3'" + sBr;

            l.getTwoDateCondition("APPTIME", v.APPTIME_START, v.APPTIME_END, ref sql, ref iP, ref p); // 繳回日期區間

            if (!String.IsNullOrEmpty(v.FRWH)) // 繳回庫房
            {
                sP = ":p" + iP++;
                sql += " and A.FRWH = " + sP + " ";
                p.Add(sP, string.Format("{0}", v.FRWH));
            }

            if (!String.IsNullOrEmpty(v.MAT_CLASS)) // 物料分類
            {
                sP = ":p" + iP++;
                sql += " and A.MAT_CLASS = " + sP + " ";
                p.Add(sP, string.Format("{0}", v.MAT_CLASS));
            }
            l.lg("GetAll()", l.getDebugSql(sql, p));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0097_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ComboModel> GetFrwh(string userId)
        {
            string sql = "";
            sql += "select " + sBr;
            sql += "WH_NO || ' ' ||  WH_NAME as COMBITEM," + sBr;
            sql += "WH_NO as VALUE" + sBr;
            sql += "-- *" + sBr;
            sql += "from MI_WHMAST" + sBr;
            sql += "where 1=1 " + sBr;
            // sql += "and PWH_NO=:USERID " + sBr; // '560000'
            sql += "and WH_NO in (" + sBr;
            sql += "    select distinct FRWH from ME_DOCM where FRWH is not null" + sBr;
            sql += ")" + sBr;
            sql += "order by WH_NO" + sBr;
            return DBWork.Connection.Query<ComboModel>(sql, new { USERID = userId }, DBWork.Transaction);
        }

        public DataTable GetExcel(AB0097_MODEL v)
        {
            int iP = 0;
            String sP = "";
            var p = new DynamicParameters();
            var sql = "";
            sql += "select " + sBr;
            sql += "A.DOCNO as 繳回單號, " + sBr; // 單據號碼
            //sql += "A.MAT_CLASS, " + sBr; // 物料分類
            sql += "(select MAT_CLSNAME from MI_MATCLASS where MAT_CLASS=A.MAT_CLASS) as 物料分類, " + sBr; // 物料分類中文
            // sql += "A.FRWH, " + sBr; // 庫房,中文名WH_NAME(FRWH)+代碼FRWH
            sql += "A.FRWH || ' ' || WH_NAME(A.FRWH) 繳回庫房," + sBr;
            sql += "to_char(A.APPTIME,'yyyy/mm/dd') 申請日期, " + sBr;
            sql += "A.APPLY_NOTE 申請備註, " + sBr;
            sql += "B.MMCODE 院內碼, " + sBr;
            //sql += "C.MMNAME_C, " + sBr;
            //sql += "C.MMNAME_E, " + sBr;
            sql += "C.MMNAME_C || ' ' || C.MMNAME_E 品名,  " + sBr;
            sql += "B.APPQTY 繳回數量, " + sBr;
            sql += "C.BASE_UNIT 單位, " + sBr;
            sql += "(" + sBr;
            sql += "    select avg(AVG_PRICE) " + sBr;
            sql += "    from MI_WHCOST where 1=1 " + sBr;
            sql += "    and MMCODE=c.MMCODE  " + sBr;
            sql += "    and DATA_YM=to_char(TO_NUMBER(to_char(sysdate,'yyyy'))-1911) || to_char(sysdate,'mm') " + sBr;
            sql += ") 平均單價, " + sBr;
            sql = sql.Substring(0, sql.Length - 4);
            sql += " FROM ME_DOCM A, ME_DOCD B, MI_MAST C " + sBr;
            sql += " WHERE 1=1 " + sBr;
            sql += " and A.DOCNO= B.DOCNO " + sBr;
            sql += " AND B.MMCODE = C.MMCODE " + sBr;
            sql += " and A.DOCTYPE = 'RN1' " + sBr;
            sql += " and A.FLOWID='3'" + sBr;

            l.getTwoDateCondition("APPTIME", v.APPTIME_START, v.APPTIME_END, ref sql, ref iP, ref p); // 繳回日期區間

            if (!String.IsNullOrEmpty(v.FRWH)) // 繳回庫房
            {
                sP = ":p" + iP++;
                sql += " and A.FRWH = " + sP + " ";
                p.Add(sP, string.Format("{0}", v.FRWH));
            }

            if (!String.IsNullOrEmpty(v.MAT_CLASS)) // 物料分類
            {
                sP = ":p" + iP++;
                sql += " and A.MAT_CLASS = " + sP + " ";
                p.Add(sP, string.Format("{0}", v.MAT_CLASS));
            }
            l.lg("GetExcel()", l.getDebugSql(sql, p));


            DataTable dt = new DataTable();
            //var sql2 = @"select Trim(INID_NAME(USER_INID(:USRID))) as INID_NAME_USER from PARAM_D where rownum = 1";
            //var title = DBWork.Connection.ExecuteScalar(sql2, new { USRID = v.USERID }, DBWork.Transaction).ToString()
            //        + v.APPTIME_START + "至" + v.APPTIME_END + "繳回入帳明細報表";
            //dt.Columns.Add(title);


            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<ComboModel> GetMatClass(string userId)
        {
            string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
                        FROM MI_MATCLASS WHERE MAT_CLSID IN ('2','3')    
                        ORDER BY MAT_CLASS";
            //return DBWork.Connection.Query<ComboModel>(sql, new { WH_USERID = userId }, DBWork.Transaction);
            return DBWork.Connection.Query<ComboModel>(sql, null, DBWork.Transaction);
        }


        //public IEnumerable<AB0097_MODEL> GetPrintData(AB0097_MODEL v)
        //{
        //    return GetAll( v, 1, 999999, "[{ property: 'DOCNO', direction: 'DESC' }]");
        //    // return DBWork.Connection.Query<AB0097_MODEL>(sql, p, DBWork.Transaction);
        //} // 


        public IEnumerable<AB0097ReportMODEL> GetPrintData(string p0, string p1, string p2,string p3)
        {
            var p = new DynamicParameters();
            var sql = "";
            sql += "select "  ;
            sql += "A.DOCNO as F1, "  ; // 單據號碼
            sql += "(select MAT_CLSNAME from MI_MATCLASS where MAT_CLASS=A.MAT_CLASS) as F2, "  ; // 物料分類中文
            sql += "A.FRWH || ' ' || WH_NAME(A.FRWH) F3,"  ;
            sql += "to_char(A.APPTIME,'yyyy/mm/dd') F4, "  ;
            sql += "A.APPLY_NOTE F10, "  ;
            sql += "B.MMCODE F5, "  ;
            sql += "C.MMNAME_C || ' ' || C.MMNAME_E F6,  "  ;
            sql += "B.APPQTY F7, "  ;
            sql += "C.BASE_UNIT F8, "  ;
            sql += "("  ;
            sql += "    select avg(AVG_PRICE) "  ;
            sql += "    from MI_WHCOST where 1=1 "  ;
            sql += "    and MMCODE=c.MMCODE  "  ;
            sql += "    and DATA_YM=to_char(TO_NUMBER(to_char(sysdate,'yyyy'))-1911) || to_char(sysdate,'mm') "  ;
            sql += ") F9 "  ;
            sql += " FROM ME_DOCM A, ME_DOCD B, MI_MAST C "  ;
            sql += " WHERE 1=1 "  ;
            sql += " and A.DOCNO= B.DOCNO "  ;
            sql += " AND B.MMCODE = C.MMCODE "  ;
            sql += " and A.DOCTYPE = 'RN1' "  ;
            sql += " and A.FLOWID='3'"  ;

            if (p0 != "" & p1 != "")
            {
                sql += " AND TWN_DATE(A.APPTIME) BETWEEN :d0 AND :d1 ";
                p.Add(":d0", string.Format("{0}", p0));
                p.Add(":d1", string.Format("{0}", p1));
            }
            if (p0 != "" & p1 == "")
            {
                sql += " AND TWN_DATE(A.APPTIME) >= :d0 ";
                p.Add(":d0", string.Format("{0}", p0));
            }
            if (p0 == "" & p1 != "")
            {
                sql += " AND TWN_DATE(A.APPTIME) <= :d1 ";
                p.Add(":d1", string.Format("{0}", p1));
            }

            if (!String.IsNullOrEmpty(p3)) // 繳回庫房
            {
                sql += " and A.FRWH = :p3 ";
                p.Add(":p3", string.Format("{0}", p3));
            }

            if (!String.IsNullOrEmpty(p2)) // 物料分類
            {
                sql += " and A.MAT_CLASS = :p2 ";
                p.Add(":p2", string.Format("{0}", p2));
            }

            return DBWork.Connection.Query<AB0097ReportMODEL>(sql, p, DBWork.Transaction);

        }









        //AB0056
        public IEnumerable<AB0097_MODEL> GetAllByDept(string userId, string APPTIME1, string p1, string task_id, string mmcode, string showopt, string showdata, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select B.MMCODE, (SELECT MMNAME_C FROM MI_MAST WHERE MMCODE=B.MMCODE) MMNAME_C, 
                      (SELECT MMNAME_E FROM MI_MAST WHERE MMCODE=B.MMCODE) MMNAME_E, 
                       C.MAT_CLSNAME, 
                      (SELECT BASE_UNIT FROM MI_MAST WHERE MMCODE=B.MMCODE) BASE_UNIT, ";

            sql += "INID_NAME(USER_INID(:INID)) INID_NAME_USER, ";
            p.Add(":INID", string.Format("{0}", userId));

            sql += "A.APPDEPT, INID_NAME(A.APPDEPT) INID_NAME, SUBSTR(TWN_DATE(B.APVTIME),1,5) APPTIME_YM, ";
            sql += "(SELECT DATA_DESC FROM PARAM_D WHERE DATA_NAME='M_STOREID' AND DATA_VALUE=:DATA_VALUE AND GRP_CODE = 'MI_MAST') DATA_DESC, ";
            p.Add(":DATA_VALUE", string.Format("{0}", showopt));
            sql += "SUM(B.APVQTY) as APVQTYN, ";
            sql += "(select AVG_PRICE from MI_WHCOST where DATA_YM = SUBSTR(TWN_DATE(B.APVTIME),1,5) and MMCODE=B.MMCODE) AVG_PRICE, ";
            sql += "(SELECT M_CONTPRICE FROM MI_MAST WHERE MMCODE=B.MMCODE) M_CONTPRICE, ";
            sql += "(SELECT M_CONTPRICE FROM MI_MAST WHERE MMCODE=B.MMCODE)*SUM(B.APVQTY) M_ALLPRICE ";
            sql += "from ME_DOCM A,ME_DOCD B,MI_MATCLASS C, PARAM_D D, MI_MAST E where A.DOCNO=B.DOCNO ";
            sql += "AND E.MMCODE = B.MMCODE ";

            sql += "AND A.APPDEPT = USER_INID(:INID2) "; //AB0056
            p.Add(":INID2", userId);

            if (showopt != "")  //庫備,非庫備
            {
                sql += "AND E.M_STOREID = :M_STOREID ";
                p.Add(":M_STOREID", string.Format("{0}", showopt));
            }
            if (task_id != "" && task_id != null)  //物料分類
            {
                string[] tmp = task_id.Split(',');
                sql += "AND C.MAT_CLASS IN :MAT_CLASS ";
                p.Add(":MAT_CLASS", tmp);
            }

            if (APPTIME1 != "" && APPTIME1 != null)
            {
                sql += "AND to_char(B.APVTIME,'yyyy-mm') >= Substr(:d0, 1, 7) AND SUBSTR(TWN_DATE(B.APVTIME),1,5) is NOT NULL ";
                p.Add(":d0", APPTIME1);
            }
            if (p1 != "" && p1 != null)
            {
                sql += "AND to_char(B.APVTIME,'yyyy-mm') <= Substr(:d1, 1, 7) ";
                p.Add(":d1", p1);
            }
            if (mmcode != "")    //院內碼
            {
                sql += "AND B.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("{0}", mmcode));
            }


            sql += " group by B.MMCODE, A.APPDEPT ,SUBSTR(TWN_DATE(B.APVTIME),1,5), C.MAT_CLSNAME";
            sql += " order by B.MMCODE, A.APPDEPT ,SUBSTR(TWN_DATE(B.APVTIME),1,5)";


            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0097_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        





        //AB0056
        public DataTable GetExcelByDept(string userId, string APPTIME1, string p1, string task_id, string mmcode, string showopt, string showdata)
        {
            //轉成需求格式 EX:10804
            string tmpTIME1 = "", tmpTIME2 = "";

            if (APPTIME1 != "" && APPTIME1 != null)
            {
                tmpTIME1 = Convert.ToDateTime(APPTIME1.Substring(4, 11)).ToString("yyyyMM");
                int Intyear1 = Convert.ToInt32(tmpTIME1.Substring(0, 4)) - 1911;
                APPTIME1 = Convert.ToString(Intyear1) + tmpTIME1.Substring(4, 2);
            }
            else
            {
                APPTIME1 = "";
            }
            if (p1 != "" && p1 != null)
            {
                tmpTIME2 = Convert.ToDateTime(p1.Substring(4, 11)).ToString("yyyyMM");
                int Intyear2 = Convert.ToInt32(tmpTIME2.Substring(0, 4)) - 1911;
                p1 = Convert.ToString(Intyear2) + tmpTIME2.Substring(4, 2);
            }
            else
            {
                p1 = "";
            }

            //MI_MATCLASS 有物料分類名稱
            DynamicParameters p = new DynamicParameters();


            var sql = @"select B.MMCODE as 院內碼, (SELECT MMNAME_C FROM MI_MAST WHERE MMCODE=B.MMCODE) as 中文品名, 
                      (SELECT MMNAME_E FROM MI_MAST WHERE MMCODE=B.MMCODE) as 英文品名, 
                       C.MAT_CLSNAME as 分類名稱, 
                      (SELECT BASE_UNIT FROM MI_MAST WHERE MMCODE=B.MMCODE) as 計量單位, ";

            sql += "A.APPDEPT as 單位代碼, INID_NAME(A.APPDEPT) as 單位名稱, ";
            if (showdata == "1") //明細才會顯示在報表
            {
                sql += "SUBSTR(TWN_DATE(B.APVTIME),1,5) as 年月, ";
            }
            sql += "SUM(B.APVQTY) as 總核撥量, ";
            if (showdata == "1")//明細才會顯示在報表
            {
                sql += "(select AVG_PRICE from MI_WHCOST where DATA_YM = SUBSTR(TWN_DATE(B.APVTIME),1,5) and MMCODE=B.MMCODE) as 庫存單價, ";
                sql += "(SELECT M_CONTPRICE FROM MI_MAST WHERE MMCODE=B.MMCODE) as 合約單價, ";
            }
            sql += "(SELECT M_CONTPRICE FROM MI_MAST WHERE MMCODE=B.MMCODE)*SUM(B.APVQTY) as 合約金額 ";
            sql += "from ME_DOCM A,ME_DOCD B,MI_MATCLASS C, PARAM_D D, MI_MAST E where A.DOCNO=B.DOCNO ";
            sql += "AND E.MMCODE = B.MMCODE ";
            sql += "AND A.APPDEPT = USER_INID(:INID2) ";  //AB0056
            p.Add(":INID2", userId);


            if (showopt != "")  //庫備,非庫備
            {
                sql += "AND E.M_STOREID = :M_STOREID ";
                p.Add(":M_STOREID", showopt); //"1")
            }
            if (task_id != "")  //物料分類
            {
                string[] tmp = task_id.Split(',');
                sql += "AND C.MAT_CLASS IN :MAT_CLASS ";
                p.Add(":MAT_CLASS", tmp);
            }
            //tmpTIME1 格式: yyyyMM
            if (tmpTIME1 != "" && tmpTIME1 != null)
            {
                sql += "AND to_char(B.APVTIME,'yyyymm') >= Substr(:d0, 1, 6) AND SUBSTR(TWN_DATE(B.APVTIME),1,5) is NOT NULL ";
                p.Add(":d0", tmpTIME1);
            }
            if (tmpTIME2 != "" && tmpTIME2 != null)
            {
                sql += "AND to_char(B.APVTIME,'yyyymm') <= Substr(:d1, 1, 6) ";
                p.Add(":d1", tmpTIME2);
            }
            if (mmcode != "")    //院內碼
            {
                sql += "AND B.MMCODE LIKE :p1 ";
                p.Add(":p1", mmcode); //"08900386"
            }
            sql += "group by B.MMCODE, A.APPDEPT ,SUBSTR(TWN_DATE(B.APVTIME),1,5), C.MAT_CLSNAME ";
            sql += "order by B.MMCODE, A.APPDEPT ,SUBSTR(TWN_DATE(B.APVTIME),1,5)";

            DataTable dt = new DataTable();


            DynamicParameters p2 = new DynamicParameters();
            var sql2 = @"select Trim(INID_NAME(USER_INID(:USRID))) as INID_NAME_USER from PARAM_D where rownum = 1";
            var title = "";


            if (showdata == "1")
            {//明細
                title = DBWork.Connection.ExecuteScalar(sql2, new { USRID = userId }, DBWork.Transaction).ToString()
                    + APPTIME1 + "至" + p1 + "品項分配明細報表";
            }
            else if (showdata == "2")
            { //彙整
                title = DBWork.Connection.ExecuteScalar(sql2, new { USRID = userId }, DBWork.Transaction).ToString()
                    + APPTIME1 + "至" + p1 + "品項分配彙整報表";
            }
            dt.Columns.Add(title);


            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        

        //AB0056
        public IEnumerable<AB0097_MODEL> GetPrintDataByDept(string userId, string APPTIME1, string p1, string task_id, string mmcode, string showopt, string showdata)
        {
            //轉成需求格式 EX:10804
            string tmpTIME1 = "", tmpTIME2 = "";

            if (APPTIME1 != "" && APPTIME1 != null)
            {
                tmpTIME1 = Convert.ToDateTime(APPTIME1.Substring(4, 11)).ToString("yyyyMM");

            }

            if (p1 != "" && p1 != null)
            {
                tmpTIME2 = Convert.ToDateTime(p1.Substring(4, 11)).ToString("yyyyMM");
            }

            var p = new DynamicParameters();

            var sql = @"select ROWNUM SEQ , total.* from (select B.MMCODE, (SELECT MMNAME_C FROM MI_MAST WHERE MMCODE=B.MMCODE) MMNAME_C, 
                      (SELECT MMNAME_E FROM MI_MAST WHERE MMCODE=B.MMCODE) MMNAME_E, 
                       C.MAT_CLSNAME, 
                      (SELECT BASE_UNIT FROM MI_MAST WHERE MMCODE=B.MMCODE) BASE_UNIT, ";

            sql += "TRIM(INID_NAME(USER_INID(:INID))) INID_NAME_USER, ";
            p.Add(":INID", string.Format("{0}", userId));

            sql += "A.APPDEPT, TRIM(INID_NAME(A.APPDEPT)) INID_NAME, SUBSTR(TWN_DATE(B.APVTIME),1,5) APPTIME_YM, ";
            sql += "(SELECT DATA_DESC FROM PARAM_D WHERE DATA_NAME='M_STOREID' AND DATA_VALUE=:DATA_VALUE AND GRP_CODE = 'MI_MAST') DATA_DESC, ";
            p.Add(":DATA_VALUE", string.Format("{0}", showopt));
            sql += "SUM(B.APVQTY) as APVQTYN, ";
            sql += "(select AVG_PRICE from MI_WHCOST where DATA_YM = SUBSTR(TWN_DATE(B.APVTIME),1,5) and MMCODE=B.MMCODE) AVG_PRICE, ";
            sql += "(SELECT M_CONTPRICE FROM MI_MAST WHERE MMCODE=B.MMCODE) M_CONTPRICE, ";
            sql += "(SELECT M_CONTPRICE FROM MI_MAST WHERE MMCODE=B.MMCODE)*SUM(B.APVQTY) M_ALLPRICE ";
            sql += "from ME_DOCM A,ME_DOCD B,MI_MATCLASS C, PARAM_D D, MI_MAST E where A.DOCNO=B.DOCNO ";
            sql += "AND E.MMCODE = B.MMCODE ";

            sql += "AND A.APPDEPT = USER_INID(:INID2) "; //AB0056
            p.Add(":INID2", userId);

            if (showopt != "")  //庫備,非庫備
            {
                sql += "AND E.M_STOREID = :M_STOREID ";
                p.Add(":M_STOREID", string.Format("{0}", showopt));
            }
            if (task_id != "")  //物料分類
            {
                string[] tmp = task_id.Split(',');
                sql += "AND C.MAT_CLASS IN :MAT_CLASS ";
                p.Add(":MAT_CLASS", tmp);
            }
            //tmpTIME1 格式: yyyyMM
            if (tmpTIME1 != "" && tmpTIME1 != null)
            {
                sql += "AND to_char(B.APVTIME,'yyyymm') >= Substr(:d0, 1, 6) AND SUBSTR(TWN_DATE(B.APVTIME),1,5) is NOT NULL ";
                p.Add(":d0", tmpTIME1);
            }
            if (tmpTIME2 != "" && tmpTIME2 != null)
            {
                sql += "AND to_char(B.APVTIME,'yyyymm') <= Substr(:d1, 1, 6) ";
                p.Add(":d1", tmpTIME2);
            }
            if (mmcode != "")    //院內碼
            {
                sql += "AND B.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("{0}", mmcode));
            }

            sql += "group by B.MMCODE, A.APPDEPT ,SUBSTR(TWN_DATE(B.APVTIME),1,5), C.MAT_CLSNAME ";
            sql += "order by B.MMCODE, A.APPDEPT ,SUBSTR(TWN_DATE(B.APVTIME),1,5)) total";


            return DBWork.Connection.Query<AB0097_MODEL>(sql, p, DBWork.Transaction);
        }

        public string INID_NAME_USER(string userId)
        {
            DynamicParameters p = new DynamicParameters();
            var sql = @"select Trim(INID_NAME(USER_INID(:USRID))) as INID_NAME_USER from PARAM_D where rownum = 1";
            return DBWork.Connection.ExecuteScalar(sql, new { USRID = userId }, DBWork.Transaction).ToString();
        }

        public string CheckMiWhid(string userId) //確定是否為什麼屬性，如果是2則就要3~8
        {
            string sql = @"SELECT TASK_ID from MI_WHID where WH_USERID=:WH_USERID and WH_NO = WHNO_MM1";
            return DBWork.Connection.ExecuteScalar(sql, new { WH_USERID = userId }, DBWork.Transaction).ToString();
        }

        
        public IEnumerable<ComboModel> GetMatClassOrigin()
        {
            string sql = @"SELECT MAT_CLASS AS VALUE,  
                        MAT_CLASS || ' ' || MAT_CLSNAME AS TEXT 
                        FROM MI_MATCLASS WHERE MAT_CLSID IN ('1','2','3')    
                        ORDER BY MAT_CLASS";
            return DBWork.Connection.Query<ComboModel>(sql, DBWork.Transaction);
        }
        public IEnumerable<COMBO_MODEL> GetMatclassCombo()
        {
            string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
                        FROM MI_MATCLASS WHERE MAT_CLSID IN ('1','2','3')   
                        ORDER BY MAT_CLASS";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<MI_MAST> GetMMCODECombo(string mmcode, string task_id, string store_id, int page_index, int page_size, string sorters)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"SELECT DISTINCT {0} MMCODE , MMNAME_C, MMNAME_E from MI_MAST A WHERE 1=1 ";


            if (task_id != "" && task_id != null)  //物料分類
            {
                string[] tmp = task_id.Split(',');
                sql += "AND MAT_CLASS IN :mat_class ";
                p.Add(":mat_class", tmp);
            }

            if (store_id != "" && store_id != null)  //庫備或是非庫備
            {
                sql += "AND M_STOREID =:M_STOREID ";
                p.Add(":M_STOREID", store_id);
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
        public string getDeptName()
        {
            string sql = @" SELECT  INID_NAME AS USER_DEPTNAME
                            FROM    UR_INID
                            WHERE   INID = (select INID from UR_ID where TUSER = (:userID)) ";

            var str = DBWork.Connection.ExecuteScalar(sql, new { userID = DBWork.ProcUser }, DBWork.Transaction);
            return str == null ? "" : str.ToString();

            //return DBWork.Connection.ExecuteScalar(sql, new { userID = DBWork.ProcUser }, DBWork.Transaction).ToString();
        }
    } // ec
} // 