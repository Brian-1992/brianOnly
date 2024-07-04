using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

using JCLib.DB.Tool;

namespace WebApp.Repository.B
{
    public class BD0005Repository : JCLib.Mvc.BaseRepository
    {
        String sBr = "\r\n";
        FL l = new FL("BD0005Respository");

        public BD0005Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<BD0005M> GetAllM(BD0005M v, int page_index, int page_size, string sorters)
        {
            int iP = 0;
            String sP = "";
            var p = new DynamicParameters();

            var sql = "";
            sql +=  " select  ";
            sql +=  " PO_NO,  ";
            sql +=  " (select  AGEN_NO||' '||AGEN_NAMEC from PH_VENDER where agen_no=m.agen_no) as AGEN_NO, ";
            sql +=  " (select data_desc from PARAM_D where  DATA_NAME='PO_STATUS' and DATA_VALUE=m.po_status) as PO_STATUS,  ";
            sql +=  " MEMO,  ";
            sql +=  " ISCONFIRM,   ";
            if (v.UPDATE_USER != null & v.UPDATE_IP != null)
            {
                sql += "'" + v.UPDATE_USER + "' as UPDATE_USER,  ";
                sql += "'" + v.UPDATE_IP + "' as UPDATE_IP,  ";
            }
            sql +=  " SMEMO ";
            sql +=  " FROM MM_PO_M m  ";
            sql += " where substr(po_no,1,3) in ('INV','GEN') ";
            sql += "  and M_CONTID=:M_CONTID ";
            p.Add(":M_CONTID", v.M_CONTID);
            sql += "  and MAT_CLASS=:MAT_CLASS ";
            p.Add(":MAT_CLASS", v.MAT_CLASS);
            if (v.TWN_DATE_START != "")
            {
                sql += " AND substr(po_no,5,7) >=:TWN_DATE_START ";
                p.Add(":TWN_DATE_START", v.TWN_DATE_START);
            }
            if (v.TWN_DATE_END != "")
            {
                sql += " AND substr(po_no,5,7) <=:TWN_DATE_END ";
                p.Add(":TWN_DATE_END", v.TWN_DATE_END);
            }

            //l.getTwoDateCondition("PO_TIME", v.TWN_DATE_START, v.TWN_DATE_END,ref sql, ref iP,ref p);
            //l.lg("GetAllM()", l.getDebugSql(sql, p));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<BD0005M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<BD0005D> GetAllD(BD0005D v, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = "";
            sql += sBr + " select  ";
            sql += sBr + " MMCODE, ";
            sql += sBr + " (select MMNAME_C from mi_mast where mmcode=a.mmcode) as MMNAME_C, ";
            sql += sBr + " (select MMNAME_E from mi_mast where mmcode=a.mmcode) as MMNAME_E, ";
            sql += sBr + " M_AGENLAB,  ";
            sql += sBr + " M_PURUN,  ";
            sql += sBr + " PO_PRICE,  ";
            sql += sBr + " PO_QTY,  ";
            sql += sBr + " PO_AMT,  ";
            sql += sBr + " MEMO  ";
            sql += sBr + " from MM_PO_D a  ";
            sql += sBr + " where a.PO_NO =:po_no ";

            p.Add(":po_no", v.PO_NO);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BD0005D>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public int Update(BD0005M v)
        {
            var sql = "";
            sql += sBr + " update MM_PO_M set ";
            sql += sBr + " MEMO=:MEMO, ";
            sql += sBr + " SMEMO=:SMEMO, ";
            sql += sBr + " UPDATE_TIME = SYSDATE, ";
            sql += sBr + " UPDATE_USER = :UPDATE_USER, ";
            sql += sBr + " UPDATE_IP = :UPDATE_IP  ";
            sql += sBr + " where PO_NO=:PO_NO ";

            int iP = 0;
            String sP = "";
            var p = new DynamicParameters();
            sP = ":p" + iP++;
            p.Add(sP, string.Format("{0}", v.MEMO));
            sP = ":p" + iP++;
            p.Add(sP, string.Format("{0}", v.SMEMO));
            sP = ":p" + iP++;
            p.Add(sP, string.Format("{0}", v.UPDATE_USER));
            sP = ":p" + iP++;
            p.Add(sP, string.Format("{0}", v.UPDATE_IP));
            sP = ":p" + iP++;
            p.Add(sP, string.Format("{0}", v.PO_NO));

            l.lg("Update()", l.getDebugSql(sql, p));
            return DBWork.Connection.Execute(sql, v, DBWork.Transaction);
        }
        public int UpdateStatus(BD0005M v)
        {
            var sql = "";
            sql +=  " update MM_PO_M set ";
            sql +=  " UPDATE_TIME = SYSDATE, ";
            sql +=  " UPDATE_USER = :UPDATE_USER, ";
            sql +=  " UPDATE_IP = :UPDATE_IP,  ";
            sql +=  " PO_STATUS ='83'  ";
            sql +=  " where PO_NO=:PO_NO ";

            int iP = 0;
            String sP = "";
            var p = new DynamicParameters();
            sP = ":p" + iP++;
            p.Add(sP, string.Format("{0}", v.UPDATE_USER));
            sP = ":p" + iP++;
            p.Add(sP, string.Format("{0}", v.UPDATE_IP));
            sP = ":p" + iP++;
            p.Add(sP, string.Format("{0}", v.PO_NO));
            return DBWork.Connection.Execute(sql, v, DBWork.Transaction);
        }
        public IEnumerable<BD0005M> Get(BD0005M v)
        {
            var sql = @"SELECT * FROM MM_PO_M WHERE PO_NO = :PO_NO";
            return DBWork.Connection.Query<BD0005M>(sql, new { PO_NO = v.PO_NO }, DBWork.Transaction);
        }

        // -- 報表用 -- 
        public IEnumerable<BD0005M> ReportsM(BD0005M v, int page_index, int page_size, string sorters)
        {
            int iP = 0;
            String sP = "";
            var p = new DynamicParameters();

            var sql = "";
            sql += " select " + sBr;
            sql += " a.po_no," + sBr;
            sql += " (select agen_no from MM_PO_M where PO_NO = a.PO_NO) AGEN_NO," + sBr;
            sql += " (select MEMO from MM_PO_M where PO_NO = a.PO_NO) MEMO," + sBr;
            sql += " (select SMEMO from MM_PO_M where PO_NO = a.PO_NO) SMEMO," + sBr;
            sql += " (select M_CONTID from MM_PO_M where PO_NO = a.PO_NO) M_CONTID," + sBr;
            sql += " (" + sBr;
            sql += "     select AGEN_NAMEC from PH_VENDER v where v.AGEN_NO =" + sBr;
            sql += "         (select agen_no from MM_PO_M where PO_NO = a.PO_NO)" + sBr;
            sql += " ) 廠商名稱," + sBr;
            sql += " a.mmcode as 院內碼, " + sBr;
            sql += " b.mmname_c as 中文品名, " + sBr;
            sql += " b.mmname_e as 英文品名, " + sBr;
            sql += " a.m_agenlab as 廠牌, " + sBr;
            sql += " a.m_purun as 單位, " + sBr;
            sql += " ltrim(to_char(a.po_price, '99999999999999999.99')) as 單價, " + sBr;
            sql += " a.po_qty as 數量," + sBr;
            sql += " ltrim(to_char(floor(a.po_amt), '99999999999999999.99')) as 金額," + sBr;
            sql += " ltrim(to_char(a.m_discperc,'99.9')) as 折讓百分比" + sBr;
            sql += " from MM_PO_D a, MI_MAST b " + sBr;
            sql += " where substr(a.po_no,1,3) in ('INV','GEN') " + sBr;
            sql += " and a.mmcode=b.mmcode " + sBr;
            if (v.PO_NO != "")
            {
                sP = ":p" + iP++;
                sql += " and a.PO_NO=" + sP + " " + sBr;
                p.Add(sP, string.Format("{0}", v.PO_NO));
            }
            l.lg("ReportsM()", l.getDebugSql(sql, p));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<BD0005M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<BD0005M> ReportsD(BD0005M v, int page_index, int page_size, string sorters)
        {
            int iP = 0;
            String sP = "";
            var p = new DynamicParameters();

            var sql = "";
            sql += " select " + sBr;
            sql += " distinct  a.mmcode as 院內碼, " + sBr;
            sql += " inid_name  as 單位名稱, " + sBr;
            sql += " floor(b.appqty/a.unit_swap) as 申請量   " + sBr;
            sql += " from  MM_PO_D a, " + sBr;
            sql += " ME_DOCD b, " + sBr;
            sql += " ME_DOCM c, " + sBr;
            sql += " UR_INID d, " + sBr;
            sql += " MI_MAST e" + sBr;
            sql += " where 1=1 " + sBr;
            sql += " and a.pr_no=b.rdocno " + sBr;
            sql += " and a.mmcode=b.mmcode " + sBr;
            sql += " and a.mmcode=e.mmcode " + sBr;
            sql += " and b.docno=c.docno " + sBr;
            sql += " and c.appdept=d.inid " + sBr;
            sql += " and e.m_storeid='0' " + sBr;
            if (v.PO_NO != "")
            {
                sP = ":p" + iP++;
                sql += " and a.PO_NO=" + sP + " " + sBr;
                p.Add(sP, string.Format("{0}", v.PO_NO));
            }
            sql += " order by a.mmcode ";
            l.lg("ReportsD()", l.getDebugSql(sql, p));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<BD0005M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public int Create_ph_maillog(PH_MAILLOG v)
        {
            if (v.MAILBODY.ToString().Length > 4000)
                v.MAILBODY = v.MAILBODY.ToString().Substring(0, 3900);
            var sql = "";
            sql += " 	Insert into PH_MAILLOG(" + sBr;
            sql += " 	SEQ, " + sBr;
            sql += " 	LOG_TIME, " + sBr;
            sql += " 	MAILFROM, " + sBr;
            sql += " 	MAILTO, " + sBr;
            sql += " 	MAILCC, " + sBr;
            sql += " 	MSUBJECT, " + sBr;
            sql += " 	MAILTYPE, " + sBr;
            sql += " 	MailBody, " + sBr;
            sql += " 	CREATE_USER, " + sBr;
            sql += " 	UPDATE_IP" + sBr;
            sql += " )" + sBr;
            sql += " Values(" + sBr;
            sql += " 	PH_MAILLOG_SEQ.nextval, " + sBr; // SEQ
            sql += " 	sysdate, " + sBr; // LOG_TIME
            sql += " 	'FAX', " + sBr;
            sql += " 	'FAX', " + sBr;
            sql += " 	null, " + sBr;
            sql += " 	:MSUBJECT, " + sBr;
            sql += " 	'衛材訂單-FAX', " + sBr;
            sql += " 	:MailBody, " + sBr;
            sql += " 	:CREATE_USER, " + sBr;
            sql += " 	null" + sBr; // UPDATE_IP
            sql += " )" + sBr;
            return DBWork.Connection.Execute(sql, v, DBWork.Transaction);
        }
   
        public class MI_MAST_QUERY_PARAMS
        {
            public string MMCODE;
            public string MMNAME_C;
            public string MMNAME_E;
            public string MAT_CLASS;

            public string WH_NO;
            public string IS_INV;  // 需判斷庫存量>0
            public string E_IFPUBLIC;  // 是否公藥
        }
    }
}
