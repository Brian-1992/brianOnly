using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JCLib.DB;
using WebApp.Models;
using Dapper;
using System.Data;


namespace WebApp.Repository.C
{
    public class CB0007Repository : JCLib.Mvc.BaseRepository
    {
        public CB0007Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<BC_ITMANAGER> GetAll(string wh_no, string mmcode, string mmname_c, string mmname_e, string status, string mat_class, string managerid, int page_index, int page_size, string sorters, string wh_userId)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT (select mat_clsname from mi_matclass where mat_class=A1.mat_class) mat_clsname,
                                       A1.*,
                                       NVL(d.una, '--') user_name,
                                       NVL('[' || A1.MANAGERID || ']' || A1.MANAGERNM || ' => ' || NVL(d.una, '') || '', '') as OPT 
                          from (SELECT A2.*
                                 FROM  (SELECT a.wh_no, a.mmcode, 
                                               (select mat_class from MI_MAST where mmcode=a.mmcode) as mat_class,
                                               (select mmname_c from MI_MAST where mmcode=a.mmcode) as mmname_c,
                                               (select mmname_e from MI_MAST where mmcode=a.mmcode) as mmname_e, 
                                               b.MANAGERID
                                          FROM MI_WHINV a 
                                          LEFT  OUTER JOIN BC_ITMANAGER b ON a.mmcode = b.mmcode and a.wh_no=b.wh_no )   A2
                                        ) A1 
                          LEFT OUTER JOIN UR_ID d ON A1.MANAGERID = d.TUSER
                         Where 1=1 ";


           // p.Add(":p0", string.Format("{0}", wh_no));

            if (mmcode != "")
            {
                sql += " and A1.MMCODE like :p1 ";
                p.Add(":p1", string.Format("%{0}%", mmcode));
            }
            if (mmname_c != "")
            {
                sql += " and A1.mmname_c  like :p2 ";
                p.Add(":p2", string.Format("%{0}%", mmname_c));
            }
            if (mmname_e != "")
            {
                sql += " and A1.mmname_e  like :p3 ";
                p.Add(":p3", string.Format("%{0}%", mmname_e));
            }
            if (status == "Y")
            {
                sql += " and MANAGERID is not null ";
            }
            if (status == "N")
            {
                sql += " and MANAGERID is null ";
            }
            if (managerid != "")
            {
                sql += " and A1.managerid  = :p4 ";
                p.Add(":p4", string.Format("{0}", managerid));
            }
            if (mat_class != "")
            {
                sql += " and A1.mat_class = :p5 ";
                p.Add(":p5", string.Format("{0}", mat_class));
            }

            sql += "order by A1.wh_no,mat_clsname,A1.mmcode";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BC_ITMANAGER>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
    }
}