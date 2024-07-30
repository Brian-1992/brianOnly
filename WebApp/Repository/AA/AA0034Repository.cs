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

namespace WebApp.Repository.AA
{
    public class AA0034Repository : JCLib.Mvc.BaseRepository
    {
        public AA0034Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_DOCM> GetAll(ME_DOCM_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT DOCNO, TWN_DATE(APPTIME) AS APPTIME
                    , APPID || ' ' || USER_NAME(APPID) AS APP_NAME
                    , USER_NAME(APPID) AS APPID 
                    , APPDEPT || ' ' || INID_NAME(APPDEPT) AS APPDEPT_NAME
                    , (SELECT FLOWID || ' ' || FLOWNAME FROM ME_FLOW WHERE DOCTYPE='SP1' AND FLOWID=A.FLOWID)  FLOWID
                    , (SELECT MAT_CLASS || ' ' || MAT_CLSNAME FROM MI_MATCLASS WHERE MAT_CLASS=A.MAT_CLASS) MAT_CLASS
                    , FRWH || ' ' || WH_NAME(FRWH) FRWH
                    , TOWH || ' ' || WH_NAME(TOWH) TOWH
                    , APPLY_NOTE,UPDATE_TIME

                    FROM ME_DOCM A WHERE 1=1";

            if (query.APPDEPT != "")
            {
                sql += " AND A.APPDEPT = :APPDEPT ";
                p.Add(":APPDEPT", query.APPDEPT);
            }

            if (query.DOCNO != "")
            {
                sql += " AND A.DOCNO LIKE :DOCNO ";
                p.Add(":DOCNO", string.Format("%{0}%", query.DOCNO));
            }

            if (query.DOCTYPE != "")
            {
                sql += " AND A.DOCTYPE LIKE :DOCTYPE ";
                p.Add(":DOCTYPE", string.Format("%{0}%", query.DOCTYPE));
            }

            if (query.FLOWID != "")
            {
                sql += " AND A.FLOWID LIKE :FLOWID ";
                p.Add(":FLOWID", string.Format("%{0}%", query.FLOWID));
            }

            if (query.MAT_CLASS != "")
            {
                if (query.MAT_CLASS == "03-08")
                    sql += " AND (A.MAT_CLASS = '03' or A.MAT_CLASS = '04' or A.MAT_CLASS = '05' or A.MAT_CLASS = '06' or A.MAT_CLASS = '07' or A.MAT_CLASS = '08' )";
                else
                    sql += " AND A.MAT_CLASS = :MAT_CLASS ";
                p.Add(":MAT_CLASS", string.Format("{0}", query.MAT_CLASS));
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

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCD> GetMeDocd(ME_DOCD_QUERY_PARAMS query)
        {
            var p = new DynamicParameters();

            //var sql = @"SELECT a.DOCNO, a.SEQ, a.MMCODE, a.APPQTY, a.APL_CONTIME, a.APLYITEM_NOTE, b.MMNAME_C, b.MMNAME_E, b.M_CONTPRICE, b.BASE_UNIT
            //            , a.APPQTY * c.AVG_PRICE AS AMOUNT, d.INV_QTY
            //            FROM ME_DOCD a 
            //            LEFT JOIN MI_MAST b ON a.MMCODE=b.MMCODE
            //            LEFT JOIN MI_WHCOST C ON a.MMCODE=C.MMCODE 
            //            LEFT JOIN MI_WHINV d ON a.MMCODE=d.MMCODE 
            //            WHERE 1=1 AND WH_NO=WHNO_MM1";
            var sql = @"SELECT a.DOCNO, a.SEQ, a.MMCODE, b.MMNAME_C, b.MMNAME_E, a.APPQTY, b.BASE_UNIT, a.APLYITEM_NOTE, c.AVG_PRICE, a.APPQTY * c.AVG_PRICE AS AMOUNT, d.INV_QTY
                        FROM ME_DOCD a, mi_mast b, mi_whcost c, mi_whinv d
                        WHERE a.MMCODE=b.MMCODE and a.mmcode=c.mmcode and a.mmcode=d.mmcode and d.wh_no='560000'";

            if (query.DOCNO != "")
            {
                sql += " AND a.DOCNO=:p0 ";
                p.Add(":p0", string.Format("{0}", query.DOCNO));
            }

            if (query.DATA_YM != "")
            {
                sql += " and c.data_ym=:p1 ";
                p.Add(":p1", string.Format("{0}", query.DATA_YM));
            }

            return DBWork.PagingQuery<ME_DOCD>(sql, p, DBWork.Transaction);
        }

        public string GetYm()
        {
            var sql = @"select SET_YM from mi_mnset where set_status='N'";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetFlowidCombo()
        {
            //string sql = @"SELECT DATA_VALUE AS VALUE, DATA_DESC AS TEXT, DATA_VALUE || ' ' || DATA_DESC AS COMBITEM
            //            FROM PARAM_D
            //            WHERE GRP_CODE='ME_DOCM' and DATA_NAME='FLOWID_SP1'
            //            order by DATA_VALUE";
            string sql = @"select '' AS VALUE, '' AS COMBITEM from dual
                            union
                            SELECT FLOWID AS VALUE, FLOWID || ' ' || FLOWNAME AS COMBITEM FROM ME_FLOW WHERE DOCTYPE='SP1' 
                            --AND FLOWID!='2'
                            ORDER BY value NULLS first";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetMatClassCombo()
        {
            string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM
                        FROM MI_MATCLASS
                        WHERE MAT_CLSid IN ('2','3') 
                        order by MAT_CLASS";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<MI_MAST> GetMmcode(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            // 判斷庫存量>0
            string sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.M_CONTPRICE, A.BASE_UNIT FROM MI_MAST A INNER JOIN MI_WHINV B ON A.MMCODE = B.MMCODE WHERE B.INV_QTY > 0";

            if (query.WH_NO != "")
            {
                sql += " AND B.WH_NO = :WH_NO ";
                p.Add(":WH_NO", string.Format("{0}", query.WH_NO));
            }

            if (query.MAT_CLASS != "")
            {
                sql += " AND A.MAT_CLASS = :MAT_CLASS ";
                p.Add(":MAT_CLASS", string.Format("{0}", query.MAT_CLASS));
            }

            if (query.MMCODE != "")
            {
                sql += " AND A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", query.MMCODE));
            }

            if (query.MMNAME_C != "")
            {
                sql += " AND A.MMNAME_C LIKE :MMNAME_C ";
                p.Add(":MMNAME_C", string.Format("%{0}%", query.MMNAME_C));
            }

            if (query.MMNAME_E != "")
            {
                sql += " AND A.MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", query.MMNAME_E));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, string wh_no, string mat_class, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.M_CONTPRICE, A.BASE_UNIT FROM MI_MAST A INNER JOIN MI_WHINV B ON A.MMCODE = B.MMCODE WHERE B.INV_QTY > 0";

            if (wh_no != "")
            {
                sql += " AND B.WH_NO = :WH_NO ";
                p.Add(":WH_NO", string.Format("{0}", wh_no));
            }

            if (mat_class != "")
            {
                sql += " AND A.MAT_CLASS = :MAT_CLASS ";
                p.Add(":MAT_CLASS", string.Format("{0}", mat_class));
            }

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(A.MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(A.MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

                sql += " OR A.MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR A.MMNAME_C LIKE :MMNAME_C) ";
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

        public IEnumerable<MI_MAST> GetMmcodeInvQty(MI_MAST_QUERY_PARAMS query)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT B.INV_QTY FROM MI_MAST A INNER JOIN MI_WHINV B ON A.MMCODE = B.MMCODE WHERE 1=1";
            if (query.WH_NO != "")
            {
                sql += " AND B.WH_NO = :WH_NO ";
                p.Add(":WH_NO", string.Format("{0}", query.WH_NO));
            }

            if (query.MAT_CLASS != "")
            {
                sql += " AND A.MAT_CLASS = :MAT_CLASS ";
                p.Add(":MAT_CLASS", string.Format("{0}", query.MAT_CLASS));
            }

            if (query.MMCODE != "")
            {
                sql += " AND A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", query.MMCODE));
            }

            return DBWork.Connection.Query<MI_MAST>(sql, p, DBWork.Transaction);
        }

        public int MasterCreate(ME_DOCM me_docm)
        {
            var sql = @"INSERT INTO ME_DOCM (DOCNO, DOCTYPE, FLOWID, APPID, APPDEPT, APPTIME, MAT_CLASS, APPLY_NOTE, FRWH, TOWH, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                    VALUES (:DOCNO, :DOCTYPE, :FLOWID, :APPID, :APPDEPT, SYSDATE, :MAT_CLASS, :APPLY_NOTE, :FRWH, :TOWH, SYSDATE
                                        , :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCM> MasterGet(string docno)
        {
            var sql = @" SELECT DOCNO FROM ME_DOCM WHERE DOCNO = :DOCNO ";
            return DBWork.Connection.Query<ME_DOCM>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public int MasterUpdate(ME_DOCM me_docm)
        {
            var sql = @"UPDATE ME_DOCM SET APPLY_NOTE=:APPLY_NOTE, UPDATE_TIME=SYSDATE, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP WHERE DOCNO=:DOCNO";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public int MasterDelete(string docno)
        {
            var sql = @" DELETE from ME_DOCM WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public int MasterReject(ME_DOCM me_docm)
        {
            var sql = @"UPDATE ME_DOCM SET FLOWID=:FLOWID, APPLY_NOTE=:APPLY_NOTE,
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        public int DetailAllDelete(string docno)
        {
            var sql = @" DELETE from ME_DOCD WHERE DOCNO=:DOCNO";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public int DetailCreate(ME_DOCD me_docd)
        {
            var sql = @"INSERT INTO ME_DOCD (DOCNO, SEQ, MMCODE, APPQTY, APLYITEM_NOTE, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                    VALUES (:DOCNO, :SEQ, :MMCODE, :APPQTY, :APLYITEM_NOTE, SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public int DetailUpdate(ME_DOCD me_docd)
        {
            var sql = @"UPDATE ME_DOCD SET MMCODE=:MMCODE, APPQTY=:APPQTY, APLYITEM_NOTE=:APLYITEM_NOTE, UPDATE_TIME=SYSDATE, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, me_docd, DBWork.Transaction);
        }

        public int DetailDelete(string docno, string seq)
        {
            var sql = @" DELETE from ME_DOCD WHERE DOCNO=:DOCNO AND SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }

        public int UpdateStatus(ME_DOCM me_docm)
        {
            var sql = @"UPDATE ME_DOCM SET FLOWID = :FLOWID, 
                                UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, me_docm, DBWork.Transaction);
        }

        // 檢查申請單院內碼項次的申請數量不得<=0,有的話則不能提出申請
        public bool CheckMeDocdAppqty(string docno)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO and APPQTY<=0";
            return (DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction) == null);
        }

        public SP_MODEL PostDoc(string docno, string updusr, string updip)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_DOCNO", value: docno, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 21);
            p.Add("I_UPDUSR", value: updusr, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 8);
            p.Add("I_UPDIP", value: updip, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 1);
            p.Add("O_ERRMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            DBWork.Connection.Query("POST_DOC", p, commandType: CommandType.StoredProcedure);
            string retid = p.Get<OracleString>("O_RETID").Value;
            string errmsg = p.Get<OracleString>("O_ERRMSG").Value;

            SP_MODEL sp = new SP_MODEL
            {
                O_RETID = p.Get<OracleString>("O_RETID").Value,
                O_ERRMSG = p.Get<OracleString>("O_ERRMSG").Value
            };
            return sp;
        }

        public class ME_DOCM_QUERY_PARAMS
        {
            public string DOCNO;
            public string DOCTYPE;
            public string APPDEPT;
            public string FLOWID;
            public string MAT_CLASS;

            public string APPTIME_S;
            public string APPTIME_E;
        }

        public class ME_DOCD_QUERY_PARAMS
        {
            public string DOCNO;
            public string DATA_YM;
        }

        public class MI_MAST_QUERY_PARAMS
        {
            public string MMCODE;
            public string MMNAME_C;
            public string MMNAME_E;
            public string MAT_CLASS;

            public string WH_NO;
        }

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
    }
}