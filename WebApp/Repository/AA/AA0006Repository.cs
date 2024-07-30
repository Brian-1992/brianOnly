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
    public class AA0006_Master : JCLib.Mvc.BaseModel
    {
        public string PO_NO { get; set; }
        public string MMCODE { get; set; }
        public string SEQ { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string STOREID { get; set; }
        public string ACC_QTY { get; set; }
        public string ACC_BASEUNIT { get; set; }
        public string BW_SQTY { get; set; }
        public string LOT_NO { get; set; }
        public string EXP_DATE { get; set; }
        public string STATUS { get; set; }
        public string ACC_USER { get; set; }
        public string ACC_TIME { get; set; }
    }

    public class AA0006_Detail : JCLib.Mvc.BaseModel
    {
        public string PO_NO { get; set; }
        public string MMCODE { get; set; }
        public string SEQ { get; set; }
        public string PR_DEPT { get; set; }
        public string PR_QTY { get; set; }
        public string DIST_BASEUNIT { get; set; }
        public string DIST_STATUS { get; set; }
        public string BW_SQTY { get; set; }
        public string DIST_QTY { get; set; }
        public string DIST_MEMO { get; set; }
        public string LOT_NO { get; set; }
        public string EXP_DATE { get; set; }
        public string DOCNO { get; set; }
        public string DIST_USER { get; set; }
        public string INID_NAME { get; set; }
        public string SUMDIST_QTY { get; set; }

        public string SUM_ACKQTY { get; set; }
    }
    public class AA0006_AckDetail {
        public string TR_DATE { get; set; }
        public string MMCODE { get; set; }
        public string TR_INV_QTY { get; set; }
    }
    public class AA0006Repository : JCLib.Mvc.BaseRepository
    {
        public AA0006Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0006_Master> GetMasterAll(string MATCLS, string ACC_TIME_Start, string ACC_TIME_End, string MMCODE, string DIST)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT a.PO_NO, a.MMCODE, a.SEQ, b.MMNAME_C, b.MMNAME_E, 
                        (case when a.STOREID='1' then '庫備' when a.STOREID='0' then '非庫備' end) STOREID,
                        a.ACC_QTY, ACC_BASEUNIT, BW_SQTY, LOT_NO, 
                        TWN_DATE(EXP_DATE) EXP_DATE, 
                        (SELECT DATA_VALUE ||':'|| DATA_DESC FROM PARAM_D WHERE GRP_CODE = 'BC_CS_ACC_LOG' AND DATA_NAME = 'STATUS' AND DATA_VALUE = STATUS ) STATUS, 
                        USER_NAME(ACC_USER) ACC_USER, TWN_TIME(ACC_TIME) ACC_TIME
                        FROM BC_CS_ACC_LOG a, MI_MAST b
                        WHERE a.MMCODE = b.MMCODE 
                        AND a.STOREID = '0' --僅顯示非庫備
                        AND a.MAT_CLASS = :MATCLS
                        AND trunc(a.ACC_TIME) >= TWN_TODATE(:ACC_TIME_Start) 
                        AND trunc(a.ACC_TIME) <= TWN_TODATE(:ACC_TIME_End) ";
            if (DIST == "L")
                sql += " AND a.seq in (select seq from BC_CS_DIST_LOG where po_no = a.po_no and mmcode = a.mmcode and dist_status in ('C','L') ) ";
            else if (DIST == "C")
                sql += " AND a.seq in (select seq from BC_CS_DIST_LOG where po_no = a.po_no and mmcode = a.mmcode and dist_status ='T' ) ";
            else
                sql += " AND a.seq in (select seq from BC_CS_DIST_LOG where po_no = a.po_no and mmcode = a.mmcode ) ";
            if (MMCODE != "")
            {
                sql += " AND (b.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", MMCODE));

                sql += " OR b.MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", MMCODE));

                sql += " OR b.MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", MMCODE));
            }
            sql += " ORDER BY a.MMCODE ";
            p.Add(":MATCLS", MATCLS);
            p.Add(":ACC_TIME_Start", ACC_TIME_Start);
            p.Add(":ACC_TIME_End", ACC_TIME_End);

            return DBWork.PagingQuery<AA0006_Master>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<AA0006_Detail> GetDetailAll(string PO_NO, string MMCODE, string SEQ)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT PO_NO, MMCODE, SEQ, PR_DEPT, PR_QTY, DIST_BASEUNIT, INID_NAME,
                        (CASE WHEN DIST_STATUS='L' THEN 'L:待分配' 
                        WHEN DIST_STATUS='C' THEN 'C:待分配'
                        WHEN DIST_STATUS='T' THEN 'T:待點收' END) DIST_STATUS,   
                        (select nvl(sum(dist_qty),0) from BC_CS_DIST_LOG where po_no=a.po_no and mmcode=a.mmcode and pr_dept=a.pr_dept and dist_status ='T'  ) SUMDIST_QTY,
                        (select sum(TR_INV_QTY) from MI_WHTRNS
                          where TR_DOCTYPE='MR4' and TR_MCODE='APLI'
                            and WH_NO=a.PR_DEPT and MMCODE=a.MMCODE and TR_DOCNO=a.DOCNO) as SUM_ACKQTY, 
                        BW_SQTY, DIST_QTY, DIST_MEMO, LOT_NO, TWN_DATE(EXP_DATE) EXP_DATE, DOCNO
                        FROM BC_CS_DIST_LOG a, UR_INID b
                        WHERE a.PR_DEPT=b.INID
                        AND PO_NO = :PO_NO
                        AND MMCODE = :MMCODE
                        AND SEQ = :SEQ
                        ORDER BY PR_QTY, PR_DEPT";

            p.Add(":PO_NO", PO_NO);
            p.Add(":MMCODE", MMCODE);
            p.Add(":SEQ", SEQ);

            return DBWork.Connection.Query<AA0006_Detail>(sql, p, DBWork.Transaction);
        }

        public int UpdateDetail(AA0006_Detail tmp_Detail)
        {
            var sql = @"UPDATE BC_CS_DIST_LOG 
                        SET BW_SQTY = :BW_SQTY , DIST_QTY = :DIST_QTY, 
                        DIST_MEMO = :DIST_MEMO, DIST_STATUS='C', DIST_TIME = SYSDATE, DIST_USER = :DIST_USER
                        WHERE PO_NO = :PO_NO
                        AND MMCODE = :MMCODE
                        AND SEQ = :SEQ
                        AND PR_DEPT = :PR_DEPT and DIST_STATUS='L'";

            return DBWork.Connection.Execute(sql, tmp_Detail, DBWork.Transaction);
        }

        public int RestoreDetail(AA0006_Detail tmp_Detail)
        {
            var sql = @"UPDATE BC_CS_DIST_LOG 
                        SET DIST_STATUS='L', DIST_TIME = SYSDATE, DIST_USER = :DIST_USER
                        WHERE PO_NO = :PO_NO
                        AND MMCODE = :MMCODE
                        AND SEQ = :SEQ
                        AND PR_DEPT = :PR_DEPT";

            return DBWork.Connection.Execute(sql, tmp_Detail, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetMatClassCombo(string str_UserID)
        {
            string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM
                        FROM MI_MATCLASS 
                        where MAT_CLASS between '02' AND '08' ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { UserID = str_UserID });
        }

        public IEnumerable<COMBO_MODEL> GetAGEN_NO()
        {
            string sql = @"SELECT AGEN_NO VALUE,
                                  AGEN_NO || ' ' || TRIM(AGEN_NAMEC) COMBITEM
                           FROM PH_VENDER
                           ORDER BY AGEN_NO";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetPO_NO(string MATCLS, string ACC_TIME_Start, string ACC_TIME_End, string AGEN_NO)
        {
            var p = new DynamicParameters();

            string sql = @"SELECT DISTINCT PO_NO AS VALUE,
                            PO_NO AS COMBITEM
                            FROM BC_CS_ACC_LOG
                            WHERE MAT_CLASS = :MATCLS
                            AND AGEN_NO = :AGEN_NO
                            AND trunc(ACC_TIME) >= TWN_TODATE(:ACC_TIME_Start) 
                            AND trunc(ACC_TIME) <= TWN_TODATE(:ACC_TIME_End) 
                            ORDER BY PO_NO";

            p.Add(":MATCLS", MATCLS);
            p.Add(":AGEN_NO", AGEN_NO);
            p.Add(":ACC_TIME_Start", ACC_TIME_Start);
            p.Add(":ACC_TIME_End", ACC_TIME_End);

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }

        public string CallSP_NumberCheck(string PO_NO, string MMOCDE, string USER_ID, string USER_IP, out string RetMsg)
        {
            var p = new OracleDynamicParameters();
            //傳入參數
            p.Add("I_PONO", value: PO_NO, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("I_MMCODE", value: MMOCDE, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("I_USERID", value: USER_ID, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("I_UPDIP", value: USER_IP, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);

            //傳出參數
            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);
            p.Add("O_RETMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            //要呼叫的SP
            DBWork.Connection.Query("INV_SET.DIST_IN", p, commandType: CommandType.StoredProcedure);
            //讀取傳出參數
            string RetCode = p.Get<OracleString>("O_RETID").Value;
            RetMsg = p.Get<OracleString>("O_RETMSG").Value;

            return RetCode;
        }
        public IEnumerable<MI_MAST> GetMMCODECombo(string mmcode, string mat_class, int page_index, int page_size, string sorters)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"SELECT DISTINCT {0} MMCODE , MMNAME_C, MMNAME_E from MI_MAST A WHERE 1=1 ";
            if (mat_class != "" && mat_class != null)  //物料分類
            {
                sql += " AND MAT_CLASS =:mat_class ";
                p.Add(":mat_class", mat_class);
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

        public IEnumerable<AA0006_AckDetail> GetAckDetails(string docno, string mmcode) {
            string sql = @"
                select twn_time(tr_date) as tr_date,
                       mmcode,
                       tr_inv_qty
                  from MI_WHTRNS
                 where tr_docType  = 'MR4'
                   and tr_mcode = 'APLI'
                   and mmcode = :mmcode
                   and tr_docno = :docno
            ";
            return DBWork.PagingQuery<AA0006_AckDetail>(sql, new { docno, mmcode }, DBWork.Transaction);
        }
    }
}