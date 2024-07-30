using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebAppVen.Models;
using System.Collections.Generic;

namespace WebAppVen.Repository.B
{
    public class BH0011Repository : JCLib.Mvc.BaseRepository
    {
        public BH0011Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ComboModel> GetDocnos(string userId) {
            string sql = @"
                select concat(a.CRDOCNO, ' ', a.REQDATE) as VALUE,
                       CRDOCNO as KEY_CODE
                  from WB_CR_DOC a
                 where agen_no = @userId
                   and reqdate >= DATEADD(dd, DATEDIFF(dd, 0, getdate()), 0)
                 order by reqdate
            ";
            return DBWork.Connection.Query<ComboModel>(sql, new { userId = userId }, DBWork.Transaction);
        }

        public IEnumerable<WB_CR_DOC> GetAll(string userId, string docno, string start_date, string end_date) {
            string sql = @"
                select cast(convert(char(8),cast(reqdate as datetime),112) as int)- 19110000 as reqdate, 
                       crdocno, mmcode, mmname_c, mmname_e,
                       appqty, base_unit, wh_name, cr_uprice, 
                       cast(convert(char(8),cast(apptime as datetime),112) as int)- 19110000 as apptime,
                       wexp_id, inqty, lot_no, patientName, chartNo,
                       cast(convert(char(8),cast(exp_date as datetime),112) as int)- 19110000 as exp_date,
                       in_status,
                       (case when in_status = 'A' then 'A 待確認'
							 when in_status = 'B' then 'B 已確認待傳內網'
							 when in_status = 'C' then 'C 已傳內網'
						end) as in_status_text
                  from WB_CR_DOC
                 where cast(agen_no as nvarchar) = @userId
            ";
            if (string.IsNullOrEmpty(docno) == false) {
                sql += "  and crdocno = @docno";
            }
            if (string.IsNullOrEmpty(start_date) == false)
            {
                sql += "  and DATEADD(dd, DATEDIFF(dd, 0, reqdate), 0) >= CAST(STR(@start_date+19110000) as datetime)";
            }
            if (string.IsNullOrEmpty(end_date) == false)
            {
                sql += "  and DATEADD(dd, DATEDIFF(dd, 0, reqdate), 0) <= CAST(STR(@end_date+19110000) as datetime)";
            }

            return DBWork.PagingQuery<WB_CR_DOC>(sql, new { userId, docno, start_date, end_date }, DBWork.Transaction);
        }

        public int UpdateCrDoc(string crdocno, string inqty, string lot_no, string exp_date, string userId, string updateIp) {
            string sql = @"
                update WB_CR_DOC
                   set inqty = @inqty,
                       lot_no = @lot_no,
                       exp_date = CAST(STR(@exp_date+19110000) as datetime),
                       update_user = @userId,
                       update_time = getDate(),
                       update_ip = @updateIp
                 where crdocno = @crdocno
            ";
            return DBWork.Connection.Execute(sql, new { crdocno, inqty, lot_no, exp_date, userId, updateIp }, DBWork.Transaction);
        }

        public int UpdateCrDocStatus(string crdocno, string userId, string updateIp)
        {
            string sql = @"
                update WB_CR_DOC
                   set in_status = 'B'
                 where crdocno = @crdocno
                   and in_status = 'A'
            ";
            return DBWork.Connection.Execute(sql, new { crdocno, userId, updateIp }, DBWork.Transaction);
        }

        public IEnumerable<WB_CR_DOC> GetPrint(string crdocno) {
            string sql = @"
                select *
                  from WB_CR_DOC
                 where crdocno = @crdocno
            ";
            return DBWork.Connection.Query<WB_CR_DOC>(sql, new { crdocno }, DBWork.Transaction);
        }
    }
}