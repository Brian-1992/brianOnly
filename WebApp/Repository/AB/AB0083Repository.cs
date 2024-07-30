using System;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using System.Text;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

namespace WebApp.Repository.AB
{
    public class AB0083ReportMODEL : JCLib.Mvc.BaseModel
    {
        public string F1 { get; set; }
        public string F2 { get; set; }
        public string F3 { get; set; }
        public string F4 { get; set; }
        public float F5 { get; set; }
        public float F6 { get; set; }
        public string F7 { get; set; }
        public string F8 { get; set; }

    }
    public class AB0083Repository : JCLib.Mvc.BaseRepository
    {
        public AB0083Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AB0083ReportMODEL> GetPrintDataA(string p1, string p2, string[] str_p3)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.CREATEDATE F1 ,A.NRNAME F2, A.ORDERCODE F3, A.ORDERENGNAME F4, ''F5,''F6 ,A.HISSYSCODENAME F7,''F8
                        FROM ME_AB0083A A WHERE 1 = 1 ";

            if (p1 != "" & p2 != "")
            {
                sql += " AND A.CREATEDATE BETWEEN :p1 AND :p2 ";
                p.Add(":p1", string.Format("{0}", p1));
                p.Add(":p2", string.Format("{0}", p2));
            }
            if (p1 != "" & p2 == "")
            {
                sql += " AND A.CREATEDATE >= :p1 ";
                p.Add(":p1", string.Format("{0}", p1));
            }
            if (p1 == "" & p2 != "")
            {
                sql += " AND A.CREATEDATE <= :p2 ";
                p.Add(":p2", string.Format("{0}", p2));
            }
            //判斷FLOWID查詢條件是否有值，有的話用字串相加的方式串接條件(IN的方法會有問題)
            if (str_p3.Length > 0)
            {
                string sql_p3 = "";
                sql += @"AND (";
                foreach (string tmp_p3 in str_p3)
                {
                    if (string.IsNullOrEmpty(sql_p3))
                    {
                        sql_p3 = @"A.NRCODE = '" + tmp_p3 + "'";
                    }
                    else
                    {
                        sql_p3 += @" OR A.NRCODE = '" + tmp_p3 + "'";
                    }
                }
                sql += sql_p3 + ") ";
            }
            sql += " ORDER BY A.ORDERCODE ASC, A.NRCODE ASC, A.CREATEDATE DESC ";
            return DBWork.Connection.Query<AB0083ReportMODEL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<AB0083ReportMODEL> GetPrintDataB(string p1, string p2, string[] str_p3)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.CREATEDATE F1 ,A.NRNAME F2, A.ORDERCODE F3, A.ORDERENGNAME F4, A.QTY F5, A.MONEY F6 ,''F7,''F8
                    FROM ME_AB0083B A  WHERE 1 = 1 ";

            if (p1 != "" & p2 != "")
            {
                sql += " AND A.CREATEDATE BETWEEN :p1 AND :p2 ";
                p.Add(":p1", string.Format("{0}", p1));
                p.Add(":p2", string.Format("{0}", p2));
            }
            if (p1 != "" & p2 == "")
            {
                sql += " AND A.CREATEDATE >= :p1 ";
                p.Add(":p1", string.Format("{0}", p1));
            }
            if (p1 == "" & p2 != "")
            {
                sql += " AND A.CREATEDATE <= :p2 ";
                p.Add(":p2", string.Format("{0}", p2));
            }
            //判斷FLOWID查詢條件是否有值，有的話用字串相加的方式串接條件(IN的方法會有問題)
            if (str_p3.Length > 0)
            {
                string sql_p3 = "";
                sql += @"AND (";
                foreach (string tmp_p3 in str_p3)
                {
                    if (string.IsNullOrEmpty(sql_p3))
                    {
                        sql_p3 = @"A.NRCODE = '" + tmp_p3 + "'";
                    }
                    else
                    {
                        sql_p3 += @" OR A.NRCODE = '" + tmp_p3 + "'";
                    }
                }
                sql += sql_p3 + ") ";
            }
            sql += " ORDER BY A.ORDERCODE ASC, A.NRCODE ASC, A.CREATEDATE DESC ";
            return DBWork.Connection.Query<AB0083ReportMODEL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<AB0083ReportMODEL> GetPrintDataC(string p1, string p2, string[] str_p3)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.CREATEDATE F1 ,A.NRNAME F2, A.ORDERCODE F3, A.ORDERENGNAME F4, A.QTY F5, A.MONEY F6 ,''F7,''F8
                    FROM ME_AB0083C A  WHERE 1 = 1 ";

            if (p1 != "" & p2 != "")
            {
                sql += " AND A.CREATEDATE BETWEEN :p1 AND :p2 ";
                p.Add(":p1", string.Format("{0}", p1));
                p.Add(":p2", string.Format("{0}", p2));
            }
            if (p1 != "" & p2 == "")
            {
                sql += " AND A.CREATEDATE >= :p1 ";
                p.Add(":p1", string.Format("{0}", p1));
            }
            if (p1 == "" & p2 != "")
            {
                sql += " AND A.CREATEDATE <= :p2 ";
                p.Add(":p2", string.Format("{0}", p2));
            }
            //判斷FLOWID查詢條件是否有值，有的話用字串相加的方式串接條件(IN的方法會有問題)
            if (str_p3.Length > 0)
            {
                string sql_p3 = "";
                sql += @"AND (";
                foreach (string tmp_p3 in str_p3)
                {
                    if (string.IsNullOrEmpty(sql_p3))
                    {
                        sql_p3 = @"A.NRCODE = '" + tmp_p3 + "'";
                    }
                    else
                    {
                        sql_p3 += @" OR A.NRCODE = '" + tmp_p3 + "'";
                    }
                }
                sql += sql_p3 + ") ";
            }
            sql += " ORDER BY A.ORDERCODE ASC, A.NRCODE ASC, A.CREATEDATE DESC ";
            return DBWork.Connection.Query<AB0083ReportMODEL>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<AB0083ReportMODEL> GetPrintDataD(string p1, string p2, string[] str_p3)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.CREATEDATE F1 ,A.NRNAME F2, A.ORDERCODE F3, A.ORDERENGNAME F4, A.QTY F5, A.MONEY F6 ,A.HISSYSCODENAME F7,''F8
                    FROM ME_AB0083D A  WHERE 1 = 1 ";

            if (p1 != "" & p2 != "")
            {
                sql += " AND A.CREATEDATE BETWEEN :p1 AND :p2 ";
                p.Add(":p1", string.Format("{0}", p1));
                p.Add(":p2", string.Format("{0}", p2));
            }
            if (p1 != "" & p2 == "")
            {
                sql += " AND A.CREATEDATE >= :p1 ";
                p.Add(":p1", string.Format("{0}", p1));
            }
            if (p1 == "" & p2 != "")
            {
                sql += " AND A.CREATEDATE <= :p2 ";
                p.Add(":p2", string.Format("{0}", p2));
            }
            //判斷FLOWID查詢條件是否有值，有的話用字串相加的方式串接條件(IN的方法會有問題)
            if (str_p3.Length > 0)
            {
                string sql_p3 = "";
                sql += @"AND (";
                foreach (string tmp_p3 in str_p3)
                {
                    if (string.IsNullOrEmpty(sql_p3))
                    {
                        sql_p3 = @"A.NRCODE = '" + tmp_p3 + "'";
                    }
                    else
                    {
                        sql_p3 += @" OR A.NRCODE = '" + tmp_p3 + "'";
                    }
                }
                sql += sql_p3 + ") ";
            }
            sql += " ORDER BY A.ORDERCODE ASC, A.NRCODE ASC, A.CREATEDATE DESC ";
            return DBWork.Connection.Query<AB0083ReportMODEL>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<COMBO_MODEL> GetKindCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_AB0083' AND DATA_NAME='KIND' 
                        ORDER BY DATA_VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetWhmastCombo()
        {
            string sql = @"SELECT DISTINCT WH_NO as VALUE, WH_NAME as TEXT ,
                        WH_NO || ' ' || WH_NAME as COMBITEM 
                        FROM MI_WHMAST
                        WHERE WH_KIND='0' AND WH_GRADE='3' 
                        ORDER BY WH_NO ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

    }
}
