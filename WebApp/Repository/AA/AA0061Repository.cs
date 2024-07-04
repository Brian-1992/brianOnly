using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using TSGH.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{

    public class AA0061_MODEL : JCLib.Mvc.BaseModel
    {
        public string SEQ { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string MAT_CLSNAME { get; set; }
        public string BASE_UNIT { get; set; }
        public string INID_NAME_USER { get; set; }
        public string APPDEPT { get; set; }
        public string INID_NAME { get; set; }
        public string APPTIME_YM { get; set; }
        public string M_STOREID { get; set; }
        public Int64 APVQTYN { get; set; }
        public string AVG_PRICE { get; set; }
        public string M_CONTPRICE { get; set; }
        public Int64 M_ALLPRICE { get; set; }


        public string TOWH { get; set; }
        public string TOWH_NAME { get; set; }
    }
    public class AA0061Repository : JCLib.Mvc.BaseRepository
    {

        public AA0061Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        #region AA0061
        public IEnumerable<AA0061_MODEL> GetAll(string userId, string APPTIME1, string APPTIME2, string task_id, string mmcode, string showopt, string showdata, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.MMCODE,A.MMNAME_C,A.MMNAME_E,C.MAT_CLSNAME,A.BASE_UNIT, 
                               INID_NAME(USER_INID(:INID)) INID_NAME_USER,
                               B.towh,WH_NAME(B.towh) towh_name,
                               B.APPTIME_YM, 
                               DECODE(A.M_STOREID,'0','非庫備','1','庫備',A.M_STOREID) DATA_DESC, 
                               B.APVQTYN,D.AVG_PRICE,A.M_CONTPRICE,ROUND(A.M_CONTPRICE*B.APVQTYN,2) M_ALLPRICE 
                          FROM MI_MAST A , 
                               (SELECT SUBSTR(TWN_DATE(B.DIS_TIME),1,5) APPTIME_YM, 
                                       A.towh,B.MMCODE,SUM(B.APVQTY) APVQTYN 
                                  FROM ME_DOCM A,ME_DOCD B 
                                 WHERE a.frwh = WHNO_MM1
                                   and A.DOCNO=B.DOCNO 
                                   and a.doctype in ('MR1', 'MR2', 'MR3', 'MR4', 'MR5', 'MR6')";

            p.Add(":INID", userId);
            if (APPTIME1 != "" && APPTIME1 != null)
            {
                sql += "           AND to_char(B.DIS_TIME,'yyyy-mm') >= Substr(:d0, 1, 7)" +
                    "              AND SUBSTR(TWN_DATE(B.DIS_TIME),1,5) is NOT NULL ";
                p.Add(":d0", APPTIME1);
            }
            if (APPTIME2 != "" && APPTIME2 != null)
            {
                sql += "           AND to_char(B.DIS_TIME,'yyyy-mm') <= Substr(:d1, 1, 7) ";
                p.Add(":d1", APPTIME2);
            }



            sql += @"            GROUP BY B.MMCODE, A.towh ,SUBSTR(TWN_DATE(B.DIS_TIME),1,5)  
                                 ORDER BY B.MMCODE, A.towh ,SUBSTR(TWN_DATE(B.DIS_TIME), 1, 5) 
                               ) B, MI_MATCLASS C, MI_WHCOST D 
                         WHERE A.MMCODE = B.MMCODE 
                           AND A.MAT_CLASS = C.MAT_CLASS 
                           AND B.APPTIME_YM = D.DATA_YM 
                           AND B.MMCODE = D.MMCODE ";

            if (showopt != "")  //庫備,非庫備
            {
                sql += "   AND A.M_STOREID = :M_STOREID ";
                p.Add(":M_STOREID", string.Format("{0}", showopt));
            }
            if (task_id != "" && task_id != null)  //物料分類
            {
                string[] tmp = task_id.Split(',');
                sql += "   AND A.MAT_CLASS IN :MAT_CLASS ";
                p.Add(":MAT_CLASS", tmp);
            }
            else
            {
                string getCheckMiWhid = CheckMiWhid(userId);
                if (getCheckMiWhid == "ALL")
                    sql += "   AND A.MAT_CLASS BETWEEN '02' and '08' ";
                else if (getCheckMiWhid == "2")
                    sql += "   AND A.MAT_CLASS = '2' ";
                else if (getCheckMiWhid == "3")
                    sql += "   AND A.MAT_CLASS BETWEEN '03' and '08' ";
                else // none
                    sql += "   AND 1=0 "; 
            }
            if (mmcode != "")    //院內碼
            {
                sql += "   AND A.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("{0}", mmcode));
            }
            sql += "     ORDER by A.MMCODE, B.TOWH, B.APPTIME_YM ";


            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.PagingQuery<AA0061_MODEL>(sql, p, DBWork.Transaction);
        }

        public DataTable GetExcel(string userId, string APPTIME1, string APPTIME2, string task_id, string mmcode, string showopt, string showdata)
        {
            //轉成需求格式 EX:10804 ; tmpTIME1 格式: yyyyMM 與查詢All方式不同
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
            if (APPTIME2 != "" && APPTIME2 != null)
            {
                tmpTIME2 = Convert.ToDateTime(APPTIME2.Substring(4, 11)).ToString("yyyyMM");
                int Intyear2 = Convert.ToInt32(tmpTIME2.Substring(0, 4)) - 1911;
                APPTIME2 = Convert.ToString(Intyear2) + tmpTIME2.Substring(4, 2);
            }
            else
            {
                APPTIME2 = "";
            }


            //MI_MATCLASS 有物料分類名稱
            DynamicParameters p = new DynamicParameters();

            var sql = @"SELECT A.MMCODE as 院內碼,
                               A.MMNAME_C as 中文品名,
                               A.MMNAME_E as 英文品名,
                               C.MAT_CLSNAME as 分類名稱,
                               A.BASE_UNIT as 計量單位, 
                               B.towh as 入庫庫房,
                               WH_NAME(B.towh) as 入庫庫房名稱, ";

            if (showdata == "1")   //明細才會顯示在報表
            {
                sql += "       B.APPTIME_YM as 年月, ";
            }
            sql += "           B.APVQTYN as 總核撥量, ";
            if (showdata == "1")   //明細才會顯示在報表
            {
                sql += "       D.AVG_PRICE as 庫存單價, ";
                sql += "       A.M_CONTPRICE as 合約單價, ";
            }

            sql += @"          ROUND(A.M_CONTPRICE*B.APVQTYN,2) as 合約金額  
                          FROM MI_MAST A, 
                               (SELECT SUBSTR(TWN_DATE(B.DIS_TIME),1,5) APPTIME_YM, 
                                       A.towh, 
                                       B.MMCODE,
                                       SUM(B.APVQTY) APVQTYN 
                                  FROM ME_DOCM A, ME_DOCD B 
                                 WHERE a.frwh = WHNO_MM1
                                   and A.DOCNO=B.DOCNO 
                                   and a.doctype in ('MR1', 'MR2', 'MR3', 'MR4','MR5','MR6')";

            //tmpTIME1 格式: yyyyMM 與查詢All方式不同
            if (tmpTIME1 != "" && tmpTIME1 != null)
            {
                sql += "           AND to_char(B.DIS_TIME,'yyyymm') >= Substr(:d0, 1, 6) " +
                    "              AND SUBSTR(TWN_DATE(B.DIS_TIME),1,5) is NOT NULL ";
                p.Add(":d0", tmpTIME1);
            }
            if (tmpTIME2 != "" && tmpTIME2 != null)
            {
                sql += "           AND to_char(B.DIS_TIME,'yyyymm') <= Substr(:d1, 1, 6) ";
                p.Add(":d1", tmpTIME2);
            }
            
            sql += @"            GROUP BY B.MMCODE, A.towh ,SUBSTR(TWN_DATE(B.DIS_TIME),1,5)  
                                 ORDER BY B.MMCODE, A.towh ,SUBSTR(TWN_DATE(B.DIS_TIME), 1, 5) 
                                ) B, MI_MATCLASS C, MI_WHCOST D 
                          WHERE A.MMCODE = B.MMCODE 
                            AND A.MAT_CLASS = C.MAT_CLASS 
                            AND B.APPTIME_YM = D.DATA_YM 
                            AND B.MMCODE = D.MMCODE ";

            if (showopt != "")  //庫備,非庫備
            {
                sql += "    AND A.M_STOREID = :M_STOREID ";
                p.Add(":M_STOREID", string.Format("{0}", showopt));
            }
            if (task_id != "" && task_id != null)  //物料分類
            {
                string[] tmp = task_id.Split(',');
                sql += "    AND A.MAT_CLASS IN :MAT_CLASS ";
                p.Add(":MAT_CLASS", tmp);
            }
            else
            {
                string getCheckMiWhid = CheckMiWhid(userId);
                if (getCheckMiWhid == "ALL")
                    sql += "   AND A.MAT_CLASS BETWEEN '02' and '08' ";
                else if (getCheckMiWhid == "2")
                    sql += "   AND A.MAT_CLASS = '2' ";
                else if (getCheckMiWhid == "3")
                    sql += "   AND A.MAT_CLASS BETWEEN '03' and '08' ";
                else // none
                    sql += "   AND 1=0 ";
            }
            if (mmcode != "")    //院內碼
            {
                sql += "    AND A.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("{0}", mmcode));
            }
            sql += "      ORDER by A.MMCODE, B.TOWH, B.APPTIME_YM ";

            DataTable dt = new DataTable();


            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;


        }

        public IEnumerable<AA0061_MODEL> GetPrintData(string userId, string APPTIME1, string APPTIME2, string task_id, string mmcode, string showopt, string showdata)
        {
            //轉成需求格式 EX:10804
            string tmpTIME1 = "", tmpTIME2 = "";

            if (APPTIME1 != "" && APPTIME1 != null)
            {
                tmpTIME1 = Convert.ToDateTime(APPTIME1.Substring(4, 11)).ToString("yyyyMM");

            }

            if (APPTIME2 != "" && APPTIME2 != null)
            {
                tmpTIME2 = Convert.ToDateTime(APPTIME2.Substring(4, 11)).ToString("yyyyMM");
            }



            var p = new DynamicParameters();

            var sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, C.MAT_CLSNAME, A.BASE_UNIT, 
                               INID_NAME(USER_INID(:INID)) INID_NAME_USER,
                               B.towh,WH_NAME(B.towh) towh_name,
                               B.APPTIME_YM, 
                               DECODE(A.M_STOREID,'0','非庫備','1','庫備',A.M_STOREID) DATA_DESC, 
                               B.APVQTYN,D.AVG_PRICE,A.M_CONTPRICE,ROUND(A.M_CONTPRICE*B.APVQTYN,2) M_ALLPRICE 
                          FROM MI_MAST A , 
                               (SELECT SUBSTR(TWN_DATE(B.DIS_TIME),1,5) APPTIME_YM, 
                                       A.towh, B.MMCODE, SUM(B.APVQTY) APVQTYN 
                                  FROM ME_DOCM A,ME_DOCD B 
                                 WHERE a.frwh = WHNO_MM1
                                   and A.DOCNO=B.DOCNO 
                                   and a.doctype in ('MR1', 'MR2', 'MR3', 'MR4','MR5','MR6')";

            p.Add(":INID", userId);
            if (tmpTIME1 != "" && tmpTIME1 != null)
            {
                sql += "           AND to_char(B.DIS_TIME,'yyyymm') >= Substr(:d0, 1, 6) " +
                    "              AND SUBSTR(TWN_DATE(B.DIS_TIME),1,5) is NOT NULL ";
                p.Add(":d0", tmpTIME1);
            }
            if (tmpTIME2 != "" && tmpTIME2 != null)
            {
                sql += "           AND to_char(B.DIS_TIME,'yyyymm') <= Substr(:d1, 1, 6) ";
                p.Add(":d1", tmpTIME2);
            }
            
            sql += @"            GROUP BY B.MMCODE, A.towh ,SUBSTR(TWN_DATE(B.DIS_TIME),1,5) 
                                 ORDER BY B.MMCODE, A.towh ,SUBSTR(TWN_DATE(B.DIS_TIME), 1, 5) 
                                ) B, MI_MATCLASS C, MI_WHCOST D 
                          WHERE A.MMCODE = B.MMCODE 
                            AND A.MAT_CLASS = C.MAT_CLASS 
                            AND B.APPTIME_YM = D.DATA_YM 
                            AND B.MMCODE = D.MMCODE ";

            if (showopt != "")  //庫備,非庫備
            {
                sql += "    AND A.M_STOREID = :M_STOREID ";
                p.Add(":M_STOREID", string.Format("{0}", showopt));
            }
            if (task_id != "" && task_id != null)  //物料分類
            {
                string[] tmp = task_id.Split(',');
                sql += "           AND A.MAT_CLASS IN :MAT_CLASS ";
                p.Add(":MAT_CLASS", tmp);
            }
            else
            {
                string getCheckMiWhid = CheckMiWhid(userId);
                if (getCheckMiWhid == "ALL")
                    sql += "   AND A.MAT_CLASS BETWEEN '02' and '08' ";
                else if (getCheckMiWhid == "2")
                    sql += "   AND A.MAT_CLASS = '2' ";
                else if (getCheckMiWhid == "3")
                    sql += "   AND A.MAT_CLASS BETWEEN '03' and '08' ";
                else // none
                    sql += "   AND 1=0 ";
            }
            if (mmcode != "")    //院內碼
            {
                sql += "    AND A.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("{0}", mmcode));
            }
            sql += "      ORDER by A.MMCODE, B.TOWH, B.APPTIME_YM ";


            return DBWork.Connection.Query<AA0061_MODEL>(sql, p, DBWork.Transaction);
        }

        #endregion

        #region AB0056

        public IEnumerable<AA0061_MODEL> GetAllByDept(string userId, string APPTIME1, string APPTIME2, string task_id, string mmcode, string showopt, string showdata, string depts)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, C.MAT_CLSNAME, A.BASE_UNIT, 
                               INID_NAME(USER_INID(:INID)) INID_NAME_USER,
                               B.towh,WH_NAME(B.towh) TOWH_NAME,
                               B.APPTIME_YM, 
                               DECODE(A.M_STOREID,'0','非庫備','1','庫備',A.M_STOREID) DATA_DESC, 
                               B.APVQTYN, D.AVG_PRICE, A.M_CONTPRICE, 
                               ROUND(A.M_CONTPRICE*B.APVQTYN,2) M_ALLPRICE 
                          FROM MI_MAST A , 
                               (SELECT SUBSTR(TWN_DATE(B.DIS_TIME),1,5) APPTIME_YM, 
                                       A.towh,B.MMCODE,SUM(B.APVQTY) APVQTYN 
                                  FROM ME_DOCM A,ME_DOCD B 
                                 WHERE a.doctype in ('MR1', 'MR2', 'MR3', 'MR4', 'MR5','MR6')
                                   and A.DOCNO=B.DOCNO  
                                    and A.APPDEPT = USER_INID(:INID2) "; //AB0056根據使用單位

            p.Add(":INID", userId);
            if (APPTIME1 != "" && APPTIME1 != null)
            {
                sql += "           AND to_char(B.DIS_TIME,'yyyy-mm') >= Substr(:d0, 1, 7) " +
                    "              AND SUBSTR(TWN_DATE(B.DIS_TIME),1,5) is NOT NULL ";
                p.Add(":d0", APPTIME1);
            }
            if (APPTIME2 != "" && APPTIME2 != null)
            {
                sql += "           AND to_char(B.DIS_TIME,'yyyy-mm') <= Substr(:d1, 1, 7) ";
                p.Add(":d1", APPTIME2);
            }
            sql += string.Format("      and a.towh in ( {0} )", depts);
            sql += @"            GROUP BY B.MMCODE, A.towh ,SUBSTR(TWN_DATE(B.DIS_TIME),1,5) 
                                 ORDER BY B.MMCODE, A.towh ,SUBSTR(TWN_DATE(B.DIS_TIME), 1, 5) 
                                ) B, MI_MATCLASS C, MI_WHCOST D 
                         WHERE A.MMCODE = B.MMCODE AND A.MAT_CLASS = C.MAT_CLASS AND B.APPTIME_YM = D.DATA_YM AND B.MMCODE = D.MMCODE ";

            p.Add(":INID2", userId);

            if (showopt != "")  //庫備,非庫備
            {
                sql += "   AND A.M_STOREID = :M_STOREID ";
                p.Add(":M_STOREID", string.Format("{0}", showopt));
            }
            if (task_id != "" && task_id != null)  //物料分類
            {
                string[] tmp = task_id.Split(',');
                sql += "   AND A.MAT_CLASS IN :MAT_CLASS ";
                p.Add(":MAT_CLASS", tmp);
            }
            else
            {
                sql += "   AND A.MAT_CLASS between '02' and '08' ";
            }
            if (mmcode != "")    //院內碼
            {
                sql += "   AND A.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("{0}", mmcode));
            }

            return DBWork.PagingQuery<AA0061_MODEL>(sql, p, DBWork.Transaction);
        }

        public DataTable GetExcelByDept(string userId, string APPTIME1, string APPTIME2, string task_id, string mmcode, string showopt, string showdata, string depts)
        {
            //轉成需求格式 EX:10804 ; tmpTIME1 格式: yyyyMM 與查詢All方式不同
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
            if (APPTIME2 != "" && APPTIME2 != null)
            {
                tmpTIME2 = Convert.ToDateTime(APPTIME2.Substring(4, 11)).ToString("yyyyMM");
                int Intyear2 = Convert.ToInt32(tmpTIME2.Substring(0, 4)) - 1911;
                APPTIME2 = Convert.ToString(Intyear2) + tmpTIME2.Substring(4, 2);
            }
            else
            {
                APPTIME2 = "";
            }


            //MI_MATCLASS 有物料分類名稱
            DynamicParameters p = new DynamicParameters();

            var sql = @"SELECT A.MMCODE as 院內碼,
                               A.MMNAME_C as 中文品名,
                               A.MMNAME_E as 英文品名,
                               C.MAT_CLSNAME as 分類名稱,
                               A.BASE_UNIT as 計量單位, 
                               B.towh as 入庫庫房,
                               WH_NAME(B.towh) as 入庫庫房名稱, ";

            if (showdata == "1")   //明細才會顯示在報表
            {
                sql += "        B.APPTIME_YM as 年月, ";
            }
            sql += "            B.APVQTYN as 總核撥量, ";
            if (showdata == "1")   //明細才會顯示在報表
            {
                sql += "        D.AVG_PRICE as 庫存單價, ";
                sql += "        A.M_CONTPRICE as 合約單價, ";
            }

            sql += @"           ROUND(A.M_CONTPRICE*B.APVQTYN,2) as 合約金額  
                           FROM MI_MAST A, 
                                (SELECT SUBSTR(TWN_DATE(B.DIS_TIME),1,5) APPTIME_YM, 
                                        A.towh, B.MMCODE, SUM(B.APVQTY) APVQTYN 
                                   FROM ME_DOCM A,ME_DOCD B 
                                  WHERE a.doctype in ('MR1', 'MR2', 'MR3', 'MR4','MR5','MR6')
                                    and A.DOCNO=B.DOCNO
                                    AND A.APPDEPT = USER_INID(:INID)";

            //tmpTIME1 格式: yyyyMM 與查詢All方式不同
            if (tmpTIME1 != "" && tmpTIME1 != null)
            {
                sql += "            AND to_char(B.DIS_TIME,'yyyymm') >= Substr(:d0, 1, 6) " +
                    "               AND SUBSTR(TWN_DATE(B.DIS_TIME),1,5) is NOT NULL ";
                p.Add(":d0", tmpTIME1);
            }
            if (tmpTIME2 != "" && tmpTIME2 != null)
            {
                sql += "            AND to_char(B.DIS_TIME,'yyyymm') <= Substr(:d1, 1, 6) ";
                p.Add(":d1", tmpTIME2);
            }
            sql += string.Format("  and a.towh in ( {0} )", depts);

            sql += @"             GROUP BY B.MMCODE, A.towh ,SUBSTR(TWN_DATE(B.DIS_TIME),1,5)  
                                  ORDER BY B.MMCODE, A.towh ,SUBSTR(TWN_DATE(B.DIS_TIME), 1, 5) 
                                ) B, MI_MATCLASS C, MI_WHCOST D 
                          WHERE A.MMCODE = B.MMCODE 
                            AND A.MAT_CLASS = C.MAT_CLASS 
                            AND B.APPTIME_YM = D.DATA_YM AND B.MMCODE = D.MMCODE ";

            p.Add(":INID", userId);

            if (showopt != "")  //庫備,非庫備
            {
                sql += "    AND A.M_STOREID = :M_STOREID ";
                p.Add(":M_STOREID", string.Format("{0}", showopt));
            }
            if (task_id != "" && task_id != null)  //物料分類
            {
                string[] tmp = task_id.Split(',');
                sql += "    AND A.MAT_CLASS IN :MAT_CLASS ";
                p.Add(":MAT_CLASS", tmp);
            }
            else
            {
                sql += "    AND A.MAT_CLASS between '02' and '08' ";
            }
            if (mmcode != "")    //院內碼
            {
                sql += "    AND A.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("{0}", mmcode));
            }


            DataTable dt = new DataTable();


            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<AA0061_MODEL> GetPrintDataByDept(string userId, string APPTIME1, string APPTIME2, string task_id, string mmcode, string showopt, string showdata, string depts)
        {
            //轉成需求格式 EX:10804
            string tmpTIME1 = "", tmpTIME2 = "";

            if (APPTIME1 != "" && APPTIME1 != null)
            {
                tmpTIME1 = Convert.ToDateTime(APPTIME1.Substring(4, 11)).ToString("yyyyMM");

            }

            if (APPTIME2 != "" && APPTIME2 != null)
            {
                tmpTIME2 = Convert.ToDateTime(APPTIME2.Substring(4, 11)).ToString("yyyyMM");
            }

            var p = new DynamicParameters();

            var sql = @"SELECT A.MMCODE,A.MMNAME_C,A.MMNAME_E,C.MAT_CLSNAME,A.BASE_UNIT, 
                               INID_NAME(USER_INID(:INID)) INID_NAME_USER,
                               B.towh,WH_NAME(B.towh) towh_name,
                               B.APPTIME_YM, 
                               DECODE(A.M_STOREID,'0','非庫備','1','庫備',A.M_STOREID) DATA_DESC, 
                               B.APVQTYN,D.AVG_PRICE,A.M_CONTPRICE,ROUND(A.M_CONTPRICE*B.APVQTYN,2) M_ALLPRICE 
                          FROM MI_MAST A , 
                               (SELECT SUBSTR(TWN_DATE(B.DIS_TIME),1,5) APPTIME_YM, 
                                       A.towh,B.MMCODE,SUM(B.APVQTY) APVQTYN
                                  FROM ME_DOCM A, ME_DOCD B 
                                 WHERE a.doctype in ('MR1', 'MR2', 'MR3', 'MR4','MR5','MR6')
                                   and A.DOCNO=B.DOCNO 
                                    AND A.APPDEPT = USER_INID(:INID2)";

            p.Add(":INID", userId);
            if (tmpTIME1 != "" && tmpTIME1 != null)
            {
                sql += "          AND to_char(B.DIS_TIME,'yyyymm') >= Substr(:d0, 1, 6) " +
                    "             AND SUBSTR(TWN_DATE(B.DIS_TIME),1,5) is NOT NULL ";
                p.Add(":d0", tmpTIME1);
            }
            if (tmpTIME2 != "" && tmpTIME2 != null)
            {
                sql += "          AND to_char(B.DIS_TIME,'yyyymm') <= Substr(:d1, 1, 6) ";
                p.Add(":d1", tmpTIME2);
            }

            sql += string.Format("  and a.towh in ( {0} )", depts);

            sql += @"           GROUP BY B.MMCODE, A.towh ,SUBSTR(TWN_DATE(B.DIS_TIME),1,5) 
                                ORDER BY B.MMCODE, A.towh ,SUBSTR(TWN_DATE(B.DIS_TIME), 1, 5) 
                              ) B, MI_MATCLASS C, MI_WHCOST D 
                        WHERE A.MMCODE = B.MMCODE 
                          AND A.MAT_CLASS = C.MAT_CLASS 
                          AND B.APPTIME_YM = D.DATA_YM 
                          AND B.MMCODE = D.MMCODE  "; //AB0056根據使用單位

            //sql += string.Format("  and b.towh in ( {0} )", depts);
            p.Add(":INID2", userId);

            if (showopt != "")  //庫備,非庫備
            {
                sql += "  AND A.M_STOREID = :M_STOREID ";
                p.Add(":M_STOREID", string.Format("{0}", showopt));
            }
            if (task_id != "" && task_id != null)  //物料分類
            {
                string[] tmp = task_id.Split(',');
                sql += "  AND A.MAT_CLASS IN :MAT_CLASS ";
                p.Add(":MAT_CLASS", tmp);
            }
            else
            {
                sql += "  and a.mat_class between '02' and '08'";
            }
            if (mmcode != "")    //院內碼
            {
                sql += "  AND A.MMCODE LIKE :p1 ";
                p.Add(":p1", string.Format("{0}", mmcode));
            }


            return DBWork.Connection.Query<AA0061_MODEL>(sql, p, DBWork.Transaction);
        }

        #endregion
        
        public String GetExcelTitle(string userId, string APPTIME1, string APPTIME2, string showdata)
        {
            //轉成需求格式 EX:10804 ; tmpTIME1 格式: yyyyMM 與查詢All方式不同
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
            if (APPTIME2 != "" && APPTIME2 != null)
            {
                tmpTIME2 = Convert.ToDateTime(APPTIME2.Substring(4, 11)).ToString("yyyyMM");
                int Intyear2 = Convert.ToInt32(tmpTIME2.Substring(0, 4)) - 1911;
                APPTIME2 = Convert.ToString(Intyear2) + tmpTIME2.Substring(4, 2);
            }
            else
            {
                APPTIME2 = "";
            }

            DynamicParameters p = new DynamicParameters();
            var sql = @"select Trim(INID_NAME(USER_INID(:USRID))) as INID_NAME_USER from PARAM_D where rownum = 1";
            var title = "";


            if (showdata == "1")
            {//明細
                title = DBWork.Connection.ExecuteScalar(sql, new { USRID = userId }, DBWork.Transaction).ToString()
                    + APPTIME1 + "至" + APPTIME2 + "品項分配明細報表";
            }
            else if (showdata == "2")
            { //彙整
                title = DBWork.Connection.ExecuteScalar(sql, new { USRID = userId }, DBWork.Transaction).ToString()
                    + APPTIME1 + "至" + APPTIME2 + "品項分配彙整報表";
            }
            return title;
        }

        //若有VIEWALL,有2或3的權限,則2跟3都可以使用(回傳ALL);都沒有則回傳none
        //(有些人會一下管衛材,一下管一般物品,也有人會互相代理)
        public string CheckMiWhid(string userId) 
        {
            string sql = @" SELECT case when (select count(*) from UR_UIR where TUSER = :WH_USERID and RLNO = 'VIEWALL') > 0 then 'ALL'
                                         when (select count(*) from MI_WHID 
                                                where WH_USERID=:WH_USERID and WH_NO = WHNO_MM1 and TASK_ID in('2','3')) > 0
                                              then 'ALL'
                                         else 'none'
                                     end 
                              from dual  ";
            return DBWork.Connection.ExecuteScalar(sql, new { WH_USERID = userId }, DBWork.Transaction).ToString();
        }

        public IEnumerable<COMBO_MODEL> GetMatClassCombo_S()
        {
            var p = new DynamicParameters();

            string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM
                        FROM MI_MATCLASS WHERE ";

            string[] tmp = "2,3".Split(',');
            sql += "MAT_CLSID IN :mat_class ";
            p.Add(":mat_class", tmp);

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<ComboModel> GetMatClassOrigin(string task_id)
        {
            // task_id = 2: 可顯示MAT_CLSID = 2; task_id = 3: 可顯示MAT_CLSID = 3; 
            // task_id = ALL: 可顯示MAT_CLSID = 2&3;

            string addSql = "";
            if (task_id == "ALL")
                addSql = "or MAT_CLSID in ('2', '3')";

            var p = new DynamicParameters();
            string sql = @"SELECT DISTINCT MAT_CLASS as VALUE, " +
                "MAT_CLASS || ' ' || MAT_CLSNAME as COMBITEM FROM MI_MATCLASS " +
                " WHERE MAT_CLSID = (case when :task_id = '2' then '2' when :task_id = '3' then '3' else '' end) " + addSql;

            p.Add(":task_id", task_id);

            return DBWork.Connection.Query<ComboModel>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<ComboModel> GetMatClassMedic(string matclass)
        {
            string sql = @"SELECT DISTINCT MAT_CLASS as VALUE, " +
                "MAT_CLASS || ' ' || MAT_CLSNAME as COMBITEM FROM MI_MATCLASS WHERE MAT_CLASS  = '02'";
            return DBWork.Connection.Query<ComboModel>(sql, DBWork.Transaction);
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
            else
            {
                //表示一般物品人員選單
                sql += "AND MAT_CLASS BETWEEN '03' and '08'";
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

        public IEnumerable<string> GetUserWhnos(string userid) {
            string sql = @"select A.WH_NO
                             from MI_WHMAST A
                            WHERE WH_KIND='1'
                              AND EXISTS(SELECT 'X' FROM UR_ID B WHERE ( A.SUPPLY_INID=B.INID OR A.INID=B.INID ) AND TUSER=:userid)
                              AND NOT EXISTS(SELECT 'X' FROM MI_WHID B WHERE TASK_ID IN ('2','3') AND WH_USERID=:userid)
                        UNION ALL 
                           SELECT A.WH_NO
                             FROM MI_WHMAST A,MI_WHID B
                            WHERE A.WH_NO=B.WH_NO AND TASK_ID IN ('2','3') AND WH_USERID=:userid
                            ORDER BY 1";

            return DBWork.Connection.Query<string>(sql, new { userid = userid}, DBWork.Transaction);
        }
    }
}