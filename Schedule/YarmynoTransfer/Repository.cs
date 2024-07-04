using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace YarmynoTransfer
{
    class Repository : JCLib.Mvc.BaseRepository
    {
        public Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public int deleteTemp() {
            string sql = @"
                delete from TEMP_YARMYNO_LOG
            ";
            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }

        public int InsertTemp() {
            string sql = @"
                insert into TEMP_YARMYNO_LOG(mmcode, YRARMYNO_N, ITEMARMYNO_N)
                select distinct mmcode, E_YRARMYNO, e_ITEMARMYNO
                  from mi_mast_log_ming 
                 where twn_date(logtime) = '1111230'
                   and E_YRARMYNO = '112~113年'
                 group by mmcode, E_YRARMYNO, e_ITEMARMYNO
            ";
            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }
        public int InsertTemp_1() {
            string sql = @"
                insert into TEMP_YARMYNO_LOG(mmcode)
                select distinct mmcode from mi_mast_log_ming a
where exists (select 1 from MI_MAST_LOG_MING where twn_time(logtime) like '111123012%' and e_YRARMYNO = '110~111年' and mmcode = a.mmcode)
and not exists (select 1 from MI_MAST_LOG_MING where twn_time(logtime) like '111123018%' and e_YRARMYNO = '112~113年' and mmcode = a.mmcode)
            ";
            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }

        public IEnumerable<TEMP_LOG> GetTemps() {
            string sql = @"
                select * from TEMP_YARMYNO_LOG
            ";
            return DBWork.Connection.Query<TEMP_LOG>(sql, DBWork.Transaction);
        }

        public MI_MAST_LOG GetMiMastLog(string mmcode) {
            string sql = @"
                select mmcode, E_YRARMYNO, e_ITEMARMYNO from MI_MAST_LOG_MING
                 where mmcode = :mmcode
                   and twn_date(logtime) = '1111230'
                   and E_YRARMYNO <> '112~113年' 
                   and E_YRARMYNO is not null
                 order by logtime desc
            ";
            return DBWork.Connection.QueryFirstOrDefault<MI_MAST_LOG>(sql, new { mmcode }, DBWork.Transaction);
        }
        public MILMED_JBID_LIST GetJbid(string yrarmyno_o, string itemarmyno_o) {
            string sql = @"
                select * from MILMED_JBID_LIST
                 where JBID_STYR = substr(:yrarmyno_o,1,3)
                   and BID_NO = :itemarmyno_o
            ";
            return DBWork.Connection.QueryFirstOrDefault<MILMED_JBID_LIST>(sql, new { yrarmyno_o, itemarmyno_o }, DBWork.Transaction);
        }

        public int UpdateTemp(string mmcode, string yrarmyno_o, string itemarmyno_o, 
                              string iswilling, string discount_qty, string disc_cost_uprice) {
            string sql = @"
                update TEMP_YARMYNO_LOG
                   set yrarmyno_o = :yrarmyno_o, itemarmyno_o = :itemarmyno_o,
                       iswilling = :iswilling, discount_qty = :discount_qty, disc_cost_uprice = :disc_cost_uprice
                 where mmcode = :mmcode
            ";
            return DBWork.Connection.Execute(sql, new { mmcode, yrarmyno_o, itemarmyno_o , iswilling , discount_qty , disc_cost_uprice }, DBWork.Transaction);
        }

        public int UpdateInrec() {

            string sql = @"
                update MM_PO_INREC a
                   set iswilling = (select iswilling from TEMP_YARMYNO_LOG where mmcode = a.mmcode),
                       discount_qty = (select discount_qty from TEMP_YARMYNO_LOG where mmcode = a.mmcode),
                       disc_cost_uprice = (select disc_cost_uprice from TEMP_YARMYNO_LOG where mmcode = a.mmcode)
                 where TWN_YYYMM(accountdate) = '11112'
                   and exists (select 1 from TEMP_YARMYNO_LOG where mmcode = a.mmcode)
            ";
            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }

        public int backupInrec() {
            string sql = @"
                insert into TEMP_MM_PO_INREC_20230103
                select * from MM_PO_INREC
                 where TWN_YYYMM(accountdate) = '11112'
                   and exists (select 1 from TEMP_YARMYNO_LOG where mmcode = a.mmcode)
            ";
            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }
    }
}
