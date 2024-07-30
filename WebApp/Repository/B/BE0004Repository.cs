using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using WebApp.Models.PH;

namespace WebApp.Repository.BE
{
    public class BE0004Repository : JCLib.Mvc.BaseRepository
    {
        public BE0004Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public int MasterCreate(PH_LOTNO ph_lotno)
        {
            //if (ph_lotno.SOURCE == "自行輸入")
            //{
            //    ph_lotno.SOURCE = "U";
            //}
            //if (ph_lotno.SOURCE == "廠商輸入")
            //{
            //    ph_lotno.SOURCE = "V";
            //}
            //if (ph_lotno.STATUS == "未結案")
            //{
            //    ph_lotno.STATUS = "N";
            //}
            //if (ph_lotno.STATUS == "已結案")
            //{
            //    ph_lotno.STATUS = "Y";
            //}
            var sql = @"INSERT INTO PH_LOTNO (SEQ, SOURCE,STATUS,EXP_DATE, LOT_NO, MEMO, MMCODE, PO_NO, QTY, CREATE_TIME, CREATE_USER)  
                                VALUES (:SEQ, 'U',:STATUS,to_date(:EXP_DATE,'yyyy/mm/dd'), :LOT_NO ,:MEMO,:MMCODE,:PO_NO,:QTY ,SYSDATE,:CREATE_USER)";
            return DBWork.Connection.Execute(sql, ph_lotno, DBWork.Transaction);
        }
        public string getSeq()
        {
            var p = new DynamicParameters();
            var sql = @" select PH_LOTNO_SEQ.nextval as SEQ
                            from dual ";
            return DBWork.Connection.QueryFirst<string>(sql, p, DBWork.Transaction);
        }
        public int MasterUpdateM(PH_LOTNO ph_lotno)
        {
            //if (ph_lotno.SOURCE == "自行輸入")
            //{
            //    ph_lotno.SOURCE = "U";
            //}
            //if (ph_lotno.SOURCE == "廠商輸入")
            //{
            //    ph_lotno.SOURCE = "V";
            //}
            //if (ph_lotno.STATUS == "未結案")
            //{
            //    ph_lotno.STATUS = "N";
            //}
            //if (ph_lotno.STATUS == "已結案")
            //{
            //    ph_lotno.STATUS = "Y";
            //}
            var sql = @"UPDATE PH_LOTNO SET MEMO=:MEMO, STATUS=:STATUS, UPDATE_TIME = SYSDATE, UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE MMCODE =:MMCODE AND SEQ=:SEQ ";
            return DBWork.Connection.Execute(sql, ph_lotno, DBWork.Transaction);
        }
        
        public int MasterUpdate(PH_LOTNO ph_lotno)
        {
            //if (ph_lotno.SOURCE=="自行輸入")
            //{
            //    ph_lotno.SOURCE = "U";
            //}
            //if (ph_lotno.SOURCE == "廠商輸入")
            //{
            //    ph_lotno.SOURCE = "V";
            //}
            //if (ph_lotno.STATUS == "未結案")
            //{
            //    ph_lotno.STATUS = "N";
            //}
            //if (ph_lotno.STATUS == "已結案")
            //{
            //    ph_lotno.STATUS = "Y";
            //}
            var sql = @"UPDATE PH_LOTNO SET MEMO=:MEMO,PO_NO=:PO_NO , QTY=:QTY, STATUS=:STATUS, UPDATE_TIME = SYSDATE, 
                             LOT_NO=:LOT_NO, EXP_DATE=to_date(:EXP_DATE,'yyyy/mm/dd'), UPDATE_USER = :UPDATE_USER, UPDATE_IP = :UPDATE_IP
                                WHERE MMCODE =:MMCODE AND SEQ=:SEQ ";
            return DBWork.Connection.Execute(sql, ph_lotno, DBWork.Transaction);
        }

        public IEnumerable<PH_LOTNO> GetMasterAll(string mclass, string bgdate, string endate,string mmcode,string lotno,string status, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters(); 
            var sql = @"SELECT SEQ, (Select mat_class||' '||mat_clsname from mi_matclass Where mat_class=m.mat_class) as mat_class,PO_NO, p.MMCODE,LOT_NO, twn_date(EXP_DATE) as EXP_DATE,QTY,MEMO, SOURCE, STATUS,   mmname_c, mmname_e
                        FROM PH_LOTNO P,MI_MAST M where P.mmcode=M.mmcode ";

            if (mclass != "")
            {
                sql += "AND M.mat_class =:p0 ";
                p.Add(":p0", mclass);
            }
            if (bgdate != "null" && bgdate != "")
            {
                sql += " AND to_char(EXP_DATE,'yyyy/mm/dd') >= Substr(:p1, 1, 10) ";
                p.Add(":p1", bgdate);
            }
            if (endate != "null" && endate != "")
            {
                sql += " AND to_char(EXP_DATE,'yyyy/mm/dd') <= Substr(:p2, 1, 10) ";
                p.Add(":p2", endate);
            }
            if (mmcode != "")
            {
                sql += "AND P.MMCODE Like :p3||'%' ";
                p.Add(":p3", mmcode);
            }
            if (lotno != "")
            {
                sql += "AND P.LOT_NO Like :p4||'%' ";
                p.Add(":p4", lotno);
            }
            if (status != "")
            {
                sql += "AND P.STATUS = :p5 ";
                p.Add(":p5", status);
            }

            sql += " order by  p.mmcode, lot_no,exp_date";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PH_LOTNO>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<PH_LOTNO> MasterGet(string mmcode, string seq)
        {
            var sql = @"SELECT seq, mat_class, PO_NO, p.MMCODE,LOT_NO, EXP_DATE, QTY, MEMO, SOURCE, STATUS,   mmname_c, mmname_e
                        FROM PH_LOTNO P,MI_MAST M where P.mmcode=M.mmcode AND P.MMCODE =:MMCODE AND SEQ=:seq ";
           
            return DBWork.Connection.Query<PH_LOTNO>(sql, new { MMCODE = mmcode, SEQ = seq }, DBWork.Transaction);
        }
        public IEnumerable<COMBO_MODEL> GetMatClassCombo()
        {
            string sql = @"SELECT MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM
                        FROM MI_MATCLASS 
                        WHERE MAT_CLSID IN ('1','2','3')";
            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
    }
}