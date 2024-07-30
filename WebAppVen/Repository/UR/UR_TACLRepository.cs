using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using JCLib.DB;
using Dapper;
using WebAppVen.Models;

namespace WebAppVen.Repository.UR
{
    public class UR_TACLRepository : JCLib.Mvc.BaseRepository
    {
        public UR_TACLRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public int Copy(string rlno_src, string rlno_des)
        {
            int _afrs = 0;

            string sql = @"INSERT INTO UR_TACL (FG, RLNO, V, R, U, P) 
                    SELECT FG, @RLNO_DES, V, R, U, P FROM UR_TACL WHERE RLNO = @RLNO_SRC";
            _afrs += DBWork.Connection.Execute(sql, new { RLNO_DES = rlno_des, RLNO_SRC = rlno_src }, DBWork.Transaction);

            return _afrs;
        }

        public int Update(string rlno, string sg, UR_TACL[] ur_tacl)
        {
            int _afrs = 0;
            string sql = @"DELETE FROM UR_TACL WHERE RLNO=@RLNO AND FG IN (SELECT FG FROM UR_MENU WHERE SG=@SG)";
            _afrs += DBWork.Connection.Execute(sql, new { RLNO = rlno, SG = sg }, DBWork.Transaction);

            sql = @"INSERT INTO UR_TACL (FG, RLNO, V, R, U, P) VALUES (@FG, @RLNO, @V, @R, @U, @P)";
            _afrs += DBWork.Connection.Execute(sql, ur_tacl, DBWork.Transaction);

            return _afrs;
        }

        public int DeleteByRole(string rlno)
        {
            string sql = @"DELETE FROM UR_TACL WHERE RLNO=@RLNO";
            return DBWork.Connection.Execute(sql, new { RLNO = rlno }, DBWork.Transaction);
        }

        public int DeleteByFG(string fg)
        {
            var _afrs = 0;
            var sql = @"DELETE FROM UR_TACL WHERE FG=@FG";

            _afrs = DBWork.Connection.Execute(sql, new { FG = fg }, DBWork.Transaction);

            return _afrs;
        }
    }
}