using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebAppVen.Models;
using System.Collections.Generic;

namespace WebAppVen.Repository.B
{
    public class BH0009Repository : JCLib.Mvc.BaseRepository
    {
        public BH0009Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //更新
        public int Update(string invoice_batno)
        {
            var sql = @"update WB_INVOICE_MAILBACK 
                           set BACK_DT=getdate(),
                               STATUS='B'
                         where INVOICE_BATNO=@invoice_batno
                           and STATUS='A'
                       ";
            return DBWork.Connection.Execute(sql, new { INVOICE_BATNO = invoice_batno }, DBWork.Transaction);
        }
    }
}