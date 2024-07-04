using Dapper;
using JCLib.DB;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using WebApp.Models;

namespace WebApp.Repository.C
{
    public class CD0008Repository : JCLib.Mvc.BaseRepository
    {
        public CD0008Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public class WhnoComboItem
        {
            public string WH_NO { get; set; }
            public string WH_NAME { get; set; }
            public string WH_KIND { get; set; }
            public string WH_USERID { get; set; }
        }
        public IEnumerable<WhnoComboItem> GetWhnoCombo(string userId)
        {
            string sql = @"select b.wh_no,
                                  b.wh_no || ' ' || b.wh_name as WH_NAME,
                                  b.wh_kind,a.wh_userid
                         from MI_WHID a, MI_WHMAST b
                        where a.wh_no=b.wh_no and b.wh_grade='1' 
                          and a.wh_userid=:userid";
            return DBWork.Connection.Query<WhnoComboItem>(sql, new { USERID = userId }, DBWork.Transaction);
        }

        public IEnumerable<string> GetDocnoCombo(string wh_no, string pick_date, bool isOut) {
            var p = new DynamicParameters();
            string sql = @"select distinct docno as VALUE, docno as TEXT
                             from BC_WHPICK
                            where wh_no= :wh_no
                              and TRUNC(pick_date, 'DD')=TO_DATE(:pick_date, 'YYYY-MM-DD')";

            if (isOut)
            {
                sql += "  and has_shipout = 'Y'";
                
            }
            else
            {
                sql += "  and has_shipout is null";
            }

            return DBWork.Connection.Query<string>(sql, new {
                WH_NO = wh_no,
                PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd") }, DBWork.Transaction);
        }

        public IEnumerable<BC_WHPICK> All(string wh_no, string pick_date, bool isOut, string docno, int page_index, int page_size, string sorters) {

            var p = new DynamicParameters();
            string sql = @"select docno, wh_no,
                                  (select appdept from ME_DOCM where docno=a.docno) as appdept,
                                  seq,mmcode,mmname_c,mmname_e,appqty,
                                  act_pick_qty,base_unit,boxno,
                                  TO_CHAR(pick_date, 'YYYY-MM-DD') as PICK_DATE,
                                  has_shipout,
                                  (select mat_class from MI_MAST where mmcode=a.mmcode) as mat_class,
                                  act_pick_userid
                             from BC_WHPICK a
                            where wh_no= :wh_no
                              and TRUNC(pick_date, 'DD')=To_DATE(:pick_date, 'YYYY-MM-DD')";

            if (isOut)
            {
                sql += "  and has_shipout = 'Y'";
            }
            else {
                sql += "  and has_shipout is null";
            }

            if (docno.Trim() != string.Empty) {
                sql += " and  docno = :docno";
                p.Add(":docno", docno);
            }
            p.Add(":wh_no", wh_no);
            p.Add(":pick_date", DateTime.Parse(pick_date).ToString("yyyy-MM-dd"));
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BC_WHPICK>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<BC_WHPICK> getDocBcWhpick(string wh_no, string pick_date, string docno)
        {
            string sql = @"select wh_no,pick_date,docno,seq,mmcode,mat_class,act_pick_userid, act_pick_qty,act_pick_time,has_shipout
                        from BC_WHPICK a
                        where wh_no=:WH_NO
                        and TRUNC(pick_date, 'DD')=To_DATE(:pick_date, 'YYYY-MM-DD')
                        and docno=:DOCNO
                        order by docno, seq ";
            return DBWork.Connection.Query<BC_WHPICK>(sql, new { WH_NO = wh_no, PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"), DOCNO = docno }, DBWork.Transaction);
        }

        public IEnumerable<BC_WHPICK> GetBcWhpickByDocno(BC_WHPICK bc_whpick)
        {

            var p = new DynamicParameters();
            string sql = @" select wh_no, 
                        to_char(pick_date, 'yyyy/mm/dd') as pick_date,docno,seq,mat_class, act_pick_userid, 
                        case when act_pick_qty is null then 0 when act_pick_qty = '' then 0 else act_pick_qty end as act_pick_qty, 
                        to_char(act_pick_time, 'yyyy/mm/dd hh:mi:ss') as act_pick_time
                        from BC_WHPICK a 
                        where wh_no=:WH_NO
                        and docno=:DOCNO ";

            return DBWork.Connection.Query<BC_WHPICK>(sql, bc_whpick, DBWork.Transaction);
        }


        public IEnumerable<BC_BOX> GetBcBoxByBoxno(string input) {
            string sql = @"select boxno, barcode, xcategory, status
                             from BC_BOX
                            where boxno = :input";
            return DBWork.Connection.Query<BC_BOX>(sql, new { INPUT = input }, DBWork.Transaction);
        }
        public IEnumerable<BC_BOX> GetBcBoxByBarcode(string input)
        {
            string sql = @"select boxno, barcode, xcategory, status
                             from BC_BOX
                            where barcode = :input";
            return DBWork.Connection.Query<BC_BOX>(sql, new { INPUT = input }, DBWork.Transaction);
        }

        public int UpdateBcwhpick(BC_WHPICK bc_whpick) {
            string sql = @"update bc_whpick a 
                              set boxno = :boxno, barcode = :barcode, xcategory = :xcategory,
                                  update_user = :update_user, update_time = sysdate, update_ip = :update_ip
                            where wh_no = :wh_no
                              and TRUNC(pick_date, 'DD')=To_DATE(:pick_date, 'YYYY-MM-DD')
                              and docno = :docno
                              and seq = :seq";
            if (bc_whpick.BOXNO != string.Empty) {
                sql += "      and boxno is null";
            }
            return DBWork.Connection.Execute(sql, bc_whpick, DBWork.Transaction);
        }

        public int UpdateBcwhpickForShipout(BC_WHPICK bc_whpick)
        {
            string sql = @"update bc_whpick a 
                              set has_shipout='Y'
                            where wh_no = :wh_no
                              and TRUNC(pick_date, 'DD')=To_DATE(:pick_date, 'yyyy/MM/dd')
                              and docno = :docno
                              and seq = :seq";
            return DBWork.Connection.Execute(sql, bc_whpick, DBWork.Transaction);
        }

        public int UpdateMeDocdForShipout(BC_WHPICK bc_whpick)
        {
            string extraSql = "";
            //if (bc_whpick.MAT_CLASS == "01")
            //    extraSql = " APVQTY=:ACT_PICK_QTY, ";
            string sql = @"update ME_DOCD a
                                set ACKQTY=:ACT_PICK_QTY,
                                PICK_QTY=:ACT_PICK_QTY,
                                " + extraSql + @"
                                PICK_USER=:ACT_PICK_USERID,
                                PICK_TIME=TO_DATE(:ACT_PICK_TIME, 'yyyy/mm/dd')
                                where docno=:DOCNO
                                and seq=:SEQ ";

            return DBWork.Connection.Execute(sql, bc_whpick, DBWork.Transaction);
        }

        public int UpdateMeDocmCancelShipout(BC_WHPICK bc_whpick)
        {
            string sql = @"update ME_DOCM
                                set flowid='3'
                                where docno=:DOCNO
                                and mat_class<>'01' ";

            return DBWork.Connection.Execute(sql, bc_whpick, DBWork.Transaction);
        }

        public int InsertBcWhpickShipout(BC_WHPICK bc_whpick) {
            string sql = @"insert into BC_WHPICK_SHIPOUT 
                                  (wh_no,shipout_date,docno,seq,mmcode,act_pick_qty, base_unit,
                                   boxno,barcode,xcategory,pick_date, 
                                   create_date, create_user, update_time, update_user, update_ip)
                           select wh_no,sysdate,docno,seq,mmcode,act_pick_qty, base_unit, 
                                  boxno,barcode,xcategory,pick_date,
                                  sysdate, :create_user, sysdate, :update_user, :update_ip
                             from BC_WHPICK a
                            where wh_no=:wh_no 
                              and TRUNC(pick_date, 'DD')=To_DATE(:pick_date, 'yyyy/MM/dd')
                              and docno=:docno
                              and seq=:seq
                              and has_shipout='Y'";
            return DBWork.Connection.Execute(sql, bc_whpick, DBWork.Transaction);
        }

        public int InsertMeDocexpShipout(BC_WHPICK bc_whpick)
        {
            string sql = @"insert into ME_DOCEXP (docno,seq,mmcode,lot_no,exp_date, apvqty)
                                select docno,seq,:MMCODE,lot_no, valid_date,act_pick_qty
                                from BC_WHPICK_VALID
                                where wh_no=:WH_NO and TRUNC(pick_date, 'DD')=TO_DATE(:pick_date, 'yyyy/MM/dd')
                                and docno=:DOCNO
                                and seq=:SEQ ";
            return DBWork.Connection.Execute(sql, bc_whpick, DBWork.Transaction);
        }

        public int UpdateBcwhpickCancelShipout(BC_WHPICK bc_whpick)
        {
            string sql = @"update bc_whpick a 
                              set has_shipout=''
                            where wh_no = :wh_no
                              and TRUNC(pick_date, 'DD')=To_DATE(:pick_date, 'YYYY-MM-DD')
                              and docno = :docno
                              and seq = :seq";
            return DBWork.Connection.Execute(sql, bc_whpick, DBWork.Transaction);
        }
        public int DeleteBcWhpickShipout(BC_WHPICK bc_whpick)
        {
            string sql = @"delete from BC_WHPICK_SHIPOUT 
                            where wh_no=:wh_no 
                              and TRUNC(pick_date, 'DD')=To_DATE(:pick_date, 'YYYY-MM-DD')
                              and docno=:docno
                              and seq=:seq";
            return DBWork.Connection.Execute(sql, bc_whpick, DBWork.Transaction);
        }

        public int DeleteMeDocexpShipout(BC_WHPICK bc_whpick)
        {
            string sql = @"delete from ME_DOCEXP 
                            where docno=:docno
                              and seq=:seq";
            return DBWork.Connection.Execute(sql, bc_whpick, DBWork.Transaction);
        }

        public int GetBcWhpickCnt(BC_WHPICK bc_whpick)
        {
            var p = new DynamicParameters();
            var sql = @" select count(*) as cnt from BC_WHPICK where wh_no=:WH_NO
                    and TRUNC(pick_date, 'DD')=To_DATE(:pick_date, 'YYYY-MM-DD')
                    and docno=:DOCNO
                    and (has_shipout is null or has_shipout<>'Y') ";

            return DBWork.Connection.QueryFirst<int>(sql, bc_whpick, DBWork.Transaction);
        }

        public int chkBcWhpick(string wh_no, string pick_date, string docno)
        {
            var p = new DynamicParameters();
            var sql = @" select count(*) as cnt from BC_WHPICK where wh_no=:WH_NO
                    and TRUNC(pick_date, 'DD')=To_DATE(:pick_date, 'YYYY-MM-DD')
                    and docno=:DOCNO
                    and act_pick_userid is null
                    and (act_pick_qty is null or act_pick_qty=0)
                    and has_shipout is null ";

            return DBWork.Connection.QueryFirst<int>(sql, new { WH_NO = wh_no, PICK_DATE = DateTime.Parse(pick_date).ToString("yyyy-MM-dd"), DOCNO = docno }, DBWork.Transaction);
        }

        public string getMmcodeByScan(string barcode)
        {
            var p = new DynamicParameters();
            var sql = @" select case when (select count(*) from BC_BARCODE where BARCODE = :BARCODE) > 0
                    then (select MMCODE from BC_BARCODE where BARCODE = :BARCODE and rownum = 1)
                    else :BARCODE end from dual ";

            return DBWork.Connection.QueryFirst<string>(sql, new { BARCODE = barcode }, DBWork.Transaction);
        }

        public int UpdateNotDrm(string docno, string updusr, string procip)
        {
            var sql = @" UPDATE ME_DOCM set FLOWID = '4', UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                            where DOCNO=:DOCNO and MAT_CLASS <> '01' ";

            return DBWork.Connection.Execute(sql, new { DOCNO = docno, UPDATE_USER = updusr, UPDATE_IP = procip }, DBWork.Transaction);
        }

        public int UpdateMeDocd(string docno, string seq)
        {
            var sql = @" update ME_DOCD set POSTID = '3', ACKQTY = 0, PICK_QTY = 0, APVQTY = 0
                            where DOCNO = :DOCNO and SEQ = :SEQ ";

            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }

        public int GetChkWh(string userid)
        {
            var p = new DynamicParameters();
            var sql = @" select count(*) from BC_WHID where WH_NO in ('PH1S', 'PH1X') and WH_USERID = :p0 ";

            p.Add(":p0", userid);

            return DBWork.Connection.QueryFirst<int>(sql, p, DBWork.Transaction);
        }

        public string POST_DOC(string I_DOCNO, string I_UPDUSR, string I_UPDIP)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_DOCNO", value: I_DOCNO, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 21);
            p.Add("I_UPDUSR", value: I_UPDUSR, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 8);
            p.Add("I_UPDIP", value: I_UPDIP, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 20);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 1);
            p.Add("O_ERRMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 255);

            DBWork.Connection.Query("POST_DOC", p, commandType: CommandType.StoredProcedure);
            string retid = p.Get<OracleString>("O_RETID").Value;
            string errmsg = p.Get<OracleString>("O_ERRMSG").Value;
            return retid;
        }
    }
}