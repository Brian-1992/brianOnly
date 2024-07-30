using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
namespace WebApp.Repository.B
{
    public class PH_MAILSP : JCLib.Mvc.BaseModel
    {  
        public string INID { get; set; }
        public string MEMO { get; set; }
        public string TP { get; set; }
        public string M_CONTID { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public string DLINE_DT { get; set; }
        public string SMEMO { get; set; }
        public string MAT_CLASS { get; set; }
    }
    public class BD0003Repository : JCLib.Mvc.BaseRepository
    {
        public BD0003Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        /*   for BD0003  table PH_MAILSP  */
        public IEnumerable<PH_MAILSP> GetPH_MAILSP_ALL(string inid, string mat_class, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"select m_contid,inid,MEMO,SMEMO,TWN_DATE(dline_dt) as dline_dt, mat_class from PH_MAILSP 
                          where  inid=:inid and mat_class=:mat_class ";
            sql += " order by m_contid";

            p.Add(":inid", inid);
            p.Add(":mat_class", mat_class);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PH_MAILSP>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<PH_MAILSP> GetPH_MAILSP(string inid, string m_contid, string mat_class)
        {
            var p = new DynamicParameters();
            var sql = @"select m_contid,inid,MEMO,SMEMO,TWN_DATE(dline_dt) as dline_dt, mat_class from PH_MAILSP 
                          where  inid=:inid and m_contid =:m_contid and mat_class=:mat_class";

            p.Add(":inid", inid);
            p.Add(":m_contid", m_contid);
            p.Add(":mat_class", mat_class);
            return DBWork.Connection.Query<PH_MAILSP>(sql, p, DBWork.Transaction);
        }
        public int CreatePH_MAILSP(PH_MAILSP PH_MAILSP)
        {
            var sql = @"INSERT INTO PH_MAILSP (INID,M_CONTID,MEMO,SMEMO,DLINE_DT,CREATE_TIME, CREATE_USER, MAT_CLASS)  
                                VALUES (:INID,:M_CONTID, :MEMO,:SMEMO,TO_DATE(:DLINE_DT,'YYYY/MM/DD'), SYSDATE, :CREATE_USER, :MAT_CLASS)";
            return DBWork.Connection.Execute(sql, PH_MAILSP, DBWork.Transaction);
        }
        public int UpdatePH_MAILSP(PH_MAILSP PH_MAILSP)
        {
            var sql = @"UPDATE PH_MAILSP SET MEMO=:MEMO, SMEMO=:SMEMO, DLINE_DT=TO_DATE(:DLINE_DT,'YYYY/MM/DD'), UPDATE_TIME = SYSDATE, 
                           UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP, MAT_CLASS=:MAT_CLASS
                                WHERE INID = :INID and M_CONTID=:M_CONTID and MAT_CLASS=:MAT_CLASS ";
            return DBWork.Connection.Execute(sql, PH_MAILSP, DBWork.Transaction);
        }
        public int DeletePH_MAILSP(string INID, string M_CONTID, string MAT_CLASS)
        {
            var sql = @" DELETE from PH_MAILSP
                                WHERE INID=:INID and M_CONTID=:M_CONTID and MAT_CLASS=:MAT_CLASS";
            return DBWork.Connection.Execute(sql, new { INID = INID, M_CONTID = M_CONTID, MAT_CLASS = MAT_CLASS }, DBWork.Transaction);
        }
        public bool CheckExistsPH_MAILSP(string inid, string m_contid, string mat_class)
        {
            string sql = @"SELECT 1 FROM PH_MAILSP WHERE  INID=:INID and M_CONTID=:M_CONTID and MAT_CLASS=:MAT_CLASS";
            return !(DBWork.Connection.ExecuteScalar(sql, new { INID = inid, M_CONTID = m_contid, MAT_CLASS = mat_class }, DBWork.Transaction) == null);
        }
        public string getUserInid(string tuser)
        {
            string sql = @" select INID from UR_ID where TUSER = :TUSER ";
            return DBWork.Connection.QueryFirst<string>(sql, new { TUSER = tuser }, DBWork.Transaction);
        }
        // ===============================================
        /*   for AA0051  table PH_MAILSP_M, PH_MAILSP_D */

        public IEnumerable<PH_MAILSP_M> GetMasterAll(string m_contid, string agen_no, string status, 
                                                     int page_index, int page_size, string sorters,string contracno)
        {
            var p = new DynamicParameters();
            var sql = @"select distinct msgno,msgrecno,msgtext,reddisp 
                          from ( select a.*,b.agen_no, b.M_CONTID,b.CONTRACNO 
                                   from ph_mailsp_m a, ph_mailsp_d b 
                                 where a.MSGNO=b.MSGNO(+) and a.MSGRECNO=b. MSGRECNO(+) ";

            //if (mgroup != "")
            //{
            //    sql += "AND a.MGROUP =:p0 ";
            //    p.Add(":p0",  mgroup);
            //}
            if (m_contid != "")
            {
                sql += "AND b.M_CONTID =:p1 ";
                p.Add(":p1", m_contid);
            }
            if (agen_no != "")
            {
                sql += "AND b.AGEN_NO =:p2 ";
                p.Add(":p2", agen_no);
            }

            if (status == "Y")
            {
                sql += "AND b.AGEN_NO is not null";

            }
            else if (status == "N")

            {
                sql += "AND  b.AGEN_NO is null ";

            }

            if (contracno != "")
            {
                sql += "AND b.CONTRACNO =:p5 ";
                p.Add(":p5", contracno);
            }

            sql += ") order by msgno, msgrecno";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PH_MAILSP_M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<PH_MAILSP_M> MasterGet(string msgrecno, string msgno)
        {
            var sql = @" select distinct msgno,msgrecno,msgtext,reddisp from ( select a.*,b.agen_no, b.M_CONTID from ph_mailsp_m a, ph_mailsp_d b where a.MSGNO=b.MSGNO(+) and a.msgno=:MSGNO and a.MSGRECNO=:MSGRECNO) ";
            return DBWork.Connection.Query<PH_MAILSP_M>(sql, new { MSGRECNO = msgrecno, MSGNO = msgno }, DBWork.Transaction);
        }

        public int MasterCreate(PH_MAILSP_M PH_MAILSP_M)
        {
            var sql = @"INSERT INTO PH_MAILSP_M (MSGNO,MSGRECNO, MSGTEXT,CREATE_TIME, CREATE_USER, REDDISP)  
                                VALUES (:MSGNO,:MSGRECNO, :MSGTEXT ,SYSDATE, :CREATE_USER, :REDDISP)";
            return DBWork.Connection.Execute(sql, PH_MAILSP_M, DBWork.Transaction);
        }

        public int MasterUpdate(PH_MAILSP_M PH_MAILSP_M)
        {
            var sql = @"UPDATE PH_MAILSP_M SET MSGRECNO=:MSGRECNO, MSGNO=:MSGNO, MSGTEXT=:MSGTEXT, UPDATE_TIME = SYSDATE, 
                               REDDISP=:REDDISP, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE MSGNO=:MSGNO and MSGRECNO=:MSGRECNO";
            return DBWork.Connection.Execute(sql, PH_MAILSP_M, DBWork.Transaction);
        }

        public int MasterDelete(string MSGNO,string MSGRECNO)
        {
            var sql = @" DELETE from PH_MAILSP_M
                                WHERE MSGNO=:MSGNO and MSGRECNO=:MSGRECNO";
            return DBWork.Connection.Execute(sql, new { MSGNO= MSGNO, MSGRECNO = MSGRECNO }, DBWork.Transaction);
        }

        public int MasterAudit(PH_MAILSP_M PH_MAILSP_M)
        {
            var sql = @"UPDATE PH_MAILSP_M SET STATUS='B', REASON = '', UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE DN = :DN AND (STATUS = 'A' OR STATUS = 'D') ";
            return DBWork.Connection.Execute(sql, PH_MAILSP_M, DBWork.Transaction);
        }

        public IEnumerable<PH_MAILSP_D> GetDetailAll(string msgno,string msgrecno, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT  MSGNO, MSGRECNO,a.CONTRACNO,
                                case when a.AGEN_NO='*' then '*_所有廠商' else a.AGEN_NO||'_'||b.agen_namec end AGEN_NAMEC,                            
                                a.AGEN_NO, M_CONTID
                          FROM PH_MAILSP_D a, PH_VENDER b  
                         WHERE a.agen_no =b.agen_no(+) ";

            sql += " AND MSGNO = :p0 and MSGRECNO =:p1";
            p.Add(":p0", msgno);
            p.Add(":p1", msgrecno);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PH_MAILSP_D>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<PH_MAILSP_D> DetailGet(PH_MAILSP_D PH_MAILSP_D)
        {
            var sql = @" SELECT  MSGNO,MSGRECNO,a.CONTRACNO, 
                                 case when a.AGEN_NO='*' then '*_所有廠商' else a.AGEN_NO||'_'||b.agen_namec end AGEN_NAMEC,
                                 a.AGEN_NO, M_CONTID 
                           FROM PH_MAILSP_D a, PH_VENDER b  
                          WHERE MSGRECNO=:MSGRECNO and MSGNO=:MSGNO 
                            and a.AGEN_NO=:AGEN_NO and M_CONTID=:M_CONTID 
                            and a.agen_no =b.agen_no(+) ";
            return DBWork.Connection.Query<PH_MAILSP_D>(sql, PH_MAILSP_D, DBWork.Transaction);
        }

        public int DetailCreate(PH_MAILSP_D PH_MAILSP_D)
        {
            var sql = @"INSERT INTO PH_MAILSP_D (MSGNO,MSGRECNO, AGEN_NO, M_CONTID,CREATE_TIME, CREATE_USER,CONTRACNO)  
                                VALUES (:MSGNO, :MSGRECNO,:AGEN_NO, :M_CONTID ,SYSDATE, :CREATE_USER,:CONTRACNO)";
            return DBWork.Connection.Execute(sql, PH_MAILSP_D, DBWork.Transaction);
        }

        public int DetailUpdate(PH_MAILSP_D PH_MAILSP_D)
        {
            var sql = @"UPDATE PH_MAILSP_D 
                           SET AGEN_NO=:AGEN_NO, M_CONTID=:M_CONTID,UPDATE_TIME = SYSDATE, 
                               UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                         WHERE MSGRECNO=:MSGRECNO and MSGNO=:MSGNO and AGEN_NO=:AGEN_NO and M_CONTID=:M_CONTID";
            return DBWork.Connection.Execute(sql, PH_MAILSP_D, DBWork.Transaction);
        }

        public int DetailDelete(PH_MAILSP_D PH_MAILSP_D)
        {
            var sql = @" DELETE from PH_MAILSP_D
                                WHERE MSGRECNO=:MSGRECNO and MSGNO=:MSGNO and AGEN_NO=:AGEN_NO and M_CONTID=:M_CONTID ";
            return DBWork.Connection.Execute(sql, PH_MAILSP_D, DBWork.Transaction);
        }
        public int DetailDeleteAll(string msgrecno, string msgno)
        {
            var sql = @" DELETE from PH_MAILSP_D
                                WHERE MSGRECNO = :MSGRECNO  and MSGNO=:MSGNO";
            return DBWork.Connection.Execute(sql, new { MSGRECNO = msgrecno, MSGNO = msgno }, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetMgroupCombo(string inid)
        {
            string sql = @"select distinct mgroup as VALUE from PH_MAILSP_M where inid=:inid";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { inid }, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetAngenoCombo()
        {
            string sql = @"select distinct agen_no as VALUE, agen_no ||' '|| agen_namec as TEXT from ph_vender  order by agen_no";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetAngenoAllCombo()
        {
            string sql = @"select agen_no as VALUE ,agen_no||' '||agen_namec as TEXT from ph_vender order by agen_no ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }

        public bool CheckExists(string msgrecno, string msgno)
        {
            string sql = @"SELECT 1 FROM PH_MAILSP_M WHERE  MSGRECNO=:MSGRECNO and MSGNO=:MSGNO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MSGRECNO = msgrecno,MSGNO=msgno }, DBWork.Transaction) == null);
        }
        public bool CheckExists_D(PH_MAILSP_D PH_MAILSP_D)
        {
            string sql = @"SELECT 1 FROM PH_MAILSP_D WHERE  MSGRECNO=:MSGRECNO and MSGNO=:MSGNO and AGEN_NO=:AGEN_NO and M_CONTID=:M_CONTID ";
            return !(DBWork.Connection.ExecuteScalar(sql, PH_MAILSP_D, DBWork.Transaction) == null);
        }

    }
}
 
 