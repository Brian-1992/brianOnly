using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.BE
{
    public class BE0009Repository : JCLib.Mvc.BaseRepository
    {
        public BE0009Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<BE0009> GetMaster0(string pACT_YM, string[] arr_p0, string p1, string p2, string p3, string p4, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string mat_class = "";
            for (int i = 0; i < arr_p0.Length; i++)
            {
                if (i == 0)
                    mat_class = @" '" + arr_p0[i] + "'";
                else
                    mat_class += @",'" + arr_p0[i] + "'";
            }
            
            var sql = @"  select t.INVOICE_DT, t.INVOICE, t.AGEN_NO, t.AGEN_NAMEC, t.UNI_NO,
                                                 listagg(t.DELI_DT,',') within group (order by t.DELI_DT) as DELI_DT
                                      from(
                                             select  distinct TWN_DATE(a.INVOICE_DT) as INVOICE_DT, a.INVOICE, a.AGEN_NO, b.AGEN_NAMEC, b.UNI_NO, 
                                                          TWN_DATE(DELI_DT) as DELI_DT                           
                                                from INVOICE a
                                                inner join PH_VENDER b on a.AGEN_NO=b.AGEN_NO 
                                                   left join PH_INVOICE c on a.INVOICE=c.INVOICE
                                              where trim(ACT_YM) is null ";
                                           

            if (pACT_YM != "")
            {
                sql += @" and 
                          (select count(*) 
                          from PH_INVOICE d, BC_CS_ACC_LOG c 
                          where d.invoice=a.invoice and d.acc_log_seq = c.seq
                                  and  twn_date(c.ACC_TIME)>=(select twn_date(SET_BTIME) from MI_MNSET where SET_YM=:pACT_YM)
                                  and  twn_date(c.ACC_TIME)<=(select twn_date(SET_CTIME) from MI_MNSET where SET_YM=:pACT_YM))>0 ";
                p.Add(":pACT_YM", string.Format("{0}", pACT_YM));
            }
            if (arr_p0.Length > 0)
            {
                sql += @" and (select count(*) from MI_MAST where MMCODE=a.MMCODE and (MAT_CLASS in (" + mat_class + ") or MAT_CLASS_SUB in (" + mat_class + ")))>0";
            }
            if (p1 != "" & p2 != "")  //112.09.13花蓮改用進貨日期
            {
                sql += "  and (select count(*) from PH_INVOICE where INVOICE=a.INVOICE and TWN_DATE(DELI_DT) BETWEEN :p1 AND :p2)>0";
                p.Add(":p1", string.Format("{0}", p1));
                p.Add(":p2", string.Format("{0}", p2));
            }
            if (p1 != "" & p2 == "")
            {
                sql += " and (select count(*) from PH_INVOICE where INVOICE=a.INVOICE and TWN_DATE(DELI_DT) >= :p1)>0";
                p.Add(":p1", string.Format("{0}", p1));
            }
            if (p1 == "" & p2 != "")
            {
                sql += " and (select count(*) from PH_INVOICE where INVOICE=a.INVOICE and TWN_DATE(DELI_DT) <= :p2)>0";
                p.Add(":p2", string.Format("{0}", p2));
            }
            if (p3 != "")
            {
                sql += " and a.INVOICE like :p3";
                p.Add(":p3", string.Format("%{0}%", p3));
            }
            if (p4 != "")
            {
                sql += " and a.AGEN_NO=:p4";
                p.Add(":p4", string.Format("{0}", p4));
            }
            sql += "     ) t";
            sql += "    group by  INVOICE_DT,INVOICE,AGEN_NO,AGEN_NAMEC,UNI_NO "; 
            sql += " order by t.INVOICE_DT, t.INVOICE, t.AGEN_NO ";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BE0009>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<BE0009> GetMaster1(string pACT_YM, string[] arr_p0, string p1, string p2, string p3, string p4, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string mat_class = "";
            for (int i = 0; i < arr_p0.Length; i++)
            {
                if (i == 0)
                    mat_class = @" '" + arr_p0[i] + "'";
                else
                    mat_class += @",'" + arr_p0[i] + "'";
            }
            var sql = @"  select t.INVOICE_DT,t.INVOICE,t.AGEN_NO,t.AGEN_NAMEC,t.UNI_NO,t.INVMARK,
                                                 listagg(t.DELI_DT,',') within group (order by t.DELI_DT) as DELI_DT
                                        from (
                                            select distinct TWN_DATE(a.INVOICE_DT) as INVOICE_DT, a.INVOICE, a.AGEN_NO, b.AGEN_NAMEC, b.UNI_NO, a.INVMARK,
                                                        TWN_DATE(DELI_DT) as DELI_DT
                                              from INVOICE a
                                              inner join PH_VENDER b on a.AGEN_NO=b.AGEN_NO
                                                  left join PH_INVOICE c on a.INVOICE=c.INVOICE
                                            where 1=1";

            if (pACT_YM != "")
            {
                sql += @" and a.ACT_YM=:pACT_YM";
                p.Add(":pACT_YM", string.Format("{0}", pACT_YM));
            }
            if (arr_p0.Length > 0)
            {
                sql += @" and (select count(*) from MI_MAST where MMCODE=a.MMCODE and (MAT_CLASS in (" + mat_class + ") or MAT_CLASS_SUB in (" + mat_class + ")))>0";
            }
            if (p1 != "" & p2 != "")  //112.09.13花蓮改用進貨日期
            {
                sql += "  and (select count(*) from PH_INVOICE where INVOICE=a.INVOICE and TWN_DATE(DELI_DT) BETWEEN :p1 AND :p2)>0";
                p.Add(":p1", string.Format("{0}", p1));
                p.Add(":p2", string.Format("{0}", p2));
            }
            if (p1 != "" & p2 == "")
            {
                sql += " and (select count(*) from PH_INVOICE where INVOICE=a.INVOICE and TWN_DATE(DELI_DT) >= :p1)>0";
                p.Add(":p1", string.Format("{0}", p1));
            }
            if (p1 == "" & p2 != "")
            {
                sql += " and (select count(*) from PH_INVOICE where INVOICE=a.INVOICE and TWN_DATE(DELI_DT) <= :p2)>0";
                p.Add(":p2", string.Format("{0}", p2));
            }
            if (p3 != "")
            {
                sql += " and a.INVOICE like :p3";
                p.Add(":p3", string.Format("%{0}%", p3));
            }
            if (p4 != "")
            {
                sql += " and a.AGEN_NO=:p4";
                p.Add(":p4", string.Format("{0}", p4));
            }
            sql += "    ) t";
            sql += "  group by  INVOICE_DT,INVOICE,AGEN_NO,AGEN_NAMEC,UNI_NO,INVMARK";
            sql += " order by t.INVOICE_DT, t.INVOICE, t.AGEN_NO ";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BE0009>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<BE0009> GetDetail(string pINVOICE, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select a.INVOICE, a.MMCODE, b.MMNAME_C, b.MMNAME_E, a.M_NHIKEY, b.BASE_UNIT, a.INVOICE_QTY, a.INVOICE_PRICE, a.INVOICE_AMOUNT,
                               a.REBATESUM, a.DISC_AMOUNT,(a.INVOICE_AMOUNT- a.REBATESUM-a.DISC_AMOUNT) as AMTPAID
                           from INVOICE a, MI_MAST b
                          where a.MMCODE=b.MMCODE";

            if (pINVOICE != "")
            {
                sql += @" and a.INVOICE=:pINVOICE";
                p.Add(":pINVOICE", string.Format("{0}", pINVOICE));
            }

            sql += " order by a.INVOICE, a.MMCODE ";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BE0009>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<BE0009> GetDetailForm(string pINVOICE, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select nvl(round(sum(a.INVOICE_AMOUNT)),0) as F1, nvl(round(sum(REBATESUM+DISC_AMOUNT)),0) as F2,
                                              nvl(round(sum(INVOICE_AMOUNT-REBATESUM-DISC_AMOUNT)),0) as F3
                           from INVOICE a, MI_MAST b
                          where a.MMCODE=b.MMCODE";

            if (pINVOICE != "")
            {
                sql += @" and a.INVOICE=:pINVOICE";
                p.Add(":pINVOICE", string.Format("{0}", pINVOICE));
            }

            sql += " group by a.INVOICE ";
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BE0009>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<COMBO_MODEL> GetActymCombo()
        {
            string sql = @" select SET_YM as VALUE, 
                            twn_date(SET_BTIME) || ' ~ ' || twn_date(SET_ETIME) as TEXT,
                            SET_YM || ' ' || twn_date(SET_BTIME) || ' ~ ' || twn_date(SET_ETIME) as COMBITEM,
                            twn_date(SET_BTIME) as EXTRA1, twn_date(SET_ETIME) as EXTRA2
                            from MI_MNSET 
                            order by SET_YM desc ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, null);
        }
        public IEnumerable<COMBO_MODEL> GetMatclassCombo()
        {
            string sql = @" SELECT MAT_CLASS AS VALUE, '全部'||MAT_CLSNAME AS TEXT, 
                                   MAT_CLASS || ' ' || '全部'|| MAT_CLSNAME AS COMBITEM
                            FROM MI_MATCLASS WHERE  mat_clsid in ('1','2')
                            union
                            select data_value as value, data_desc as text, 
                            data_value || ' ' || data_desc as COMBITEM 
                            from PARAM_D
	                        where grp_code ='MI_MAST' 
	                        and data_name = 'MAT_CLASS_SUB'
	                        and data_value <> '1'
                            and data_desc not in ('衛材', '共同衛材')
	                        and trim(data_desc) is not null
                            ORDER BY VALUE";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, null);
        }
        public IEnumerable<PH_VENDER> GetAgennoCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"select {0} AGEN_NO, AGEN_NAMEC, AGEN_NAMEE, AGEN_NO||' '||AGEN_NAMEC as EASYNAME
                            from PH_VENDER where (REC_STATUS <> 'X' or REC_STATUS is null) ";

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(AGEN_NO, :AGEN_NO_I), 1000) + NVL(INSTR(AGEN_NAMEC, :AGEN_NAMEC_I), 100) * 10 + NVL(INSTR(AGEN_NAMEE, :AGEN_NAMEE_I), 100) * 10) IDX,");
                p.Add(":AGEN_NO_I", p0);
                p.Add(":AGEN_NAMEC_I", p0);
                p.Add(":AGEN_NAMEE_I", p0);

                sql += " AND (AGEN_NO LIKE :AGEN_NO ";
                p.Add(":AGEN_NO", string.Format("{0}%", p0));

                sql += " OR AGEN_NAMEC LIKE :AGEN_NAMEC ";
                p.Add(":AGEN_NAMEC", string.Format("%{0}%", p0));

                sql += " OR AGEN_NAMEE LIKE :AGEN_NAMEE) ";
                p.Add(":AGEN_NAMEE", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY AGEN_NO ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PH_VENDER>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<BE0009> GetINVMARK(string act_ym)
        {
            string sql = @"select distinct INVMARK from INVOICE
                            where ACT_YM=:ACT_YM";
            return DBWork.Connection.Query<BE0009>(sql, new { ACT_YM = act_ym }, DBWork.Transaction);
        }
        public bool CheckActYmExists(string act_ym)
        {
            string sql = @"Select 1 from INVOICE where ACT_YM=:ACT_YM ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { ACT_YM = act_ym }, DBWork.Transaction) == null);
        }
        public int MasterAdd(BE0009 be0009)
        {
            string sql = @"update INVOICE set ACT_YM=:act_ym, 
                             UPDATE_TIME = SYSDATE, UPDATE_USER=:UPDATE_USER,UPDATE_IP=:UPDATE_IP 
                           where INVOICE = :invoice";
            return DBWork.Connection.Execute(sql, be0009, DBWork.Transaction);
        }
        public int MasterRemove(BE0009 be0009)
        {
            string sql = @"update INVOICE set ACT_YM=null, 
                             UPDATE_TIME = SYSDATE, UPDATE_USER=:UPDATE_USER,UPDATE_IP=:UPDATE_IP 
                           where INVOICE = :invoice";
            return DBWork.Connection.Execute(sql, be0009, DBWork.Transaction);
        }
        public int MasterFinish(BE0009 be0009)
        {
            string sql = @"update INVOICE set INVMARK='1', 
                             UPDATE_TIME = SYSDATE, UPDATE_USER=:UPDATE_USER,UPDATE_IP=:UPDATE_IP 
                           where ACT_YM = :act_ym";
            return DBWork.Connection.Execute(sql, be0009, DBWork.Transaction);
        }
    }
}