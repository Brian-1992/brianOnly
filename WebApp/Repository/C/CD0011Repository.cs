using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.C
{
    public class CD0011Repository : JCLib.Mvc.BaseRepository
    {
        public CD0011Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<BC_WHPICK> GetAll(string wh_no, string pick_date, string pick_userid, string lot_no, string barcode, string docno, string appdept, int page_index, int page_size, string sorters)
        {
            // sorters = sorters.Replace("STORE_LOC", "substr(STORE_LOC,3,18)");

            var p = new DynamicParameters();
            var sql = @" select A.DOCNO, (select inid_name from UR_INID where inid=(select appdept from ME_DOCM where docno=A.docno)) as APPDEPT, A.SEQ, A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.APPQTY, A.ACT_PICK_QTY, A.BASE_UNIT, A.STORE_LOC,
                            (select APPLY_NOTE from ME_DOCM where DOCNO=A.DOCNO) as APPLY_NOTE,
                            (select APLYITEM_NOTE from ME_DOCD where DOCNO=A.DOCNO and SEQ=A.SEQ) as APLYITEM_NOTE,
                            A.WEXP_ID, (select DATA_DESC from PARAM_D where GRP_CODE='MI_MAST' and DATA_NAME='WEXP_ID' and DATA_VALUE=A.WEXP_ID) as WEXP_ID_DESC, 
                            (select inv_qty from MI_WHINV where wh_no=A.WH_NO and mmcode=A.MMCODE) as INV_QTY, A.ACT_PICK_NOTE,
                            (select MAT_CLASS from MI_MAST where MMCODE=A.MMCODE) as MAT_CLASS,
                            A.WH_NO, to_char(A.PICK_DATE,'yyyy-mm-dd') as PICK_DATE, A.PICK_USERID,
                            (select lot_no from BC_WHPICKDOC where DOCNO = A.DOCNO and WH_NO = A.WH_NO and to_char(PICK_DATE,'yyyy-mm-dd') = to_char(A.PICK_DATE,'yyyy-mm-dd')) as LOT_NO,
                            (select lot_no from BC_WHPICK_VALID where DOCNO = A.DOCNO and SEQ = A.SEQ and WH_NO = A.WH_NO and to_char(PICK_DATE,'yyyy-mm-dd') = to_char(A.PICK_DATE,'yyyy-mm-dd')) as LOT_NO_F,
                            (select TWN_DATE(VALID_DATE) from BC_WHPICK_VALID where DOCNO = A.DOCNO and SEQ = A.SEQ and WH_NO = A.WH_NO and to_char(PICK_DATE,'yyyy-mm-dd') = to_char(A.PICK_DATE,'yyyy-mm-dd')) as VALID_DATE,
                            (select count(*) from BC_WHPICK where WH_NO = :p0 and DOCNO = A.DOCNO and (PICK_USERID = :p2 or PICK_USERID is null) and PICK_DATE is not null ) as NEED_PICK_ITEMS,
                            (select count(*) from BC_WHPICK where WH_NO = :p0 and DOCNO = A.DOCNO and (PICK_USERID = :p2 or PICK_USERID is null) and ACT_PICK_USERID is null and PICK_DATE is not null) as LACK_PICK_ITEMS, A.ACT_PICK_USERID,
                            (select count(*) from BC_WHPICK_SHIPOUT where DOCNO=A.DOCNO) as SHIPOUTCNT
                            from BC_WHPICK A
                            where 1=1 ";

            sql += " AND A.WH_NO = :p0 ";
            p.Add(":p0", wh_no);

            sql += " AND to_char(A.PICK_DATE,'yyyy-mm-dd') = Substr(:p1, 1, 10) ";
            p.Add(":p1", pick_date);

            p.Add(":p2", pick_userid);
            p.Add(":p3", lot_no);
            if (lot_no != "" && lot_no != null)
            {
                sql += " AND A.PICK_USERID = :p2 ";
                sql += " AND (select lot_no from BC_WHPICKDOC where DOCNO = A.DOCNO and WH_NO = :p0 and to_char(PICK_DATE,'yyyy-mm-dd') = Substr(:p1, 1, 10)) = :p3 ";
            }
            else if (docno != "" && docno != null)
            {
                sql += " AND A.DOCNO = :p6 ";
                p.Add(":p6", docno);
            }
            else if (appdept != "" && appdept != null)
            {
                sql += " AND A.PICK_USERID = :p2 ";
                sql += " AND (select appdept from ME_DOCM where docno=A.docno) = :p7 ";
                p.Add(":p7", appdept);
            }

            sql += " AND A.MMCODE like :p4 ";
            p.Add(":p4", string.Format("%{0}%", barcode));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BC_WHPICK>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<BC_WHPICK_VALID> GetValidAll(string wh_no, string pick_date, string docno, string seq)
        {
            var p = new DynamicParameters();
            var sql = @" select LOT_NO, TWN_DATE(VALID_DATE) as VALID_DATE, ACT_PICK_QTY
                            from BC_WHPICK_VALID
                            where 1=1 ";

            sql += " AND WH_NO = :p0 ";
            p.Add(":p0", wh_no);

            sql += " AND to_char(PICK_DATE,'yyyy-mm-dd') = Substr(:p1, 1, 10) ";
            p.Add(":p1", pick_date);

            sql += " AND DOCNO = :p2 ";
            p.Add(":p2", docno);

            sql += " AND SEQ = :p3 ";
            p.Add(":p3", seq);

            return DBWork.PagingQuery<BC_WHPICK_VALID>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<BC_WHPICK> GetPickAll(string wh_no, string pick_date, string docno, string pick_userid)
        {
            var p = new DynamicParameters();
            var sql = @" select wh_no,TWN_DATE(pick_date) as pick_date,docno,seq,mmname_c,mmname_e,(select barcode from BC_BARCODE where mmcode=a.mmcode and rownum = 1) as barcode,mmcode
                            from BC_WHPICK a
                            where wh_no=:p0 and to_char(pick_date,'yyyy-mm-dd') = Substr(:p1, 1, 10)
                            and docno=:p2                      
                            and act_pick_userid is null ";

            if (wh_no != "PH1S")
                sql += " and pick_userid=:p3 ";

            p.Add(":p0", wh_no);
            p.Add(":p1", pick_date);
            p.Add(":p2", docno);
            p.Add(":p3", pick_userid);

            return DBWork.PagingQuery<BC_WHPICK>(sql, p, DBWork.Transaction);
        }

        //public IEnumerable<BC_WHPICK> Get(string wh_no, string pick_date, string docno, string seq, string pick_userid, string lot_no, string mmcode)
        //{
        //    var p = new DynamicParameters();
        //    var sql = @" select A.DOCNO, A.SEQ, A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.APPQTY, A.ACT_PICK_QTY, A.BASE_UNIT, A.STORE_LOC,
        //                    A.WH_NO, to_char(A.PICK_DATE,'yyyy-mm-dd') as PICK_DATE, A.PICK_USERID,
        //                    (select lot_no from BC_WHPICKDOC where DOCNO = A.DOCNO and WH_NO = A.WH_NO and to_char(PICK_DATE,'yyyy-mm-dd') = to_char(A.PICK_DATE,'yyyy-mm-dd')) as LOT_NO
        //                    from BC_WHPICK A
        //                    where 1=1 ";

        //    sql += " AND A.WH_NO = :p0 ";
        //    p.Add(":p0", wh_no);

        //    sql += " AND to_char(A.PICK_DATE,'yyyy-mm-dd') = Substr(:p1, 1, 10) ";
        //    p.Add(":p1", pick_date);

        //    sql += " AND A.DOCNO = :p2 ";
        //    p.Add(":p2", docno);

        //    sql += " AND A.SEQ = :p3 ";
        //    p.Add(":p3", seq);

        //    sql += " AND A.PICK_USERID = :p4 ";
        //    p.Add(":p4", pick_userid);

        //    sql += " AND (select lot_no from BC_WHPICKDOC where DOCNO = A.DOCNO and WH_NO = :p0 and to_char(PICK_DATE,'yyyy-mm-dd') >= Substr(:p1, 1, 10)) = :p5 ";
        //    p.Add(":p5", lot_no);

        //    sql += " AND A.MMCODE = :p6 ";
        //    p.Add(":p6", mmcode);

        //    sql += "and ROWNUM = 1 ";

        //    return DBWork.Connection.Query<BC_WHPICK>(sql, p, DBWork.Transaction);
        //}

        public int ChkMmcode(string wh_no, string pick_date, string pick_userid, string lot_no, string mmcode, string docno)
        {
            var p = new DynamicParameters();
            var sql = @" select count(*)
                            from BC_WHPICK A
                            where 1=1 ";

            sql += " AND A.WH_NO = :p0 ";
            p.Add(":p0", wh_no);

            sql += " AND to_char(A.PICK_DATE,'yyyy-mm-dd') = Substr(:p1, 1, 10) ";
            p.Add(":p1", pick_date);

            if (wh_no != "PH1S")
                sql += " AND A.PICK_USERID = :p2 ";
            p.Add(":p2", pick_userid);

            sql += " AND A.MMCODE = :p4 ";
            p.Add(":p4", mmcode);

            if (lot_no != "" && lot_no != null)
            {
                sql += " AND (select lot_no from BC_WHPICKDOC where DOCNO = A.DOCNO and WH_NO = :p0 and to_char(PICK_DATE,'yyyy-mm-dd') >= Substr(:p1, 1, 10)) = :p3 ";
                p.Add(":p3", lot_no);
            }
            else if (docno != "" && docno != null)
            {
                sql += " AND A.DOCNO = :p5 ";
                p.Add(":p5", docno);
            }

            return DBWork.Connection.QueryFirst<int>(sql, p, DBWork.Transaction);
        }

        public int ChkDocno(string wh_no, string pick_date, string pick_userid, string barcode)
        {
            var p = new DynamicParameters();
            var sql = @" select count(*)
                            from BC_WHPICK A
                            where A.HAS_SHIPOUT is null ";

            sql += " AND A.WH_NO = :p0 ";
            p.Add(":p0", wh_no);

            sql += " AND to_char(A.PICK_DATE,'yyyy-mm-dd') = Substr(:p1, 1, 10) ";
            p.Add(":p1", pick_date);

            sql += " AND A.PICK_USERID = :p2 ";
            p.Add(":p2", pick_userid);

            sql += " AND A.DOCNO = :p4 ";
            p.Add(":p4", barcode);

            return DBWork.Connection.QueryFirst<int>(sql, p, DBWork.Transaction);
        }

        public int ChkAppdept(string wh_no, string pick_date, string pick_userid, string barcode)
        {
            var p = new DynamicParameters();
            var sql = @" select count(*)
                            from BC_WHPICK A
                            where A.HAS_SHIPOUT is null ";

            sql += " AND A.WH_NO = :p0 ";
            p.Add(":p0", wh_no);

            sql += " AND to_char(A.PICK_DATE,'yyyy-mm-dd') = Substr(:p1, 1, 10) ";
            p.Add(":p1", pick_date);

            sql += " AND A.PICK_USERID = :p2 ";
            p.Add(":p2", pick_userid);

            sql += " AND (select APPDEPT from ME_DOCM where DOCNO=A.DOCNO) = :p4 ";
            p.Add(":p4", barcode);

            return DBWork.Connection.QueryFirst<int>(sql, p, DBWork.Transaction);
        }

        public int Update(string wh_no, string pick_date, string docno, string seq, string pick_userid, string lot_no, string mmcode, string act_pick_qty, string act_pick_note, string setType,
                        string name, string procip)
        {
            var sql = @" UPDATE BC_WHPICK A ";
            if (setType == "U")
            {
                sql = sql + @" set A.ACT_PICK_USERID=:NAME, A.ACT_PICK_QTY=:ACT_PICK_QTY, A.ACT_PICK_TIME=SYSDATE, HAS_CONFIRMED='Y', ACT_PICK_NOTE=:ACT_PICK_NOTE,
                            A.UPDATE_TIME = SYSDATE, A.UPDATE_USER = :NAME, A.UPDATE_IP = :UPDATE_IP ";
            }
            else if (setType == "D")
            {
                sql = sql + @" set A.ACT_PICK_USERID='', A.ACT_PICK_QTY=null, A.ACT_PICK_TIME=null, HAS_CONFIRMED='', ACT_PICK_NOTE='',
                            A.UPDATE_TIME = SYSDATE, A.UPDATE_USER = :NAME, A.UPDATE_IP = :UPDATE_IP ";
            }

            sql = sql + @" where A.WH_NO=:WH_NO and to_char(A.PICK_DATE,'yyyy-mm-dd')=:PICK_DATE 
                            and A.DOCNO=:DOCNO and A.SEQ=:SEQ
                            and A.PICK_USERID=:PICK_USERID
                            and (select LOT_NO from BC_WHPICKDOC 
                            where WH_NO=:WH_NO and to_char(PICK_DATE,'yyyy-mm-dd')=:PICK_DATE
                            and DOCNO=A.DOCNO)=:LOT_NO
                            and A.MMCODE=:MMCODE ";

            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, PICK_DATE = pick_date, DOCNO = docno, SEQ = seq, PICK_USERID = pick_userid, LOT_NO = lot_no, MMCODE = mmcode, ACT_PICK_QTY = act_pick_qty, ACT_PICK_NOTE = act_pick_note, NAME = name, UPDATE_IP = procip }, DBWork.Transaction);
        }

        public int CreateWhpickValid(string wh_no, string pick_date, string docno, string seq, string lot_no, string valid_date, string act_pick_qty, string name, string procip)
        {
            var sql = @" insert into BC_WHPICK_VALID (WH_NO, PICK_DATE, DOCNO, SEQ, LOT_NO, VALID_DATE, ACT_PICK_QTY, CREATE_DATE, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP)
                            values(:WH_NO, to_date(:PICK_DATE, 'yyyy-mm-dd'), :DOCNO, :SEQ, :LOT_NO, to_date(:VALID_DATE, 'yyyy-mm-dd'), :ACT_PICK_QTY, SYSDATE, :NAME, SYSDATE, :NAME, :UPDATE_IP)";

            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, PICK_DATE = pick_date, DOCNO = docno, SEQ = seq, LOT_NO = lot_no, VALID_DATE = valid_date, ACT_PICK_QTY = act_pick_qty, NAME = name, UPDATE_IP = procip }, DBWork.Transaction);
        }

        public int DeleteWhpickValid(string wh_no, string pick_date, string docno, string seq)
        {
            var sql = @" delete from BC_WHPICK_VALID where WH_NO=:WH_NO and to_char(PICK_DATE,'yyyy-mm-dd')=:PICK_DATE 
                            and DOCNO=:DOCNO and SEQ=:SEQ ";

            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, PICK_DATE = pick_date, DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }

        // for 藥品以外
        public int UpdateMeDocd(string docno, string seq, string act_pick_qty)
        {
            var sql = @" update ME_DOCD set APVQTY = :ACT_PICK_QTY
                            where DOCNO = :DOCNO and SEQ = :SEQ ";

            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq, ACT_PICK_QTY = act_pick_qty }, DBWork.Transaction);
        }
        // for 藥品
        public int UpdateMeDocd(string docno, string seq, string act_pick_qty, string pick_user)
        {
            var sql = @" update ME_DOCD set POSTID = '3', ACKQTY = :ACT_PICK_QTY, PICK_QTY = :ACT_PICK_QTY, APVQTY = :ACT_PICK_QTY, 
                            PICK_USER = :PICK_USER, PICK_TIME = sysdate
                            where DOCNO = :DOCNO and SEQ = :SEQ ";

            return DBWork.Connection.Execute(sql, new { DOCNO = docno, SEQ = seq, ACT_PICK_QTY = act_pick_qty, PICK_USER = pick_user }, DBWork.Transaction);
        }

        public int UpdateMeDocdAll(string wh_no, string pick_date, string pick_userid, string lot_no, string docno, string appdept)
        {
            var sql = @" update ME_DOCD a
                            set ACKQTY=(select act_pick_qty from BC_WHPICK 
                            where docno=a.docno and seq=a.seq and has_shipout='Y'),
                            PICK_QTY=(select act_pick_qty from BC_WHPICK 
                            where docno=a.docno and seq=a.seq and has_shipout='Y'),
                            PICK_USER=(select act_pick_userid from BC_WHPICK 
                            where docno=a.docno and seq=a.seq and has_shipout='Y'),
                            PICK_TIME=(select act_pick_time from BC_WHPICK 
                            where docno=a.docno and seq=a.seq and has_shipout='Y')
                            where pick_user is null ";

            if (lot_no != "" && lot_no != null)
            {
                sql += " AND DOCNO in (select DOCNO from BC_WHPICK where DOCNO = a.DOCNO and SEQ = a.SEQ and WH_NO = :WH_NO and to_char(PICK_DATE,'yyyy-mm-dd') = Substr(:PICK_DATE, 1, 10) and PICK_USERID = :PICK_USERID and HAS_SHIPOUT='Y' )  ";
                sql += " AND (select lot_no from BC_WHPICKDOC where DOCNO = a.DOCNO and WH_NO = :WH_NO and to_char(PICK_DATE,'yyyy-mm-dd') = Substr(:PICK_DATE, 1, 10)) = :LOT_NO ";
            }
            else if (docno != "" && docno != null)
            {
                sql += " and DOCNO = :DOCNO ";
            }
            else if (appdept != "" && appdept != null)
            {
                sql += " AND DOCNO in (select DOCNO from BC_WHPICK where DOCNO = a.DOCNO and SEQ = a.SEQ and WH_NO = :WH_NO and to_char(PICK_DATE,'yyyy-mm-dd') = Substr(:PICK_DATE, 1, 10) and PICK_USERID = :PICK_USERID and HAS_SHIPOUT='Y' )  ";
                sql += " AND (select appdept from ME_DOCM where docno=a.docno) = :APPDEPT ";
            }

            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, PICK_DATE = pick_date, PICK_USERID = pick_userid, LOT_NO = lot_no, DOCNO = docno, APPDEPT = appdept }, DBWork.Transaction);
        }

        public int UpdateMeDocmShipout(string wh_no, string pick_date, string pick_userid, string lot_no, string docno, string appdept)
        {
            var sql = @" update ME_DOCM a set flowid='4'
                            where mat_class<>'01' and flowid='3'
                            and not exists (select 'x' from BC_WHPICK 
                            where docno=a.docno and has_shipout is null) ";

            if (lot_no != "" && lot_no != null)
            {
                sql += " AND DOCNO in (select DOCNO from BC_WHPICK where DOCNO = a.DOCNO and WH_NO = :WH_NO and to_char(PICK_DATE,'yyyy-mm-dd') = Substr(:PICK_DATE, 1, 10) and PICK_USERID = :PICK_USERID) ";
                sql += " AND (select lot_no from BC_WHPICKDOC where DOCNO = a.DOCNO and WH_NO = :WH_NO and to_char(PICK_DATE,'yyyy-mm-dd') = Substr(:PICK_DATE, 1, 10)) = :LOT_NO ";
            }
            else if (docno != "" && docno != null)
            {
                sql += " and DOCNO = :DOCNO ";
            }
            else if (appdept != "" && appdept != null)
            {
                sql += " AND DOCNO in (select DOCNO from BC_WHPICK where DOCNO = a.DOCNO and WH_NO = :WH_NO and to_char(PICK_DATE,'yyyy-mm-dd') = Substr(:PICK_DATE, 1, 10) and PICK_USERID = :PICK_USERID) ";
                sql += " AND (select appdept from ME_DOCM where docno=a.docno) = :APPDEPT ";
            }

            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, PICK_DATE = pick_date, PICK_USERID = pick_userid, LOT_NO = lot_no, DOCNO = docno, APPDEPT = appdept }, DBWork.Transaction);
        }

        public int updateLotNo(string wh_no, string docno, string lot_no)
        {
            var sql = @" update BC_WHPICKDOC set LOT_NO = :LOT_NO
                            where WH_NO = :WH_NO and DOCNO = :DOCNO and (LOT_NO is null or LOT_NO = 0) ";

            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, DOCNO = docno, LOT_NO = lot_no }, DBWork.Transaction);
        }

        public int updatePickUser(string wh_no, string docno, string pick_user)
        {
            var sql = @" update BC_WHPICK set PICK_USERID = :PICK_USER
                            where WH_NO = :WH_NO and DOCNO = :DOCNO and PICK_USERID is null ";

            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, DOCNO = docno, PICK_USER = pick_user }, DBWork.Transaction);
        }

        public int updateShipout(string wh_no, string pick_date, string docno, string seq)
        {
            var sql = @" update BC_WHPICK set HAS_SHIPOUT = 'Y'
                            where WH_NO = :WH_NO 
                            and PICK_DATE = to_date(:PICK_DATE, 'yyyy-mm-dd') 
                            and DOCNO = :DOCNO
                            and SEQ = :SEQ ";

            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, PICK_DATE = pick_date, DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }

        public int updateShipoutAll(string wh_no, string pick_date, string pick_userid, string lot_no, string docno, string appdept)
        {
            var sql = @" update BC_WHPICK A set HAS_SHIPOUT = 'Y'
                            where WH_NO = :WH_NO 
                            and to_char(PICK_DATE,'yyyy-mm-dd') = Substr(:PICK_DATE, 1, 10)
                            and ACT_PICK_USERID is not null and HAS_SHIPOUT is null ";

            if (lot_no != "" && lot_no != null)
            {
                sql += " AND A.PICK_USERID = :PICK_USERID ";
                sql += " AND (select lot_no from BC_WHPICKDOC where DOCNO = A.DOCNO and WH_NO = :WH_NO and to_char(PICK_DATE,'yyyy-mm-dd') = Substr(:PICK_DATE, 1, 10)) = :LOT_NO ";
            }
            else if (docno != "" && docno != null)
            {
                sql += " and DOCNO = :DOCNO ";
            }
            else if (appdept != "" && appdept != null)
            {
                sql += " AND A.PICK_USERID = :PICK_USERID ";
                sql += " AND (select appdept from ME_DOCM where docno=A.docno) = :APPDEPT ";
            }

            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, PICK_DATE = pick_date, PICK_USERID = pick_userid, LOT_NO = lot_no, DOCNO = docno, APPDEPT = appdept }, DBWork.Transaction);
        }

        public int updatePickAll(string wh_no, string pick_date, string pick_userid, string lot_no, string mmcode, string docno, string appdept)
        {
            var p = new DynamicParameters();
            var sql = @" update BC_WHPICK a
                            set a.ACT_PICK_USERID=:p2, a.ACT_PICK_QTY=a.APPQTY, a.ACT_PICK_TIME=sysdate, HAS_CONFIRMED='Y'
                            where a.WH_NO = :p0
                            and to_char(a.PICK_DATE,'yyyy-mm-dd') = Substr(:p1, 1, 10) ";

            p.Add(":p0", wh_no);
            p.Add(":p1", pick_date);
            p.Add(":p2", pick_userid);

            if (lot_no != "" && lot_no != null)
            {
                sql += " AND a.PICK_USERID = :p2 ";
                sql += " AND (select lot_no from BC_WHPICKDOC where DOCNO = a.DOCNO and WH_NO = :p0 and to_char(PICK_DATE,'yyyy-mm-dd') >= Substr(:p1, 1, 10)) = :p3 ";
                p.Add(":p3", lot_no);
            }
            else if (docno != "" && docno != null)
            {
                sql += " AND a.DOCNO = :p5 ";
                p.Add(":p5", docno);
            }
            else if (appdept != "" && appdept != null)
            {
                sql += " AND a.PICK_USERID = :p2 ";
                sql += " AND (select appdept from ME_DOCM where docno=a.docno) = :p6 ";
                p.Add(":p6", appdept);
            }
            sql += " and a.ACT_PICK_USERID is null ";
            sql += " and (a.WEXP_ID is null or a.WEXP_ID = 'N') ";

            return DBWork.Connection.Execute(sql, p, DBWork.Transaction);
        }

        public int insertNewWhpick(string wh_no)
        {
            var sql = @" insert into BC_WHPICK (wh_no,pick_date,docno,seq,mmcode,appqty,base_unit,        
                            aplyitem_note, mat_class,mmname_c,mmname_e,wexp_id,store_loc)
                            select :WH_NO as wh_no,trunc(sysdate) as pick_date,a.docno,b.seq, b.mmcode,b.appqty,
                            (select base_unit from MI_MAST where mmcode=b.mmcode) as base_unit,
                            b.aplyitem_note,
                            (select mat_class from MI_MAST where mmcode=b.mmcode) as mat_class,
                            (select mmname_c from MI_MAST where mmcode=b.mmcode) as mmname_c,
                            (select mmname_e from MI_MAST where mmcode=b.mmcode) as mmname_e,
                            (select wexp_id from MI_MAST where mmcode=b.mmcode) as wexp_id,
                            (select store_loc from MI_WLOCINV where wh_no=:WH_NO and mmcode=b.mmcode and rownum=1) as store_loc
                            from ME_DOCM a,ME_DOCD b
                            where a.docno=b.docno
                            and TWN_DATE(a.apptime)>=TWN_DATE(sysdate-10) and a.frwh=:WH_NO
                            and a.doctype in ('MR','MS','MR1','MR2','MR3','MR4') 
                            and a.flowid in ('0102','0602','3')
                            and not exists (select 'x' from BC_WHPICKDOC where wh_no=a.frwh and docno=a.docno)
                            order by docno ";

            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no }, DBWork.Transaction);
        }

        public int insertNewWhpickDoc(string wh_no)
        {
            var sql = @" insert into BC_WHPICKDOC (wh_no,pick_date,docno,apply_kind,complexity,lot_no)
                            select :WH_NO as wh_no,trunc(sysdate) as pick_date,docno,apply_kind, 1,0
                            from ME_DOCM a
                            where TWN_DATE(a.apptime)>=TWN_DATE(sysdate-10) and frwh=:WH_NO
                            and doctype in ('MR','MS','MR1','MR2','MR3','MR4') 
                            and a.flowid in ('0102','0602','3')
                            and not exists (select 'x' from BC_WHPICKDOC where wh_no=a.frwh and     docno=a.docno)
                            order by docno ";

            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no }, DBWork.Transaction);
        }

        public int insertWhpicklot(string wh_no, string pick_date, string lot_no, string pick_userid)
        {
            var sql = @" insert into BC_WHPICKLOT (WH_NO, PICK_DATE, LOT_NO, PICK_USERID, PICK_STATUS)
                            values(:WH_NO, to_date(:PICK_DATE, 'yyyy-mm-dd'), :LOT_NO, :PICK_USERID, 'A') ";

            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, PICK_DATE = pick_date, LOT_NO = lot_no, PICK_USERID = pick_userid }, DBWork.Transaction);
        }

        public int insertWhpickShipout(string wh_no, string pick_date, string docno, string seq)
        {
            var sql = @" insert into BC_WHPICK_SHIPOUT (wh_no,shipout_date,docno,seq,mmcode,act_pick_qty, base_unit,boxno,barcode,xcategory,pick_date)
                            select wh_no,sysdate,docno,seq,mmcode,act_pick_qty, base_unit,boxno,barcode,xcategory,pick_date
                            from BC_WHPICK
                            where WH_NO = :WH_NO 
                            and PICK_DATE = to_date(:PICK_DATE, 'yyyy-mm-dd') 
                            and DOCNO = :DOCNO
                            and SEQ = :SEQ
                            and HAS_SHIPOUT = 'Y' ";

            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, PICK_DATE = pick_date, DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }

        public int insertWhpickShipoutAll(string wh_no, string pick_date, string pick_userid, string lot_no, string docno, string appdept)
        {
            var sql = @" insert into BC_WHPICK_SHIPOUT (wh_no,shipout_date,docno,seq,mmcode,act_pick_qty, base_unit,boxno,barcode,xcategory,pick_date)
                            select wh_no,sysdate,docno,seq,mmcode,act_pick_qty, base_unit,boxno,barcode,xcategory,pick_date
                            from BC_WHPICK A
                            where WH_NO = :WH_NO 
                            and to_char(PICK_DATE,'yyyy-mm-dd') = Substr(:PICK_DATE, 1, 10)
                            and HAS_SHIPOUT = 'Y'
                            and not exists (select 'x' from BC_WHPICK_SHIPOUT where DOCNO=A.DOCNO and SEQ=A.SEQ) ";

            if (lot_no != "" && lot_no != null)
            {
                sql += " AND A.PICK_USERID = :PICK_USERID ";
                sql += " AND (select lot_no from BC_WHPICKDOC where DOCNO = A.DOCNO and WH_NO = :WH_NO and to_char(PICK_DATE,'yyyy-mm-dd') = Substr(:PICK_DATE, 1, 10)) = :LOT_NO ";
            }
            else if (docno != "" && docno != null)
            {
                sql += " and DOCNO = :DOCNO ";
            }
            else if (appdept != "" && appdept != null)
            {
                sql += " AND A.PICK_USERID = :PICK_USERID ";
                sql += " AND (select appdept from ME_DOCM where docno=A.docno) = :APPDEPT ";
            }

            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, PICK_DATE = pick_date, PICK_USERID = pick_userid, LOT_NO = lot_no, DOCNO = docno, APPDEPT = appdept }, DBWork.Transaction);
        }

        public int insertDocexp(string wh_no, string pick_date, string docno, string seq, string mmcode)
        {
            var sql = @" insert into ME_DOCEXP (docno,seq,mmcode,lot_no,exp_date, apvqty)
                            select docno,seq,:MMCODE,lot_no, valid_date,act_pick_qty
                            from BC_WHPICK_VALID
                            where WH_NO = :WH_NO 
                            and PICK_DATE = to_date(:PICK_DATE, 'yyyy-mm-dd') 
                            and DOCNO = :DOCNO
                            and SEQ = :SEQ ";

            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, PICK_DATE = pick_date, DOCNO = docno, SEQ = seq, MMCODE = mmcode }, DBWork.Transaction);
        }

        public int insertDocexpAll(string wh_no, string pick_date, string pick_userid, string lot_no, string docno, string appdept)
        {
            var sql = @" insert into ME_DOCEXP (docno,seq,mmcode,lot_no,exp_date, apvqty)
                            select docno,seq,(select mmcode from BC_WHPICK where WH_NO=a.WH_NO and DOCNO=a.DOCNO and SEQ=a.SEQ) as mmcode,
                            lot_no, valid_date,act_pick_qty
                            from BC_WHPICK_VALID a
                            where WH_NO = :WH_NO 
                            and to_char(PICK_DATE,'yyyy-mm-dd') = Substr(:PICK_DATE, 1, 10)
                            and not exists (select 'x' from ME_DOCEXP where docno=a.docno and seq=a.seq) ";

            if (lot_no != "" && lot_no != null)
            {
                sql += " AND DOCNO in (select DOCNO from BC_WHPICK where DOCNO = a.DOCNO and WH_NO = :WH_NO and SEQ = a.SEQ amd PICK_USERID = :PICK_USERID and HAS_SHIPOUT='Y' ) ";
                sql += " AND (select lot_no from BC_WHPICKDOC where DOCNO = a.DOCNO and WH_NO = :WH_NO and to_char(PICK_DATE,'yyyy-mm-dd') = Substr(:PICK_DATE, 1, 10)) = :LOT_NO ";
            }
            else if (docno != "" && docno != null)
            {
                sql += " and DOCNO = :DOCNO ";
            }
            else if (appdept != "" && appdept != null)
            {
                sql += " AND DOCNO in (select DOCNO from BC_WHPICK where DOCNO = a.DOCNO and SEQ = a.SEQ and WH_NO = :WH_NO and to_char(PICK_DATE,'yyyy-mm-dd') = Substr(:PICK_DATE, 1, 10) and PICK_USERID = :PICK_USERID and HAS_SHIPOUT='Y' ) ";
                sql += " AND (select appdept from ME_DOCM where docno=a.docno) = :APPDEPT ";
            }

            return DBWork.Connection.Execute(sql, new { WH_NO = wh_no, PICK_DATE = pick_date, PICK_USERID = pick_userid, LOT_NO = lot_no, DOCNO = docno, APPDEPT = appdept }, DBWork.Transaction);
        }

        public IEnumerable<MI_WHMAST> GetWH_NoCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.WH_NO, A.WH_NAME FROM MI_WHMAST A 
                            WHERE WH_GRADE = '1' ";


            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.WH_NO, :WH_NO_I), 1000) + NVL(INSTR(A.WH_NAME, :WH_NAME_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大                
                p.Add(":WH_NO_I", p0);
                p.Add(":WH_NAME_I", p0);

                sql += " AND (A.WH_NO LIKE :WH_NO ";
                p.Add(":WH_NO", string.Format("%{0}%", p0));

                sql += " OR A.WH_NAME LIKE :WH_NAME) ";
                p.Add(":WH_NAME", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, WH_NO", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.WH_NO ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_WHMAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetPickUserCombo(string wh_no)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT distinct A.WH_USERID as VALUE, 
                            (select UNA from UR_ID where TUSER = A.WH_USERID) as COMBITEM from BC_WHID A
                            WHERE 1=1 ";

            if (wh_no != "" && wh_no != null)
            {
                sql += " AND A.WH_NO = :p0 ";
                p.Add(":p0", wh_no);
            }

            sql += " order by (select UNA from UR_ID where TUSER = A.WH_USERID) ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetLotNoCombo(string wh_no, string pick_date, string pick_user)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT distinct A.LOT_NO as VALUE, A.LOT_NO as COMBITEM
                            from BC_WHPICKDOC A
                            WHERE A.LOT_NO <> '0' ";

            if (wh_no != "" && wh_no != null)
            {
                sql += " AND A.WH_NO = :p0 ";
                p.Add(":p0", wh_no);
            }

            if (pick_date != "" && pick_date != null)
            {
                sql += " AND to_char(A.PICK_DATE,'yyyy-mm-dd') = Substr(:p1, 1, 10) ";
                p.Add(":p1", pick_date);
            }

            if (wh_no != "" && wh_no != null && pick_date != "" && pick_date != null && pick_user != "" && pick_user != null)
            {
                sql += " and (select count(*) from BC_WHPICK where WH_NO = A.WH_NO and DOCNO = A.DOCNO and to_char(PICK_DATE,'yyyy-mm-dd') = to_char(A.PICK_DATE,'yyyy-mm-dd') and PICK_USERID = :p2) > 0";
                p.Add(":p2", pick_user);
            }

            sql += " order by A.LOT_NO ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetDocnoCombo(string wh_no, string pick_date, string pick_user)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT DOCNO as VALUE, 
                            DOCNO as COMBITEM
                            from BC_WHPICKDOC A
                            WHERE WH_NO = :p0 and to_char(PICK_DATE,'yyyy-mm-dd') = Substr(:p1, 1, 10)
                            and DOCNO 
                            in (select distinct DOCNO from BC_WHPICK where WH_NO =:p0 
                            and to_char(PICK_DATE,'yyyy-mm-dd') = Substr(:p1, 1, 10)
                            and PICK_USERID = :p2
                            ) and (select count(*) from BC_WHPICK_SHIPOUT where DOCNO=A.DOCNO) = 0 ";

            p.Add(":p0", wh_no);
            p.Add(":p1", pick_date);
            p.Add(":p2", pick_user);

            sql += " order by DOCNO ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetAppdeptCombo(string wh_no, string pick_date, string pick_user)
        {
            var p = new DynamicParameters();

            var sql = @" SELECT distinct (select appdept from ME_DOCM where docno=A.docno) as VALUE, 
                            (select appdept from ME_DOCM where docno=A.docno) || ' ' ||
                            (select inid_name from UR_INID where inid=(select appdept from ME_DOCM where docno=A.docno))
                            as COMBITEM
                            from BC_WHPICKDOC A
                            WHERE WH_NO = :p0 and to_char(PICK_DATE,'yyyy-mm-dd') = Substr(:p1, 1, 10)
                            and DOCNO 
                            in (select distinct DOCNO from BC_WHPICK where WH_NO =:p0 
                            and to_char(PICK_DATE,'yyyy-mm-dd') = Substr(:p1, 1, 10)
                            and PICK_USERID = :p2
                            ) and (select count(*) from BC_WHPICK_SHIPOUT where DOCNO=A.DOCNO) = 0 ";

            p.Add(":p0", wh_no);
            p.Add(":p1", pick_date);
            p.Add(":p2", pick_user);

            sql += " order by VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<MI_WEXPINV> GetLotNoDataCombo(string wh_no, string mmcode)
        {
            var p = new DynamicParameters();

            var sql = @" select WH_NO, MMCODE, LOT_NO, TWN_DATE(EXP_DATE) as EXP_DATE, INV_QTY
                            from MI_WEXPINV A
                            WHERE WH_NO = :WH_NO and MMCODE = :MMCODE ";

            p.Add(":WH_NO", wh_no);
            p.Add(":MMCODE", mmcode);

            sql += " order by EXP_DATE ";

            return DBWork.Connection.Query<MI_WEXPINV>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<MI_WHMAST> GetChkUserWhno(string userId)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.WH_NO, A.WH_NAME from MI_WHMAST A, BC_WHID B
                            WHERE A.WH_NO=B.WH_NO and rownum = 1 ";

            if (userId != "" && userId != null)
            {
                sql += " AND B.WH_USERID = :p0 ";
                p.Add(":p0", userId);
            }

            return DBWork.Connection.Query<MI_WHMAST>(sql, p, DBWork.Transaction);
        }

        // 依院內碼取得物料類別
        public string getMatClass(string mmcode)
        {
            string sql = @"select MAT_CLASS from MI_MAST
                            where MMCODE=:MMCODE";
            return DBWork.Connection.QueryFirst<string>(sql, new { MMCODE = mmcode }, DBWork.Transaction);
        }

        public string chkLotNo(string wh_no, string docno)
        {
            string sql = @"select case when LOT_NO is null then '0' when LOT_NO = 0 then '0' else '1' end from BC_WHPICKDOC
                            where WH_NO=:WH_NO and DOCNO = :DOCNO";
            return DBWork.Connection.QueryFirst<string>(sql, new { WH_NO = wh_no, DOCNO = docno }, DBWork.Transaction);
        }

        public int chkPickCnt(string wh_no, string pick_date, string pick_userid, string lot_no, string docno, string appdept)
        {
            // sub-query使用的條件
            string extraStr = "";
            if (lot_no != "" && lot_no != null)
            {
                extraStr += " AND PICK_USERID = :PICK_USERID ";
                extraStr += " AND (select lot_no from BC_WHPICKDOC where DOCNO = BC_WHPICK.DOCNO and WH_NO = :WH_NO and to_char(PICK_DATE,'yyyy-mm-dd') = Substr(:PICK_DATE, 1, 10)) = :LOT_NO ";
            }
            else if (docno != "" && docno != null)
            {
                extraStr += " and DOCNO = :DOCNO ";
            }
            else if (appdept != "" && appdept != null)
            {
                extraStr += " AND PICK_USERID = :PICK_USERID ";
                extraStr += " AND (select appdept from ME_DOCM where docno=BC_WHPICK.docno) = :APPDEPT ";
            }

            string sql = @" select count(*) from BC_WHPICK A
                            where WH_NO = :WH_NO AND to_char(PICK_DATE,'yyyy-mm-dd') = Substr(:PICK_DATE, 1, 10) 
                            and not exists (select 'x' from BC_WHPICK where 1=1 " + extraStr  +  @" and DOCNO = A.DOCNO and ACT_PICK_USERID is null)
                            and has_shipout is null ";

            if (lot_no != "" && lot_no != null)
            {
                sql += " AND A.PICK_USERID = :PICK_USERID ";
                sql += " AND (select lot_no from BC_WHPICKDOC where DOCNO = A.DOCNO and WH_NO = :WH_NO and to_char(PICK_DATE,'yyyy-mm-dd') = Substr(:PICK_DATE, 1, 10)) = :LOT_NO ";
            }
            else if (docno != "" && docno != null)
            {
                sql += " and DOCNO = :DOCNO ";
            }
            else if (appdept != "" && appdept != null)
            {
                sql += " AND A.PICK_USERID = :PICK_USERID ";
                sql += " AND (select appdept from ME_DOCM where docno=A.docno) = :APPDEPT ";
            }

            return DBWork.Connection.QueryFirst<int>(sql, new { WH_NO = wh_no, PICK_DATE = pick_date, PICK_USERID = pick_userid, LOT_NO = lot_no, DOCNO = docno, APPDEPT = appdept }, DBWork.Transaction);
        }

        public string getMatClass(string wh_no, string docno, string seq)
        {
            // 原從BC_WHPICK取會有無資料問題,改從ME_DOCD取
            string sql = @"select MAT_CLASS from MI_MAST where MMCODE 
                            = (select MMCODE from ME_DOCD where DOCNO = :DOCNO and SEQ = :SEQ)";
            return DBWork.Connection.QueryFirst<string>(sql, new { DOCNO = docno, SEQ = seq }, DBWork.Transaction);
        }

        public string getApplyKind(string wh_no, string docno)
        {
            string sql = @" select APPLY_KIND from BC_WHPICKDOC 
                            where WH_NO = :WH_NO and DOCNO = :DOCNO ";
            return DBWork.Connection.QueryFirst<string>(sql, new { WH_NO = wh_no, DOCNO = docno }, DBWork.Transaction);
        }

        // 取新揀貨批次
        public string getNewLotNO(string wh_no, string docno, string pick_date, string mat_class, string apply_kind)
        {
            string sql = @" select case when max(A.LOT_NO) is null then 1 else max(A.LOT_NO) + 1 end from BC_WHPICKDOC A
                            where A.WH_NO=:WH_NO and to_char(A.PICK_DATE,'yyyy-mm-dd')=:PICK_DATE 
                            and A.LOT_NO < (case when :MAT_CLASS='02' and :APPLY_KIND = '1' then 1000 else A.LOT_NO + 1 end) ";
            return DBWork.Connection.QueryFirst<string>(sql, new { WH_NO = wh_no, DOCNO = docno, PICK_DATE = pick_date, MAT_CLASS = mat_class, APPLY_KIND = apply_kind }, DBWork.Transaction);
        }

        public string getChkMmcode(string barcode)
        {
            var sql = @"select case when (select count(*) from BC_BARCODE where BARCODE = :BARCODE) > 0
                            then (select MMCODE from BC_BARCODE where BARCODE = :BARCODE and rownum = 1)
                            else :BARCODE end as BARCODE from dual ";
            return DBWork.Connection.ExecuteScalar(sql, new { BARCODE = barcode }, DBWork.Transaction).ToString();
        }

        public string procDistin(string DOCNO, string UPDUSR, string UPDIP)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_DOCNO", value: DOCNO, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("I_UPDUSR", value: UPDUSR, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("I_UPDIP", value: UPDIP, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);
            p.Add("O_ERRMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            DBWork.Connection.Query("POST_DOC", p, commandType: CommandType.StoredProcedure);
            string RetId = p.Get<OracleString>("O_RETID").Value;
            string ErrMsg = p.Get<OracleString>("O_ERRMSG").Value;

            if (RetId == "N")
                return "核撥過帳錯誤 SP:" + ErrMsg;
            else if (RetId == "Y")
                return "Y";
            else
                return "";
        }
    }
}