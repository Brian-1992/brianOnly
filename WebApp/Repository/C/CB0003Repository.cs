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
    public class CB0003Repository : JCLib.Mvc.BaseRepository
    {
        public CB0003Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<CB0003> GetAll( string mat_class, string mmcode, string mmname_c , string mmname_e, string status, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"select  (select mat_class || ' ' || mat_clsname from mi_matclass where mat_class=A2.mat_class) as mat_clsname,
                                    A1.* ,
                                    A2.mmname_c,
                                    A2.mmname_e
                             from (select a.mmcode,   a.barcode, b.xcategory, b.descript,a.status, 
                                          (case when a.status ='Y' then 'Y 使用中'
                                                when a.status ='N' then 'N 停用'
                                                else '' end) as status_name 
                                     from BC_BARCODE a 
                                     left join BC_CATEGORY b on a.xcategory = b.xcategory ) A1, 
                                  mi_mast A2
                            where A1.mmcode=A2.mmcode";

            if (mat_class != "") {
                sql += " and A2.mat_class = :p0 ";
                p.Add(":p0", string.Format("{0}", mat_class));
            }
            if (mmcode != "")
            {
                sql += " and A2.mmcode like :p1 ";
                p.Add(":p1", string.Format("%{0}%", mmcode));
            }
            if (mmname_c != "")
            {
                sql += " and A2.mmname_c like :p2 ";
                p.Add(":p2", string.Format("%{0}%", mmname_c));
            }
            if (mmname_e != "")
            {
                sql += " and A2.mmname_e like :p3 ";
                p.Add(":p3", string.Format("%{0}%", mmname_e));
            }
            if (status != "")
            {
                sql += "  and A1.status = :p4 ";
                p.Add(":p4", string.Format("{0}", status));
            }


            sql += " order by A2.mmcode ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CB0003>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetMatclassCombo()
        {
            string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLASS || ' ' ||MAT_CLSNAME AS TEXT
                             FROM MI_MATCLASS
                            ORDER BY MAT_CLASS";
            return DBWork.Connection.Query<ComboItemModel>(sql, DBWork.Transaction);
        }

        public DataTable GetExcel(string mat_class, string mmcode, string mmname_c, string mmname_e, string status)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"select  (select mat_class || ' ' || mat_clsname from mi_matclass where mat_class=A2.mat_class) as 物料分類,
                                    A1.mmcode AS 院內碼,
                                    A2.MMNAME_C AS 中文品名,
                                    A2.MMNAME_E AS 英文品名,
                                    A1.barcode AS 國際條碼,
                                    A1.xcategory AS 條碼類別代碼,
                                    A1.descript AS 條碼類別敘述,
                                    A1.status_name AS 狀態碼
                             from (select a.mmcode ,   
                                          a.barcode,
                                          b.xcategory,
                                          b.descript, 
                                          a.status,
                                          (case when a.status ='Y' then 'Y 使用中'
                                                when a.status ='N' then 'N 停用'
                                                else '' end) as status_name  
                                     from BC_BARCODE a 
                                     left join BC_CATEGORY b on a.xcategory = b.xcategory ) A1, 
                                  mi_mast A2
                            where A1.mmcode=A2.mmcode";

            if (mat_class != "")
            {
                sql += " and A2.mat_class = :p0 ";
                p.Add(":p0", string.Format("{0}", mat_class));
            }
            if (mmcode != "")
            {
                sql += " and A2.mmcode like :p1 ";
                p.Add(":p1", string.Format("%{0}%", mmcode));
            }
            if (mmname_c != "")
            {
                sql += " and A2.mmname_c like :p2 ";
                p.Add(":p2", string.Format("%{0}%", mmname_c));
            }
            if (mmname_e != "")
            {
                sql += " and A2.mmname_e like :p3 ";
                p.Add(":p3", string.Format("%{0}%", mmname_e));
            }
            if (status != "")
            {
                sql += "  and A1.status = :p4 ";
                p.Add(":p4", string.Format("{0}", status));
            }


            sql += " order by A2.mmcode ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
    }
}