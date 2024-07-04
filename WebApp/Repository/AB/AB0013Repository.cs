using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using TSGH.Models;

namespace WebApp.Repository.AB
{
    public class AB0013Repository : JCLib.Mvc.BaseRepository
    {
        public AB0013Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        public int UpdateEnd(string dno, string UPUSER, string UIP)
        {

            var sql = @"UPDATE ME_DOCM SET FLOWID='1302', UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO = :DOCNO";
            //return DBWork.Connection.Execute(sql, record, DBWork.Transaction);
            return DBWork.Connection.Execute(sql, new { DOCNO = dno, UPDATE_USER = UPUSER, UPDATE_IP = UIP }, DBWork.Transaction);
        }
        public bool CheckExistsD(string id)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public string GetDocDSeq(string id)
        {
            string sql = @"SELECT MAX(SEQ)+1 as SEQ FROM ME_DOCD WHERE DOCNO=:DOCNO ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public int CreateD(ME_DOCM medocm)
        {
            var sql = @"INSERT INTO ME_DOCD (
                        DOCNO, SEQ, MMCODE , APPQTY , APLYITEM_NOTE,GTAPL_RESON, 
                        CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                      VALUES (
                        :DOCNO, :SEQ, :MMCODE , :APPQTY ,  :APLYITEM_NOTE,:GTAPL_RESON, 
                        :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, medocm, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCM> GetD(string id)
        {
            var sql = @"SELECT A.*,
                        B.MMNAME_C, B.MMNAME_E, B.BASE_UNIT,B.M_CONTPRICE,
                        ( SELECT AVG_PRICE FROM MI_WHCOST WHERE MMCODE = A.MMCODE AND ROWNUM=1 ) AS AVG_PRICE,
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO=C.FRWH ) AS INV_QTY,
                        ( SELECT AVG_APLQTY FROM V_MM_AVGAPL WHERE MMCODE = A.MMCODE AND WH_NO=C.FRWH ) AS AVG_APLQTY,
                        ( SELECT NVL(TOT_APVQTY,0) FROM V_MM_TOTAPL WHERE DATE_YM=SUBSTR(TWN_SYSDATE,0,5) and MMCODE=A.MMCODE ) AS TOT_APVQTY,
                        ( SELECT SUM(TOT_BWQTY) FROM V_MM_TOTAPL WHERE DATE_YM=SUBSTR(TWN_SYSDATE,0,5) and MMCODE=A.MMCODE ) AS TOT_BWQTY,
                        ( SELECT MIN_ORDQTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO IN (SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='1' AND WH_GRADE='1')) AS TOT_DISTUN,
                        C.FLOWID 
                        FROM ME_DOCD A,MI_MAST B, ME_DOCM C WHERE C.DOCNO=A.DOCNO AND A.MMCODE = B.MMCODE
                        AND A.DOCNO = :DOCNO";
            return DBWork.Connection.Query<ME_DOCM>(sql, new { DOCNO = id }, DBWork.Transaction);
        }
        public int UpdateD(ME_DOCM medocm)
        {
            var sql = @"UPDATE ME_DOCD SET MMCODE = :MMCODE, APPQTY = :APPQTY, APLYITEM_NOTE = :APLYITEM_NOTE,GTAPL_RESON =:GTAPL_RESON,
                        UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                      WHERE DOCNO = :DOCNO AND SEQ = :SEQ ";
            return DBWork.Connection.Execute(sql, medocm, DBWork.Transaction);
        }
        public int DeleteD(string docno, string seq)
        {
            var sql = @" DELETE from ME_DOCD WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCM> GetD(string docno, string kind)
        {

            string sql = @"SELECT ME_DOCD.SEQ,ME_DOCM.DOCNO,ME_DOCM.FRWH,ME_DOCD.FRWH_D,ME_DOCD.MMCODE as MMCODE,MI_MAST.MMNAME_E,ME_DOCD.APVQTY,APPQTY,MI_MAST.BASE_UNIT,ME_DOCD.GTAPL_RESON,ME_DOCD.APLYITEM_NOTE";

            if (kind == "一般藥")
            {
                sql += " ,ME_DOCD.NRCODE,ME_DOCD.BEDNO,ME_DOCD.MEDNO,ME_DOCD.CHINNAME,ME_DOCD.ORDERDATE FROM ME_DOCM,ME_DOCD, MI_MAST WHERE ME_DOCM.DOCNO= ME_DOCD.DOCNO and ME_DOCD.DOCNO = :DOCNO and ME_DOCD.mmcode = MI_MAST.mmcode";
            }
            else
            {
                sql += " FROM ME_DOCM,ME_DOCD,MI_MAST WHERE ME_DOCM.DOCNO = ME_DOCD.DOCNO and ME_DOCD.DOCNO = :DOCNO and ME_DOCD.mmcode = MI_MAST.mmcode";
            }


            return DBWork.Connection.Query<ME_DOCM>(sql, new { DOCNO = docno }, DBWork.Transaction);
            //return DBWork.Connection.Query<PRM>(GetPagingStatement(sql, sorters), p);
        }
        public string GetFrwh(string docno)
        {
            string sql = @"SELECT FRWH FROM ME_DOCM WHERE DOCNO=:DOCNO";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }
        public int MasterUpdate(ME_DOCM me_docm)
        {
            var sql = @"UPDATE ME_DOCM SET FRWH=:FRWH, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public int MasterUpdateFrwh(ME_DOCM me_docm)
        {
            var sql = @"UPDATE ME_DOCM SET FRWH=:FRWH, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCM> QueryMEDOCD(string docno, string kind, int page_index, int page_size, string sorters)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"SELECT ME_DOCM.DOCNO,ME_DOCD.SEQ,WH_NAME(ME_DOCM.FRWH) as FRWH,ME_DOCD.FRWH_D,ME_DOCD.MMCODE as MMCODE,MI_MAST.MMNAME_E,ME_DOCD.APVQTY,APPQTY,MI_MAST.BASE_UNIT,case when ME_DOCD.GTAPL_RESON='01' then '01 班長運送途中破損'
                          when ME_DOCD.GTAPL_RESON= '02'then '02 氣送過程破損' when ME_DOCD.GTAPL_RESON= '03' then '03 醫護人員取用不慎破損' when ME_DOCD.GTAPL_RESON= '04' then '04 未收到藥品' when ME_DOCD.GTAPL_RESON= '05' then '05 其他' 
                          when ME_DOCD.GTAPL_RESON= '06' then '06 破損' when ME_DOCD.GTAPL_RESON= '07' then '07 過效期' when ME_DOCD.GTAPL_RESON= '08' then '08 變質' end as GTAPL_RESON,ME_DOCD.APLYITEM_NOTE";

            if (kind == "一般藥")
            {
                sql += " ,ME_DOCD.NRCODE,ME_DOCD.BEDNO,ME_DOCD.MEDNO,ME_DOCD.CHINNAME,ME_DOCD.ORDERDATE FROM ME_DOCM,ME_DOCD, MI_MAST WHERE ME_DOCM.DOCNO= ME_DOCD.DOCNO and ME_DOCD.DOCNO = :p0 and ME_DOCD.mmcode = MI_MAST.mmcode";
            }
            else
            {
                sql += " FROM ME_DOCM,ME_DOCD,MI_MAST WHERE ME_DOCM.DOCNO = ME_DOCD.DOCNO and ME_DOCD.DOCNO = :p0 and ME_DOCD.mmcode = MI_MAST.mmcode";
            }

            p.Add(":p0", docno);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p);
            //return DBWork.Connection.Query<PRM>(GetPagingStatement(sql, sorters), p);
        }
        public int MasterCreate(ME_DOCM me_docm)
        {
            var sql = @"INSERT INTO ME_DOCM (DOCNO, DOCTYPE, FLOWID, APPID, APPDEPT, APPTIME, FRWH,STKTRANSKIND, CREATE_TIME, CREATE_USER)  
                                    VALUES (:DOCNO, :DOCTYPE, :FLOWID, :APPID, :APPDEPT, SYSDATE,  :FRWH, :STKTRANSKIND, SYSDATE, :CREATE_USER)";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCM> MasterGet(string docno)
        {
            var sql = @" SELECT DOCNO FROM ME_DOCM WHERE DOCNO = :DOCNO ";
            return DBWork.Connection.Query<ME_DOCM>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }
        public IEnumerable<ComboItemModel> GetWhnoCombo(string tuser)
        {
            //string sql = @"SELECT a.INID AS VALUE, a.INID ||' '|| b.WH_NAME AS TEXT
            //                 FROM UR_ID a, MI_WHMAST b
            //                WHERE a.TUSER = :tuser
            //                  AND b.WH_NO = a.INID";select wh_no
            

            string sql = @"select wh_no as VALUE, wh_name as text from mi_whmast where wh_kind = '0' and wh_grade = '2' order by wh_no ";

            return DBWork.Connection.Query<ComboItemModel>(sql, new { TUSER = tuser }, DBWork.Transaction);
        }
        public IEnumerable<ComboItemModel> GetFrwhCombo()
        {
            string sql = @"select wh_no as VALUE, wh_no || ' ' || wh_name as text from mi_whmast where wh_kind = '0' and wh_grade = '2' order by wh_no ";

            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }
        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0,string kind, int page_index, int page_size, string sorters)
        {
            string sql = "";
            var p = new DynamicParameters();
            if (kind == "一般藥")
            {
                sql = @"SELECT DISTINCT {0} b.MMCODE , b.MMNAME_C, b.MMNAME_E, B.BASE_UNIT from MI_WINVCTL a, mi_mast b where a.mmcode = b.mmcode and b.mat_class = '01' and cancel_id = 'N' and b.e_restrictcode = 'N'";

            }
            else
            {
                 sql = @"SELECT DISTINCT {0} b.MMCODE , b.MMNAME_C, b.MMNAME_E, B.BASE_UNIT from MI_WINVCTL a, mi_mast b where a.mmcode = b.mmcode and b.mat_class = '01' and cancel_id = 'N' and b.e_restrictcode in ('1','2','3')";

            }



            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR MMNAME_E LIKE :MMNAME_E ";
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


        public string GetDrugWhnoByINID(string inid)
        {
            string sql = @"select wh_no from MI_WHMAST where inid = :inid and wh_kind = '0'";
            return DBWork.Connection.QueryFirst<string>(sql, new { inid = inid }, DBWork.Transaction);
        }
    }
}