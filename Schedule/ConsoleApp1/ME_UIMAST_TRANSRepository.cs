using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Models;

namespace ConsoleApp1
{
    public class ME_UIMAST_TRANSRepository : JCLib.Mvc.BaseRepository
    {
        public ME_UIMAST_TRANSRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ME_UIMAST> GetUimasts2() {
            string sql = @"
                select * from ME_UIMAST a
                 where exists (select 1 from MI_WHMAST where wh_no = a.wh_no and wh_grade = '2' and wh_kind = '0')
                   and exists (select 1 from MI_MAST where mmcode = a.mmcode and mat_class = '01')
                 order by wh_no, mmcode
            ";
            return DBWork.Connection.Query<ME_UIMAST>(sql, DBWork.Transaction);
        }
        public ME_UIMAST GetPh1sUimast(string mmcode) {
            string sql = @"
                select * from ME_UIMAST
                 where wh_no = 'PH1S'
                   and mmcode = :mmcode
            ";
            return DBWork.Connection.QueryFirstOrDefault<ME_UIMAST>(sql, new { mmcode }, DBWork.Transaction);
        }

        public int UpdateMeUimast(string wh_no, string mmcode, string pack_unit, string pack_times) {
            string sql = @"
                update ME_UIMAST
                   set pack_unit = :pack_unit,
                       pack_times = :pack_times
                 where wh_no = :wh_no
                   and mmcode = :mmcode
            ";
            return DBWork.Connection.Execute(sql, new { wh_no, mmcode, pack_unit, pack_times }, DBWork.Transaction);
        }
    }
}
