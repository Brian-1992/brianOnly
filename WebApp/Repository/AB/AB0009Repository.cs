using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using JCLib.DB;
using JCLib.Mvc;
using Dapper;
using WebApp.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.AB
{
    public class AB0009Repository : JCLib.Mvc.BaseRepository
    {
        public AB0009Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public SP_MODEL CreateMrDoc()
        {
            var p = new OracleDynamicParameters();
            p.Add("I_APPID", value: "", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 8);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 1);
            p.Add("O_ERRMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 600);

            DBWork.Connection.Query("CREATE_MR_DOC", p, commandType: CommandType.StoredProcedure);

            string errmsg = "";
            if (!p.Get<OracleString>("O_ERRMSG").IsNull)        // 因store procedure呼叫正常, O_ERRMSG會回傳空字串, 會導致取值是NULL, 所以要先判定
                errmsg = p.Get<OracleString>("O_ERRMSG").Value;

            SP_MODEL sp = new SP_MODEL
            {
                O_RETID = p.Get<OracleString>("O_RETID").Value,
                O_ERRMSG = errmsg
            };
            return sp;
        }

    }
}