using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using System.Data;
using System;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.B
{
    public class BA0006M : JCLib.Mvc.BaseModel
    {
        public string XACTION { get; set; }
        public string TX_QTY_T { get; set; }	//進貨數量 
        public string WH_NO { get; set; }
        public string PO_NO { get; set; }
        public string MMCODE { get; set; }
        public string SEQ { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string M_STOREID { get; set; }
        public string ACC_QTY { get; set; } // 進貨數量=入帳數量*UNIT_SWAP
        public string ACC_BASEUNIT { get; set; }
        public string ACC_PURUN { get; set; }
        public string BW_SQTY { get; set; }
        public string LOT_NO { get; set; }
        public string EXP_DATE { get; set; }
        public string STATUS { get; set; }
        public string ACC_USER { get; set; }
        public string ACC_TIME { get; set; }
        public string AGEN_NAMEC { get; set; }
        public string AGEN_NO { get; set; }
        public string M_CONTID { get; set; }  //合約
        public string M_CONTPRICE { get; set; }//合約價
        public string APL_INQTY { get; set; }  //本月進貨
        public string UPRICE { get; set; }
        public string M_PURUN { get; set; }
        public string UNIT_SWAP { get; set; }	//轉換率
        public string INV_QTY { get; set; }     //庫存量
        public string M_DISCPERC { get; set; }  //折讓
        public string DISC_CPRICE { get; set; } //優惠價
        public string INQTY { get; set; }	//進貨數量 
        public string BASE_UNIT { get; set; }   //最小計量單位 
        public string MAT_CLASS { get; set; }   
        public DateTime CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public DateTime UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string WEXP_ID { get; set; } //批號追蹤
        public string MEMO { get; set; }
        // for MM_PO_M, MM_PO_D
        public string INID { get; set; }
        public string DISC_UPRICE { get; set; }
    }
    public class BA0006Repository : JCLib.Mvc.BaseRepository
    {
        public BA0006Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<BA0006M> getBC_CS_ACC_LOG(string MAT_CLASS, string ACC_TIME_B, string ACC_TIME_E, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string sql = @"select a.WH_NO, PO_NO, a.MMCODE, SEQ, b.MMNAME_C, b.MMNAME_E, a.STOREID, ACC_QTY, INV.INV_QTY, INV.APL_INQTY,
                           ACC_BASEUNIT, ACC_PURUN, LOT_NO, EXP_DATE, a.STATUS, ACC_USER,  TWN_DATE(ACC_TIME) ACC_TIME, b.UPRICE,
                           a.AGEN_NO, p.AGEN_NAMEC, a.TX_QTY_T,
						   B.MAT_CLASS, B.M_STOREID, B.M_DISCPERC, B.WEXP_ID, B.M_CONTID,
						   B.BASE_UNIT, B.M_PURUN, B.M_CONTPRICE,  a.memo,
						   (SELECT EXCH_RATIO FROM MI_UNITEXCH WHERE UNIT_CODE =b.M_PURUN AND AGEN_NO=b.M_AGENNO AND MMCODE = a.MMCODE) AS UNIT_SWAP 
                           FROM BC_CS_ACC_LOG A, MI_MAST B, PH_VENDER P,  MI_WHINV INV
                           where a.po_no like 'TXT%' and a.mmcode=b.mmcode and a.agen_no=p.agen_no 
                           and a.mat_class = :MAT_CLASS 
                           and trunc(acc_time) >= TO_DATE(:ACC_TIME_B,'YYYY/mm/dd') 
                           and trunc(acc_time) <= TO_DATE(:ACC_TIME_E,'YYYY/mm/dd') 
                           and a.wh_no =INV.wh_no(+)
                           and a.mmcode =INV.mmcode(+) ";
            p.Add(":MAT_CLASS", MAT_CLASS);
            p.Add(":ACC_TIME_B", string.Format("{0}", DateTime.Parse(ACC_TIME_B).ToString("yyyy/MM/dd")));
            p.Add(":ACC_TIME_E", string.Format("{0}", DateTime.Parse(ACC_TIME_E).ToString("yyyy/MM/dd")));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BA0006M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<BA0006M> GetSelectMmcodeDetail(string MMCODE, string WH_NO)
        {
            var p = new DynamicParameters();
            string sql = @"select M.mmcode, M.mat_class, M.M_STOREID, M.mmname_c, M.mmname_e, 
                            V.AGEN_NO, V.AGEN_NAMEC, M.M_DISCPERC, M.WEXP_ID,
                            M.UPRICE, M.DISC_CPRICE, M.DISC_UPRICE,   ---for MM_PO_D
                            M.M_CONTID, M.BASE_UNIT, M.M_PURUN, M.UPRICE, M.M_CONTPRICE,
                            nvl(INV.INV_QTY,0) INV_QTY, nvl(INV.APL_INQTY,0) APL_INQTY, 
                            (select EXCH_RATIO from MI_UNITEXCH where unit_code =M.M_PURUN and agen_no=M.m_agenno and mmcode = :MMCODE) as UNIT_SWAP 
                            from MI_MAST M, PH_VENDER V, 
                            (select MMCODE,INV_QTY, APL_INQTY from MI_WHINV where wh_no=:WH_NO ) INV  
                            where M.m_agenno = V.agen_no 
                            and M.mmcode = :MMCODE 
                            and M.mmcode =INV.mmcode(+) ";

            p.Add(":MMCODE", string.Format("{0}", MMCODE));
            p.Add(":WH_NO", string.Format("{0}", WH_NO));

            return DBWork.Connection.Query<BA0006M>(sql, p, DBWork.Transaction);
        }
        public IEnumerable<BA0006M> GetD(string PO_NO, string SEQ)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT a.WH_NO, PO_NO, a.MMCODE, SEQ, b.MMNAME_C, b.MMNAME_E, a.STOREID, ACC_QTY, INV.INV_QTY, INV.APL_INQTY,
                           ACC_BASEUNIT, ACC_PURUN, LOT_NO, EXP_DATE, a.STATUS, ACC_USER,  TWN_DATE(ACC_TIME) ACC_TIME, b.UPRICE,
                           a.AGEN_NO, p.AGEN_NAMEC, a.TX_QTY_T,
						   B.MAT_CLASS, B.M_STOREID, B.M_DISCPERC, B.WEXP_ID, B.M_CONTID,
						   B.BASE_UNIT, B.M_PURUN, B.M_CONTPRICE,  
						   (SELECT EXCH_RATIO FROM MI_UNITEXCH WHERE UNIT_CODE =b.M_PURUN AND AGEN_NO=b.M_AGENNO AND MMCODE = a.MMCODE) AS UNIT_SWAP 
                           FROM BC_CS_ACC_LOG A, MI_MAST B, PH_VENDER P,  MI_WHINV INV
                           where a.mmcode=b.mmcode and a.agen_no=p.agen_no 
                           and INV.wh_no=a.wh_no
                           and a.mmcode =INV.mmcode(+)
                           and po_no=:PO_NO ";
            p.Add(":PO_NO",  PO_NO);
            if (SEQ !=null )
            {
                sql += " and seq=:SEQ ";
                p.Add(":SEQ", SEQ);
            }
            return DBWork.Connection.Query<BA0006M>(sql, p, DBWork.Transaction);
        }
        public string getNewPO_NO(string M_STOREID)
        {
            string dn = "TXT" + M_STOREID;
            string sql = @"select :dn ||TWN_DATE(sysdate)||LPAD(TO_CHAR(nvl2(count(po_no),count(po_no)+1, 1)),4,'0')  from MM_PO_M 
                             where po_no like :dn ||TWN_DATE(sysdate)||'%' ";
            return DBWork.Connection.QueryFirst<string>(sql, new { dn = dn }, DBWork.Transaction);
        }

        public string getSeq()
        {
            var p = new DynamicParameters();
            var sql = @" select BC_CS_ACC_LOG_SEQ.nextval as SEQ
                            from dual ";
            return DBWork.Connection.QueryFirst<string>(sql, p, DBWork.Transaction);
        }
        public string getPO_NO(string seq)
        {
            var p = new DynamicParameters();
            var sql = @" select po_no from BC_CS_ACC_LOG where seq=:SEQ ";
            p.Add(":SEQ", seq);
            return DBWork.Connection.QueryFirst<string>(sql, p, DBWork.Transaction);
        }
        public int CreateBC_CS_ACC_LOG(BA0006M BA0006M)
        {
            var sql = @"insert into BC_CS_ACC_LOG (WH_NO, PO_NO, MMCODE, SEQ, AGEN_NO, LOT_NO, EXP_DATE, 
                                BW_SQTY, TX_QTY_T, ACC_QTY, CFM_QTY, ACC_BASEUNIT, STATUS, MEMO, ACC_TIME, ACC_USER, STOREID, MAT_CLASS, PO_QTY, ACC_PURUN, UNIT_SWAP, WEXP_ID)  
                                values (:WH_NO, :PO_NO, :MMCODE, BC_CS_ACC_LOG_SEQ.nextval, :AGEN_NO, :LOT_NO, 
                                case when to_char(:EXP_DATE, 'yyyy/mm/dd') = '0001/01/01' then null else to_date(to_char(:EXP_DATE, 'yyyy/mm/dd'), 'yyyy/mm/dd') end, 
                                0, :TX_QTY_T, :ACC_QTY, :ACC_QTY, :BASE_UNIT, :STATUS, trim(:MEMO), sysdate, :ACC_USER, :M_STOREID, :MAT_CLASS, :TX_QTY_T, :M_PURUN, :UNIT_SWAP, :WEXP_ID)";
            return DBWork.Connection.Execute(sql, BA0006M, DBWork.Transaction);
        }
        public int InsertMM_PO_M(BA0006M BA0006M)
        {
            var sql = @"insert into  MM_PO_M(po_no, po_time, pr_dept, wh_no, mat_class, agen_no, m_contid, po_status, 
                                   memo, create_time, create_user,  update_ip, xaction)
                         VALUES (:PO_NO, SYSDATE, :INID, :WH_NO, :MAT_CLASS, :AGEN_NO, :M_CONTID , '82', 
                                  :MEMO, SYSDATE, :ACC_USER, :UPDATE_IP, '0')";
            return DBWork.Connection.Execute(sql, BA0006M, DBWork.Transaction);
        }
        public int DeleteMM_PO_M(BA0006M BA0006M)
        {
            var sql = @"delete from MM_PO_M where PO_NO=:PO_NO ";
            return DBWork.Connection.Execute(sql, BA0006M, DBWork.Transaction);
        }
        public int InsertMM_PO_D(BA0006M BA0006M)
        {
            var sql = @"insert into MM_PO_D (po_no, mmcode, po_qty, po_price, m_purun, po_amt, m_discperc, pr_no ,unit_swap,
                               uprice, disc_cprice, disc_uprice, create_time, create_user, update_ip, storeid, deli_status) 
                        values(:PO_NO, :MMCODE, :TX_QTY_T, :M_CONTPRICE, :M_PURUN, :TX_QTY_T*:M_CONTPRICE, :M_DISCPERC, null, :UNIT_SWAP,    
	                           :UPRICE, :DISC_CPRICE, :DISC_UPRICE, SYSDATE, :ACC_USER, :UPDATE_IP, :M_STOREID, 'Y') ";
            return DBWork.Connection.Execute(sql, BA0006M, DBWork.Transaction);
        }
        public int DeleteMM_PO_D(BA0006M BA0006M)
        {
            var sql = @"delete from MM_PO_D where PO_NO=:PO_NO ";
            return DBWork.Connection.Execute(sql, BA0006M, DBWork.Transaction);
        }
        public int UpdateBC_CS_ACC_LOG(BA0006M BA0006M)
        {
            var sql = @"update BC_CS_ACC_LOG 
                          set LOT_NO=:LOT_NO, MEMO=trim(:MEMO), ACC_TIME=sysdate, ACC_USER=:ACC_USER, 
                            EXP_DATE=case when to_char(:EXP_DATE, 'yyyy/mm/dd') = '0001/01/01' then null else to_date(to_char(:EXP_DATE, 'yyyy/mm/dd'), 'yyyy/mm/dd') end, 
                            TX_QTY_T=:TX_QTY_T, ACC_QTY=:ACC_QTY, STATUS=:STATUS, WH_NO=:WH_NO, CFM_QTY=:ACC_QTY
                        where po_no=:PO_NO and seq=:SEQ ";
            return DBWork.Connection.Execute(sql, BA0006M, DBWork.Transaction);
        }
        public int DeleteBC_CS_ACC_LOG(string PO_NO, string SEQ)
        {
            var sql = @"delete from BC_CS_ACC_LOG 
                        where po_no=:PO_NO and seq=:SEQ and STATUS='A' ";
            return DBWork.Connection.Execute(sql, new { po_no = PO_NO, seq = SEQ }, DBWork.Transaction);
        }
        public string procDocin(string PO_NO, string USER_ID, string USER_IP)
        {
            var p = new OracleDynamicParameters();
            p.Add("I_PONO", value: PO_NO, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("I_USERID", value: USER_ID, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);
            p.Add("I_UPDIP", value: USER_IP, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input, size: 200);

            p.Add("O_RETID", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);
            p.Add("O_RETMSG", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 200);

            DBWork.Connection.Query("INV_SET.PO_DOCIN", p, commandType: CommandType.StoredProcedure);
            string RetId = p.Get<OracleString>("O_RETID").Value;
            string RetMsg = p.Get<OracleString>("O_RETMSG").Value;

            if (RetId == "N")
                return "SP:" + RetMsg;
            else if (RetId == "Y")
                return "入庫除帳..完成";
            else
                return "";
        }
        public IEnumerable<ComboItemModel> GetMATCombo()
        {
            string sql = @"Select MAT_CLASS||' '||MAT_CLSNAME TEXT, MAT_CLASS VALUE from MI_MATCLASS
                            where MAT_CLASS between '02' AND '08'
                            ORDER BY VALUE";
            return DBWork.Connection.Query<ComboItemModel>(sql, null, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetWh_noCombo()
        {
            var p = new DynamicParameters();
            string sql = @"select wh_no||' '||wh_name TEXT ,wh_no VALUE from MI_WHMAST where wh_kind='1' and inid=:inid ";
            sql += "order by Value ";
            return DBWork.Connection.Query<ComboItemModel>(sql, new { inid = DBWork.UserInfo.Inid }, DBWork.Transaction);
        }
        public IEnumerable<MI_MAST> GetMmcodeCombo(string MAT_CLASS, string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"select {0} MMCODE, MMNAME_C, MMNAME_E
                            from MI_MAST where mat_class=:mat_class  ";
            p.Add(":mat_class", MAT_CLASS);
            if (p0 == null) p0 = "";
            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10 + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_C_I", p0);
                p.Add(":MMNAME_E_I", p0);

                sql += " AND (MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR MMNAME_C LIKE :MMNAME_C ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql += " OR MMNAME_E LIKE :MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, MMCODE ", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY MMCODE ";
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
    }
}