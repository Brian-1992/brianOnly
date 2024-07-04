using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AA
{
    public class AA0052Repository : JCLib.Mvc.BaseRepository
    {
        public AA0052Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0052> GetAll(string MMCODE, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT  M.OrderHospName,M.OrderEasyName,M.ScientificName,D.InsuOrderCode
                        ,MT.MMNAME_C,MT.MMNAME_E,MT.E_CODATE,MT.M_PURUN,MT.E_DRUGAPLTYPE,MT.E_PURTYPE,MT.PackType,MT.WEXP_ID,MT.MMCODE,M.ORDERCODE
                    FROM HIS_BASORDM M 
                          Left join  HIS_BASORDD D ON M.ORDERCODE=D.ORDERCODE
                          Left join MI_MAST MT ON D.ORDERCODE=MT.MMCODE
                    WHERE TWN_DATE(SYSDATE) >= D.BeginDate AND TWN_DATE(SYSDATE) <= D.EndDate ";

            if (MMCODE != "")
            {
                sql += " AND D.OrderCode   LIKE :p0 ";
                p.Add(":p0", string.Format("%{0}%", MMCODE));
            }


            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0052>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA0052> Get(AA0052 aa0052)
        {
            var sql = @"SELECT M.OrderHospName,M.OrderEasyName,M.ScientificName,D.InsuOrderCode
                        ,MT.MMNAME_C,MT.MMNAME_E,MT.E_CODATE,MT.M_PURUN,MT.E_DRUGAPLTYPE,MT.E_PURTYPE,MT.PackType,MT.WEXP_ID
                    FROM HIS_BASORDM M 
                          Left join  HIS_BASORDD D ON M.ORDERCODE=D.ORDERCODE
                          Left join MI_MAST MT ON D.ORDERCODE=MT.MMCODE
                    WHERE TWN_DATE(SYSDATE) >= D.BeginDate AND TWN_DATE(SYSDATE) <= D.EndDate 
                           AND  MT.MMCODE=:MMCODE";
            return DBWork.Connection.Query<AA0052>(sql, aa0052, DBWork.Transaction);
        }

        //public int Create(TC_MMAGEN tc_mmagen)
        //{
        //    var sql = @"INSERT INTO TC_MMAGEN (MMCODE, AGEN_NAMEC, PUR_UNIT, IN_PURPRICE,  CREATE_TIME, CREATE_USER,  UPDATE_IP,UPDATE_USER, UPDATE_TIME)  
        //                        VALUES (:MMCODE, :AGEN_NAMEC, :PUR_UNIT, :IN_PURPRICE,  sysdate, :CREATE_USER,  :UPDATE_IP, :UPDATE_USER , sysdate)";
        //    return DBWork.Connection.Execute(sql, tc_mmagen, DBWork.Transaction);
        //}

        public int Update(AA0052 aa0052)
        {
            var sql = @"UPDATE MI_MAST 
                                SET  E_CODATE=:E_CODATE, M_PURUN=:M_PURUN, E_DRUGAPLTYPE=:E_DRUGAPLTYPE,
                                    E_PURTYPE=:E_PURTYPE, PackType=:PackType, WEXP_ID=:WEXP_ID, 
                                    UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE MMCODE=:ORDERCODE ";
            return DBWork.Connection.Execute(sql, aa0052, DBWork.Transaction);
        }

        //public int Delete(TC_MMAGEN tc_mmagen)
        //{
        //    var sql = @"DELETE  TC_MMAGEN 
        //                        WHERE MMCODE=:MMCODE AND AGEN_NAMEC=:AGEN_NAMEC";
        //    return DBWork.Connection.Execute(sql, tc_mmagen, DBWork.Transaction);
        //}

        //public bool CheckExists(TC_MMAGEN tc_mmagen)
        //{
        //    string sql = @"SELECT 1 FROM TC_MMAGEN WHERE MMCODE=:MMCODE AND AGEN_NAMEC=:AGEN_NAMEC";
        //    return !(DBWork.Connection.ExecuteScalar(sql, tc_mmagen, DBWork.Transaction) == null);
        //}
    }
}