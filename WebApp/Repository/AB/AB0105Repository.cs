using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.AB
{
    public class AB0105Repository : JCLib.Mvc.BaseRepository
    {
        public AB0105Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AB0105> GetAll(string data_ym_s, string data_ym_e, string towh, string mmcode) {
            var p = new DynamicParameters();

            string sql = @"
                    select b.MMCODE,
                           (select WH_NO||' '||WH_NAME from MI_WHMAST where WH_NO=a.TOWH) as towh,
                           a.SET_YM,
                           b.APPQTY,b.APVQTY,b.APVTIME,
                           b.ACKQTY,b.ACKTIME,b.UPDATE_IP,a.POST_TIME,
                           a.DOCNO,
                           (select DATA_VALUE||' '||DATA_DESC from PARAM_D
                             where GRP_CODE='ME_DOCM' and DATA_NAME='DOCTYPE' 
                               and DATA_VALUE=a.DOCTYPE) as doctype,
                           (select INID||' '||INID_NAME from UR_INID where INID=a.APPDEPT)as appdept,
                           a.APPTIME,
                           (select WH_NO||' '||WH_NAME from MI_WHMAST where WH_NO=a.FRWH) as frwh,
                           (select mat_class||' '||mat_clsname from MI_MATCLASS where mat_class = a.mat_class) as mat_class
                    from 
                      (select DOCNO,DOCTYPE,APPDEPT,APPTIME,
                              FRWH,TOWH,MAT_CLASS,POST_TIME,SET_YM
                         from ME_DOCM
                        where 1=1 
                          and UPDATE_IP='強迫點收'
                      ) a
                    inner join
                      (select DOCNO,MMCODE,APPQTY,APVQTY,APVTIME,
                              ACKQTY,ACKTIME,UPDATE_IP
                         from ME_DOCD
                        where UPDATE_USER='強迫點收'
                      ) b
                    on a.DOCNO=b.DOCNO
                    where 1=1
            ";
            if (mmcode != string.Empty) {
                sql += "  and mmcode = :mmcode";
            }
            if (data_ym_s != string.Empty)
            {
                sql += "  and set_ym >=:data_ym_s";
            }
            if (data_ym_e != string.Empty)
            {
                sql += "  and set_ym <=:data_ym_e";
            }
            if (towh != string.Empty)
            {
                sql += "  and towh  = :towh";
            }

            p.Add("mmcode", mmcode);
            p.Add("data_ym_s", data_ym_s);
            p.Add("data_ym_e", data_ym_e);
            p.Add("towh", towh);

            return DBWork.PagingQuery<AB0105>(sql, p, DBWork.Transaction);
        }


        #region combo

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, a.mmname_c, a.mmname_e
                        FROM MI_MAST a
                        WHERE 1=1 ";

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(A.MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(A.MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

                sql += " OR A.MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR A.MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetTowhCombo()
        {
            string sql = @"select WH_NO as value, WH_NAME as text
                             from MI_WHMAST a
                            where exists
                                   (select 1 from ME_DOCM 
                                     where UPDATE_IP='強迫點收' and TOWH=a.WH_NO) 
                            order by WH_NO

                           ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql,  DBWork.Transaction);
        }

        #endregion
    }
}