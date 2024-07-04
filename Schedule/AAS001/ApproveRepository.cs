using Dapper;
using JCLib.DB;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Models;

namespace AAS001
{
    class ApproveRepository : JCLib.Mvc.BaseRepository
    {
        public ApproveRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public string GetSysdate()
        {
            string sql = @"select twn_time(sysdate) from dual";

            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }

        #region 1. 取得撥發資料寫入LIS
        public IEnumerable<LIS_ACC> GetLisAccs(string towh) {
            string sql = @"select b.docno as docno, 
                                  nvl((select purchno from LIS_APP where docno = b.docno and mmcode = b.mmcode), 0) as purchno,
                                  b.seq,
                                  b.mmcode,
                                  b.apvqty,
                                  base_unit(b.mmcode) as  base_unit
                             from ME_DOCM a, ME_DOCD b
                            where 1=1
                              and a.towh = :towh
                              and a.flowid in ('3', '4')
                              and b.docno = a.docno
                              and not exists (select 1 from LIS_ACC where docno = a.docno)";
            return DBWork.Connection.Query<LIS_ACC>(sql, new { towh = towh}, DBWork.Transaction);
        }

        public IEnumerable<ME_DOCEXP> GetDocexps(string docno, string mmcode) {
            string sql = @"select * from ME_DOCEXP 
                            where docno = :docno and mmcode = :mmcode";
            return DBWork.Connection.Query<ME_DOCEXP>(sql, new { docno = docno, mmcode = mmcode }, DBWork.Transaction);
        }

        public int InsertLisAcc(LIS_ACC acc) {
            string sql = @"insert into LIS_ACC (docno, purchno, seq, mmcode, lot_no, exp_date, apvqty, 
                                                base_unit, instime)
                           values (:docno, :purchno, :seq, :mmcode, :lot_no, :exp_date, :apvqty, 
                                   :base_unit, sysdate)";
            return DBWork.Connection.Execute(sql, new {
                    docno = acc.DOCNO, purchno = acc.PURCHNO, seq = acc.SEQ, mmcode = acc.MMCODE,
                    lot_no = acc.LOT_NO, exp_date = acc.EXP_DATE, apvqty = acc.APVQTY, 
                    base_unit = acc.BASE_UNIT
            }, DBWork.Transaction);
        }
        #endregion

        // 呼叫stored procedure
        public string CallProc(string id, string upuser, string upip)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_DOCNO", value: id, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 21);
            p.Add("I_UPDUSR", value: upuser, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 8);
            p.Add("I_UPDIP", value: upip, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 1);
            p.Add("O_ERRMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 255);

            DBWork.Connection.Query("POST_DOC", p, commandType: CommandType.StoredProcedure);
            string retid = p.Get<OracleString>("O_RETID").Value;
            string errmsg = p.Get<OracleString>("O_ERRMSG").Value;
            if (retid == "N")
            {
                retid = errmsg;
            }
            return retid;
        }

        // 取得點收資料 isacc = Y : 相符  isacc = N：不相符
        public IEnumerable<LIS_ACC> GetLisAcc() {
            string sql = @"select *
                             from LIS_ACC a
                            where exists (select 1 from ME_DOCM where docno = a.docno and flowid in ('3', '4'))
                              and a.rdtime is not null";
            return DBWork.Connection.Query<LIS_ACC>(sql, DBWork.Transaction);
        }
        public int UpdateDocd(string docno) {
            var sql = @"update ME_DOCD 
                          SET apvqty = ackqty, 
                              ackid = 'LIS', 
                              acktime = sysdate,
                              update_time = sysdate, 
                              update_user = 'LIS', 
                              update_ip = 'LIS',
                              apvtime = sysdate, 
                              apvid = 'LIS'
                        where docno = :docno";
            return DBWork.Connection.Execute(sql, new { docno = docno}, DBWork.Transaction);
        }

        public string GetMedocmFlowid(string docno) {
            string sql = @"select flowid from ME_DOCM where docno = :docno";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { docno = docno }, DBWork.Transaction);
        }
        // 不符 
        public int DeleteMeDocExp(string docno) {
            string sql = @"delete from ME_DOCEXP a
                            where a.docno = :docno
                              and not exists (select 1 from LIS_ACC
                                               where docno = a.docno
                                                 and mmcode = a.mmcode
                                                 and lot_no = a.lot_no
                                                 and exp_date = a.exp_date
                                                 and seq = a.seq
                                                 and isacc = 'Y')";
            return DBWork.Connection.Execute(sql, new { docno = docno }, DBWork.Transaction);
        }
        public int InsertMeDocExp(string docno) {
            string sql = @"insert into ME_DOCEXP (docno, seq, exp_date, lot_no, mmcode, apvqty,
                                                  update_time, update_user, update_ip)
                           select a.docno,
                                  a.seq,
                                  a.exp_date,
                                  a.lot_no,
                                  a.mmcode,
                                  a.lis_apvqty as apv_qty,
                                  sysdate,
                                  'LIS',
                                  'LIS'
                             from LIS_ACCREJ a
                            where a.docno = :docno";
            return DBWork.Connection.Execute(sql, new { docno = docno }, DBWork.Transaction);
        }
        public int UpdateMeDocdAchkqty(string docno) {
            string sql = @"update ME_DOCD a
                              set a.ack_qty = (select sum(apvqty) from ME_DOCEXP
                                                where docno = a.docno
                                                  and seq = a.seq
                                                  and mmcode = a.mmcode
                                                group by docno, seq, mmcode),
                                  a.update_time = sysdate,
                                  a.update_user = 'LIS',
                                  a.update_ip = 'LIS'
                            where a.docno = :docno";
            return DBWork.Connection.Execute(sql, new { docno = docno }, DBWork.Transaction);
        }
    }
}
