using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using JCLib.DB;
using JCLib.Mvc;
using Dapper;
using WebApp.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using MMSMSREPORT.Models;


namespace WebApp.Repository.AB
{
    public class AB0012Repository : JCLib.Mvc.BaseRepository
    {
        public AB0012Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAll(ME_DOCM_QUERY_PARAMS query)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT DOCNO, TWN_DATE(APPTIME) AS APPTIME
                    , APPID || ' ' || USER_NAME(APPID) AS APP_NAME
                    , USER_NAME(APPID) AS APPID 
                    , APPDEPT || ' ' || INID_NAME(APPDEPT) AS APPDEPT_NAME
                    , (SELECT FLOWID || ' ' || FLOWNAME FROM ME_FLOW WHERE FLOWID=A.FLOWID) FLOWID
                    , TOWH, TOWH || ' ' || WH_NAME(TOWH) AS TOWH_NAME
                    , FRWH || ' ' || WH_NAME(FRWH) AS FRWH_NAME
                    , RETURN_NOTE
                    FROM ME_DOCM A WHERE 1=1 ";

            if (query.DOCNO != "")
            {
                sql += " AND A.DOCNO LIKE :DOCNO ";
                p.Add(":DOCNO", string.Format("%{0}%", query.DOCNO));
            }

            if (query.APPID != "")
            {
                //sql += " AND A.APPID LIKE :APPID ";
                //p.Add(":APPID", string.Format("%{0}%", query.APPID));
                sql += " AND ( a.appid = user_id(:APPID) or :APPID is null ) ";
                p.Add(":APPID", string.Format("{0}", query.APPID));
            }

            if (query.APPDEPT != "")
            {
                sql += " AND A.APPDEPT LIKE :APPDEPT ";
                p.Add(":APPDEPT", string.Format("%{0}%", query.APPDEPT));
            }

            if (query.DOCTYPE != "")
            {
                sql += " AND A.DOCTYPE LIKE :DOCTYPE ";
                p.Add(":DOCTYPE", string.Format("%{0}%", query.DOCTYPE));
            }

            if (query.FLOWID != "")
            {
                string[] tmp = query.FLOWID.Split(',');
                sql += " AND A.FLOWID IN :FLOWID";
                p.Add(":FLOWID", tmp);
            }

            if (query.TOWH != "")
            {
                sql += " AND A.TOWH LIKE :TOWH ";
                p.Add(":TOWH", string.Format("%{0}%", query.TOWH));
            }

            if (query.FRWH != "")
            {
                sql += " AND A.FRWH LIKE :FRWH ";
                p.Add(":FRWH", string.Format("%{0}%", query.FRWH));
            }

            if (query.APPTIME_S != "" && query.APPTIME_E != "")
            {
                sql += " AND TO_DATE(A.APPTIME) BETWEEN TO_DATE(:APPTIME_S, 'yyyy/mm/dd') AND TO_DATE(:APPTIME_E, 'yyyy/mm/dd')";
                p.Add(":APPTIME_S", string.Format("{0}", query.APPTIME_S));
                p.Add(":APPTIME_E", string.Format("{0}", query.APPTIME_E));
            }
            if (query.APPTIME_S != "" && query.APPTIME_E == "")
            {
                sql += " AND TO_DATE(A.APPTIME) BETWEEN TO_DATE(:APPTIME_S, 'yyyy/mm/dd') AND TO_DATE('3000/01/01', 'yyyy/mm/dd')";
                p.Add(":APPTIME_S", string.Format("{0}", query.APPTIME_S));
            }
            if (query.APPTIME_S == "" && query.APPTIME_E != "")
            {
                sql += " AND TO_DATE(A.APPTIME) BETWEEN TO_DATE('1900/01/01', 'yyyy/mm/dd') AND TO_DATE(:APPTIME_E, 'yyyy/mm/dd')";
                p.Add(":APPTIME_E", string.Format("{0}", query.APPTIME_E));
            }
            return DBWork.PagingQuery<ME_DOCM>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetMeDocd(ME_DOCD_QUERY_PARAMS query)
        {
            var p = new DynamicParameters();
            //string sql = @"SELECT a.DOCNO, a.SEQ, a.MMCODE, a.APPQTY, A.APVQTY, A.ACKQTY, a.APL_CONTIME, a.APLYITEM_NOTE, b.MMNAME_C, b.MMNAME_E, b.M_CONTPRICE, b.BASE_UNIT
            //            , CASE WHEN C.INV_QTY IS NULL THEN 0 ELSE C.INV_QTY END AS INV_QTY_FR
            //            , CASE WHEN D.INV_QTY IS NULL THEN 0 ELSE D.INV_QTY END AS INV_QTY_TO
            //            , TWN_DATE(A.APVTIME) AS APVTIME
            //            , TWN_DATE(A.ACKTIME) AS ACKTIME
            //            FROM ME_DOCD a LEFT JOIN MI_MAST b ON a.MMCODE=b.MMCODE 
            //            LEFT JOIN MI_WHINV C ON A.MMCODE=C.MMCODE AND C.WH_NO=:FRWH
            //            LEFT JOIN MI_WHINV D ON A.MMCODE=D.MMCODE AND D.WH_NO=:TOWH
            //            WHERE 1=1 ";
            string sql = "SELECT a.DOCNO, a.SEQ, a.MMCODE, a.APPQTY, A.APVQTY, A.ACKQTY, a.APL_CONTIME, a.APLYITEM_NOTE, a.FRWH_D, b.MMNAME_C, b.MMNAME_E, b.M_CONTPRICE, b.BASE_UNIT"
                        + ", TWN_DATE(A.APVTIME) AS APVTIME"
                        + ", TWN_DATE(A.ACKTIME) AS ACKTIME"
                        + ", NVL(A.SAFE_QTY, 0) SAFE_QTY"
                        + ", NVL(A.OPER_QTY, 0) OPER_QTY"
                        + ", A.PACK_QTY, A.PACK_UNIT, A.E_ORDERDCFLAG"
                        + ", NVL(INV_QTY(c.TOWH, a.MMCODE), 0) INV_QTY"
                        + ", NVL(INV_QTY(c.FRWH, a.MMCODE), 0) S_INV_QTY"
                        + ", (select DISC_CPRICE from MI_MAST where mmcode = a.mmcode) as DISC_CPRICE"
                        //+ ", NVL(A.S_INV_QTY, 0) S_INV_QTY"
                        + ", ( SELECT NVL(SUM(D.APPQTY), 0) APPQTY"
                        + "      FROM ME_DOCM C, ME_DOCD D"
                        + "      WHERE C.FLOWID = '0602'"
                        + "      AND   C.DOCNO = D.DOCNO"
                        + "      AND   D.MMCODE = a.MMCODE) APP_QTY_NOT_APPROVED "
                        + "FROM ME_DOCD a "
                        + "INNER JOIN ME_DOCM c on a.DOCNO=C.DOCNO "
                        + "LEFT JOIN MI_MAST b ON a.MMCODE=b.MMCODE "
                        + "WHERE 1=1 ";

            if (query.DOCNO != "")
            {
                sql += " AND a.DOCNO=:p0 ";
                p.Add(":p0", string.Format("{0}", query.DOCNO));
            }

            p.Add(":FRWH", string.Format("{0}", query.FRWH));
            p.Add(":TOWH", string.Format("{0}", query.TOWH));

            return DBWork.PagingQuery<ME_DOCD>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetPrintData(ME_DOCM_QUERY_PARAMS query)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT B.MMCODE,           
                       MMCODE_NAME(B.MMCODE) AS MMCODE_NAME,
                       NVL(B.SAFE_QTY, 0) SAFE_QTY,
                       NVL(B.OPER_QTY, 0) OPER_QTY,
                       NVL(INV_QTY(A.TOWH, B.MMCODE), 0) INV_QTY,
                    (SELECT COUNT(*) FROM ME_DOCM AA, ME_DOCD BB WHERE AA.DOCNO = BB.DOCNO AND AA.TOWH = A.TOWH AND BB.MMCODE = B.MMCODE AND TWN_DATE(A.APPTIME) >= TWN_DATE(ADD_MONTHS(SYSDATE, -3))) AS LAST_X_MONTH_ITEM,
                     (SELECT SUM(B.APPQTY) FROM ME_DOCM AA, ME_DOCD BB WHERE AA.DOCNO = BB.DOCNO AND AA.TOWH = A.TOWH AND BB.MMCODE = B.MMCODE AND TWN_DATE(A.APPTIME) >= TWN_DATE(ADD_MONTHS(SYSDATE, -3))) AS LAST_X_MONTH_APPQTY,
                         A.FRWH as FRWH_D,
                       NVL(INV_QTY(A.FRWH, B.MMCODE), 0) S_INV_QTY,
                       NVL(APL_QTY(A.TOWH, B.MMCODE), 0) AS APL_QTY,
                       B.APPQTY, B.APLYITEM_NOTE
                FROM ME_DOCM A, ME_DOCD B
                WHERE 1 = 1
                AND A.DOCNO = :DOCNO
                 AND A.DOCNO = B.DOCNO ORDER BY SEQ";

            p.Add(":DOCNO", query.DOCNO);

            return DBWork.Connection.Query<ME_DOCD>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetAB0012()
        {
            string sql = @"select a.ordercode MMCODE, a.Stockunit BASE_UNIT, sum(a.useqty) APPQTY, b.E_DRUGCLASSIFY from ME_AB0012 a inner join MI_MAST b on a.ordercode=b.mmcode
                            where a.RDOCNO is NULL
                            group by a.ordercode, a.stockunit, b.E_DRUGCLASSIFY
                            order by b.E_DRUGCLASSIFY";

            return DBWork.Connection.Query<ME_DOCD>(sql, DBWork.Transaction);
        }
        public IEnumerable<ME_DOCD> GetWinctl(string whno)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT A.MMCODE,ROUND(HIGH_QTY - INV_QTY(A.WH_NO,A.MMCODE)) APPQTY, SUPPLY_WHNO FRWH_D,
                                  SAFE_QTY,OPER_QTY,PACK_QTY,PACK_UNIT, E_ORDERDCFLAG(A.MMCODE)E_ORDERDCFLAG 
                            FROM MI_WINVCTL A, ME_UIMAST B
                            WHERE A.WH_NO = :TOWH
                              AND INV_QTY(A.WH_NO,A.MMCODE) < HIGH_QTY
                              AND E_RESTRICTCODE(A.MMCODE) <> 'N'
                              AND A.WH_NO = B.WH_NO  AND A.MMCODE=B.MMCODE ORDER BY MMCODE ";

            p.Add(":TOWH", whno);

            return DBWork.Connection.Query<ME_DOCD>(sql, p, DBWork.Transaction);
        }

        public int UpdateAB0012(string docno, string seq, string mmcode)
        {
            var sql = @"UPDATE ME_AB0012 SET RDOCNO=:RDOCNO, RSEQ=:RSEQ WHERE RDOCNO is NULL AND ORDERCODE=:ORDERCODE";
            return DBWork.Connection.Execute(sql, new { RDOCNO = docno, RSEQ = seq, ORDERCODE = mmcode }, DBWork.Transaction);
        }

        public int Import(IList<ME_AB0012Modles> getPubDrugList)
        {
            string sql = @"INSERT INTO MMSADM.ME_AB0012 (
                        ORDERNO, DETAILNO, NRCODE,  BEDNO, MEDNO, CHARTNO, VISITSEQ,
                        ORDERCODE, DOSE, ORDERDR, USEDATETIME, CREATEDATETIME, SIGNOPID, 
                        USEQTY, RESTQTY, PROVEDR, PROVEID2, MEMO, CHINNAME, ORDERENGNAME, 
                        SPECNUNIT, ORDERUNIT, STOCKUNIT, FLOORQTY, PROVEID1, CARRYKINDI, 
                        STOCKTRANSQTYI, STOCKCODE, STARTDATATIME) 
                        VALUES ( 
                        :ORDERNO, :DETAILNO, :NRCODE, :BEDNO, :MEDNO, :CHARTNO, :VISITSEQ,
                         :ORDERCODE, NVL(:DOSE, 0), :ORDERDR, :USEDATETIME, :CREATEDATETIME, :SIGNOPID,
                        NVL(:USEQTY, 0), NVL(:RESTQTY, 0), :PROVEDR, :PROVEID2, :MEMO, :CHINNAME, :ORDERENGNAME,
                        :SPECNUNIT, :ORDERUNIT, :STOCKUNIT, NVL(:FLOORQTY, 0), :PROVEID1, :CARRYKINDI,
                        NVL(:STOCKTRANSQTYI, 0), :STOCKCODE, :STARTDATATIME )";
            return DBWork.Connection.Execute(sql, getPubDrugList, DBWork.Transaction);
        }

        public int UpdateDllctl(string dllcode, string wh_no)
        {
            var sql = @"UPDATE ME_DLLCTL SET ENDDATE=SYSDATE WHERE DLLCODE=:DLLCODE AND WH_NO=:WH_NO";
            return DBWork.Connection.Execute(sql, new { DLLCODE = dllcode, WH_NO = wh_no }, DBWork.Transaction);
        }

        //public IEnumerable<COMBO_MODEL> GetTowhCombo(string inid)
        public IEnumerable<COMBO_MODEL> GetTowhCombo(string userId)
        {
            //string sql = @"SELECT WH_NO AS VALUE
            //                , WH_NO || ' ' || WH_NAME AS COMBITEM 
            //            FROM MI_WHMAST 
            //            WHERE INID=:INID
            //            order by WH_NO";
            //return DBWork.Connection.Query<COMBO_MODEL>(sql, new { INID = inid }, DBWork.Transaction);
            // TASK_ID=1 屬於藥材
            var sql = @"SELECT A.WH_NO AS VALUE, A.WH_NO || ' ' || A.WH_NAME AS COMBITEM 
                        FROM MI_WHMAST A INNER JOIN MI_WHID B ON A.WH_NO=B.WH_NO 
                        WHERE B.WH_USERID=:WH_USERID AND B.TASK_ID='1' AND A.WH_GRADE>'1' and a.cancel_id = 'N'";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { WH_USERID = userId }, DBWork.Transaction);
        }
        public IEnumerable<COMBO_MODEL> GetTowhComboQ(string userId)
        {

            var sql = @" SELECT WH_NO AS VALUE,WH_NAME(WH_NO)AS TEXT,WH_NO || ' ' ||WH_NAME(WH_NO) AS COMBITEM
                        FROM MI_WHID WHERE WH_USERID = :WH_USERID AND TASK_ID='1'  ORDER BY WH_NO ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { WH_USERID = userId }, DBWork.Transaction);
        }
        public IEnumerable<COMBO_MODEL> GetFrwhComboQ()
        {
            string sql = @"SELECT DISTINCT WH_NO as VALUE, WH_NAME as TEXT,
                        WH_NO || ' ' || WH_NAME as COMBITEM 
                        FROM MI_WHMAST 
                        WHERE 1=1 AND WH_KIND = '0' AND WH_GRADE < '3'  
                        ORDER BY WH_NO";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        // 取得核撥庫房, 只可選上級庫
        public IEnumerable<MI_WHMAST> GetFrwh(string inid, string wh_grade)
        {
            var sql = @"SELECT WH_NO, WH_GRADE || ' ' || WH_NO || ' ' || WH_NAME AS WH_NAME, SUPPLY_INID FROM MI_WHMAST WHERE wh_kind = '0' and WH_GRADE < :WH_GRADE ORDER BY WH_GRADE";
            return DBWork.Connection.Query<MI_WHMAST>(sql, new { INID = inid, WH_GRADE = wh_grade }, DBWork.Transaction);
        }

        public string GetFrwhWithMmcode(string towh, string mmcode)
        {
            //var sql = @"select CASE WHEN E_IFPUBLIC='1' THEN 'PH1A' WHEN E_IFPUBLIC='2' THEN 'PH1S' WHEN E_IFPUBLIC='3' THEN 'PH1A' END FRWH from MI_MAST where E_IFPUBLIC<>'0' and MMCODE=:MMCODE";
            var sql = @"SELECT SUPPLY_WHNO FROM MI_WINVCTL WHERE WH_NO = :TOWH AND MMCODE = :MMCODE";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { TOWH = towh, MMCODE = mmcode }, DBWork.Transaction);
        }

        public string GetMinSeq(string docno)
        {
            var sql = @"select MIN(SEQ) SEQ from me_docd where DOCNO=:DOCNO";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public string GetThisTowh(string docno)
        {
            var sql = @"select TOWH from me_docm where DOCNO=:DOCNO";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public string GetThisFrwh(string docno)
        {
            var sql = @"select FRWH from me_docm where DOCNO=:DOCNO";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public string GetThisApptime(string docno)
        {
            var sql = @"select twn_time(APPTIME) AS APPTIME from me_docm where DOCNO=:DOCNO";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public string GetThisPackQty(string docno, string seq)
        {
            var sql = @"select PACK_QTY from me_docd where DOCNO=:DOCNO AND SEQ=:SEQ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }

        public string GetPwhno(string userId)
        {
            // TASK_ID=1 屬於藥材
            var sql = @"select A.PWH_NO from MI_WHMAST A INNER JOIN MI_WHID B ON A.WH_NO=B.WH_NO WHERE B.WH_USERID=:WH_USERID AND B.TASK_ID=1";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { WH_USERID = userId }, DBWork.Transaction);
        }

        public string GetEnddate(string dllcode, string wh_no)
        {
            var sql = @"select TWN_TIME(ENDDATE) from me_dllctl where DLLCODE=:DLLCODE and WH_NO=:WH_NO";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { DLLCODE = dllcode, WH_NO = wh_no }, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetFrwhValue(string docno)
        {
            //var sql = @"select distinct SEQ, FRWH_D from me_docd where DOCNO=:DOCNO ORDER BY SEQ, FRWH_D";
            var sql = @"select distinct FRWH_D from me_docd where DOCNO=:DOCNO";
            return DBWork.Connection.Query<ME_DOCD>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMultiMmcode(MI_MAST_QUERY_PARAMS query)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT a.MMCODE, b.MMNAME_C,
                           MMCODE_NAME(a.MMCODE) MMNAME_E,
                           NVL(SAFE_QTY, 0) SAFE_QTY,  
                           NVL(OPER_QTY, 0) OPER_QTY,
                           STOCKUNIT(a.MMCODE) BASE_UNIT, 
                           NVL(INV_QTY(:TOWH, a.MMCODE), 0) TO_INV_QTY, 
                           NVL(INV_QTY(SUPPLY_WHNO, a.MMCODE), 0) FR_INV_QTY,
                           SUPPLY_WHNO || '_' || WH_NAME(SUPPLY_WHNO) SUPPLY_WHNO,
                            ( SELECT NVL(SUM(D.APPQTY), 0) APPQTY
                              FROM ME_DOCM C, ME_DOCD D
                              WHERE C.FLOWID = '0602'
                              AND   C.DOCNO = D.DOCNO
                              AND   D.MMCODE = a.MMCODE) APP_QTY_NOT_APPROVED,
                          (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='MI_MAST' AND DATA_NAME='E_RESTRICTCODE' AND DATA_VALUE=b.E_RESTRICTCODE)E_RESTRICTCODE 
                    FROM MI_WINVCTL a, MI_MAST b
                    WHERE A.MMCODE=B.MMCODE and WH_NO = :TOWH
                    AND CTDMDCCODE(:TOWH, a.MMCODE) <> '1' 
                    and a.ctdmdccode <> '1'
                    and b.E_ORDERDCFLAG = 'N'";

            p.Add(":TOWH", query.TOWH);
            //p.Add(":FRWH", query.FRWH);

            //if (query.WH_NO != "")
            //{
            //    sql += " AND B.WH_NO = :WH_NO ";
            //    p.Add(":WH_NO", string.Format("{0}", query.WH_NO));
            //}

            if (query.MMCODE != "")
            {
                sql += " AND upper(A.MMCODE) LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", query.MMCODE));
            }

            if (query.MMNAME_C != "")
            {
                sql += " AND upper(b.MMNAME_C) LIKE :MMNAME_C ";
                p.Add(":MMNAME_C", string.Format("%{0}%", query.MMNAME_C));
            }

            if (query.MMNAME_E != "")
            {
                sql += " AND upper(b.MMNAME_E) LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", query.MMNAME_E));
            }

            if (query.E_RESTRICTCODE != "")
            {
                if (query.E_RESTRICTCODE == "1")
                {
                    sql += " AND b.E_RESTRICTCODE IN ('0','1','2','3') ";
                }
                else if (query.E_RESTRICTCODE == "4")
                {
                    sql += " AND b.E_RESTRICTCODE ='4' ";
                }
                else if (query.E_RESTRICTCODE == "M")
                {
                    sql += " AND b.E_DRUGAPLTYPE ='1' ";
                }
                else
                {
                    sql += " AND b.E_RESTRICTCODE ='N' ";
                }
            }
            return DBWork.PagingQuery<MI_MAST>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMmcode(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            // 是否公藥E_IFPUBLIC  0-非公藥, 1-存點為病房，上級庫為住院藥局(PH1A), 2-存點為病房，上級庫為藥庫(PH1S), 3-存點為病房，設為備用藥，上級庫為住院藥局(PH1A)
            //string sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.M_CONTPRICE, A.BASE_UNIT FROM MI_MAST A INNER JOIN MI_WHMM B ON A.MMCODE=B.MMCODE WHERE 1=1 AND A.E_IFPUBLIC<>'0'";
            string sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT FROM MI_MAST A INNER JOIN MI_WINVCTL B ON A.MMCODE=B.MMCODE WHERE 1=1";

            if (query.WH_NO != "")
            {
                sql += " AND B.WH_NO = :WH_NO ";
                p.Add(":WH_NO", string.Format("{0}", query.WH_NO));
            }

            if (query.MMCODE != "")
            {
                sql += " AND upper(A.MMCODE) LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", query.MMCODE));
            }

            if (query.MMNAME_C != "")
            {
                sql += " AND upper(A.MMNAME_C) LIKE :MMNAME_C ";
                p.Add(":MMNAME_C", string.Format("%{0}%", query.MMNAME_C));
            }

            if (query.MMNAME_E != "")
            {
                sql += " AND upper(A.MMNAME_E) LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", query.MMNAME_E));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, string wh_no, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            //var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT FROM MI_MAST A INNER JOIN MI_WHMM B ON A.MMCODE=B.MMCODE WHERE 1=1 AND A.E_IFPUBLIC<>'0'";
            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT FROM MI_MAST A INNER JOIN MI_WINVCTL B ON A.MMCODE=B.MMCODE WHERE 1=1";
            if (wh_no != "")
            {
                sql += " AND B.WH_NO = :WH_NO ";
                p.Add(":WH_NO", string.Format("{0}", wh_no));
            }

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(A.MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(A.MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (upper(A.MMCODE) LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

                sql += " OR upper(A.MMNAME_E) LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR upper(A.MMNAME_C) LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }


        public int MasterCreate(ME_DOCM me_docm)
        {
            var sql = @"INSERT INTO ME_DOCM (DOCNO, DOCTYPE, FLOWID, APPID, APPDEPT, APPTIME, USEID, USEDEPT, TOWH, FRWH, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, MAT_CLASS)  
                                    VALUES (:DOCNO, :DOCTYPE, :FLOWID, :APPID, :APPDEPT, SYSDATE, :APPID, :APPDEPT, :TOWH, :FRWH, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP, :MAT_CLASS)";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCM> MasterGet(string docno)
        {
            var sql = @" SELECT DOCNO FROM ME_DOCM WHERE DOCNO = :DOCNO ";
            return DBWork.Connection.Query<ME_DOCM>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public int MasterUpdate(ME_DOCM me_docm)
        {
            var sql = @"UPDATE ME_DOCM SET FRWH=:FRWH, TOWH=:TOWH, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public int MasterUpdateFrwh(ME_DOCM me_docm)
        {
            var sql = @"UPDATE ME_DOCM SET FRWH=:FRWH, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public int MasterDelete(string docno)
        {
            var sql = @" DELETE from ME_DOCM WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public int DetailAllDelete(string docno)
        {
            var sql = @" DELETE from ME_DOCD WHERE DOCNO=:DOCNO";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public int DetailCreate(ME_DOCD me_docd)
        {
            var sql = @"INSERT INTO ME_DOCD (DOCNO, SEQ, MMCODE, APPQTY, FRWH_D, SAFE_QTY, OPER_QTY, PACK_QTY, PACK_UNIT, E_ORDERDCFLAG, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                    VALUES (:DOCNO, :SEQ, :MMCODE, :APPQTY, :FRWH_D, :SAFE_QTY, :OPER_QTY, :PACK_QTY, :PACK_UNIT, :E_ORDERDCFLAG, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public int DetailUpdate(ME_DOCD me_docd)
        {
            var sql = @"UPDATE ME_DOCD SET 
                        MMCODE=:MMCODE, APPQTY=:APPQTY, FRWH_D=:FRWH_D, UPDATE_TIME=SYSDATE, UPDATE_USER=:UPDATE_USER, 
                        UPDATE_IP=:UPDATE_IP, APLYITEM_NOTE=:APLYITEM_NOTE  
                        WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public int DetailUpdateDocno(string old_docno, string frwh, string new_docno, string update_user, string update_ip)
        {
            var sql = @"UPDATE ME_DOCD SET DOCNO=:NEW_DOCNO, UPDATE_TIME=SYSDATE, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP WHERE DOCNO=:DOCNO AND FRWH_D=:FRWH_D";
            return DBWork.Connection.Execute(sql, new { DOCNO = old_docno, FRWH_D = frwh, NEW_DOCNO = new_docno, UPDATE_USER = update_user, UPDATE_IP = update_ip }, DBWork.Transaction);
        }

        public int DetailDelete(string docno, string seq)
        {
            var sql = @" DELETE from ME_DOCD WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }

        public string GetTowh(string docno)
        {
            string sql = @"SELECT TOWH FROM ME_DOCM WHERE DOCNO=:DOCNO";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public string Get_SAFE_OPER_QTY(string towh, string mmcode)
        {
            string sql = @"SELECT SAFE_QTY || ',' || OPER_QTY FROM MI_WINVCTL WHERE WH_NO=:TOWH AND MMCODE=:MMCODE";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { TOWH = towh, MMCODE = mmcode }, DBWork.Transaction);
        }

        public string Get_PACK(string towh, string mmcode)
        {
            string sql = @"SELECT PACK_QTY || ',' || PACK_UNIT FROM ME_UIMAST WHERE WH_NO=:TOWH AND MMCODE=:MMCODE";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { TOWH = towh, MMCODE = mmcode }, DBWork.Transaction);
        }

        public string Get_ORDERDCFLAG(string mmcode)
        {
            string sql = @"SELECT E_ORDERDCFLAG FROM MI_MAST WHERE MMCODE=:MMCODE";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { MMCODE = mmcode }, DBWork.Transaction);
        }

        public int AB0012_NULL(string docno, string seq)
        {
            var sql = @"update ME_AB0012 set RDOCNO=NULL, RSEQ=NULL WHERE RDOCNO=:RDOCNO AND RSEQ=:RSEQ";
            return DBWork.Connection.Execute(sql, new { RDOCNO = docno, RSEQ = seq }, DBWork.Transaction);
        }

        public int Copy(string newDocno, string oldDocno, string user, string ip)
        {
            // copy master
            var sql = @"INSERT INTO ME_DOCM (DOCNO, DOCTYPE, FLOWID, APPID, APPDEPT, APPTIME, USEID, USEDEPT, FRWH, TOWH, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, srcDocno, mat_class)
                                    SELECT :NEWDOCNO, DOCTYPE, '0601', :CREATE_USER, APPDEPT, SYSDATE, :CREATE_USER, USEDEPT, FRWH, TOWH, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP, :NEWDOCNO, mat_class
                                      FROM ME_DOCM where DOCNO=:OLDDOCNO";
            DBWork.Connection.Execute(sql, new { NEWDOCNO = newDocno, OLDDOCNO = oldDocno, CREATE_USER = user, UPDATE_USER = user, UPDATE_IP = ip }, DBWork.Transaction);

            // copy detail
            sql = @"INSERT INTO ME_DOCD (DOCNO, SEQ, MMCODE, APPQTY, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, pack_qty)  
                               SELECT :NEWDOCNO, SEQ, MMCODE, '0', SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP, pack_qty
                                 FROM ME_DOCD where DOCNO=:OLDDOCNO";

            return DBWork.Connection.Execute(sql, new { NEWDOCNO = newDocno, OLDDOCNO = oldDocno, CREATE_USER = user, UPDATE_USER = user, UPDATE_IP = ip }, DBWork.Transaction);
        }

        public int UpdateStatus(ME_DOCM me_docm)
        {
            var sql = @"UPDATE ME_DOCM SET FLOWID = :FLOWID, 
                                apptime = sysdate,
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }


        public DataTable GetExcel(string docno, string frwh, string towh)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT
                        a.SEQ AS 項次,
                        a.MMCODE AS 院內碼,
                        a.APPQTY AS 申請數量,
                        A.APVQTY AS 核撥數量,
                        A.ACKQTY AS 點收數量,
                        a.APLYITEM_NOTE AS 備註, 
                        b.MMNAME_C AS 中文品名,
                        b.MMNAME_E AS 英文品名,
                        b.BASE_UNIT AS 計量單位,
                        TWN_DATE(A.APVTIME) AS 核撥日期,
                        TWN_DATE(A.ACKTIME) AS 點收日期,
                        NVL(INV_QTY(c.TOWH, a.MMCODE), 0) AS 庫存量,
                        NVL(INV_QTY(c.FRWH, a.MMCODE), 0) AS 上級庫庫存量,
                        NVL(A.SAFE_QTY, 0) as 安全量,
                        NVL(A.OPER_QTY, 0) as 基準量,
                        A.PACK_QTY AS 包裝劑量,
                        A.PACK_UNIT AS 包裝單位,
                        A.E_ORDERDCFLAG AS 藥品停用碼,
                        ( SELECT NVL(SUM(D.APPQTY), 0) APPQTY
                              FROM ME_DOCM C, ME_DOCD D
                              WHERE C.FLOWID = '0602'
                              AND   C.DOCNO = D.DOCNO
                              AND   D.MMCODE = a.MMCODE) AS 未核撥數量
                        FROM ME_DOCD a 
                        INNER JOIN ME_DOCM c on a.DOCNO=C.DOCNO
                        LEFT JOIN MI_MAST b ON a.MMCODE=b.MMCODE 
                        WHERE 1=1 ";


            if (docno != "")
            {
                sql += " AND a.DOCNO=:p0 ";
                p.Add(":p0", docno);
            }

            p.Add(":FRWH", frwh);
            p.Add(":TOWH", towh);

            sql += @" ORDER BY seq";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<COMBO_MODEL> GetRestrictCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_MAST' AND DATA_NAME='E_RESTRICTCODE2' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public string getTowhFromDocno(string docno)
        {
            var sql = @" select TOWH from ME_DOCM where DOCNO = :DOCNO ";

            return DBWork.Connection.QueryFirst<string>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public IEnumerable<string> getMmcodeFromDocno(string docno)
        {
            string sql = @" select MMCODE from ME_DOCD where DOCNO = :DOCNO 
                        order by SEQ ";

            return DBWork.Connection.Query<string>(sql, new { DOCNO = docno });
        }

        public string getUserName(string userId)
        {
            var sql = @" select TUSER || '.' || UNA from UR_ID where TUSER = :USERID ";

            return DBWork.Connection.QueryFirst<string>(sql, new { USERID = userId }, DBWork.Transaction);
        }

        public string twnDateNow()
        {
            var sql = @" select twn_date(sysdate) || to_char(sysdate, 'HH24MISS') from dual ";

            return DBWork.Connection.QueryFirst<string>(sql, DBWork.Transaction);
        }

        // 取院內碼上一次申請的點收時間(若有單但尚無點收資料,預設為30天前;無單則回傳空白)
        public string getLastAcktimeFromMmcode(string mmcode, string towh, string docno)
        {
            var sql = @" select twn_date(nvl(max(ACKTIME), sysdate - 30)) || to_char(nvl(max(ACKTIME), sysdate - 30), 'HH24MISS') from ME_DOCD 
                        where DOCNO in (select DOCNO from ME_DOCM A where TOWH = :TOWH and (select count(*) from ME_DOCD where DOCNO = A.DOCNO and MMCODE = :MMCODE ) > 0)
                        and MMCODE = :MMCODE 
                        and DOCNO <> :DOCNO ";

            try
            {
                return DBWork.Connection.QueryFirst<string>(sql, new { MMCODE = mmcode, TOWH = towh, DOCNO = docno }, DBWork.Transaction);
            }
            catch (InvalidOperationException ex)
            {
                return "";
            }
        }

        // 取交易記錄檔中,該藥上次入庫使用的申請單號
        public string getLastAckDocno(string mmcode, string towh, string docno)
        {
            var sql = @" select TR_DOCNO from (
                        select * from MI_WHTRNS 
                        where TR_DOCNO in (select DOCNO from ME_DOCM A where TOWH = :TOWH and (select count(*) from ME_DOCD where DOCNO = A.DOCNO and MMCODE = :MMCODE ) > 0)
                        and MMCODE = :MMCODE 
                        and TR_DOCNO <> :DOCNO
                        and TR_MCODE = 'APLI'
                        order by TR_DATE desc)
                        where rownum = 1 ";

            try
            {
                return DBWork.Connection.QueryFirst<string>(sql, new { MMCODE = mmcode, TOWH = towh, DOCNO = docno }, DBWork.Transaction);
            }
            catch (InvalidOperationException ex)
            {
                return "";
            }
        }

        public string getMMNAME_E(string mmcode)
        {
            var sql = @" select MMNAME_E from MI_MAST
                        where MMCODE = :MMCODE ";

            try
            {
                return DBWork.Connection.QueryFirst<string>(sql, new { MMCODE = mmcode }, DBWork.Transaction);
            }
            catch (InvalidOperationException ex)
            {
                return "";
            }
        }

        // 取指定藥的日結存量
        public string getDayInvqty(string vDate, string wh_no, string mmcode)
        {
            var sql = @" select DAY_PINVQTY(:VDATE, :WH_NO, :MMCODE) from dual ";

            try
            {
                return DBWork.Connection.QueryFirst<string>(sql, new { VDATE = vDate, WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction);
            }
            catch (InvalidOperationException ex)
            {
                return "";
            }
        }

        // 取交易記錄檔中,該藥該庫上次入庫時間
        public string getLastApliTrDate(string docno, string mmcode)
        {
            var sql = @" select twn_date(TR_DATE) || to_char(TR_DATE, 'HH24MISS') from (
                        select * from MI_WHTRNS 
                        where TR_DOCNO = :DOCNO
                        and MMCODE = :MMCODE 
                        and TR_MCODE = 'APLI'
                        order by TR_DATE desc)
                        where rownum = 1 ";

            try
            {
                return DBWork.Connection.QueryFirst<string>(sql, new { DOCNO = docno, MMCODE = mmcode }, DBWork.Transaction);
            }
            catch (InvalidOperationException ex)
            {
                return "1090731235900";
            }
        }

        // 領入數量=取上一筆申請單在MI_WHTRNS,所有APLI的異動數量和
        public string getLastApliTrInv(string docno, string mmcode)
        {
            var sql = @" select sum(TR_INV_QTY) from MI_WHTRNS 
                        where TR_DOCNO = :DOCNO
                        and MMCODE = :MMCODE 
                        and TR_MCODE = 'APLI' ";

            try
            {
                return DBWork.Connection.QueryFirst<string>(sql, new { DOCNO = docno, MMCODE = mmcode }, DBWork.Transaction);
            }
            catch (InvalidOperationException ex)
            {
                return "";
            }
        }

        // 結存量=取上一筆申請單在MI_WHTRNS,APLI最後一筆的異動後數量
        public string getLastApliAF(string docno, string mmcode, string stockcode)
        {
            var sql = @" select AF_TR_INVQTY from (
                        select * from MI_WHTRNS 
                        where TR_DOCNO = :DOCNO
                        and MMCODE = :MMCODE 
                        and TR_MCODE = 'APLI'
                        order by TR_DATE desc)
                        where rownum = 1 ";

            try
            {
                return DBWork.Connection.QueryFirst<string>(sql, new { DOCNO = docno, MMCODE = mmcode }, DBWork.Transaction);
            }
            catch (InvalidOperationException ex)
            {
                // 若無資料則取MI_WHINV_INIT的原始結存
                return getLastApliAF_INIT(docno, mmcode, stockcode);
            }
        }

        // 取原始結存量
        public string getLastApliAF_INIT(string docno, string mmcode, string stockcode)
        {
            var sql = @" select INV_QTY from MI_WHINV_INIT
                        where WH_NO = :WH_NO
                        and MMCODE = :MMCODE ";

            try
            {
                return DBWork.Connection.QueryFirst<string>(sql, new { WH_NO = stockcode, MMCODE = mmcode }, DBWork.Transaction);
            }
            catch (InvalidOperationException ex)
            {
                return "";
            }
        }


        public int AB0012aErrLog(string send, string receive, string msg, string userId)
        {
            string sql = @"INSERT INTO ERROR_LOG (
                        LOGTIME, PG, MSG,  USERID) 
                        VALUES ( 
                        sysdate, 'AB0012a', 'SEND:' || :SEND || ',RECEIVE:' || :RECEIVE || ',MSG:' || :MSG, :USERID )";
            return DBWork.Connection.Execute(sql, new { SEND = send, RECEIVE = receive, MSG = msg, USERID = userId }, DBWork.Transaction);
        }

        public class ME_DOCM_QUERY_PARAMS
        {
            public string DOCNO;
            public string DOCTYPE;
            public string APPID;
            public string APPDEPT;
            public string FLOWID;
            public string FRWH;
            public string TOWH;

            public string APPTIME_S;
            public string APPTIME_E;
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
            public string TOWH;
            public string FRWH;
            public string E_RESTRICTCODE;
        }

        public class ME_DOCD_QUERY_PARAMS
        {
            public string DOCNO;
            public string FRWH;
            public string TOWH;
        }

        #region 2020-06-23 送核撥更新me_docd.apl_contime
        public int UpdateDocdAplcontime(string docno, string update_user, string update_ip)
        {
            string sql = @"update ME_DOCD
                              set apl_contime = sysdate,
                                  APVQTY=APPQTY,
                                  update_time = sysdate,
                                  update_ip = :update_ip,
                                  update_user = :update_user
                            where docno = :docno";
            return DBWork.Connection.Execute(sql, new { docno = docno, update_user = update_user, update_ip = update_ip }, DBWork.Transaction);
        }
        #endregion

        #region 2020-09-23 新增: 麻藥管制表多顯示退藥資訊

        public IEnumerable<APIResultData> GetReturnDatas(string start_time, string wh_no, string mmcode) {
            string sql = @"select :start_time as startdatatime,
                                  twn_time(tr_date)  as usedatetime,
                                  mmcode as ordercode,
                                  mmcode_name(mmcode) orderengname,
                                  (select low_qty from MI_WINVCTL where wh_no = a.wh_no and mmcode = a.mmcode) as floorqty,
                                  (select e_specnunit from MI_MAST where  mmcode = a.mmcode) as specnunit,
                                  (select base_unit from MI_MAST where  mmcode = a.mmcode) as base_unit,
                                  '藥品繳庫' as chinname,
                                  tr_inv_qty as useqty,
                                  wh_no as stockcode,
                                  twn_time(tr_date)  as createdatetime
                             from MI_WHTRNS a
                            where twn_time(tr_date) >= :start_time
                              and wh_no = :wh_no
                              and mmcode = :mmcode
                              and tr_mcode = 'BAKO'
                              and tr_doctype = 'RN'";

            return DBWork.Connection.Query<APIResultData>(sql, new { start_time  = start_time , wh_no = wh_no, mmcode = mmcode}, DBWork.Transaction);
        }

        #endregion

        #region 2020-09-30 新增: 麻藥管製錶取得多筆點收資料

        public IEnumerable<APIResultData> GetApplyInDatas(string start_time, string wh_no, string mmcode)
        {
            string sql = @"select :start_time as startdatatime,
                                  twn_time(tr_date)  as usedatetime,
                                  mmcode as ordercode,
                                  mmcode_name(mmcode) orderengname,
                                  (select low_qty from MI_WINVCTL where wh_no = a.wh_no and mmcode = a.mmcode) as floorqty,
                                  (select e_specnunit from MI_MAST where  mmcode = a.mmcode) as specnunit,
                                  (select base_unit from MI_MAST where  mmcode = a.mmcode) as base_unit,
                                  '撥發入庫' as chinname,
                                  tr_inv_qty as inv_qty,
                                  wh_no as stockcode,
                                  twn_time(tr_date)  as createdatetime
                             from MI_WHTRNS a
                            where twn_time(tr_date) >= :start_time
                              and wh_no = :wh_no
                              and mmcode = :mmcode
                              and tr_mcode = 'APLI'";

            return DBWork.Connection.Query<APIResultData>(sql, new { start_time = start_time, wh_no = wh_no, mmcode = mmcode }, DBWork.Transaction);
        }

        #endregion

        #region 2020-11-12 新增：麻藥管制表取得盤點異動資料
        public IEnumerable<APIResultData> GetChkDatas(string start_time, string wh_no, string mmcode) {
            string sql = @"select :start_time as startdatatime,
                                  twn_time(tr_date)  as usedatetime,
                                  mmcode as ordercode,
                                  mmcode_name(mmcode) orderengname,
                                  (select low_qty from MI_WINVCTL where wh_no = a.wh_no and mmcode = a.mmcode) as floorqty,
                                  (select e_specnunit from MI_MAST where  mmcode = a.mmcode) as specnunit,
                                  (select base_unit from MI_MAST where  mmcode = a.mmcode) as base_unit,
                                  '盤點異動' as chinname,
                                  ((tr_inv_qty) * (-1)) as useqty,
                                  wh_no as stockcode,
                                  twn_time(tr_date)  as createdatetime
                             from MI_WHTRNS a
                            where twn_time(tr_date) >= :start_time
                              and wh_no = :wh_no
                              and mmcode = :mmcode
                              and tr_mcode = 'CHIO'";

            return DBWork.Connection.Query<APIResultData>(sql, new { start_time = start_time, wh_no = wh_no, mmcode = mmcode }, DBWork.Transaction);
        }

        #endregion

        #region 20201-05-06新增: 修改、刪除時檢查flowId
        public bool ChceckFlowId01(string docno)
        {
            string sql = @"
                select 1 from ME_DOCM
                 where docno = :docno
                   and (substr(flowId, length(flowId)-1 , 2) = '01'
                       or substr(flowId, length(flowId)-1 , 2) = '00'
                       or substr(flowId, length(flowId)-1 , 2) = '1')
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { docno }, DBWork.Transaction) != null;
        }
        #endregion

        #region 2021-07-30 新增: 新增主檔、送核撥 檢查申請庫房是否作廢
        public bool CheckIsTowhCancelByWhno(string towh)
        {
            string sql = @"
               select 1 from MI_WHMAST 
                where wh_no = :towh
                  and cancel_id = 'N'
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { towh }, DBWork.Transaction) == null;
        }

        public bool CheckIsTowhCancelByDocno(string docno)
        {
            string sql = @"
               select 1 from ME_DOCM  a
                where a.docno = :docno
                  and exists (select 1 from MI_WHMAST 
                               where wh_no = a.towh
                                 and cancel_id = 'N')
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { docno }, DBWork.Transaction) == null;
        }
        #endregion

        #region 2021-11-24 新增：麻藥管制表取得調帳資料
        public IEnumerable<APIResultData> GetAdjDatas(string start_time, string wh_no, string mmcode)
        {
            string sql = @"select :start_time as startdatatime,
                                  twn_time(tr_date)  as usedatetime,
                                  mmcode as ordercode,
                                  mmcode_name(mmcode) orderengname,
                                  (select low_qty from MI_WINVCTL where wh_no = a.wh_no and mmcode = a.mmcode) as floorqty,
                                  (select e_specnunit from MI_MAST where  mmcode = a.mmcode) as specnunit,
                                  (select base_unit from MI_MAST where  mmcode = a.mmcode) as base_unit,
                                  (case when tr_mcode = 'ADJI' 
                                        then '調帳入庫'
                                        else '調帳出庫' end) as chinname,
                                  (case when tr_mcode = 'ADJI' 
                                        then tr_inv_qty *(-1)
                                        else tr_inv_qty end) as useqty,
                                  wh_no as stockcode,
                                  twn_time(tr_date)  as createdatetime
                             from MI_WHTRNS a
                            where twn_time(tr_date) >= :start_time
                              and wh_no = :wh_no
                              and mmcode = :mmcode
                              and tr_doctype = 'AJ'";

            return DBWork.Connection.Query<APIResultData>(sql, new { start_time = start_time, wh_no = wh_no, mmcode = mmcode }, DBWork.Transaction);
        }

        public string GetSpecnunit(string mmcode) {
            string sql = @"
                select e_specnunit from MI_MAST where mmcode = :mmcode
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { mmcode }, DBWork.Transaction);
        }

        public string GetBaseunit(string mmcode)
        {
            string sql = @"
                select base_unit from MI_MAST where mmcode = :mmcode
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { mmcode }, DBWork.Transaction);
        }

        public string GetFloorQty(string wh_no, string mmcode)
        {
            string sql = @"
                select low_qty from MI_WINVCTL where mmcode = :mmcode and wh_no = :wh_no
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { mmcode , wh_no }, DBWork.Transaction);
        }

        #endregion
    }
}