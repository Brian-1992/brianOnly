using System.Data;
using JCLib.DB;
using Dapper;
using MMSMSBAS.Models;
using System.Collections.Generic;

using JCLib.DB.Tool;
using DLLImport.Models;
using System.Linq;

namespace DLLImport.Repository
{
    public class ME_DLLCTLRepository : JCLib.Mvc.BaseRepository
    {
        string sBr = "\r\n";
        FL l = new FL("DLLImport.Repository.ME_DLLCTLRepository");
        
        public ME_DLLCTLRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public ME_DLLCTLModels QueryFirst(ME_DLLCTLModels v)
        {
            var p = new DynamicParameters();

            string sql = "";
            sql += "select ";
            sql += sBr + " DLLCODE, "; // 00.DLL代碼(VARCHAR2)
            sql += sBr + " WH_NO, "; // 01.庫房代碼(VARCHAR2)
            sql += sBr + " TWN_TIME(ENDDATE) ENDDATE, "; // 02.結束時間(DATE)
            sql = sql.Substring(0, sql.Length - 2);
            sql += sBr + " from ME_DLLCTL ";
            sql += sBr + " where 1=1 ";
            if (!string.IsNullOrEmpty(v.WH_NO))
            {
                sql += sBr + " and WH_NO = '" + v.WH_NO + "' "; // 針對 排程程式使用( WH_NO 一律填* )
            }
            else
            {
                sql += sBr + " and WH_NO like '*' "; // 針對 排程程式使用( WH_NO 一律填* )
            }
            if (v.DLLCODE != "")
            {
                sql += sBr + " and DLLCODE = :dllcode ";
                p.Add(":dllcode", string.Format("{0}", v.DLLCODE));
            }
            return DBWork.Connection.QueryFirst<ME_DLLCTLModels>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<ME_DLLCTLModels> Query(ME_DLLCTLModels v)
        {
            var p = new DynamicParameters();

            string sql = "";
            sql += "select ";
            sql += sBr + " DLLCODE, "; // 00.DLL代碼(VARCHAR2)
            sql += sBr + " WH_NO, "; // 01.庫房代碼(VARCHAR2)
            sql += sBr + " TWN_TIME(ENDDATE) ENDDATE, "; // 02.結束時間(DATE)
            sql = sql.Substring(0, sql.Length - 2);
            sql += sBr + " from ME_DLLCTL ";
            sql += sBr + " where 1=1 ";
            if (!string.IsNullOrEmpty(v.WH_NO))
            {
                sql += sBr + " and WH_NO = '" + v.WH_NO + "' "; 
            }
            else
            {
                sql += sBr + " and WH_NO like '*' "; // 針對 排程程式使用( WH_NO 一律填* )
            }
            if (!string.IsNullOrEmpty(v.DLLCODE))
            {
                sql += sBr + " and DLLCODE = :dllcode ";
                p.Add(":dllcode", string.Format("{0}", v.DLLCODE));
            }
            return DBWork.Connection.Query<ME_DLLCTLModels>(sql, p, DBWork.Transaction);
        }
        public int ins(ME_DLLCTLModels v)
        {
            string sql = @"
INSERT INTO MMSADM.ME_DLLCTL (
    DLLCODE, WH_NO, ENDDATE
) VALUES ( 
    :DLLCODE, 
    :WH_NO, 
    sysdate - 180 -- ENDDATE(第一次匯檔，取近半年的資料)
)";
            return DBWork.Connection.Execute(sql, v, DBWork.Transaction);
        } //

        public ME_DLLCTLModels QueryEndDate()
        {
            var p = new DynamicParameters();

            string sql = "";
            sql += "select ";
            //sql += sBr + " DLLCODE, "; // 00.DLL代碼(VARCHAR2)
            //sql += sBr + " WH_NO, "; // 01.庫房代碼(VARCHAR2)
            sql += sBr + " TWN_TIME(SYSDATE) ENDDATE, "; // 02.結束時間(DATE)
            sql = sql.Substring(0, sql.Length - 2);
            sql += sBr + " from dual ";
            return DBWork.Connection.QueryFirst<ME_DLLCTLModels>(sql, null, DBWork.Transaction);
        }
        
        public int UpdateEndDate(ME_DLLCTLModels v)
        {
            string sBr = "\r\n";
            string sql = "";
            sql += " UPDATE ME_DLLCTL SET ENDDATE = TWN_TODATE(:enddate) " + sBr;
            sql += " WHERE 1=1 ";
            sql += " and DLLCODE = :dllcode";
            if (!string.IsNullOrEmpty(v.WH_NO))
            {
                sql += " and WH_NO = :WH_NO";
            }
            return DBWork.Connection.Execute(sql, v, DBWork.Transaction);
        }

    } // ec
} // en
