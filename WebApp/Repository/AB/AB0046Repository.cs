using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using WebApp.Models.AB;
using WebApp.Models.C;
using System.Globalization;

namespace WebApp.Repository.AB                      // WebApp\Repository\C\CD0007Repository.cs          
{
    public class AB0046Repository : JCLib.Mvc.BaseRepository
    {
        public AB0046Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public int Create(AB0046 ab0046)
        {
            /////var maxFreqNo = getMaxFreqNo().ToString();

            var sql = @"INSERT INTO ME_FSBL (VISITKIND, LOCATION, FREQNO,BEGINTIME,ENDTIME,DEFAULTSTOCKCODE,ROUTINESTOCKCODE,EXCEPTSTOCKCODE,
                                             TAKEOUTSTOCKCODE,TPNSTOCKCODE,PCASTOCKCODE,CHEMOSTOCKCODE,RESEARCHSTOCKCODE,RETURNSTOCKCODE,
                                             CREATEDATETIME,CREATEOPID,PROCDATETIME, PROCOPID     ) 
                        VALUES ('" + ab0046.VISITKIND + "','"
                                   + ab0046.LOCATION + "','"
                                   + ab0046.FREQNO + "','"
                                   + ab0046.BEGINTIME + "','"
                                   + ab0046.ENDTIME + "','"
                                   + ab0046.DEFAULTSTOCKCODE + "','"
                                   + ab0046.ROUTINESTOCKCODE + "','"
                                   + ab0046.EXCEPTSTOCKCODE + "','"
                                   + ab0046.TAKEOUTSTOCKCODE + "','"
                                   + ab0046.TPNSTOCKCODE + "','"
                                   + ab0046.PCASTOCKCODE + "','"
                                   + ab0046.CHEMOSTOCKCODE + "','"
                                   + ab0046.RESEARCHSTOCKCODE + "','"
                                   + ab0046.RETURNSTOCKCODE + "',sysdate" + ",'"
                                   + ab0046.CREATEOPID + "',sysdate" + ",'"
                                   + ab0046.PROCOPID + "')";
            return DBWork.Connection.Execute(sql, ab0046, DBWork.Transaction);
        }

        public int Update(AB0046 ab0046)
        {
            var sql = @"UPDATE ME_FSBL SET BEGINTIME = '" + ab0046.BEGINTIME + "', ENDTIME = '" + ab0046.ENDTIME + "', DEFAULTSTOCKCODE = '" + ab0046.DEFAULTSTOCKCODE + "',";
            sql += " ROUTINESTOCKCODE = '" + ab0046.ROUTINESTOCKCODE + "', EXCEPTSTOCKCODE = '" + ab0046.EXCEPTSTOCKCODE + "',TAKEOUTSTOCKCODE = '" + ab0046.TAKEOUTSTOCKCODE + "',";
            sql += " TPNSTOCKCODE = '" + ab0046.TPNSTOCKCODE + "', PCASTOCKCODE = '" + ab0046.PCASTOCKCODE + "',";
            sql += " CHEMOSTOCKCODE = '" + ab0046.CHEMOSTOCKCODE + "', RESEARCHSTOCKCODE = '" + ab0046.RESEARCHSTOCKCODE + "', RETURNSTOCKCODE='" + ab0046.RETURNSTOCKCODE + "',CREATEDATETIME= sysdate,";
            sql += " CREATEOPID = '" + ab0046.CREATEOPID + "', PROCDATETIME = sysdate, PROCOPID= '" + ab0046.PROCOPID + "'";
            sql += " WHERE VISITKIND = '" + ab0046.VISITKIND + "' and LOCATION = '"+ ab0046.LOCATION + "' and FREQNO= '" + ab0046.FREQNO + "'";
            return DBWork.Connection.Execute(sql, ab0046, DBWork.Transaction);
        }

        // public int Delete(string visitkind, string location, string freqno)
        public int Delete(AB0046 ab0046)
        {
            // 刪除資料
            var sql = @"DELETE FROM ME_FSBL WHERE VISITKIND = '" + ab0046.VISITKIND + "' and LOCATION = '" + ab0046.LOCATION + "' and FREQNO='"+ ab0046.FREQNO + "'";
            //return DBWork.Connection.Execute(sql, new { VISITKIND = visitkind, MMCODE = location, FREQNO=freqno }, DBWork.Transaction);
            return DBWork.Connection.Execute(sql, ab0046, DBWork.Transaction);
        }

        public bool CheckExists(string visitkind, string location,string freqno )
        {
            string sql = @"SELECT 1 FROM ME_FSBL WHERE VISITKIND=:visitkind and LOCATION=:location and FREQNO=:freqno";
            return !(DBWork.Connection.ExecuteScalar(sql, new { VISITKIND = visitkind, LOCATION = location, FREQNO = freqno }, DBWork.Transaction) == null);
        }

        public object getMaxFreqNo()
        {
            string sql = @"SELECT max(freqNo) from ME_FSBL";
            return (DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction));
        }

        public IEnumerable<MI_WINVCTL> Get(string visitkind, string location, string freqno)
        {
            var sql = @"SELECT A.* FROM ME_FSBL a WHERE 1=1 AND A.VISITKIND = :visitkind and A.LOCATION = :location and A.FREQNO=:freqno";
            return DBWork.Connection.Query<MI_WINVCTL>(sql, new { VISITKIND = visitkind, LOCATION = location, FREQNO=freqno }, DBWork.Transaction);
        }

        // 醫師
        public IEnumerable<AB0046> GetOrderDrCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @" SELECT distinct(OrderDr) as ORDERDR, chinName as CHINNAME from ME_AB0079D";

            sql += " ORDER BY OrderDr ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0046>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        
        // 門住別
        public IEnumerable<AB0046> GetVisitKindCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT a.WH_NO  as VALUE ,a.WH_NAME as TEXT FROM MI_WHMAST a WHERE a.WH_KIND = '1' ";
            
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0046>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);

        }

        // 診間/病房代碼
        public IEnumerable<AB0046> GetLocationCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT a.WH_NO  as VALUE ,a.WH_NAME as TEXT FROM MI_WHMAST a WHERE a.WH_KIND = '0' ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0046>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AB0046> GetCommonCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT a.WH_NO  as VALUE ,a.WH_NAME as TEXT FROM MI_WHMAST a WHERE a.WH_KIND = '0' and WH_GRADE ='2' ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0046>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AB0046> GetAB0046Detail(string visitkind, string location, int page_index, int page_size, string sorters) 
        {
            //  VISITKIND                -- 門住別
            //  LOCATION                 -- 診間/護理站",
         
            var p = new DynamicParameters();
            
            var sql = @"select VISITKIND,LOCATION,FREQNO, BEGINTIME,ENDTIME,DEFAULTSTOCKCODE,ROUTINESTOCKCODE,EXCEPTSTOCKCODE,TAKEOUTSTOCKCODE,
                        TPNSTOCKCODE, PCASTOCKCODE,  CHEMOSTOCKCODE, RESEARCHSTOCKCODE, RETURNSTOCKCODE  from ME_FSBL  ";
            sql += " where 1=1 ";

            // 門住別
            if (visitkind.Trim() != "")
            {
                sql += " and VISITKIND = :visitkind ";
                p.Add(":VISITKIND", string.Format("{0}", visitkind.Trim()));
            }

            // 起始時間 (例行時間起)       
            if (location.Trim() != "")
            {
                sql += " and LOCATION = :location ";
                p.Add(":LOCATION", string.Format("{0}", location.Trim()));
            }

            sql += " order by visitkind,location,freqno asc ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0046>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);

        }

       
    }
}