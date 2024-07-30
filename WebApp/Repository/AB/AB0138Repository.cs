using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.AB
{
    public class AB0138Repository : JCLib.Mvc.BaseRepository
    {
        public AB0138Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<PUBDGM> GetAllM(string p0,  int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select A.DOCNO, A.APPTIME, TWN_DATE(A.APPTIME) APPTIME_T 
                                    from PUBDGM A
                                 where A.APPDEPT = :p0
                                     and A.IS_DEL = 'N' and A.STATUS = 'N' ";

            p.Add(":p0", string.Format("{0}", p0));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PUBDGM>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<PUBDGL> GetAllD(string DOCNO, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            //HIGH_QTY預設值1000待確認邏輯再改
            var sql = @"select A.DOCNO, B.MMCODE , C.MMNAME_C, B.APPQTY,  C.BASE_UNIT, 
                                               C.DISC_UPRICE , 
                                               (B.APPQTY * C.DISC_UPRICE) AS APPAMT,
                                               nvl(INV_QTY(B.MMCODE, USER_INID(:P1)),0) as INV_QTY,
                                               1000 as HIGH_QTY, B.ISDEF
                                    from PUBDGM A, PUBDGL B, MI_MAST C
                                  where A.DOCNO = B.DOCNO and B.MMCODE = C.MMCODE and B.IS_DEL <> 'Y' ";

            if (DOCNO != "")
            {
                sql += " and A.DOCNO = :p0 ";
                p.Add(":p0", string.Format("{0}", DOCNO));
            }

            p.Add(":p1", string.Format("{0}", p1));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PUBDGL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<PUBDGL> GetD(string id, string docno, string mmcode)
        {
            var p = new DynamicParameters();
            var sql = @"select B.MMCODE ,  C.MMNAME_C, B.APPQTY,  C.BASE_UNIT,  C.DISC_UPRICE , 
                                               (B.APPQTY * C.DISC_UPRICE) as APPAMT,
                                                nvl(INV_QTY(B.MMCODE, USER_INID(:TUSER)),0) as INV_QTY,
                                                1000 as HIGH_QTY, B.ISDEF
                                       from PUBDGM A, PUBDGL B, MI_MAST C
                                     where A.DOCNO = B.DOCNO and B.MMCODE = C.MMCODE
                                         and A.DOCNO = :DOCNO 
                                         and B.MMCODE= :MMCODE ";

            p.Add(":TUSER", string.Format("{0}", id));
            p.Add(":DOCNO", string.Format("{0}", docno));
            p.Add(":MMCODE", string.Format("{0}", mmcode));

            return DBWork.Connection.Query<PUBDGL>(sql, p, DBWork.Transaction);
        }

        public int CreateM(PUBDGM pubdgm)
        {
            var sql = @"insert into PUBDGM (
                                       DOCNO, APPTIME, APPID , APPDEPT , MEMO, STATUS , IS_DEL , 
                                       CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                    values (
                                       :DOCNO, SYSDATE,  :APPID , :APPDEPT , :MEMO, :STATUS , :IS_DEL , 
                                      SYSDATE, :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, pubdgm, DBWork.Transaction);
        }

        public int CreateD(PUBDGL pubdgl)
        {
            var sql = @"insert into PUBDGL (
                                      DOCNO, MMCODE , APPQTY , HIGH_QTY, INV_QTY,
                                      MEMO, ISISSUE, ISDEF, ISWAS, IS_DEL,  ACKQTY,
                                      CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)  
                                   values (
                                       :DOCNO, :MMCODE , :APPQTY ,:HIGH_QTY, :INV_QTY,
                                       :MEMO, :ISISSUE, :ISDEF, :ISWAS, :IS_DEL, :ACKQTY, 
                                       :CREATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, pubdgl, DBWork.Transaction);
        }

        public int UpdateD(PUBDGL pubdgl)
        {
            var sql = @"update PUBDGL set 
                                          APPQTY = :APPQTY, ISDEF = :ISDEF,
                                          UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                    where DOCNO = :DOCNO and MMCODE = :MMCODE ";
            return DBWork.Connection.Execute(sql, pubdgl, DBWork.Transaction);
        }
        public int ApplyM(PUBDGM pubdgm)
        {
            var sql = @"update PUBDGM set STATUS = 'S',
                                           UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                     where DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, pubdgm, DBWork.Transaction);
        }
        public int ApplyD(PUBDGM pubdgm)
        {
            var sql = @"update PUBDGL set ACKQTY = APPQTY, 
                                           UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                    where DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, pubdgm, DBWork.Transaction);
        }
        public int DeleteM(PUBDGM pubdgm)
        {
            var sql = @"update PUBDGM set IS_DEL = 'Y', 
                                           UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                   where DOCNO = :DOCNO";
            return DBWork.Connection.Execute(sql, pubdgm, DBWork.Transaction);
        }

        public int DeleteD(PUBDGL pubdgl)
        {
            var sql = @"update PUBDGL set IS_DEL = 'Y', 
                                           UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                    where DOCNO = :DOCNO and MMCODE = :MMCODE ";
            return DBWork.Connection.Execute(sql, pubdgl, DBWork.Transaction);
        }

        public bool CheckMmcode(string id, string doc)
        {
            string sql = @"SELECT 1 FROM MI_MAST WHERE MMCODE=:MMCODE AND MAT_CLASS = '01' ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = id, DOCNO = doc }, DBWork.Transaction) == null);
        }
        public bool CheckExists(string id)
        {
            string sql = @"SELECT 1 FROM PUBDGM WHERE DOCNO=:DOCNO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsD(string id)
        {
            string sql = @"SELECT 1 FROM PUBDGL WHERE DOCNO=:DOCNO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id }, DBWork.Transaction) == null);
        }
        public bool CheckExistsDD(string id, string sid)
        {
            string sql = @"SELECT 1 FROM PUBDGL WHERE DOCNO=:DOCNO AND MMCODE=:MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id, MMCODE = sid }, DBWork.Transaction) == null);
        }
        public bool CheckExistsMM(string id, string mm)
        {
            string sql = @"select 1 from PUBDGL where DOCNO=:DOCNO and MMCODE=:MMCODE and IS_DEL<>'Y'";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = id, MMCODE = mm }, DBWork.Transaction) == null);
        }
        public string GetUridInid(string id)
        {
            string sql = @"SELECT INID FROM UR_ID WHERE TUSER=:TUSER ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { TUSER = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetInidName(string id)
        {
            string sql = @"SELECT INID_NAME(USER_INID(:TUSER)) INIDNAME FROM DUAL ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { TUSER = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetTwnapptime(string docno)
        {
            string sql = @"SELECT TWN_TIME_FORMAT(APPTIME) D1 FROM PUBDGM WHERE DOCNO=:DOCNO";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction).ToString();
            return rtn;
        }
        public string GetHighQty(string id,string mmcode)
        {
            string sql = @" SELECT NVL(HIGH_QTY,0) HIGH_QTY FROM MI_WHMAST D, MI_WINVCTL F 
                                        WHERE D.INID = USER_INID(:TUSER) AND D.WH_NO = F.WH_NO AND F.MMCODE = :MMCODE AND ROWNUM = 1 ";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { TUSER = id, MMCODE = mmcode }, DBWork.Transaction);
        }
        public string GetInvQty(string id, string mmcode)
        {
            string sql = @" SELECT INV_QTY(:MMCODE,  USER_INID(:TUSER)) from dual ";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { TUSER = id, MMCODE = mmcode }, DBWork.Transaction);
        }

        public string GetDocno()
        {
            var p = new OracleDynamicParameters();
            p.Add("O_DOCNO", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 12);

            DBWork.Connection.Query("GET_DOCNO", p, commandType: CommandType.StoredProcedure);
            return p.Get<OracleString>("O_DOCNO").Value;
        }
        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, string p1, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select  {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT, A.DISC_UPRICE,
                                                 NVL(INV_QTY(A.MMCODE,USER_INID('NRS00001')),0)  AS INV_QTY,
                                                 1000 as HIGH_QTY
                                    from MI_MAST A 
                                  where  A.MAT_CLASS = '01'  ";

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

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
            p.Add(":P1", p1);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetYN()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                                                       DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                                          FROM PARAM_D
                                        WHERE GRP_CODE='Y_OR_N' AND DATA_NAME='YN' 
                                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<AB0003Model> GetLoginInfo(string id, string ip)
        {
            string sql = @"SELECT TUSER AS USERID, UNA AS USERNAME, USER_INID(TUSER) AS INID, 
                                                      INID_NAME(INID) AS INIDNAME,
                                                       WHNO_MM1 CENTER_WHNO,INID_NAME(WHNO_MM1) AS CENTER_WHNAME,
                                                      TO_CHAR(SYSDATE,'YYYYMMDD') AS TODAY, :UPDATE_IP
                                         FROM UR_ID
                                       WHERE UR_ID.TUSER=:TUSER";

            return DBWork.Connection.Query<AB0003Model>(sql, new { TUSER = id, UPDATE_IP = ip });
        }

        public string GetDocAppAmout(string docno)
        {
            var sql = @" select  round(nvl(sum(B.DISC_UPRICE * A.APPQTY), 0),0) as APP_AMOUT 
                                      from PUBDGL A,MI_MAST B 
                                   where A.MMCODE = B.MMCODE and A.DOCNO = :DOCNO and A.IS_DEL <> 'Y'";
            return DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction).ToString();
        }

        public IEnumerable<AB0138PrintModel> GetPrintData(string p0, string p1)
        {
            var p = new DynamicParameters();
            var sql = @"select ROWNUM AS SEQ , T.*
                                     from ( 
                                         select  B.MMCODE ,  C.MMNAME_C,  B.APPQTY,  C.BASE_UNIT,  C.DISC_UPRICE , 
                                                      (B.APPQTY * C.DISC_UPRICE) AS APPAMT,
                                                       INV_QTY(B.MMCODE,USER_INID(:P1)),
                                                      1000 as HIGH_QTY, B.ISDEF
                                          from PUBDGM A, PUBDGL B, MI_MAST C
                                        where A.DOCNO = B.DOCNO and B.MMCODE = C.MMCODE
                                            and A.DOCNO = :P0   and B.IS_DEL <> 'Y'
                                         order by B.MMCODE DESC 
                                    ) T";

            p.Add(":P1", string.Format("{0}", p1));
            p.Add(":P0", string.Format("{0}", p0));

            return DBWork.Connection.Query<AB0138PrintModel>(sql, p, DBWork.Transaction);
        }
    }

    public class AB0138PrintModel : JCLib.Mvc.BaseModel
    {
        public string SEQ { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public decimal APPQTY { get; set; }
        public string BASE_UNIT { get; set; }
        public decimal UPRICE { get; set; }
        public decimal APPAMT { get; set; }
        public int INV_QTY { get; set; }
        public int HIGH_QTY { get; set; }
        public string ISDEF { get; set; }
    }
}
