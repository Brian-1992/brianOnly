using Dapper;
using JCLib.DB;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.AA
{
    public class AA0160Repository : JCLib.Mvc.BaseRepository
    {
        public AA0160Repository(IUnitOfWork unitOfWork) : base(unitOfWork) {}
        
        public IEnumerable<AA0160> GetAll(string data_ym, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select :DATA_YM as DATA_YM, T.INID, T.INID_NAME,
                listagg(nvl(T.SEC_DISRATIO, '0'), ',')
                within group (order by SECTIONNO) as EXTRA_DATA
                from (
                    select A.INID, A.INID_NAME, A.SECTIONNO, B.SEC_DISRATIO 
                    from (
                        select A1.INID, A1.INID_NAME, A2.SECTIONNO
                        from UR_INID A1, SEC_MAST A2 where A2.SEC_ENABLE = 'Y' order by A1.INID, A2.SECTIONNO
                    ) A
                    left join (
                        select B1.DATA_YM, B1.INID, B1.SECTIONNO, B1.SEC_DISRATIO 
                        from SEC_CALLOC B1, SEC_MAST B2 where B1.SECTIONNO = B2.SECTIONNO and B2.SEC_ENABLE = 'Y' 
                        and B1.DATA_YM = :DATA_YM
                    ) B 
                    on A.INID = B.INID and A.SECTIONNO = B.SECTIONNO
                    order by INID, SECTIONNO
                ) T
                group by T.INID, T.INID_NAME ";

            p.Add(":DATA_YM", string.Format("{0}", data_ym));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0160>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<ColumnItem> GetColumnItems()
        {
            var sql = @"SELECT SECTIONNO as DATAINDEX, SECTIONNAME as TEXT FROM SEC_MAST WHERE SEC_ENABLE = 'Y' order by SECTIONNO ";
            return DBWork.Connection.Query<ColumnItem>(sql, null, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetYmCombo()
        {
            var sql = @"
                    select SET_YM as VALUE from MI_MNSET where SET_STATUS = 'N'
                    union
                    select twn_pym(SET_YM) as VALUE from MI_MNSET where SET_STATUS = 'N'
                    union
                    select distinct DATA_YM as VALUE from SEC_CALLOC
                    order by VALUE desc
                    ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, null, DBWork.Transaction);
        }

        public string GetEditable(string data_ym)
        {
            string sql = @"select (case when :DATA_YM = (select SET_YM from MI_MNSET where SET_STATUS = 'N') then 'Y'
                           when :DATA_YM = (select twn_pym(SET_YM) as VALUE from MI_MNSET where SET_STATUS = 'N') then 'Y'
                            else 'N' end) as VAL from dual";
            return DBWork.Connection.ExecuteScalar(sql, new { DATA_YM = data_ym }, DBWork.Transaction).ToString();
        }

        public int MergeDisratio(string data_ym, string inid, string sectionno, string sec_disratio, string update_user, string update_ip)
        {
            var sql = @"merge into SEC_CALLOC T
                    using ( 
                        select :DATA_YM as DATA_YM, :INID as INID, :SECTIONNO as SECTIONNO, :SEC_DISRATIO as SEC_DISRATIO from dual
                    ) S 
                    on (T.DATA_YM = S.DATA_YM and T.INID = S.INID and T.SECTIONNO = S.SECTIONNO)
                    when matched then
                         update set SEC_DISRATIO = S.SEC_DISRATIO, UPDATE_TIME = sysdate, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                    when not matched then
                         insert (DATA_YM, INID, SECTIONNO, SEC_DISRATIO, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP) values (:DATA_YM, :INID, :SECTIONNO, :SEC_DISRATIO, SYSDATE, :UPDATE_USER, SYSDATE, :UPDATE_USER, :UPDATE_IP)";
            return DBWork.Connection.Execute(sql, new { DATA_YM = data_ym, INID = inid, SECTIONNO = sectionno, SEC_DISRATIO = sec_disratio,
                UPDATE_USER = update_user, UPDATE_IP = update_ip}, DBWork.Transaction);
        }
    }
}