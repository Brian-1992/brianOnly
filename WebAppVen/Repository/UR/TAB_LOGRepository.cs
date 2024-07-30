using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using JCLib.DB;
using Dapper;
using WebAppVen.Models;

namespace WebAppVen.Repository.UR
{
    public class TAB_LOGRepository : JCLib.Mvc.BaseRepository
    {
        public TAB_LOGRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<TAB_LOG> Query(int page_index, int page_size, string sorters, string table_key, string table_key_value, string field_name)
        {
            DynamicParameters p = new DynamicParameters();

            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT replace(CONVERT(varchar, proc_dt, 120 ),'-','/') proc_dt,isnull((SELECT UNA FROM UR_ID WHERE TUSER = proc_user),proc_user) proc_user");
            sql.Append(",c.name FIELD_NAME, new_value,OLD_VALUE,a.PROC_IP");
            sql.Append("  FROM tab_log a ,TAB_LOG_REF b ,UR_PARAM_D c");
            sql.Append(" Where a.OBJECTCLASS=b.OBJECTCLASS and a.TABLE_NAME=b.TABLE_NAME and c.M_ID=a.TABLE_NAME");
            sql.Append(" and a.field_name=c.id and c.VALUE='Y'");
            sql.Append(" and a.TABLE_KEY=@p0 and TABLE_KEY_VALUE=@p1");

            p.Add("@p0", string.Format("{0}", table_key));
            p.Add("@p1", string.Format("{0}", table_key_value));
            if (field_name != "" & field_name !=null)
            {
                //sql.Append(" and a.FIELD_NAME in (@p2)");
                //p.Add("@p2", string.Format("{0}", field_name));
                string param = "";
                string[] field_nameList = field_name.Split(',');
                for (int i = 0; i < field_nameList.Length; i++)
                {
                    if (i >= 1) param += ",";
                    param += "@p2_" + i;

                    p.Add("@p2_" + i, field_nameList[i]);
                }

                sql.Append(" AND a.FIELD_NAME in (" + param + ")");
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<TAB_LOG>(GetPagingStatement(sql.ToString(), sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ComboModel> GetTabLogRefCombo()
        {
            string sql = "select distinct TABLE_KEY KEY_CODE ,table_key_name COMBITEM,REMARK VALUE from TAB_LOG_REF order by 1";

            return DBWork.Connection.Query<ComboModel>(sql);
        }

        public IEnumerable<ComboModel> GetFieldNameCombo(string p0)
        {
            DynamicParameters p = new DynamicParameters();
            string sql = "select ID KEY_CODE,NAME COMBITEM from UR_PARAM_D,TAB_LOG_REF where M_ID=table_name and TABLE_KEY=@p0 and value='Y' order by 2";
            p.Add("@p0", p0);
            return DBWork.Connection.Query<ComboModel>(sql,p);
        }
    }
}