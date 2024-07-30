using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.B
{
    public class BD0004Repository : JCLib.Mvc.BaseRepository
    {
        public BD0004Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        public IEnumerable<MM_PO_M> GetMasterAll(string apptime_bg, string apptime_ed, string m_contid, string mat_class, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"select PO_NO, m.AGEN_NO||' '||p.AGEN_NAMEC as AGEN_NO, p.EMAIL,
                       (select data_desc from PARAM_D where  DATA_NAME='PO_STATUS' and DATA_VALUE=m.po_status) as PO_STATUS , m.MEMO, m.ISCONFIRM,  m.SMEMO,
                       (case when m.iscr = 'Y' then '是' else '否'  end) as iscr
                       FROM MM_PO_M m, PH_VENDER p
                       where m.agen_no=p.agen_no(+) and substr(po_no,1,3) in ('INV','GEN')  ";

            if (apptime_bg != "")
            {
                sql += " AND substr(po_no,5,7) >=:p1 ";
                p.Add(":p1", apptime_bg);
            }
            if (apptime_ed != "")
            {
                sql += " AND substr(po_no,5,7) <=:p2 ";
                p.Add(":p2", apptime_ed);
            }
            if (m_contid != "")
            {
                sql += "AND M_CONTID =:p3 ";
                p.Add(":p3", m_contid);
            }
            if (mat_class != "")
            {
                sql += " AND MAT_CLASS =:p4 ";
                p.Add(":p4", mat_class);
            }
            sql += "order by po_no, AGEN_NO";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MM_PO_M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<MM_PO_D> GetDetailAll(string pono, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select MMCODE,(select MMNAME_C from mi_mast where mmcode=a.mmcode) as MMNAME_C,
                       (select MMNAME_E from mi_mast where mmcode=a.mmcode) as MMNAME_E,
                       M_AGENLAB,M_PURUN,PO_PRICE,PO_QTY,PO_AMT,MEMO from MM_PO_D a where po_no =:p0 Order by mmcode ";

            p.Add(":p0", pono);


            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MM_PO_D>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public int MasterUpdate(MM_PO_M mm_po_m)
        {
            var sql = @"update MM_PO_M SET MEMO=:MEMO, SMEMO=:SMEMO , UPDATE_TIME=SYSDATE, UPDATE_USER=:UPDATE_USER,UPDATE_IP=:UPDATE_IP
                        where PO_NO =:PO_NO";
            return DBWork.Connection.Execute(sql, mm_po_m, DBWork.Transaction);
        }



        public int MasterUpdateMAIL(string pono, string upuser, string upip)
        {
            var sql = @"update MM_PO_M SET PO_STATUS='84', UPDATE_TIME=SYSDATE, UPDATE_USER=:UPDATE_USER,UPDATE_IP=:UPDATE_IP
                        where PO_NO =:PO_NO AND PO_STATUS='80'";
            return DBWork.Connection.Execute(sql, new { PO_NO = pono, UPDATE_USER = upuser, UPDATE_IP = upip }, DBWork.Transaction);
        }

        public IEnumerable<MM_PO_M> MasterGet(string pono)
        {
            var sql = @" select PO_NO, (select  AGEN_NO||' '||AGEN_NAMEC from PH_VENDER where agen_no=m.agen_no) as AGEN_NO,
                       (select data_desc from PARAM_D where  DATA_NAME='PO_STATUS' and DATA_VALUE=m.po_status) as PO_STATUS , MEMO, ISCONFIRM,  SMEMO
                       FROM MM_PO_M m where po_no=:PO_NO ";
            return DBWork.Connection.Query<MM_PO_M>(sql, new { PO_NO = pono }, DBWork.Transaction);
        }
    }
}