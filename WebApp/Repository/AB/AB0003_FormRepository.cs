using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.AB
{
    public class AB0003_FormRepository : JCLib.Mvc.BaseRepository
    {
        public AB0003_FormRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MI_MAST> GetAll(string docno, string mat_class, string m_storeid, string mmcode, string mmname_c, string mmname_e) {
            var p = new DynamicParameters();

            string sql = string.Format(@"
                           select a.mmcode, a.mmname_c, a.mmname_e, a.base_unit, a.disc_uprice as m_contprice,
                                  ( SELECT AVG_PRICE FROM MI_WHCOST WHERE MMCODE = A.MMCODE AND DATA_YM=CUR_SETYM AND ROWNUM=1 ) AS AVG_PRICE,
                                  ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO=c.frwh ) AS INV_QTY,
                                  ( SELECT AVG_APLQTY FROM V_MM_AVGAPL WHERE MMCODE = A.MMCODE AND WH_NO=c.towh ) AS AVG_APLQTY ,
                                  NVL(( SELECT HIGH_QTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO=c.towh ),0) AS HIGH_QTY,
                                  NVL(E.TOT_APVQTY,0) TOT_APVQTY ,
                                  NVL( ( SELECT MIN_ORDQTY FROM MI_WINVCTL WHERE MMCODE = A.MMCODE AND WH_NO ='560000' AND ROWNUM=1 ) ,1) AS TOT_DISTUN,
                                  {1}PFILE_ID,
                                  (case
                                       when ((select 1 from dual where not exists (select 1 from MI_WHMM where mmcode = a.mmcode)
                                                union
                                             select 1 from MI_WHMM where wh_no = c.towh and mmcode = a.mmcode
                                            )) = '1' 
                                      then 'Y' else 'N'
                                   end) as whmm_valid
                             from {0}MI_MAST a
                            INNER JOIN ME_DOCM C ON C.DOCNO=:docno
                       LEFT OUTER JOIN MI_WINVCTL D ON D.MMCODE = A.MMCODE AND D.WH_NO IN (SELECT WH_NO FROM MI_WHMAST WHERE WH_KIND='1' AND WH_GRADE='1')
                       LEFT OUTER JOIN V_MM_TOTAPL2 E ON E.DATE_YM=SUBSTR(TWN_DATE(C.APPTIME),0,5) and E.MMCODE=A.MMCODE and E.towh = c.towh
                            where a.mat_class = :mat_class
                              and a.m_storeid = :m_storeid
                              and nvl(a.cancel_id, 'N') = 'N'
                              and not exists (select 1 from ME_DOCD where docno = :docno and mmcode = a.mmcode)
                              and a.m_contid <> '3'
                              and a.m_applyid <> 'E'"
                            , m_storeid == "0" ? "V_" : string.Empty
                            , m_storeid == "1" ? "A." : "(select PFILE_ID from MI_MAST where mmcode = a.mmcode) as ");
            if (mmcode != string.Empty)
            {
                sql += "      and a.mmcode = :mmcode";
            }
            if (mmname_c != string.Empty)
            {
                sql += "      and a.mmname_c like :mmname_c";
            }
            if (mmname_e != string.Empty)
            {
                sql += "      and a.mmname_e = :mmname_e";
            }
            

            p.Add(":docno", docno);
            p.Add(":mat_class", mat_class);
            p.Add(":m_storeid", m_storeid);
            p.Add(":mmcode", mmcode);
            p.Add(":mmname_c", string.Format("%{0}%", mmname_c));
            p.Add(":mmname_e", string.Format("%{0}%", mmname_e));

            return DBWork.PagingQuery<MI_MAST>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetReasonCombo() {
            string sql = @"select data_value as value, data_value ||' '|| data_desc as text 
                             from PARAM_D
                            where grp_code = 'ME_DOCD'
                              and data_name = 'GTAPL_REASON'
                            order by data_value";
            return DBWork.PagingQuery<COMBO_MODEL>(sql, DBWork.Transaction);
        }

        public bool CheckMeDocdExists(string docno)
        {
            string sql = @"SELECT 1 FROM ME_DOCD WHERE DOCNO=:DOCNO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { DOCNO = docno }, DBWork.Transaction) == null);
        }

        public string GetMaxSeq(string docno)
        {
            var sql = @"SELECT MAX(SEQ) + 1 AS MAXSEQ FROM ME_DOCD WHERE DOCNO=:DOCNO";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { DOCNO = docno }, DBWork.Transaction);
        }

        public int InsertDetail(ME_DOCD item) {
            string sql = @"insert into ME_DOCD (docno, seq, mmcode, appqty, gtapl_reson, aplyitem_note,
                                                create_time, create_user, update_time, update_user, update_ip)
                           values (:docno, :seq, :mmcode, :appqty, :gtapl_reson, :aplyitem_note,
                                   sysdate, :create_user, sysdate, :update_user, :update_ip)";
            return DBWork.Connection.Execute(sql, item, DBWork.Transaction);
        }

        #region 20201-05-06新增: 刪除時檢查flowId
        public bool ChceckFlowId01(string docno)
        {
            string sql = @"
                select 1 from ME_DOCM
                 where docno = :docno
                   and (substr(flowId, length(flowId)-1 , 2) = '01'
                       or substr(flowId, length(flowId)-1 , 2) = '00'
                       or substr(flowId, length(flowId)-1 , 2) = '1')
                       
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { docno }, DBWork.Transaction) != null;
        }
        #endregion
    }
}