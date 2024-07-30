using Dapper;
using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.AB
{
    public class AB0100Repository : JCLib.Mvc.BaseRepository
    {
        public AB0100Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }


        public string GetTowh() {
            string sql = @"select INID from UR_INID where INID_NAME='臨床病理科'";

            return DBWork.Connection.QueryFirst<string>(sql, DBWork.Transaction);
        }

        #region transfer
        public int UpdateDocnoProcess() {
            string sql = @"update LIS_APP
                              set docno = 'process'
                            where rdtime is null";

            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }

        public int UpdateMmcodeNotExists() {
            string sql = @"update LIS_APP a
                              set a.rej_note = '院內碼'||a.MMCODE||'未於藥衛材基本檔建擋',
                                  a.rdtime = sysdate
                            where a.rdtime is null
                              and a.docno = 'process'
                              and not exists (select 1 from MI_MAST where mmcode = a.mmcode and mat_class = '02')";


            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }

        public int UpdateNotMR4Condition()
        {
            string sql = @"update LIS_APP a
                              set a.rej_note = '院內碼'||a.MMCODE||
                                               '不符合衛材庫備申請條件(物料分類代碼=02,庫備識別碼=1,'||
                                               '合約識別碼<>3,申請申購識別碼<>E)',
                                  a.rdtime = sysdate
                            where a.rdtime is null
                              and a.docno = 'process'
                              and not exists (select 1 from MI_MAST
                                               where mat_class = '02' 
                                                 and m_storeid = '1'
                                                 and m_contid <> '3'
                                                 and m_applyid <> 'E'
                                                 and mmcode = a.mmcode)";


            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }

        public int UpdateBaseunitNotMatch()
        {
            string sql = @"update LIS_APP a
                              set (REJ_NOTE,RDTIME)=
                                     (select '院內碼'||a.mmcode||'計量單位('||a.base_unit||')'||
                                                '與藥衛材基本檔('||b.base_unit||')不同',
                                              sysdate 
                                        from (select mmcode,base_unit 
                                                from MI_MAST 
                                               where mat_class='02' and m_storeid='1' 
                                                 and m_contid<>'3' and m_applyid<>'E'
                                                 and mmcode=a.mmcode
                                              ) b 
                                        where a.mmcode=b.mmcode 
                                          and a.base_unit <> b.base_unit
                                      )

                            where a.rdtime is null
                              and a.docno = 'process'";


            return DBWork.Connection.Execute(sql, DBWork.Transaction);
        }

        public int CheckApplyKindNum(string mat_class, string towh)
        {
            string sql = @"select count(*) from ME_DOCM 
                            where doctype='MR2' 
                              and apply_kind='1' 
                              and apptime between next_day(sysdate-7,1) and next_day(sysdate-7,1)+6  
                              and mat_class = :mat_class 
                              and towh = :towh ";
            int rtn = Convert.ToInt32(DBWork.Connection.ExecuteScalar(sql, new { mat_class = mat_class, towh = towh }, DBWork.Transaction).ToString());
            return rtn;
        }

        public int CheckApplyKindDaily(string mat_class, string towh)
        {
            string sql = @"select count(*) from ME_DOCM 
                            where doctype='MR2' 
                              and apply_kind='1' 
                              and twn_date(apptime) = twn_date(sysdate)
                              and mat_class = :mat_class 
                              and towh = :towh";
            int rtn = Convert.ToInt32(DBWork.Connection.ExecuteScalar(sql, new { mat_class = mat_class, towh = towh }, DBWork.Transaction).ToString());
            return rtn;
        }

        public int InsertMedocm(string docno, string apply_kind, string appid, string towh) {
            DynamicParameters p = new DynamicParameters();

            string sql = @"insert into ME_DOCM (docno, doctype, flowid, appid,
                                                appdept, apptime, useid, usedept,
                                                frwh, towh, list_id, apply_kind, mat_class,
                                                create_time, create_user)
                           values (
                                :docno, 'MR2', '2', :appid,
                                :towh, sysdate, :appid, :towh ,
                                (select wh_no from MI_WHMAST where wh_kind = '1' and wh_grade = '1' and cancel_id = 'N'),
                                :towh, 'N', :apply_kind , '02',
                                sysdate , :appid
                            )";
            p.Add(":docno", docno);
            p.Add(":apply_kind", apply_kind);
            p.Add(":appid", appid);
            p.Add(":towh", towh);

            return DBWork.Connection.Execute(sql, new{ docno = docno, apply_kind = apply_kind, appid = appid, towh = towh}
            ,  DBWork.Transaction);
        }

        public int InsertMedocd(string docno, string appid)
        {
            DynamicParameters p = new DynamicParameters();

            string sql = @"insert into ME_DOCD (docno, seq, mmcode, appqty, aplyitem_note,
                                                create_time, create_user, apl_contime, expt_distqty)
                           select :docno as docno,
                                  rownum as seq,
                                  mmcode as mmcode,
                                  appqty as appqty,
                                  apply_note as aplyitem_note,
                                  sysdate as create_time,
                                  :appid as create_user,
                                  sysdate as apl_contime,
                                  appqty as expt_distqty
                             from LIS_APP a
                            where a.docno = 'process'
                              and exists (select 1 from MI_MAST
                                           where mat_class = '02'
                                             and m_storeid = '1'
                                             and m_contid <> '3'
                                             and m_applyid <> 'E'
                                             and mmcode = a.mmcode
                                             and base_unit = a.base_unit
                                          )
        
                            ";
            p.Add(":docno", docno);
            p.Add(":appid", appid);

            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public int UpdateLisapp(string docno) {
            string sql = @"update LIS_APP
                              set docno = :docno, rdtime = sysdate
                            where docno = 'process'
                              and rej_note is null";
            return DBWork.Connection.Execute(sql, new { docno = docno }, DBWork.Transaction);
        }

        #endregion

        #region all
        public IEnumerable<LIS_APP> All(string apptime_s, string apptime_e) {
            string sql = @"select purchno, mmcode, appqty, base_unit, appusr,
                                  twn_time_format(apptime) as apptime,
                                  apply_note,
                                  twn_time_format(instime) as instime,  
                                  twn_time_format(rdtime) as rdtime,  
                                  docno, rej_note  
                             from LIS_APP
                            where twn_date(apptime) between :apptime_s and :apptime_e
                                  ";
            return DBWork.PagingQuery<LIS_APP>(sql, 
                new { apptime_s = apptime_s,
                      apptime_e = apptime_e}, DBWork.Transaction);
        }
        #endregion
    }
}