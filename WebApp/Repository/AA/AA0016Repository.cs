using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using System.Linq;
using TSGH.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using JCLib.Mvc;

namespace WebApp.Repository.AA
{
    public class AA0016Repository : JCLib.Mvc.BaseRepository
    {
        public AA0016Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        // GET api/<controller>
        public IEnumerable<ME_DOCM> QueryME(string queryfrom, string lfdat_bg, string lfdat_ed, string frwh, string kind, string flowid,string id, string inid, string appid, int page_index, int page_size, string sorters)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @" Select DOCNO,case when flowid='1301' then '申請中' when flowid='1302' then '補發中' when flowid='1399' then '已結案' else 'OTHER' end as flowid , 
                            APPID || ' ' || USER_NAME(APPID) as APP_NAME, 
                            APPDEPT || ' ' || INID_NAME(APPDEPT) as APPDEPT_NAME,
                            TWN_DATE(APPTIME) AS APPTIME,WH_NAME(FRWH) as FRWH, FRWH as FRWHID, case when STKTRANSKIND='1' then '一般藥' when STKTRANSKIND='2' then '1至3級管制藥' end as STKTRANSKIND,DOCTYPE
                            FROM ME_DOCM where DOCTYPE='RR' ";

            if (queryfrom == "AB0013")
                sql += " and APPID = :userid ";
            else if (queryfrom == "AA0016")
                sql += " and FRWH in ( select wh_no from mi_whid where wh_userid = :userid and task_id = '1') ";

            p.Add(":userid", id);

            if (lfdat_bg != "" && lfdat_bg != null)
            {
                sql += " AND TWN_DATE(apptime) >= :p1 ";
                p.Add("@:p1", lfdat_bg);
            }
            if (lfdat_ed != "" && lfdat_ed != null)
            {
                sql += " AND TWN_DATE(apptime) <= :p2 ";
                p.Add("@:p2", lfdat_ed);
            }

            if (frwh != "" && frwh != null)
            {
                sql += " AND FRWH = :p3 ";
                p.Add(":p3", frwh);
            }
            if (kind != "" && kind != null)
            {
                sql += " AND STKTRANSKIND = :p4 ";
                p.Add(":p4", kind);
            }
            if (flowid != "" && flowid != null)
            {
                sql += " AND FLOWID = :p5 ";
                p.Add(":p5", flowid);
            }
            if (inid != "" && inid != null)
            {
                sql += " AND APPDEPT = :p6 ";
                p.Add(":p6", inid);
            }
            if (appid != "" && appid != null)
            {
                sql += " AND USER_NAME(APPID) like :p7 ";
                p.Add(":p7", string.Format("%{0}%", appid));
            }
            //p.Add(":p6", id);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<ME_DOCM>(GetPagingStatement(sql, sorters), p);
            //return DBWork.Connection.Query<PRM>(GetPagingStatement(sql, sorters), p);
        }
        public IEnumerable<ME_DOCM> QueryMEDOCD(string docno, string kind, int page_index, int page_size, string sorters)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"SELECT ME_DOCM.DOCNO,ME_DOCM.FRWH || ' ' || WH_NAME(ME_DOCM.FRWH) as FRWH,
             (case when ME_DOCD.FRWH_D is null then ME_DOCM.FRWH else ME_DOCD.FRWH_D end) as FRWH_D,
             ME_DOCD.MMCODE as MMCODE,MI_MAST.MMNAME_E,ME_DOCD.APVQTY,APPQTY,MI_MAST.BASE_UNIT,
             case when ME_DOCD.GTAPL_RESON='01' then '01.班長運送途中破損' when ME_DOCD.GTAPL_RESON='02' then '02.氣送過程破損' when ME_DOCD.GTAPL_RESON='03' then '03.醫護人員取用不慎破損'
             when ME_DOCD.GTAPL_RESON='04' then '04.未收到藥品' when ME_DOCD.GTAPL_RESON='05' then '05.其他' when ME_DOCD.GTAPL_RESON='06' then '06.破損' when ME_DOCD.GTAPL_RESON='07' then '07.過效期'
             when ME_DOCD.GTAPL_RESON='08' then '08.變質' end as GTAPL_RESON,ME_DOCD.APLYITEM_NOTE,ME_DOCD.SEQ";
          
            if (kind=="一般藥")
            {
          

                sql += " ,ME_DOCD.BEDNO,ME_DOCD.MEDNO,ME_DOCD.CHINNAME,ME_DOCD.ORDERDATE, case when ME_DOCD.CONFIRMSWITCH='B' then 'B.不補發' when ME_DOCD.CONFIRMSWITCH='C' then 'C.補發不扣庫' when ME_DOCD.CONFIRMSWITCH='D' then 'D.作廢' when ME_DOCD.CONFIRMSWITCH='N' then 'N.未確認' when ME_DOCD.CONFIRMSWITCH='Y' then 'Y.補發扣帳' end as CONFIRMSWITCH " +
                    "   FROM ME_DOCM,ME_DOCD, MI_MAST WHERE ME_DOCM.DOCNO= ME_DOCD.DOCNO and ME_DOCD.DOCNO = :p0 and ME_DOCD.mmcode = MI_MAST.mmcode";
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

        public int UpdateEnd(string dno, string UPUSER, string UIP)
        {

            var sql = @"UPDATE ME_DOCM SET FLOWID='1399', UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE to_number(DOCNO) = :DOCNO";
            //return DBWork.Connection.Execute(sql, record, DBWork.Transaction);
            return DBWork.Connection.Execute(sql, new { DOCNO = dno, UPDATE_USER = UPUSER, UPDATE_IP = UIP }, DBWork.Transaction);
        }

        public int Update(ME_DOCM record)
        {

            var sql = @"UPDATE ME_DOCD SET FRWH_D = :FRWH_D,APVQTY =:APVQTY,CONFIRMSWITCH=:CONFIRMSWITCH,GTAPL_RESON=:GTAPL_RESON,APVTIME=SYSDATE, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP WHERE DOCNO = :DOCNO AND SEQ = :SEQ";//, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
            //STKTRANSKIND=:STKTRANSKIND,
            return DBWork.Connection.Execute(sql, record, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetStatusCombo()
        {
            string sql = @" select data_value as VALUE, data_desc as TEXT from param_d where grp_code = 'ME_DOCM' and data_name = 'FLOWID' and data_value like '13%' order by data_value ";

            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetGridWhnoCombo()
        {
            string sql = @" select wh_no as VALUE,wh_name as TEXT from mi_whmast where wh_kind = '0' and wh_grade = '2' order by wh_no ";

            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }

        public string CallProc(string id, string upuser, string upip)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_DOCNO", value: id, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 21);
            p.Add("I_UPDUSR", value: upuser, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 8);
            p.Add("I_UPDIP", value: upip, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 1);
            p.Add("O_ERRMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 255);

            DBWork.Connection.Query("POST_DOC", p, commandType: CommandType.StoredProcedure);
            string retid = p.Get<OracleString>("O_RETID").Value;
            string errmsg = p.Get<OracleString>("O_ERRMSG").Value;
            return retid;
        }
        public string POST_DOC(string I_DOCNO, string I_UPDUSR, string I_UPDIP)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_DOCNO", value: I_DOCNO, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 21);
            p.Add("I_UPDUSR", value: I_UPDUSR, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 8);
            p.Add("I_UPDIP", value: I_UPDIP, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 1);
            p.Add("O_ERRMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 255);

            DBWork.Connection.Query("POST_DOC", p, commandType: CommandType.StoredProcedure);
            string retid = p.Get<OracleString>("O_RETID").Value;
            string errmsg = p.Get<OracleString>("O_ERRMSG").Value;
            return retid;
        }
    }
}