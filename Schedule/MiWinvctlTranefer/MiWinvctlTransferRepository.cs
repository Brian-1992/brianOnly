using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiWinvctlTranefer
{
    public class MiWinvctlTransferRepository : JCLib.Mvc.BaseRepository
    {
        public MiWinvctlTransferRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public int InsertWinvctl02() {
            string sql = @"
                insert into mi_winvctl
                         (wh_no,mmcode,safe_day,oper_day,ship_day,high_qty,min_ordqty)
                select a.wh_no, a.mmcode,
                       '15' as safe_day,
                       '15' as oper_day,
                       '0' as ship_day,
                       '0' as high_qty,
                       '1' as min_ordqty
                  from MI_WHINV a, MI_MAST b, MI_WHMAST c
                 where a.mmcode = b.mmcode and b.mat_class = '02'
                   and a.wh_no = c.wh_no and c.wh_kind = '1' and c.wh_grade = '2'
                   and not exists (select 1 from MI_WINVCTL where wh_no = a.wh_no and mmcode = a.mmcode) 
            ";
            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }

        public int InsertWinvctl01()
        {
            string sql = @"
                insert into mi_winvctl
                         (wh_no,mmcode,safe_day,oper_day,ship_day,high_qty,min_ordqty,
                          ctdmdccode, create_time, create_user)
                select a.wh_no, a.mmcode,
                       '0' as safe_day,
                       '0' as oper_day,
                       '0' as ship_day,
                       '0' as high_qty,
                       '1' as min_ordqty,
                       '0' as ctdmdccode,
                       sysdate as create_time,
                       '排程新增' as create_user
                  from MI_WHINV a, MI_MAST b, MI_WHMAST c
                 where a.mmcode = b.mmcode and b.mat_class = '01'
                   and a.wh_no = c.wh_no and c.wh_kind = '0' and c.wh_grade in ('2','3','4')
                   and not exists (select 1 from MI_WINVCTL where wh_no = a.wh_no and mmcode = a.mmcode) 
            ";
            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }
    }
}
