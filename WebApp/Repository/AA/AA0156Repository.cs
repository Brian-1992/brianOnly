using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.AA
{
    public class AA0156Repository : JCLib.Mvc.BaseRepository
    {
        public AA0156Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ComboItemModel> GetMclassCombo(string tuser)
        {
            var sql = @"with temp_whkinds as (
                        select b.wh_no, b.wh_kind, nvl((case when a.task_id = '3' then '2' else a.task_id end), '2') as task_id
                        from MI_WHID a, MI_WHMAST b
                        where wh_userid = :TUSER
                        and a.wh_no = b.wh_no
                        and b.wh_grade = '1'
                        and a.task_id in ('1','2','3')
                    )
                    select distinct b.mat_class as VALUE, b.mat_clsname as TEXT, b.mat_class || ' ' ||  b.mat_clsname as COMBITEM, a.wh_kind as EXTRA1  
                    from temp_whkinds a, MI_MATCLASS b
                    where (a.task_id = b.mat_clsid)
                    ";

            return DBWork.Connection.Query<ComboItemModel>(sql, new { TUSER = tuser }, DBWork.Transaction);
        }

        public IEnumerable<AA0156> Print(string mat_class, string startDate, string endDate, string stat)
        {

            var p = new DynamicParameters();

            string sql = string.Format(@" SELECT 
                                     b.APPQTY, b.UP, b.AMT, b.MMCODE, c.MMNAME_C, c.MMNAME_E, c.BASE_UNIT
                            FROM ME_DOCM a, ME_DOCD b, MI_MAST c
                            WHERE a.DOCTYPE in ('XR2','XR3')
                                AND a.FLOWID in ('1899', '1999')
                                and a.MAT_CLASS = :MAT_CLASS
                                AND TWN_DATE(APPTIME) BETWEEN :SD and :ED
                                and B.STAT = :STAT
                                AND b.DOCNO = a.DOCNO
                                AND c.MMCODE = b.MMCODE
                            order by b.mmcode
                            ");

            p.Add("MAT_CLASS", mat_class);
            p.Add("SD", startDate);
            p.Add("ED", endDate);
            p.Add("STAT", stat);

            return DBWork.Connection.Query<AA0156>(sql, p, DBWork.Transaction);
        }

        public string GetHospName()
        {
            string sql = @" select data_value from PARAM_D 
                where grp_code = 'HOSP_INFO' and data_name = 'HospName' and rownum=1 ";
            return DBWork.Connection.ExecuteScalar(sql, null, DBWork.Transaction).ToString();
        }

        public string GetMatClsName(string mat_class)
        {
            string sql = @" select mat_clsname from MI_MATCLASS where mat_class = :MAT_CLASS ";
            return DBWork.Connection.ExecuteScalar(sql, new { MAT_CLASS = mat_class }, DBWork.Transaction).ToString();
        }

        public string GetUserName(string id)
        {
            string sql = @"SELECT UNA FROM UR_ID WHERE UR_ID.TUSER=:TUSER";
            return DBWork.Connection.ExecuteScalar(sql, new { TUSER = id }, DBWork.Transaction).ToString();
        }
    }
}