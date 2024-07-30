using System;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using System.Text;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

namespace WebApp.Repository.F
{
    public class FA0054AReportMODEL : JCLib.Mvc.BaseModel
    {
        public double F1 { get; set; }
        public double F2 { get; set; }
        public double F3 { get; set; }
        public double F4 { get; set; }
        public double F5 { get; set; }
        public double F6 { get; set; }
        public double F7 { get; set; }
        public double F8 { get; set; }

    }
    public class FA0054BReportMODEL : JCLib.Mvc.BaseModel
    {
        public string F1 { get; set; }
        public double F2 { get; set; }
        public double F3 { get; set; }
        public double F4 { get; set; }
        public double F5 { get; set; }
        public double F6 { get; set; }

    }
    public class FA0054Repository : JCLib.Mvc.BaseRepository
    {
        public FA0054Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<FA0054AReportMODEL> GetPrintDataA(string p0)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT '' F1,'' F2,'' F3,'' F4,'' F5,'' F6,'' F7,'' F8 FROM DUAL ";
            return DBWork.Connection.Query<FA0054AReportMODEL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<FA0054BReportMODEL> GetPrintDataB(string p0)
        {
            var p = new DynamicParameters();
            var sql = @"(SELECT to_number(substr(:p0,4,2))||'月1日' F1,'' F2,'' F3,'' F4,'' F5,'' F6,'' F7,'' F8 FROM DUAL)
                        UNION ALL 
                        (SELECT to_number(substr(:p0,4,2))||'月15日' F1,'' F2,'' F3,'' F4,'' F5,'' F6,'' F7,'' F8 FROM DUAL) ";
            p.Add(":p0", p0);
            return DBWork.Connection.Query<FA0054BReportMODEL>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<FA0054BReportMODEL> GetPrintDataC(string p0)
        {
            var p = new DynamicParameters();
            var sql = @"(SELECT to_number(substr(:p0,4,2))||'月1日' F1,'' F2,'' F3,'' F4,'' F5,'' F6,'' F7,'' F8 FROM DUAL)
                        UNION ALL 
                        (SELECT to_number(substr(:p0,4,2))||'月15日' F1,'' F2,'' F3,'' F4,'' F5,'' F6,'' F7,'' F8 FROM DUAL) ";
            p.Add(":p0", p0);
            return DBWork.Connection.Query<FA0054BReportMODEL>(sql, p, DBWork.Transaction);
        }

    }
}
