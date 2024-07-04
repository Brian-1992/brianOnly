using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models.AA;
using System.Collections.Generic;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using WebApp.Models;
using System.Text;

namespace WebApp.Repository.AA
{
    public class AA0193Repository : JCLib.Mvc.BaseRepository
    {
        public AA0193Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0193_M> GetAllM(string p1, string p2, string tuser, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT
                                A.DOCNO AS DOCNO, --補撥單編號,  
                                A.APPDEPT AS APPDEPT, --補撥單位,  
                                INID_NAME(A.APPDEPT) AS INID_NAME --補撥單位名稱
                                FROM PUBDGM A
                                WHERE IS_DEL = 'N'
                                AND STATUS = 'S'
                                 ";

            if (p1 != "" & p2 != "")
            {
                sql += "AND (a.APPDEPT BETWEEN :p1 and :p2) ";
                p.Add(":p1", p1);
                p.Add(":p2", p2);
            }
            else if (p1 != "" & p2 == "")
            {
                sql += "AND a.APPDEPT = :p1 ";
                p.Add(":p1", p1);
            }
            else if (p1 == "" & p2 != "")
            {
                sql += "AND a.APPDEPT = :p2 ";
                p.Add(":p2", p2);
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0193_M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<AA0193_D> GetAllD(string DOCNO, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @" SELECT
                        A.MMCODE, --藥材代碼, 
                        MMCODE_NAMEC(A.MMCODE) AS MCODE_NAME, --藥材名稱,
                        BASE_UNIT(A.MMCODE) AS BASE_UNIT, --單位,
                        A.ACKQTY, --補撥數量, 
                        A.HIGH_QTY, --藥基準量, 
                        DECODE(A.ISWAS,'Y','是','N','否','') AS ISWAS, --列為消耗,
                        DECODE(A.ISDEF,'Y','是','N','否','') AS ISDEF --定時補撥
                        FROM PUBDGL A
                        WHERE DOCNO = :p0 and IS_DEL<>'Y'
                        ORDER BY A.MMCODE
";
            p.Add(":p0", DOCNO);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0193_D>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public int UpdateM(string docno, string mmcode, string iswas,string ackqty, string update_user, string update_ip)
        {
               var p = new DynamicParameters();
            var sql = @" UPDATE PUBDGL
                        SET ISWAS =:ISWAS 
                            ,ACKQTY=:ACKQTY
                            ,UPDATE_USER=:UPDATE_USER
                            ,UPDATE_TIME =sysdate
                            ,UPDATE_IP =:UPDATE_IP
                        WHERE DOCNO=:DOCNO AND MMCODE =:MMCODE";
            p.Add(":ISWAS", iswas);
            p.Add(":ACKQTY", ackqty);
            p.Add(":UPDATE_USER", update_user);
            p.Add(":UPDATE_IP", update_ip);
            p.Add(":docno", docno);
            p.Add(":MMCODE", mmcode);
            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public int Apply(string[] docnoArr, string update_user, string update_ip)
        {
            var p = new DynamicParameters();
            var sql = @"UPDATE PUBDGM 
                    SET APVTIME = SYSDATE,
                        STATUS  = 'Y'  ,
                        UPDATE_TIME = SYSDATE, 
                        UPDATE_USER = :UPDATE_USER,
                        UPDATE_IP = :UPDATE_IP
                        WHERE DOCNO IN :DOCNO";
            p.Add(":UPDATE_USER", update_user);
            p.Add(":UPDATE_IP", update_ip);
            p.Add(":docno", docnoArr);
            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetInidCombo()
        {
            StringBuilder sql = new StringBuilder();
            DynamicParameters p = new DynamicParameters();
            sql.Append("select inid VALUE ,inid ||' ' || inid_name TEXT from UR_INID");
            sql.Append(" ORDER BY 1");

            return DBWork.Connection.Query<COMBO_MODEL>(sql.ToString(), p);
        }
    }
}
