using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.F
{
    public class FA0015Repository : JCLib.Mvc.BaseRepository
    {
        public FA0015Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCD> GetAll(string MAT_CLASS,string DIS_TIME_B, string DIS_TIME_E, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"    SELECT
                                B.MMCODE,
                                SUBSTR(TWN_DATE(B.DIS_TIME), 1, 5) AS DIS_DATEYM,
                                SUM(B.BW_MQTY) AS BW_MQTY
                            FROM
                                ME_DOCM A, ME_DOCD B
                            WHERE
                                A.DOCNO = B.DOCNO
                                AND B.BW_MQTY IS NOT NULL
                                AND B.BW_MQTY <> 0 ";

            if (MAT_CLASS != "")
            {
                sql += " AND A.MAT_CLASS  = :MAT_CLASS ";
                p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
            }
            if (DIS_TIME_B != "")
            {
                sql += " AND TO_CHAR(DIS_TIME, 'yyyyMM') >= TO_CHAR(TO_DATE(:DIS_TIME_B + 191100, 'yyyyMM'), 'yyyyMM') ";
                p.Add(":DIS_TIME_B", string.Format("{0}", DIS_TIME_B));
            }
            if (DIS_TIME_E != "")
            {
                sql += " AND TO_CHAR(DIS_TIME, 'yyyyMM') <= TO_CHAR(TO_DATE(:DIS_TIME_E + 191100, 'yyyyMM'), 'yyyyMM') ";
                p.Add(":DIS_TIME_E", string.Format("{0}", DIS_TIME_E));
            }

            sql += " GROUP BY B.MMCODE, SUBSTR(TWN_DATE(B.DIS_TIME), 1, 5) ";
            sql += " ORDER BY B.MMCODE, SUBSTR(TWN_DATE(B.DIS_TIME), 1, 5) ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCD>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }


        public IEnumerable<ComboItemModel> GetMATCombo(bool matUserID)
        {
            string sql;
            if (matUserID == true)
            {
                sql = @"Select MAT_CLASS||' '||MAT_CLSNAME TEXT, MAT_CLASS VALUE from MI_MATCLASS
                            where MAT_CLSID = WHM1_TASK(:userID)
                            ORDER BY VALUE";
            }
            else
            {
                sql = @"Select MAT_CLASS||' '||MAT_CLSNAME TEXT, MAT_CLASS VALUE from MI_MATCLASS 
                             where mat_class <> '01' 
                            ORDER BY VALUE";
            }

            return DBWork.Connection.Query<ComboItemModel>(sql, new { userID=DBWork.ProcUser }, DBWork.Transaction);
        }


        public DataTable GetExcel(string MAT_CLASS, string DIS_TIME_B, string DIS_TIME_E)
        {
            var p = new DynamicParameters();

            var sql = @"    select 
                                b.mmcode as 院內碼,
                                substr(TWN_DATE(b.dis_time),1,5) as 年月, --dis_dateym
                                (select mmname_c from MI_MAST where mmcode=b.mmcode) as 中文品名, --mmname_c
                                (select mmname_e from MI_MAST where mmcode=b.mmcode) as 英文品名, --mmname_e
                                (select base_unit from MI_MAST where mmcode=b.mmcode) as 計量單位, --base_unit
                                (select inv_qty from MI_WHINV where wh_no=(select wh_no from MI_WHMAST where wh_kind='1' and wh_grade='5' and rownum=1) and mmcode=b.mmcode and substr(TWN_DATE(b.dis_time),1,5)>=substr(TWN_DATE(sysdate),1,5)) ||(select inv_qty from MI_WINVMON where data_ym=substr(TWN_DATE(b.dis_time),1,5) and wh_no=(select wh_no from MI_WHMAST where wh_kind='1' and wh_grade='5' and rownum=1) and mmcode=b.mmcode and substr(TWN_DATE(b.dis_time),1,5)<substr(TWN_DATE(sysdate),1,5)) as 戰備量, --invqty
                                sum(b.bw_mqty) as 調撥量, --bw_mqty
                                sum(b.bw_mqty) as 累計調撥量, -- bw_mqty_s
                                (select inv_qty from MI_WHINV where wh_no=(select wh_no from MI_WHMAST where wh_kind='1' and wh_grade='5' and rownum=1) and mmcode=b.mmcode and substr(TWN_DATE(b.dis_time),1,5)>=substr(TWN_DATE(sysdate),1,5)) ||(select inv_qty from MI_WINVMON where data_ym=substr(TWN_DATE(b.dis_time),1,5) and wh_no=(select wh_no from MI_WHMAST where wh_kind='1' and wh_grade='5' and rownum=1) and mmcode=b.mmcode and substr(TWN_DATE(b.dis_time),1,5)<substr(TWN_DATE(sysdate),1,5)) - sum(b.bw_mqty) as 現有量 --currqty
                            from 
                                ME_DOCM a,ME_DOCD b
                            where 
                                a.docno=b.docno 
                                and b.bw_mqty is not null 
                                and b.bw_mqty<>0 ";

            if (MAT_CLASS != "")
            {
                sql += " AND A.MAT_CLASS  = :MAT_CLASS ";
                p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
            }
            if (DIS_TIME_B != "")
            {
                sql += " AND TO_CHAR(DIS_TIME, 'yyyyMM') >= TO_CHAR(TO_DATE(:DIS_TIME_B + 191100, 'yyyyMM'), 'yyyyMM') ";
                p.Add(":DIS_TIME_B", string.Format("{0}", DIS_TIME_B));
            }
            if (DIS_TIME_E != "")
            {
                sql += " AND TO_CHAR(DIS_TIME, 'yyyyMM') <= TO_CHAR(TO_DATE(:DIS_TIME_E + 191100, 'yyyyMM'), 'yyyyMM') ";
                p.Add(":DIS_TIME_E", string.Format("{0}", DIS_TIME_E));
            }

            sql += " GROUP BY B.MMCODE, SUBSTR(TWN_DATE(B.DIS_TIME), 1, 5) ";
            sql += " ORDER BY B.MMCODE, SUBSTR(TWN_DATE(B.DIS_TIME), 1, 5) ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }


        public DataTable Report(string MAT_CLASS, string DIS_TIME_B, string DIS_TIME_E)
        {
            var p = new DynamicParameters();

            var sql = @"    select 
                                b.mmcode as mmcode, --院內碼
                                substr(TWN_DATE(b.dis_time),1,5) as dis_dateym, --年月
                                (select mmname_c from MI_MAST where mmcode=b.mmcode) as mmname_c, --中文品名
                                (select mmname_e from MI_MAST where mmcode=b.mmcode) as mmname_e, --英文品名
                                (select base_unit from MI_MAST where mmcode=b.mmcode) as base_unit , --計量單位
                                (select inv_qty from MI_WHINV where wh_no=(select wh_no from MI_WHMAST where wh_kind='1' and wh_grade='5' and rownum=1) and mmcode=b.mmcode and substr(TWN_DATE(b.dis_time),1,5)>=substr(TWN_DATE(sysdate),1,5)) ||(select inv_qty from MI_WINVMON where data_ym=substr(TWN_DATE(b.dis_time),1,5) and wh_no=(select wh_no from MI_WHMAST where wh_kind='1' and wh_grade='5' and rownum=1) and mmcode=b.mmcode and substr(TWN_DATE(b.dis_time),1,5)<substr(TWN_DATE(sysdate),1,5)) as invqty, --戰備量
                                sum(b.bw_mqty) as bw_mqty, --調撥量
                                sum(b.bw_mqty) as bw_mqty_s, --累計調撥量
                                (select inv_qty from MI_WHINV where wh_no=(select wh_no from MI_WHMAST where wh_kind='1' and wh_grade='5' and rownum=1) and mmcode=b.mmcode and substr(TWN_DATE(b.dis_time),1,5)>=substr(TWN_DATE(sysdate),1,5)) ||(select inv_qty from MI_WINVMON where data_ym=substr(TWN_DATE(b.dis_time),1,5) and wh_no=(select wh_no from MI_WHMAST where wh_kind='1' and wh_grade='5' and rownum=1) and mmcode=b.mmcode and substr(TWN_DATE(b.dis_time),1,5)<substr(TWN_DATE(sysdate),1,5)) - sum(b.bw_mqty) as currqty --現有量
                            from 
                                ME_DOCM a,ME_DOCD b
                            where 
                                a.docno=b.docno 
                                and b.bw_mqty is not null 
                                and b.bw_mqty<>0 ";

            if (MAT_CLASS != "")
            {
                sql += " AND A.MAT_CLASS  = :MAT_CLASS ";
                p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
            }
            if (DIS_TIME_B != "")
            {
                sql += " AND TO_CHAR(DIS_TIME, 'yyyyMM') >= TO_CHAR(TO_DATE(:DIS_TIME_B + 191100, 'yyyyMM'), 'yyyyMM') ";
                p.Add(":DIS_TIME_B", string.Format("{0}", DIS_TIME_B));
            }
            if (DIS_TIME_E != "")
            {
                sql += " AND TO_CHAR(DIS_TIME, 'yyyyMM') <= TO_CHAR(TO_DATE(:DIS_TIME_E + 191100, 'yyyyMM'), 'yyyyMM') ";
                p.Add(":DIS_TIME_E", string.Format("{0}", DIS_TIME_E));
            }

            sql += " GROUP BY B.MMCODE, SUBSTR(TWN_DATE(B.DIS_TIME), 1, 5) ";
            sql += " ORDER BY B.MMCODE, SUBSTR(TWN_DATE(B.DIS_TIME), 1, 5) ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
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


        public string getMatName(string parMAT_CLASS)
        {
            string sql = "";

            if (parMAT_CLASS == "")
            {
                return "";
            }
            else
            {
                sql += @"select MAT_CLSNAME from MI_MATCLASS where MAT_CLASS = :p0 ";
            }

            var str = DBWork.Connection.ExecuteScalar(sql, new { p0 = parMAT_CLASS }, DBWork.Transaction);
            return str == null ? "" : str.ToString();

            //return DBWork.Connection.ExecuteScalar(sql, new { userID = DBWork.ProcUser }, DBWork.Transaction).ToString();
        }

        //public string GetReportWH_NAME()
        //{
        //    string sql = @"select WH_NAME(WHNO_MM1) from DUAL";
        //    var str = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction);
        //    return str==null? "":str.ToString();
        //}
    }
}