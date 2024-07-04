using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Models;
using WebApp.Models.MI;

namespace AA0112
{
    public class PhrWhinvCheckRepository : JCLib.Mvc.BaseRepository
    {
        public PhrWhinvCheckRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public string GetMnSetDate()
        {
            string sql = @"select to_char(set_ctime, 'YYYY-MM-DD') from MI_MNSET 
                            where set_status = 'N'";
            return DBWork.Connection.QueryFirst<string>(sql, DBWork.Transaction);
        }

        public MI_MNSET GetMnset() {
            string sql = @"select * from MI_MNSET 
                            where set_status = 'N'";
            return DBWork.Connection.QueryFirstOrDefault<MI_MNSET>(sql, DBWork.Transaction);
        }

        public IEnumerable<CHK_MAST> GetPhrChkMasts(string chk_ym) {
            string sql = @"
                select * from CHK_MAST
                 where chk_ym = :chk_ym
                   and chk_wh_kind = '0'
                   and chk_wh_grade = '2'
                   and chk_level = '1'
            ";
            return DBWork.Connection.Query<CHK_MAST>(sql, new { chk_ym }, DBWork.Transaction);
        }

        public IEnumerable<string> GetNotExists(string chk_no)
        {
            string sql = @"
                select a.mmcode
                  from CHK_DETAILTOT a
                 where a.chk_no = :chk_no
                   and not exists (select 1 from MI_WHINV 
                                    where wh_no = a.wh_no 
                                      and mmcode = a.mmcode)
            ";
            return DBWork.Connection.Query<string>(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }

        public int InsertWhinv(string chk_no) {
            string sql = @"
                insert into MI_WHINV(wh_no, mmcode)
                select a.wh_no, a.mmcode
                  from CHK_DETAILTOT a
                 where a.chk_no = :chk_no
                   and not exists (select 1 from MI_WHINV 
                                    where wh_no = a.wh_no 
                                      and mmcode = a.mmcode)
            ";
            return DBWork.Connection.Execute(sql, new { chk_no = chk_no }, DBWork.Transaction);
        }
    }
}
