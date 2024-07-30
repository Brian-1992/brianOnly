using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebAppVen.Models;
using System.Collections.Generic;

namespace WebAppVen.Repository.B
{
    public class BH0007Repository : JCLib.Mvc.BaseRepository
    {
        public BH0007Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //新增
        public int Create(ME_MAILBACK ME_MAILBACK)
        {
            var sql = @"INSERT INTO ME_MAILBACK 
                                    (SEQ, AGEN_NO, MMCODE, EXP_QTY, BACK_DT, STATUS, CREATE_TIME, UPDATE_IP,MAIL_NO)
                        VALUES ( (SELECT ISNULL(MMK.SEQ, 0) + 1 SEQ 
                                  FROM (SELECT MAX(SEQ) SEQ 
			                            FROM ME_MAILBACK) MMK), 
		                          @AGEN_NO, 
		                      	  '1',
			                      0,
			                      SYSDATETIME(), 
		                      	  'A', 
			                      SYSDATETIME(), 
			                      @UPDATE_IP,
                                  @MAIL_NO)";
            return DBWork.Connection.Execute(sql, ME_MAILBACK, DBWork.Transaction);
        }

    }
}
