using System;
using System.Collections.Generic;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Text;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using TSGH.Models;

namespace WebApp.Repository.F
{
    public class FA0044Repository : JCLib.Mvc.BaseRepository
    {
        public FA0044Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ComboModel> GetMatClassCombo()
        {
            string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM " +
                        "FROM MI_MATCLASS  " +
                        "WHERE MAT_CLSID <= '3' " +
                        "ORDER BY 1";
            return DBWork.Connection.Query<ComboModel>(sql, new {}, DBWork.Transaction);
        }

        public IEnumerable<MI_WHMAST> GetWH()
        {
            string sql = @"SELECT WH_NO, WH_NAME " +
                        "FROM MI_WHMAST " +
                        "WHERE WH_KIND IN ('0', '1') AND WH_GRADE <= '4' " +
                        "ORDER BY 1";
            return DBWork.Connection.Query<MI_WHMAST>(sql, new {}, DBWork.Transaction);
        }
    }
}