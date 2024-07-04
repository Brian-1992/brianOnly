using Dapper;
using JCLib.DB;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Collections.Generic;
using System.Data;
using WebApp.Models;

namespace WebApp.Repository.UR
{
    public class MassDataRepository : JCLib.Mvc.BaseRepository
    {
        public MassDataRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public DataTable GetAll()
        {
            var dt = new DataTable();
            var sql = @"SELECT * FROM MI_MAST";
            using (var rdr = DBWork.Connection.ExecuteReader(sql))
            {
                dt.Load(rdr);
            }
            return dt;
        }
        
    }
}