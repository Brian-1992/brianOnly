using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using BarcodeLib;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace WebApp.Repository.AA
{
    public class AA0013Report_MODEL : JCLib.Mvc.BaseModel
    {
        public string DOCNO { get; set; }
        public string FLOWID { get; set; }
        public string APPID { get; set; }
        public string APPDEPT { get; set; }
        public string APPLY_KIND { get; set; }
        public string MAT_CLASS { get; set; }
        public string SEQ { get; set; }
        public string LIST_ID { get; set; }
        public string DIS_USER { get; set; }
        public string MMCODE { get; set; }
        public Int32 APPQTY { get; set; }
        public Int32 APVQTY { get; set; }
        public string ACKQTY { get; set; }
        public Int32 EXPT_DISTQTY { get; set; }
        public string BW_MQTY { get; set; }
        public string APLYITEM_NOTE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT { get; set; }
        public string M_STOREID { get; set; }
        public string FLOWID_N { get; set; }
        public string APPDEPT_N { get; set; }
        public string APPLY_KIND_N { get; set; }
        public string MATCLASS_N { get; set; }
        public string M_STOREID_N { get; set; }
        public string APPID_N { get; set; }
        public string DIS_USER_N { get; set; }
        public string PRINT_USER_N { get; set; }
        public string LIST_ID_N { get; set; }
        public string DOCNO_BarCode { get; set; }
        public string MAT_CLSNAME { get; set; }
        public string APPTIME { get; set; }
        public string TOWH_NAME { get; set; }
        public string FRWH_NAME { get; set; }
        public string STORELOC { get; set; }
    }


    public class AA0013Repository : JCLib.Mvc.BaseRepository
    {
        public AA0013Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public int Update_ME_DOCM_ListID_To_Y(string[] StrDOCNO)
        {
            var sql = @"UPDATE ME_DOCM SET LIST_ID='Y' WHERE DOCNO IN :DOCNO";
            return DBWork.Connection.Execute(sql, new { DOCNO = StrDOCNO }, DBWork.Transaction);
        }

        public IEnumerable<AA0013Report_MODEL> GetAllM(bool isGas, string[] str_APPDEPT, string[] str_MAT_CLASS, string AppTime_Start, string AppTime_End, string[] str_FLOWID, string[] str_DOCTYPE, string DIS_TIME_Start, string DIS_TIME_End, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"SELECT A.DOCNO, A.FLOWID, A.APPID, A.APPDEPT, A.APPLY_KIND, A.MAT_CLASS
                        ,(SELECT DISTINCT RTrim(DATA_DESC) FROM PARAM_D WHERE DATA_NAME = 'APPLY_KIND' AND DATA_VALUE = A.APPLY_KIND) AS APPLY_KIND_N
                        ,(SELECT DISTINCT RTrim(DATA_VALUE || ' ' || DATA_DESC) FROM PARAM_D WHERE DATA_NAME = 'FLOWID_MR1' AND DATA_VALUE = A.FLOWID) AS FLOWID_N
                        ,(SELECT RTrim(WH_NO || ' ' || WH_NAME) FROM MI_WHMAST WHERE WH_NO = A.TOWH) AS APPDEPT_N                        
                        ,(SELECT RTrim(MAT_CLASS || ' ' || MAT_CLSNAME) FROM MI_MATCLASS WHERE MAT_CLASS = A.MAT_CLASS) AS MATCLASS_N                        
                        ,(SELECT RTrim(MAT_CLSNAME) FROM MI_MATCLASS WHERE MAT_CLASS = A.MAT_CLASS) AS MAT_CLSNAME    
                        ,TWN_DATE(A.APPTIME) APPTIME
                        ,A.TOWH || ' ' || WH_NAME(A.TOWH) TOWH_NAME
                        ,A.FRWH || ' ' || WH_NAME(A.FRWH) FRWH_NAME
                        ,(CASE WHEN A.LIST_ID = 'Y' THEN '已列印' ELSE '未列印' END) AS LIST_ID_N 
                        FROM ME_DOCM A WHERE 1 = 1 ";

            if (isGas)
            {
                sql += @"   and A.MAT_CLASS in ( '09') and A.DOCTYPE in ('AIR')";
            }
            else
            {
                sql += @"   and A.MAT_CLASS in ( '02','03','04','05','06','07','08') 
                            and A.DOCTYPE in ('MR1','MR2','MR3','MR4')";
            }

            if (str_APPDEPT.Length > 0)
            {
                string sql_APPDEPT = "";
                sql += @" AND (";
                foreach (string tmp_APPDEPT in str_APPDEPT)
                {
                    if (string.IsNullOrEmpty(sql_APPDEPT))
                    {
                        sql_APPDEPT = @"A.TOWH = '" + tmp_APPDEPT + "'";
                    }
                    else
                    {
                        sql_APPDEPT += @" OR A.TOWH = '" + tmp_APPDEPT + "'";
                    }
                }
                sql += sql_APPDEPT + ") ";
            }
            if (str_MAT_CLASS.Length > 0)
            {
                string sql_MAT_CLASS = "";
                sql += @" AND (";
                foreach (string tmp_MAT_CLASS in str_MAT_CLASS)
                {
                    if (string.IsNullOrEmpty(sql_MAT_CLASS))
                    {
                        sql_MAT_CLASS = @"A.MAT_CLASS = '" + tmp_MAT_CLASS + "'";
                    }
                    else
                    {
                        sql_MAT_CLASS += @" OR A.MAT_CLASS = '" + tmp_MAT_CLASS + "'";
                    }
                }
                sql += sql_MAT_CLASS + ") ";
            }
            
            if (!string.IsNullOrEmpty(AppTime_Start) && !string.IsNullOrEmpty(AppTime_End))
            {
                //sql += " AND SUBSTR(A.APPTIME,1,10) BETWEEN TO_DATE(:AppTime_Start,'YYYY-MM-DD') AND TO_DATE(:AppTime_End,'YYYY-MM-DD') ";
                sql += " AND TWN_DATE(A.APPTIME) BETWEEN :AppTime_Start AND :AppTime_End ";
                p.Add(":AppTime_Start", AppTime_Start);
                p.Add(":AppTime_End", AppTime_End);
            }
            if (!string.IsNullOrEmpty(AppTime_Start) && string.IsNullOrEmpty(AppTime_End))
            {
                //sql += " AND SUBSTR(A.APPTIME,1,10) >= TO_DATE(:AppTime_Start,'YYYY-MM-DD') ";
                sql += " AND TWN_DATE(A.APPTIME) >= :AppTime_Start ";
                p.Add(":AppTime_Start", string.Format("{0}", AppTime_Start));
            }
            if (string.IsNullOrEmpty(AppTime_Start) && !string.IsNullOrEmpty(AppTime_End))
            {
                //sql += " AND SUBSTR(A.APPTIME,1,10) <= TO_DATE(:AppTime_End,'YYYY-MM-DD') ";
                sql += " AND TWN_DATE(A.APPTIME) <= :AppTime_End ";
                p.Add(":AppTime_End", string.Format("{0}", AppTime_End));
            }

            if (!string.IsNullOrEmpty(DIS_TIME_Start) || !string.IsNullOrEmpty(DIS_TIME_End))
            {
                sql += " AND EXISTS ( SELECT 1 FROM ME_DOCD B WHERE B.DOCNO = A.DOCNO ";
                if (!string.IsNullOrEmpty(DIS_TIME_Start) && !string.IsNullOrEmpty(DIS_TIME_End))
                {
                    
                    //sql += " AND SUBSTR(B.DIS_TIME,1,10) BETWEEN TO_DATE(:DIS_TIME_Start,'YYYY-MM-DD') AND TO_DATE(:DIS_TIME_End,'YYYY-MM-DD') ";
                    sql += " AND TWN_DATE(B.DIS_TIME) BETWEEN :DIS_TIME_Start AND :DIS_TIME_End ";
                    p.Add(":DIS_TIME_Start", DIS_TIME_Start.Substring(0, 10));
                    p.Add(":DIS_TIME_End", DIS_TIME_End.Substring(0, 10));
                }

                if (!string.IsNullOrEmpty(DIS_TIME_Start) && string.IsNullOrEmpty(DIS_TIME_End))
                {
                    //sql += " AND SUBSTR(B.DIS_TIME,1,10) >= TO_DATE(:DIS_TIME_Start,'YYYY-MM-DD') ";
                    sql += " AND SUBSTR(B.DIS_TIME,1,10) >= :DIS_TIME_Start ";
                    p.Add(":DIS_TIME_Start", string.Format("{0}", DIS_TIME_Start));
                }
                if (string.IsNullOrEmpty(DIS_TIME_Start) && !string.IsNullOrEmpty(DIS_TIME_End))
                {
                    //sql += " AND SUBSTR(B.DIS_TIME,1,10) <= TO_DATE(:DIS_TIME_End,'YYYY-MM-DD') ";
                    sql += " AND TWN_DATE(B.DIS_TIME) <= :DIS_TIME_End";
                    p.Add(":DIS_TIME_End", string.Format("{0}", DIS_TIME_End));
                }
                sql += "    ) "; 
            }

            if (str_FLOWID.Length > 0)
            {
                string sql_FLOWID = "";
                sql += @" AND (";
                foreach (string tmp_FLOWID in str_FLOWID)
                {
                    if (string.IsNullOrEmpty(sql_FLOWID))
                    {
                        sql_FLOWID = @"A.FLOWID = '" + tmp_FLOWID + "'";
                    }
                    else
                    {
                        sql_FLOWID += @" OR A.FLOWID = '" + tmp_FLOWID + "'";
                    }
                }
                sql += sql_FLOWID + ") ";
            }
            if (str_DOCTYPE.Length > 0)
            {
                string sql_DOCTYPE = "";
                sql += @" AND (";
                foreach (string tmp_DOCTYPE in str_DOCTYPE)
                {
                    if (string.IsNullOrEmpty(sql_DOCTYPE))
                    {
                        sql_DOCTYPE = @" A.DOCTYPE = '" + tmp_DOCTYPE + "'";
                    }
                    else
                    {
                        sql_DOCTYPE += @" OR A.DOCTYPE = '" + tmp_DOCTYPE + "'";
                    }
                }
                sql += sql_DOCTYPE + ") ";
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0013Report_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public string GetTwnToday()
        {
            string sql = @"SELECT TWN_DATE(SYSDATE)TWN_TODAY FROM DUAL ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }
        public IEnumerable<AA0013Report_MODEL> GetPrintData(bool isGas, string[] str_APPDEPT, string[] str_MAT_CLASS, string AppTime_Start, string AppTime_End, string[] str_FLOWID, string[] str_DOCNO, string DIS_TIME_Start, string DIS_TIME_End, string UserName, string order)
        {
            var p = new DynamicParameters();

            string sql = @" SELECT ROWNUM AS SEQ, X.* FROM (
                        SELECT A.DOCNO, A.FLOWID, A.APPID, A.APPDEPT, A.APPLY_KIND, A.MAT_CLASS,
                        A.LIST_ID, B.DIS_USER, B.MMCODE, B.APPQTY, B.APVQTY, B.ACKQTY, B.EXPT_DISTQTY,
                        B.BW_MQTY, B.APLYITEM_NOTE, C.MMNAME_C, C.MMNAME_E, C.BASE_UNIT, C.M_STOREID
                        ,(SELECT DISTINCT RTrim(DATA_DESC) FROM PARAM_D WHERE DATA_NAME = 'FLOWID_MR1' AND DATA_VALUE = A.FLOWID) AS FLOWID_N
                        ,(SELECT RTrim(WH_NO || ' ' || WH_NAME) FROM MI_WHMAST WHERE WH_NO = A.TOWH) AS APPDEPT_N                        
                        ,(SELECT DISTINCT RTrim(DATA_DESC) FROM PARAM_D WHERE DATA_NAME = 'APPLY_KIND' AND DATA_VALUE = A.APPLY_KIND) AS APPLY_KIND_N
                        ,(SELECT RTrim(MAT_CLASS || ' ' || MAT_CLSNAME) FROM MI_MATCLASS WHERE MAT_CLASS = A.MAT_CLASS) AS MATCLASS_N                        
                        ,(SELECT RTrim(MAT_CLSNAME) FROM MI_MATCLASS WHERE MAT_CLASS = A.MAT_CLASS) AS MAT_CLSNAME                        
                        ,(SELECT DISTINCT RTrim(DATA_DESC) FROM PARAM_D WHERE DATA_NAME = 'M_STOREID' AND DATA_VALUE = C.M_STOREID) AS M_STOREID_N
                        ,(SELECT UNA FROM UR_ID WHERE A.APPID = TUSER) AS APPID_N
                        ,(SELECT UNA FROM UR_ID WHERE B.DIS_USER = TUSER) AS DIS_USER_N
                        ,(SELECT UNA FROM UR_ID WHERE TUSER = :PRINT_USER) AS PRINT_USER_N
                        ,(SELECT (CASE WHEN A.LIST_ID = 'Y' THEN '(補印)' ELSE '' END) FROM ME_DOCM WHERE DOCNO = A.DOCNO ) AS LIST_ID_N
                        ,GET_STORELOC(A.FRWH, B.MMCODE)AS STORELOC 
                        FROM ME_DOCM A, ME_DOCD B, MI_MAST C
                        WHERE A.DOCNO = B.DOCNO AND B.MMCODE = C.MMCODE ";

            p.Add(":PRINT_USER", UserName);

            if (isGas)
            {
                sql += @"   and A.MAT_CLASS in ( '09') and A.DOCTYPE in ('AIR')";
            }
            else
            {
                sql += @"   and A.MAT_CLASS in ( '02','03','04','05','06','07','08') 
                            and A.DOCTYPE in ('MR1','MR2','MR3','MR4')";
            }

            //APPDEPT
            //if (str_APPDEPT.Length > 0)
            //{
            //    sql += @"AND A.APPDEPT IN :APPDEPT ";
            //    p.Add("APPDEPT", str_APPDEPT);                
            //}

            if (str_APPDEPT.Length > 0)
            {
                string sql_APPDEPT = "";
                sql += @" AND (";
                foreach (string tmp_APPDEPT in str_APPDEPT)
                {
                    if (string.IsNullOrEmpty(sql_APPDEPT))
                    {
                        sql_APPDEPT = @"A.TOWH = '" + tmp_APPDEPT + "'";
                    }
                    else
                    {
                        sql_APPDEPT += @" OR A.TOWH = '" + tmp_APPDEPT + "'";
                    }
                }
                sql += sql_APPDEPT + ") ";
            }
            //判斷MAT_CLASS查詢條件是否有值
            if (str_MAT_CLASS.Length > 0)
            {
                sql += @"AND A.MAT_CLASS IN :MAT_CLASS ";
                p.Add("MAT_CLASS", str_MAT_CLASS);

            }
            
            if (!string.IsNullOrEmpty(AppTime_Start) && !string.IsNullOrEmpty(AppTime_End))
            {
                sql += " AND SUBSTR(A.APPTIME,1,10) BETWEEN TO_DATE(:AppTime_Start,'YYYY-MM-DD') AND TO_DATE(:AppTime_End,'YYYY-MM-DD') ";
                p.Add(":AppTime_Start", Convert.ToDateTime(AppTime_Start.Substring(4, 11)).ToString("yyyy-MM-dd"));
                p.Add(":AppTime_End", Convert.ToDateTime(AppTime_End.Substring(4, 11)).ToString("yyyy-MM-dd"));
            }

            if (!string.IsNullOrEmpty(DIS_TIME_Start) && !string.IsNullOrEmpty(DIS_TIME_End))
            {
                sql += " AND SUBSTR(B.DIS_TIME,1,10) BETWEEN TO_DATE(:DIS_TIME_Start,'YYYY-MM-DD') AND TO_DATE(:DIS_TIME_End,'YYYY-MM-DD') ";
                p.Add(":DIS_TIME_Start", Convert.ToDateTime(DIS_TIME_Start.Substring(4, 11)).ToString("yyyy-MM-dd"));
                p.Add(":DIS_TIME_End", Convert.ToDateTime(DIS_TIME_End.Substring(4, 11)).ToString("yyyy-MM-dd"));
            }

            //判斷FLOWID查詢條件是否有值
            if (str_FLOWID.Length > 0)
            {
                sql += @"AND A.FLOWID IN :FLOWID ";
                p.Add("FLOWID", str_FLOWID);
            }
            //判斷DOCNO查詢條件是否有值
            if (str_DOCNO.Length > 0)
            {
                sql += @"AND A.DOCNO IN :DOCTYPE ";
                p.Add("DOCTYPE", str_DOCNO);
            }
            sql += " ORDER BY  A.DOCNO, STORELOC ";
            sql += string.Format(@" 
                    ) X 
                     order by  DOCNO,{0}", order);

            //先將查詢結果暫存在tmp_AA0013Report_MODEL，接著產生BarCode的資料
            IEnumerable<AA0013Report_MODEL> tmp_AA0013Report_MODEL = DBWork.Connection.Query<AA0013Report_MODEL>(sql, p);
            //================================產生BarCode的資料=======================================
            Barcode tmp_BarCode = new Barcode();

            foreach (AA0013Report_MODEL tmp_AA0013Data in tmp_AA0013Report_MODEL)
            {
                TYPE type = TYPE.CODE128;

                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    try
                    {
                        Bitmap img_BarCode = (Bitmap)tmp_BarCode.Encode(type, tmp_AA0013Data.DOCNO, 550, 45);

                        img_BarCode.Save(ms, ImageFormat.Jpeg);
                        byte[] byteImage = new Byte[ms.Length];
                        byteImage = ms.ToArray();
                        string strB64 = Convert.ToBase64String(byteImage);
                        tmp_AA0013Data.DOCNO_BarCode = strB64;
                    }
                    catch (FormatException ex)
                    {
                        tmp_AA0013Data.DOCNO_BarCode = null;
                    }
                }
            }
            //================================產生BarCode的資料=======================================

            //return DBWork.Connection.Query<AA0013Report_MODEL>(sql, p);
            return tmp_AA0013Report_MODEL;
        }

        public IEnumerable<AA0013Report_MODEL> SearchPrintData(bool isGas, string[] str_APPDEPT, string[] str_MAT_CLASS, string AppTime_Start, string AppTime_End, string[] str_FLOWID, string[] str_DOCTYPE, string DIS_TIME_Start, string DIS_TIME_End, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"SELECT A.DOCNO, A.FLOWID, A.APPID, A.APPDEPT, A.APPLY_KIND, A.MAT_CLASS,
                        B.SEQ, A.LIST_ID, B.DIS_USER, B.MMCODE, B.APPQTY, B.APVQTY, B.ACKQTY, B.EXPT_DISTQTY,
                        B.BW_MQTY, B.APLYITEM_NOTE, C.MMNAME_C, C.MMNAME_E, C.BASE_UNIT, C.M_STOREID
                        ,(SELECT DISTINCT RTrim(DATA_VALUE || ' ' || DATA_DESC) FROM PARAM_D WHERE DATA_NAME = 'FLOWID_MR1' AND DATA_VALUE = A.FLOWID) AS FLOWID_N
                        ,(SELECT RTrim(WH_NO || ' ' || WH_NAME) FROM MI_WHMAST WHERE WH_NO = A.APPDEPT) AS APPDEPT_N                        
                        ,(SELECT DISTINCT RTrim(DATA_DESC) FROM PARAM_D WHERE DATA_NAME = 'APPLY_KIND' AND DATA_VALUE = A.APPLY_KIND) AS APPLY_KIND_N
                        ,(SELECT RTrim(MAT_CLASS || ' ' || MAT_CLSNAME) FROM MI_MATCLASS WHERE MAT_CLASS = A.MAT_CLASS) AS MATCLASS_N                        
                        ,(SELECT RTrim(MAT_CLSNAME) FROM MI_MATCLASS WHERE MAT_CLASS = A.MAT_CLASS) AS MAT_CLSNAME    
                        ,TWN_DATE(A.APPTIME) APPTIME
                        ,A.TOWH || ' ' || WH_NAME(A.TOWH) TOWH_NAME
                        ,A.FRWH || ' ' || WH_NAME(A.FRWH) FRWH_NAME
                        FROM ME_DOCM A, ME_DOCD B, MI_MAST C
                        WHERE A.DOCNO = B.DOCNO AND B.MMCODE = C.MMCODE ";
            if (isGas)
            {
                sql += @"   and A.MAT_CLASS in ( '09') and A.DOCTYPE in ('AIR')";
            }
            else {
                sql += @"   and A.MAT_CLASS in ( '02','03','04','05','06','07','08') 
                            and A.DOCTYPE in ('MR1','MR2','MR3','MR4')";
            }

            if (str_APPDEPT.Length > 0)
            {
                string sql_APPDEPT = "";
                sql += @" AND (";
                foreach (string tmp_APPDEPT in str_APPDEPT)
                {
                    if (string.IsNullOrEmpty(sql_APPDEPT))
                    {
                        sql_APPDEPT = @"A.TOWH = '" + tmp_APPDEPT + "'";
                    }
                    else
                    {
                        sql_APPDEPT += @" OR A.TOWH = '" + tmp_APPDEPT + "'";
                    }
                }
                sql += sql_APPDEPT + ") ";
            }
            //判斷MAT_CLASS查詢條件是否有值
            if (str_MAT_CLASS.Length > 0)
            {
                sql += @"AND A.MAT_CLASS IN :MAT_CLASS ";
                p.Add("MAT_CLASS", str_MAT_CLASS);
            }
            
            if (!string.IsNullOrEmpty(AppTime_Start) && !string.IsNullOrEmpty(AppTime_End))
            {
                sql += " AND SUBSTR(A.APPTIME,1,10) BETWEEN TO_DATE(:AppTime_Start,'YYYY-MM-DD') AND TO_DATE(:AppTime_End,'YYYY-MM-DD') ";
                p.Add(":AppTime_Start", AppTime_Start.Substring(0, 10));
                p.Add(":AppTime_End", AppTime_End.Substring(0, 10));
            }
            if (!string.IsNullOrEmpty(DIS_TIME_Start) && !string.IsNullOrEmpty(DIS_TIME_End))
            {
                sql += " AND SUBSTR(B.DIS_TIME,1,10) BETWEEN TO_DATE(:DIS_TIME_Start,'YYYY-MM-DD') AND TO_DATE(:DIS_TIME_End,'YYYY-MM-DD') ";
                p.Add(":DIS_TIME_Start", DIS_TIME_Start.Substring(0, 10));
                p.Add(":DIS_TIME_End", DIS_TIME_End.Substring(0, 10));
            }
            //判斷FLOWID查詢條件是否有值
            if (str_FLOWID.Length > 0)
            {
                sql += @"AND A.FLOWID IN :FLOWID ";
                p.Add("FLOWID", str_FLOWID);
            }
            //判斷DOCNO查詢條件是否有值
            if (str_DOCTYPE.Length > 0)
            {
                sql += @"AND A.DOCTYPE IN :DOCTYPE ";
                p.Add("DOCTYPE", str_DOCTYPE);
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0013Report_MODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        /// <summary>
        /// 取得申請單位ComboList
        /// </summary>
        /// <param name="id">User責任中心代碼</param>
        /// <returns></returns>
        public IEnumerable<COMBO_MODEL> GetAppDeptCombo(string User_WHNO)
        {
            //如果USER_INID等於WHNO_MM1，列出MI_WHMAST資料表內WH_KIND='1' 且 WH_GRADE='2'的WH_NO
            string sql = @"SELECT WH_NO AS VALUE, WH_NAME AS TEXT,
                        RTrim(WH_NO || ' ' || WH_NAME) AS COMBITEM
                        FROM MI_WHMAST 
                        WHERE WH_KIND='1' AND WH_GRADE='2'
                        AND WHNO_MM1 = :User_WHNO
                        ORDER BY WH_NO 
                        ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { User_WHNO = User_WHNO });
        }

        /// <summary>
        /// 取得物料分類ComboList
        /// </summary>
        /// <param name="isGas">是否為氣體功能</param>
        /// <returns></returns>
        public IEnumerable<COMBO_MODEL> GetMatclassCombo(bool isGas)
        {
            string sql = "";
                sql = @"SELECT a.MAT_CLASS AS VALUE, a.MAT_CLSNAME AS TEXT, 
                        a.MAT_CLASS || ' ' || a.MAT_CLSNAME AS COMBITEM 
                        FROM MI_MATCLASS a
                        where 1=1 
                          and a.mat_class in (select mat_class from MI_MATCLASS 
                                               where mat_clsid in (";

            sql += isGas ? "'6'" : "'2', '3'";

            sql += @"               )
                            )      ORDER BY MAT_CLASS";
            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        /// <summary>
        /// 取得申請單狀態ComboList
        /// </summary>
        /// <returns></returns>
        public IEnumerable<COMBO_MODEL> GetFlowidCombo(bool isGas)
        {
            string sql = string.Format(@"
                        SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                               DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                          FROM PARAM_D
                         WHERE GRP_CODE='ME_DOCM' 
                           AND (DATA_NAME='{0}') 
                         ORDER BY DATA_VALUE",
                           isGas ? "FLOWID_AIR" : "FLOWID_MR1");

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        /// <summary>
        /// 取得申請單號ComboList
        /// </summary>
        /// <param name="IsUserWHNO_Equal_WHNOMM1">UserWHNO是否等於WHNOMM1</param>
        /// <param name="TASK_ID">工作編號</param>
        /// <param name="id">使用者責任中心代碼</param>
        /// <param name="str_APPDEPT">申請單位'</param>
        /// <param name="str_MAT_CLASS">物料分類代碼</param>
        /// <param name="str_FLOWID">申請單狀態</param>
        /// <returns></returns>
        public IEnumerable<COMBO_MODEL> GetDocnoCombo(bool isGas, string[] str_APPDEPT, string[] str_MAT_CLASS, string[] str_FLOWID)
        {
            var p = new DynamicParameters();

            string sql = "";

            sql = @"select distinct docno as value, docno as text, docno as cobmitem
                          from ME_DOCM
                        where 1=1 ";

            if (isGas)
            {
                sql += @"   and MAT_CLASS in ( '09') and DOCTYPE in ('AIR')";
            }
            else
            {
                sql += @"   and MAT_CLASS in ( '02','03','04','05','06','07','08') 
                            and DOCTYPE in ('MR1','MR2','MR3','MR4')";
            }

            //APPDEPT，有的話用字串相加的方式串接條件(IN的方法會有問題)
            if (str_APPDEPT.Length > 0)
            {
                string sql_APPDEPT = "";
                sql += @" AND (";
                foreach (string tmp_APPDEPT in str_APPDEPT)
                {
                    if (string.IsNullOrEmpty(sql_APPDEPT))
                    {
                        sql_APPDEPT = @"APPDEPT = '" + tmp_APPDEPT + "'";
                    }
                    else
                    {
                        sql_APPDEPT += @" OR APPDEPT = '" + tmp_APPDEPT + "'";
                    }
                }
                sql += sql_APPDEPT + ") ";
            }
            //判斷FLOWID查詢條件是否有值，有的話用字串相加的方式串接條件(IN的方法會有問題)
            if (str_FLOWID.Length > 0)
            {
                string sql_FLOWID = "";
                sql += @"AND (";
                foreach (string tmp_FLOWID in str_FLOWID)
                {
                    if (string.IsNullOrEmpty(sql_FLOWID))
                    {
                        sql_FLOWID = @"FLOWID = '" + tmp_FLOWID + "'";
                    }
                    else
                    {
                        sql_FLOWID += @" OR FLOWID = '" + tmp_FLOWID + "'";
                    }
                }
                sql += sql_FLOWID + ") ";
            }
            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }

        /// <summary>
        /// 根據使用者名稱取得USER_WHNO
        /// </summary>
        /// <param name="UserName"></param>
        /// <returns></returns>
        public string GetUSER_WHNO(string UserName)
        {
            string sql = @"SELECT WH_NO FROM MI_WHMAST 
                        WHERE INID = (SELECT INID FROM UR_ID WHERE TUSER=:TUSER) AND ROWNUM = 1 ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { TUSER = UserName }, DBWork.Transaction).ToString();
            return rtn;
        }

        /// <summary>
        /// 根據使用者名稱取得TASK_ID
        /// </summary>
        /// <param name="UserName">使用者名稱</param>
        /// <returns></returns>
        public string GetTASK_ID(string UserName)
        {
            string sql = @"SELECT TASK_ID FROM MI_WHID 
                        WHERE WH_USERID = :TUSER AND WH_NO = WHNO_MM1 ";
            var rtn = DBWork.Connection.ExecuteScalar(sql, new { TUSER = UserName }, DBWork.Transaction);
            rtn = (rtn is null) ? "" : rtn.ToString();
            return rtn.ToString();
        }
        public IEnumerable<string> GetTaskids(string userid) {
            string sql = @"select task_id from MI_WHID where wh_userid = :userid and wh_no = WHNO_MM1";
            return DBWork.Connection.Query<string>(sql, new { userid = userid }, DBWork.Transaction);
        }

        /// <summary>
        /// 根據使用者名稱判斷UserWHNO是否等於WHNO_MM1
        /// </summary>
        /// <param name="UserName">使用者名稱</param>
        /// <returns>true=相同，false=不同</returns>
        public bool GetIsUserWHNO_Equal_WHNOMM1(string UserName)
        {

            string sql = @"SELECT COUNT(*) FROM DUAL 
                        WHERE WHNO_MM1 = (SELECT INID FROM UR_ID WHERE TUSER=:TUSER)";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { TUSER = UserName }, DBWork.Transaction).ToString();
            return (rtn == "0") ? false : true;
        }

        public string GetUridInid(string id)
        {
            string sql = @"SELECT INID FROM UR_ID WHERE TUSER=:TUSER ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { TUSER = id }, DBWork.Transaction).ToString();
            return rtn;
        }

        /// <summary>
        /// 取得申請單類別
        /// </summary>
        /// <returns></returns>
        public IEnumerable<COMBO_MODEL> GetDocType(bool isGas)
        {
            var p = new DynamicParameters();

            string sql = @"SELECT DOCTYPE VALUE,DOCTYPE_NAME TEXT
                            FROM MI_DOCTYPE 
                            WHERE DOCTYPE IN (";

            sql += isGas  ? "'AIR'" : "'MR1','MR2','MR3','MR4'";

            sql += ")";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

    }
}