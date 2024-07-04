using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.B
{
    public class BD0007Repository : JCLib.Mvc.BaseRepository
    {
        public BD0007Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MM_PO_M> GetMasterAll(DateTime startDate, DateTime endDate, string replyType, string mat_class, bool isHospCode0) {

            var p = new DynamicParameters();

            string sql = string.Empty;

            if (isHospCode0)
            {
                sql = @"select PO_NO as PO_NO, p.EMAIL,
                                  p.AGEN_NO||' '||p.AGEN_NAMEC as AGEN_NO,
                                  (case when isback='Y' then '已回覆' 
                                        when isback='N' then '未回覆' end) ISBACK,
                                  (select data_desc from PARAM_D 
                                    where  DATA_NAME='PO_STATUS' 
                                      and DATA_VALUE=m.po_status) as PO_STATUS , 
                                  m.MEMO, m.ISCONFIRM,  m.SMEMO
                             from MM_PO_M m, PH_VENDER p
                            where m.agen_no=p.agen_no(+) and substr(m.po_no,1,3) in ('INV','GEN') and mat_class=:p4 and isback=:p2
                              and TRUNC(m.PO_TIME, 'DD') BETWEEN TO_DATE(:p0,'YYYY-MM-DD') AND TO_DATE(:p1,'YYYY-MM-DD')
                            order by po_no, AGEN_NO";
            }
            else {
                sql = @"select PO_NO as PO_NO, p.EMAIL,
                                  p.AGEN_NO||' '||p.AGEN_NAMEC as AGEN_NO,
                                  (case when isback='Y' then '已回覆' 
                                        when isback='N' then '未回覆' end) ISBACK,
                                  (select data_desc from PARAM_D 
                                    where  DATA_NAME='PO_STATUS' 
                                      and DATA_VALUE=m.po_status) as PO_STATUS , 
                                  m.MEMO, m.ISCONFIRM,  m.SMEMO
                             from MM_PO_M m, PH_VENDER p
                            where m.agen_no=p.agen_no(+) and mat_class=:p4 and isback=:p2
                              and TRUNC(m.PO_TIME, 'DD') BETWEEN TO_DATE(:p0,'YYYY-MM-DD') AND TO_DATE(:p1,'YYYY-MM-DD')
                            order by po_no, AGEN_NO";
            }

            p.Add("p0", startDate.ToString("yyyy-MM-dd"));
            p.Add("p1", endDate.ToString("yyyy-MM-dd"));
            p.Add("p2", replyType);
            p.Add("p4", mat_class);
            return DBWork.Connection.Query<MM_PO_M>(sql, p, DBWork.Transaction);
        }

        public int MasterUpdate(MM_PO_M mm_po_m) {
            var sql = @"UPDATE MM_PO_M
                           SET MEMO = :MEMO, SMEMO = :SMEMO, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                         WHERE PO_NO = :PO_NO";
            return DBWork.Connection.Execute(sql, mm_po_m, DBWork.Transaction);
        }

        public int SendEmail(MM_PO_M mm_po_m)
        {
            var sql = @"UPDATE MM_PO_M
                           SET PO_STATUS = :PO_STATUS, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                         WHERE PO_NO = :PO_NO";
            return DBWork.Connection.Execute(sql, new { PO_NO = mm_po_m.PO_NO, PO_STATUS = mm_po_m.PO_STATUS, UPDATE_USER = mm_po_m.UPDATE_USER, UPDATE_IP = mm_po_m.UPDATE_IP}, DBWork.Transaction);
        }

        public IEnumerable<MM_PO_D> GetDetailAll(string po_no) {
            string sql = @"select MMCODE,
                                  (select MMNAME_C from mi_mast where mmcode=a.mmcode) as MMNAME_C,
                                  (select MMNAME_E from mi_mast where mmcode=a.mmcode) as MMNAME_E,
                                  M_AGENLAB,
                                  M_PURUN,
                                  PO_PRICE,
                                  PO_QTY,
                                  PO_AMT,
                                  MEMO 
                             from MM_PO_D a where PO_NO = :PO_NO
                            order by mmcode";
            return DBWork.Connection.Query<MM_PO_D>(sql, new { PO_NO = po_no}, DBWork.Transaction);
        }

        public string GetHospCode()
        {
            string sql = @"
                select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode'
            ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }
    }
}