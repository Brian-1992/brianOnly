using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.AA
{
    public class AA0086Repository : JCLib.Mvc.BaseRepository
    {
        public AA0086Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0086M> GetAll(string startDate, string endDate, string printType, int page_index, int page_size, string sorters) {
            var p = new DynamicParameters();
            string sql =  string.Format(@" SELECT 
                                   b.APVQTY as APVQTY,
                                   b.APPQTY as APPQTY,
                                   b.UP as UP,
                                   b.AMT as AMT,
                                   c.E_ITEMARMYNO as E_ITEMARNYNO,
                                   c.MMNAME_E as MMNAME_C,
                                   c.E_COMPUNIT as E_COMPUNIT,
                                   c.E_MANUFACT as E_MANUFACT,
                                   c.BASE_UNIT as BASE_UNIT,
                                   '{0}' as PrintType
                              FROM ME_DOCM a, ME_DOCD b, MI_MAST c
                             WHERE a.DOCTYPE = 'XR'
                               AND a.{0} = 'PH1X'
                               AND a.FLOWID = '0399'
                               AND TRUNC(a.APPTIME, 'DD')BETWEEN TO_DATE(:p0,'YYYY-MM-DD') AND TO_DATE(:p1,'YYYY-MM-DD')
                               AND b.DOCNO = a.DOCNO
                               AND c.MMCODE = b.MMCODE
                            ORDER BY c.E_ITEMARNYNO"
                , printType);

            // b.UP as UP,
            // b.AMT as AMT,

            // AND a.{ 0} = 'PH1X'
            //  AND a.FLOWID = '0699'

            p.Add("p0", startDate);
            p.Add("p1", endDate);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0086M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA0086M> Print(string startDate, string endDate, string printType, string userId) {

            var p = new DynamicParameters();

            string sql = string.Format(@" SELECT 
                                   b.APVQTY as APVQTY,
                                   b.APPQTY as APPQTY,
                                   b.UP as UP,
                                   b.AMT as AMT,
                                   c.E_ITEMARMYNO as E_ITEMARMYNO,
                                   c.MMNAME_E as MMNAME_E,
                                   c.E_COMPUNIT as E_COMPUNIT,
                                   c.E_MANUFACT as E_MANUFACT,
                                   c.BASE_UNIT as BASE_UNIT,
                                   '{0}' as PrintType,
                                   d.UNA as PrintUser
                              FROM ME_DOCM a, ME_DOCD b, MI_MAST c, UR_ID d
                             WHERE a.DOCTYPE in ('XR','XR3')
                               AND a.{0} = WHNO_1X('0') 
                               AND a.FLOWID in ('0399', '1999')
                               AND TRUNC(a.APPTIME, 'DD')BETWEEN TO_DATE(:p0,'YYYY-MM-DD') AND TO_DATE(:p1,'YYYY-MM-DD')
                               AND b.DOCNO = a.DOCNO
                               AND c.MMCODE = b.MMCODE
                               AND d.TUSER = :userid
                             ORDER BY c.E_ITEMARMYNO"
                , printType);
            // b.UP as UP,
            // b.AMT as AMT,
            p.Add("p0", startDate);
            p.Add("p1", endDate);
            p.Add("userid", userId);

            return DBWork.Connection.Query<AA0086M>(sql, p, DBWork.Transaction);

            // a.{0} = 'PH1X' AND  AND a.FLOWID = '0699'
        }
    }
}