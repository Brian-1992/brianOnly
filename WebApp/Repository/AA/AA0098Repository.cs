using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.AA
{
    public class AA0098Repository : JCLib.Mvc.BaseRepository
    {
        public AA0098Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0098> GetAllM(string date1, string date2, string mmcode, string diff)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT 
                        A.MMCODE MMCODEA,B.MMCODE MMCODEB,
                        A.MMNAME_E MMNAME_EA,B.MMNAME_E MMNAME_EB,
                        A.MMNAME_C MMNAME_CA,B.MMNAME_C MMNAME_CB,
                        A.BASE_UNIT BASE_UNITA,B.BASE_UNIT BASE_UNITB,
                        A.M_PURUN M_PURUNA,B.M_PURUN M_PURUNB,
                        A.CLASS_FLAG M_APPLYIDA,B.M_APPLYID M_APPLYIDB,
                        A.M_PHCTNCO M_PHCTNCOA,B.M_PHCTNCO M_PHCTNCOB,
                        A.M_AGENLAB M_AGENLABA,B.M_AGENLAB M_AGENLABB,
                        A.UNIT_SWAP,(SELECT EXCH_RATIO FROM MI_UNITEXCH WHERE  MMCODE=B.MMCODE AND UNIT_CODE=B.M_PURUN AND AGEN_NO=B.M_AGENNO )EXCH_RATIO,
                        A.M_DISCPERC M_DISCPERCA,ROUND(B.M_DISCPERC,4) M_DISCPERCB,
                        A.DISC_CPRICE DISC_CPRICEA,ROUND(B.DISC_CPRICE,4) DISC_CPRICEB,
                        ROUND(B.DISC_UPRICE,4) DISC_UPRICE,
                        A.UPRICE UPRICEA,ROUND(B.UPRICE,4) UPRICEB,
                        A.M_CONTPRICE M_CONTPRICEA,ROUND(B.M_CONTPRICE,4) M_CONTPRICEB,
                        A.M_NHIKEY M_NHIKEYA,B.M_NHIKEY M_NHIKEYB,
                        A.M_AGENNO M_AGENNOA,B.M_AGENNO M_AGENNOB,
                        A.CASEFROM M_MATID ,B.M_MATID M_MATIDB,
                        B.M_CONTID M_CONTIDB,A.PROCDATETIME
                        FROM V_HIS_MAST A
                        LEFT OUTER JOIN MI_MAST B ON B.MMCODE=A.MMCODE AND B.MAT_CLASS='02'
                        WHERE 1=1 ";
            if (date1 != "" & date2 != "")
            {
                sql += " AND A.PROCDATETIME >= :d0 || '000000' AND A.PROCDATETIME <=:d1 || '999999'";
                p.Add(":d0", string.Format("{0}", date1));
                p.Add(":d1", string.Format("{0}", date2));
            }
            if (date1 != "" & date2 == "")
            {
                sql += " AND A.PROCDATETIME >= :d0  || '000000'";
                p.Add(":d0", string.Format("{0}", date1));
            }
            if (date1 == "" & date2 != "")
            {
                sql += " AND A.PROCDATETIME <= :d1  || '999999'";
                p.Add(":d1", string.Format("{0}", date2));
            }
            if (mmcode != "")
            {
                sql += " AND B.MMCODE = :p2 ";
                p.Add(":p2", string.Format("{0}", mmcode));
            }
            if (diff == "Y")
            {
                sql += @" AND NOT EXISTS 
                            (SELECT 1 FROM MI_MAST
                            WHERE MMCODE = A.MMCODE
                              AND MMNAME_E = A.MMNAME_E
                              AND MMNAME_C = A.MMNAME_C
                              AND BASE_UNIT = A.BASE_UNIT
                              AND M_PURUN = A.M_PURUN
                              AND M_PHCTNCO = A.M_PHCTNCO
                              AND M_AGENLAB = A.M_AGENLAB
                              AND M_DISCPERC = A.M_DISCPERC
                              AND DISC_CPRICE = A.DISC_CPRICE
                              AND UPRICE = A.UPRICE
                              AND M_CONTPRICE = A.M_CONTPRICE
                              AND M_NHIKEY = A.M_NHIKEY
                              AND M_AGENNO = A.M_AGENNO
                            ) ";
            }
            return DBWork.Connection.Query<AA0098>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<AA0098> GetM(string id)
        {
            var sql = @"SELECT 
                        A.MMCODE MMCODEA,B.MMCODE MMCODEB,
                        A.MMNAME_E MMNAME_EA,B.MMNAME_E MMNAME_EB,
                        A.MMNAME_C MMNAME_CA,B.MMNAME_C MMNAME_CB,
                        A.BASE_UNIT BASE_UNITA,B.BASE_UNIT BASE_UNITB,
                        A.M_PURUN M_PURUNA,B.M_PURUN M_PURUNB,
                        A.CLASS_FLAG M_APPLYIDA,B.M_APPLYID M_APPLYIDB,
                        A.M_PHCTNCO M_PHCTNCOA,B.M_PHCTNCO M_PHCTNCOB,
                        A.M_AGENLAB M_AGENLABA,B.M_AGENLAB M_AGENLABB,
                        A.UNIT_SWAP,(SELECT EXCH_RATIO FROM MI_UNITEXCH WHERE  MMCODE=B.MMCODE AND UNIT_CODE=B.M_PURUN AND AGEN_NO=B.M_AGENNO )EXCH_RATIO,
                        A.M_DISCPERC M_DISCPERCA,ROUND(B.M_DISCPERC,4) M_DISCPERCB,
                        A.DISC_CPRICE DISC_CPRICEA,ROUND(B.DISC_CPRICE,4) DISC_CPRICEB,
                        ROUND(B.DISC_UPRICE,4) DISC_UPRICE,
                        A.UPRICE UPRICEA,ROUND(B.UPRICE,4) UPRICEB,
                        A.M_CONTPRICE M_CONTPRICEA,ROUND(B.M_CONTPRICE,4) M_CONTPRICEB,
                        A.M_NHIKEY M_NHIKEYA,B.M_NHIKEY M_NHIKEYB,
                        A.M_AGENNO M_AGENNOA,B.M_AGENNO M_AGENNOB,
                        A.CASEFROM M_MATID ,B.M_MATID M_MATIDB,
                        B.M_CONTID M_CONTIDB,A.PROCDATETIME
                        FROM V_HIS_MAST A
                        LEFT OUTER JOIN MI_MAST B ON B.MMCODE=A.MMCODE AND B.MAT_CLASS='02'
                        WHERE 1=1 AND A.MMCODE = :MMCODE";
            return DBWork.Connection.Query<AA0098>(sql, new { MMCODE = id }, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCodeDocd(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E  
                        FROM MI_MAST A WHERE 1=1 AND MAT_CLASS='02' ";
            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR MMNAME_E LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

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
        public string GetTwnsystime()
        {
            string sql = @"SELECT TWN_SYSTIME FROM DUAL ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }
        public bool CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM MI_MAST WHERE MMCODE=:MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsN(string id)
        {
            string sql = @"SELECT 1 FROM MI_MAST_N WHERE MMCODE=:MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsUnit(string mmcode, string unitcode, string agen_no)
        {
            string sql = @"SELECT 1 FROM MI_UNITEXCH WHERE MMCODE=:MMCODE AND UNIT_CODE=:UNITCODE AND AGEN_NO=:AGEN_NO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode, UNITCODE = unitcode, AGEN_NO = agen_no }, DBWork.Transaction) == null);
        }
        public bool CheckExistsInv(string mmcode)
        {
            string sql = @"SELECT 1 FROM MI_WINVCTL WHERE MMCODE=:MMCODE AND WH_NO= WHNO_MM1 ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode }, DBWork.Transaction) == null);
        }
        public int CreateM(AA0098 aa0098)
        {
            var sql = @"INSERT INTO MI_MAST (
                        MMCODE , MMNAME_E , MMNAME_C , BASE_UNIT , M_PURUN , 
                        M_APPLYID , M_PHCTNCO , M_AGENLAB , M_DISCPERC , DISC_CPRICE , 
                        UPRICE , M_CONTPRICE , M_NHIKEY , M_AGENNO , M_MATID , 
                        M_CONTID , MAT_CLASS , M_STOREID , DISC_UPRICE , 
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :MMCODEB , :MMNAME_EB , :MMNAME_CB , :BASE_UNITB , :M_PURUNB , 
                        :M_APPLYIDB , :M_PHCTNCOB , :M_AGENLABB , :M_DISCPERCB , :DISC_CPRICEB , 
                        :UPRICEB , :M_CONTPRICEB , :M_NHIKEYB , :M_AGENNOB , :M_MATIDB , 
                        :M_CONTIDB , '02' , '0' , :DISC_UPRICE , 
                        SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, aa0098, DBWork.Transaction);
        }
        public int CreateN(AA0098 aa0098)
        {
            var sql = @"INSERT INTO MI_MAST_N (
                        MMCODE , MMNAME_E , MMNAME_C , BASE_UNIT , M_PURUN , 
                        M_APPLYID , M_PHCTNCO , M_AGENLAB , M_DISCPERC , DISC_CPRICE , 
                        UPRICE , M_CONTPRICE , M_NHIKEY , M_AGENNO , M_MATID , 
                        M_CONTID , MAT_CLASS , M_STOREID , DISC_UPRICE , 
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :MMCODEB , :MMNAME_EB , :MMNAME_CB , :BASE_UNITB , :M_PURUNB , 
                        :M_APPLYIDB , :M_PHCTNCOB , :M_AGENLABB , :M_DISCPERCB , :DISC_CPRICEB , 
                        :UPRICEB , :M_CONTPRICEB , :M_NHIKEYB , :M_AGENNOB , :M_MATIDB , 
                        :M_CONTIDB , '02' , '0' , :DISC_UPRICE , 
                        SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, aa0098, DBWork.Transaction);
        }

        public int UpdateM(AA0098 aa0098)
        {
            var sql = @"UPDATE MI_MAST SET 
                        MMNAME_E = :MMNAME_EB, MMNAME_C = :MMNAME_CB, BASE_UNIT = :BASE_UNITB, M_PURUN = :M_PURUNB, 
                        M_APPLYID = :M_APPLYIDB, M_PHCTNCO = :M_PHCTNCOB, M_AGENLAB = :M_AGENLABB, M_DISCPERC = :M_DISCPERCB, DISC_CPRICE = :DISC_CPRICEB,
                        UPRICE = :UPRICEB ,M_CONTPRICE = :M_CONTPRICEB, M_NHIKEY = :M_NHIKEYB ,M_AGENNO = :M_AGENNOB ,M_MATID = :M_MATIDB ,
                        M_CONTID = :M_CONTIDB , DISC_UPRICE = :DISC_UPRICE , 
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE MMCODE = :MMCODEB";
            return DBWork.Connection.Execute(sql, aa0098, DBWork.Transaction);
        }
        public int UpdateN(AA0098 aa0098)
        {
            var sql = @"UPDATE MI_MAST_N SET 
                        MMNAME_E = :MMNAME_EB, MMNAME_C = :MMNAME_CB, BASE_UNIT = :BASE_UNITB, M_PURUN = :M_PURUNB, 
                        M_APPLYID = :M_APPLYIDB, M_PHCTNCO = :M_PHCTNCOB, M_AGENLAB = :M_AGENLABB, M_DISCPERC = :M_DISCPERCB, DISC_CPRICE = :DISC_CPRICEB,
                        UPRICE = :UPRICEB ,M_CONTPRICE = :M_CONTPRICEB, M_NHIKEY = :M_NHIKEYB ,M_AGENNO = :M_AGENNOB ,M_MATID = :M_MATIDB ,
                        M_CONTID = :M_CONTIDB , DISC_UPRICE = :DISC_UPRICE , 
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE MMCODE = :MMCODEB";
            return DBWork.Connection.Execute(sql, aa0098, DBWork.Transaction);
        }

        public int CreateUnit(AA0098 aa0098)
        {
            var sql = @"INSERT INTO MI_UNITEXCH (
                        MMCODE , UNIT_CODE , AGEN_NO , EXCH_RATIO , 
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :MMCODEB , :M_PURUNB , :M_AGENNOB , :EXCH_RATIO , 
                        SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, aa0098, DBWork.Transaction);
        }

        public int CreateInv(AA0098 aa0098)
        {
            var sql = @"INSERT INTO MI_WINVCTL (
                        MMCODE , WH_NO , MIN_ORDQTY , 
                        CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :MMCODEB , WHNO_MM1 , 1 , 
                        SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, aa0098, DBWork.Transaction);
        }
        public int UpdateUnit(AA0098 aa0098)
        {
            var sql = @"UPDATE MI_UNITEXCH SET 
                        EXCH_RATIO = :EXCH_RATIO,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                       WHERE MMCODE=:MMCODEB AND UNIT_CODE=:M_PURUNB AND AGEN_NO=:M_AGENNOB";
            return DBWork.Connection.Execute(sql, aa0098, DBWork.Transaction);
        }
    }
}
