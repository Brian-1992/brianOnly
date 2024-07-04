using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.B
{
    public class BD0002Repository : JCLib.Mvc.BaseRepository
    {
        public BD0002Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //查詢Master
        public IEnumerable<BD0002M> GetAllM(string PR_DEPT, string MAT_CLASS, string START_DATE, string END_DATE, string XACTION, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"  SELECT MPM.PR_NO,
                                 TO_CHAR (MPM.PR_TIME, 'YYYYMMDD') - 19110000 PR_TIME,
                                 (SELECT MMS.MAT_CLASS || ' ' || MMS.MAT_CLSNAME TEXT
                                  FROM MI_MATCLASS MMS
                                  WHERE MMS.MAT_CLASS = SUBSTR (MPM.PR_NO, 20, 2)) MAT_CLASS,
                                 MPM.M_STOREID,
                                 (CASE
                                      WHEN MPM.M_STOREID = '1' THEN '庫備'
                                      WHEN MPM.M_STOREID = '0' THEN '非庫備'
                                 END) M_STOREID_NAME,
                                 (CASE
                                      WHEN MPM.M_STOREID = '1' THEN '1 庫備'
                                      WHEN MPM.M_STOREID = '0' THEN '0 非庫備'
                                 END) M_STOREID_CODE,
                                 MPM.PR_STATUS,
                                 (CASE
                                      WHEN MPM.PR_STATUS = '34' THEN '申購進貨完成'
                                      WHEN MPM.PR_STATUS = '35' THEN '申購單開立'
                                      WHEN MPM.PR_STATUS = '36' THEN '已轉訂單'
                                 END) PR_STATUS_NAME,
                                 (CASE
                                      WHEN MPM.PR_STATUS = '34' THEN '34 申購進貨完成'
                                      WHEN MPM.PR_STATUS = '35' THEN '35 申購單開立'
                                      WHEN MPM.PR_STATUS = '36' THEN '36 已轉訂單'
                                 END) PR_STATUS_CODE,
                                 MPM.XACTION,
                                 (CASE
                                      WHEN MPM.XACTION = '0' THEN '臨時申請'
                                      WHEN MPM.XACTION = '1' THEN '常態申請'
                                 END) XACTION_NAME,
                                 (CASE
                                      WHEN MPM.XACTION = '0' THEN '0 臨時申請'
                                      WHEN MPM.XACTION = '1' THEN '1 常態申請'
                                 END) XACTION_CODE
                                ,(case when create_user = '緊急醫療出貨' then '是' else '否' end) as is_cr
                          FROM MM_PR_M MPM
                          WHERE     SUBSTR (MPM.PR_NO, 7, 7) >= :START_DATE
                          AND       SUBSTR (MPM.PR_NO, 7, 7) <= :END_DATE
                          AND       MPM.XACTION = :XACTION
                          AND       MPM.MAT_CLASS = :MAT_CLASS
                          AND       MPM.PR_DEPT = :PR_DEPT
                          ORDER BY PR_NO";

            p.Add(":PR_DEPT", string.Format("{0}", PR_DEPT));
            p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
            p.Add(":START_DATE", string.Format("{0}", START_DATE));
            p.Add(":END_DATE", string.Format("{0}", END_DATE));
            p.Add(":XACTION", string.Format("{0}", XACTION));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BD0002M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //查詢Detail
        public IEnumerable<BD0002D> GetAllD(string PR_NO, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"  SELECT MPD.MMCODE,
                                 MMT.MMNAME_C,
                                 MMT.MMNAME_E,
                                 MMT.M_PURUN,
                                 MMT.M_AGENLAB,
                                 MPD.PR_QTY,
                                 MPD.PR_PRICE,
                                 MPD.M_CONTPRICE,
                                 MPD.UNIT_SWAP,
                                 MPD.AGEN_NO,
                                 MPD.DISC,
                                 MPD.REC_STATUS,
                                 MPD.AGEN_NAME,
                                 P.AGEN_TEL,
                                 MMT.BASE_UNIT,
                                 MPD.REQ_QTY_T
                          FROM MM_PR_D MPD, MI_MAST MMT, PH_VENDER P
                          WHERE MPD.MMCODE = MMT.MMCODE
                          AND   MPD.AGEN_NO = P.AGEN_NO
                          AND   MPD.PR_NO = :PR_NO
                          ORDER BY MPD.MMCODE";

            p.Add(":PR_NO", string.Format("{0}", PR_NO));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BD0002D>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public string GetINID(string USER_ID)
        {
            string sql = @" SELECT URID.INID
                            FROM UR_ID URID
                            WHERE URID.TUSER = :USER_ID";
            return DBWork.Connection.QueryFirst<string>(sql, new { USER_ID = USER_ID }, DBWork.Transaction).ToString();
        }

        //產生臨時申購訂單
        public string CALL_BD0002_0(string START_DATE, string END_DATE, string INID, string MAT_CLASS, string USERID, string USERIP, string WH_NO)
        {
            var p = new OracleDynamicParameters();
            p.Add("i_yyymmdd_s", value: START_DATE, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 7);
            p.Add("i_yyymmdd_e", value: END_DATE, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 7);
            p.Add("i_inid", value: INID, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 6);
            p.Add("i_mat_class", value: MAT_CLASS, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 2);
            p.Add("i_userid", value: USERID, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 8);
            p.Add("i_ip", value: USERIP, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);
            p.Add("i_wh_no", value: WH_NO, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 6);

            p.Add("ret_code", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            DBWork.Connection.Query("BD0002_1", p, commandType: CommandType.StoredProcedure);

            /* string errmsg = string.Empty;
             if (p.Get<OracleString>("O_ERRMSG") != null)
             {
                 errmsg = p.Get<OracleString>("O_ERRMSG").Value;
             }

             SP_MODEL sp = new SP_MODEL
             {
                 O_RETID = p.Get<OracleString>("O_RETID").Value,
                 O_ERRMSG = errmsg
             };*/
            return p.Get<OracleString>("ret_code").ToString();
        }

        //產生常態申購訂單
        public string CALL_BD0002_1(string START_DATE, string END_DATE, string INID, string MAT_CLASS, string USERID, string USERIP, string WH_NO)
        {
            var p = new OracleDynamicParameters();
            p.Add("i_yyymmdd_s", value: START_DATE, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 7);
            p.Add("i_yyymmdd_e", value: END_DATE, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 7);
            p.Add("i_inid", value: INID, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 6);
            p.Add("i_mat_class", value: MAT_CLASS, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 2);
            p.Add("i_userid", value: USERID, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 8);
            p.Add("i_ip", value: USERIP, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);
            p.Add("i_wh_no", value: WH_NO, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 6);

            p.Add("ret_code", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            DBWork.Connection.Query("BD0002", p, commandType: CommandType.StoredProcedure);

            /* string errmsg = string.Empty;
             if (p.Get<OracleString>("O_ERRMSG") != null)
             {
                 errmsg = p.Get<OracleString>("O_ERRMSG").Value;
             }

             SP_MODEL sp = new SP_MODEL
             {
                 O_RETID = p.Get<OracleString>("O_RETID").Value,
                 O_ERRMSG = errmsg
             };*/
            return p.Get<OracleString>("ret_code").ToString();
        }

        //庫房代碼combox
        public IEnumerable<ComboItemModel> GetWH_NO(string USER_ID)
        {
            string sql = @" SELECT wh_no VALUE, 
                            wh_no || ' ' || wh_name TEXT 
                            FROM MI_WHMAST 
                            WHERE wh_kind = '1' AND  wh_grade = '1'　";
            return DBWork.Connection.Query<ComboItemModel>(sql, new { USER_ID = USER_ID }, DBWork.Transaction);
        }

        //物料類別combox
        public IEnumerable<ComboItemModel> GetMAT_CLASS()
        {
            string sql = @"SELECT MAT_CLASS || ' ' || MAT_CLSNAME TEXT,
                                  MAT_CLASS VALUE
                           FROM MI_MATCLASS
                           WHERE MAT_CLASS BETWEEN '02' AND '08'
                           ORDER BY VALUE";
            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }

    }
}
