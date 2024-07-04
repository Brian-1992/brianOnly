using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebAppVen.Models;
using System.Collections.Generic;

namespace WebAppVen.Repository.B
{
    public class BH0006Repository : JCLib.Mvc.BaseRepository
    {
        public BH0006Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //新增
        public int Create(WB_MAILBACK WB_MAILBACK)
        {
            var sql = @"INSERT INTO WB_MAILBACK (SEQ, AGEN_NO, PO_NO, BACK_DT, STATUS, UPDATE_IP)
                        VALUES ( ( SELECT ISNULL( WMK.SEQ, 0) + 1 SEQ 
                                   FROM ( SELECT MAX (SEQ) SEQ
                                          FROM WB_MAILBACK ) WMK),
                                 @AGEN_NO,
                                 @PO_NO,
                                 SYSDATETIME(),
                                 'A',
                                 @UPDATE_IP
                                )";
            return DBWork.Connection.Execute(sql, WB_MAILBACK, DBWork.Transaction);
        }

    }
}
