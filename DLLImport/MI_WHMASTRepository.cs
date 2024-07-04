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
    public class MI_WHMASTRepository : JCLib.Mvc.BaseRepository
    {
        string sBr = "\r\n";
        FL l = new FL("DLLImport.Repository.MI_WHMASTRepository");

        public MI_WHMASTRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }


        public IEnumerable<MI_WHMASTModels> GetDist_WH_NO()
        {
            var p = new DynamicParameters();
            string sql = "";
            sql += "select distinct wh_no ";
            sql += "from MI_WHMAST ";
            sql += "where 1=1  ";
            sql += "and wh_kind = '0' ";
            sql += "and wh_grade >= '2' ";
            sql += "and wh_grade <> '5' ";
            return DBWork.Connection.Query<MI_WHMASTModels>(sql, p, DBWork.Transaction);
        } // 

    } // ec
} // en
