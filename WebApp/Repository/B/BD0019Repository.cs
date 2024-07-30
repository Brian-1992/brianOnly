using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Data;
using System;
using System.Data.SqlClient;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Types;
using System.Linq;


namespace WebApp.Repository.B
{
    public class BD0019Repository : JCLib.Mvc.BaseRepository
    {
        public BD0019Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MM_PR_M> GetMST(string MAT_CLASS, string PR_TIME_B, string PR_TIME_E, string M_STOREID, string PR_DEPT, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            string sql = @"SELECT 
                    pr_no, mat_class, pr_status, pr_time, memo, (SELECT una FROM ur_id WHERE tuser = a.create_user ) AS create_user, isfromdocm, 
                    (SELECT SUM(floor(nvl(pr_price,0) * pr_qty) ) FROM mm_pr_d WHERE pr_no = a.pr_no ) AS pr_amt
                FROM
                    mm_pr_m a
                WHERE
                    pr_dept =:pr_dept
                AND   xaction = '0'";

            p.Add(":PR_DEPT", string.Format("{0}", PR_DEPT));

            if (!string.IsNullOrWhiteSpace(MAT_CLASS))
            {
                if (MAT_CLASS.Contains("SUB_"))
                {
                    sql += " and (select count(*) from MM_PR_D C left join MI_MAST D on C.MMCODE = D.MMCODE where A.PR_NO = C.PR_NO and D.MAT_CLASS_SUB = :MAT_CLASS) > 0";
                    p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS.Replace("SUB_", "")));
                } 
                else
                {
                    sql += " AND mat_class = :MAT_CLASS ";
                    p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
                } 
            }
            if (!string.IsNullOrWhiteSpace(PR_TIME_B))
            {
                sql += " AND trunc(pr_time) >= TO_DATE(:PR_TIME_B,'YYYY/mm/dd') ";
                p.Add(":PR_TIME_B", string.Format("{0}", DateTime.Parse(PR_TIME_B).ToString("yyyy/MM/dd")));
            }

            if (!string.IsNullOrWhiteSpace(PR_TIME_E))
            {
                sql += " AND trunc(pr_time) <= TO_DATE(:PR_TIME_E,'YYYY/mm/dd') ";
                p.Add(":PR_TIME_E", string.Format("{0}", DateTime.Parse(PR_TIME_E).ToString("yyyy/MM/dd")));
            }

            if (!string.IsNullOrWhiteSpace(M_STOREID))
            {
                sql += " AND M_STOREID =:M_STOREID ";
                p.Add(":M_STOREID", string.Format("{0}", M_STOREID));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MM_PR_M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<MM_PR_D> GetD(string PR_NO, string MMCODE, string WH_NO, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            string sql = @"select d.mmcode, nvl(d.m_contprice,0) as m_contprice, d.pr_price, 
                                  d.agen_no, d.agen_name, d.agen_fax,  d.pr_no, d.pr_qty, 
                                  a.mmname_c, a.mmname_e,d.m_contid, d.base_unit, d.m_purun , 
                                  floor(nvl(d.pr_price,0)*d.pr_qty) as total_price, 0 as  tot,
                                  d.m_discperc, (select inv_qty from MI_WHINV where wh_no=:wh_no and mmcode=d.mmcode and rownum <= 1) inv_qty,
                                  d.memo as memo, d.disc, d.disc_cprice,
                                  get_param('MI_MAST','ORDERKIND', d.orderkind) as orderkind, d.caseNo, d.unitRate as unit_swap,
                                  get_param('MI_MAST','E_SOURCECODE', d.e_sourcecode) as e_sourcecode, d.pr_qty as ori_pr_qty,d.seq,
                                  (select discount_qty from mi_mast where mmcode=a.mmcode) as discount_qty,
                                  d.chinname, d.chartno,
                                  TWN_DATE(d.E_CODATE) as E_CODATE,  d.E_ITEMARMYNO
                             from MM_PR_M m, MM_PR_D d , MI_MAST a
                            where m.PR_NO=d.PR_NO and d.mmcode=a.mmcode 
                              and m.pr_no= : PR_NO
                            ";

            p.Add(":PR_NO", string.Format("{0}", PR_NO));
            p.Add(":WH_NO", string.Format("{0}", WH_NO));
            if (!string.IsNullOrWhiteSpace(MMCODE))
            {
                sql += " AND A.mmcode like :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", MMCODE));
            }

            sql += " order by A.mmcode, d.seq ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MM_PR_D>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="mm_pr_m"></param>
        /// <returns>
        /// 1:物料類別} +類申購單產生成功
        /// 2:本月份此類申購單已存在
        /// 3:衛材每月22日起開放下月常態申購!!
        /// 4:一般物品每月13 - 19日開放下月常態申購!!
        /// </returns>



        public string MasterCreate(MM_PR_M mm_pr_m)
        {
            string sql = "";



            //Dictionary<string, int> TXTDAY = new Dictionary<string, int>();
            //Dictionary<string, string> MAT = new Dictionary<string, string>();
            //foreach (ComboItemModel data in GetTXTDAY_ED())
            //{
            //    TXTDAY.Add(data.TEXT, int.Parse(data.VALUE));
            //}
            //foreach (ComboItemModel data in GetMATCombo())
            //{
            //    MAT.Add(data.VALUE, data.TEXT);
            //}

            ////v_pr_no = {user inid} +TO_CHAR(TO_NUMBER(TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'))-19110000000000)+ {物料類別}
            string v_pr_no = ":PR_DEPT||''||TO_CHAR(TO_NUMBER(TO_CHAR(SYSDATE, 'YYYYMMDDHH24MISS')) - 19110000000000)||''||:MAT_CLASS";
            sql = string.Format(@" 
                     insert into MM_PR_M (PR_NO, PR_DEPT, PR_TIME,  PR_USER , MAT_CLASS, PR_STATUS, MEMO, CREATE_TIME, CREATE_USER, UPDATE_IP, XACTION) 
                     values ( {0 }, user_inid(:PR_USER), sysdate, :PR_USER, :MAT_CLASS, '35', :MEMO, sysdate, :CREATE_USER, :UPDATE_IP, '0')"
            , v_pr_no);

            //DBWork.Connection.Execute(sql, mm_pr_m, DBWork.Transaction);
            ////提示 { { 物料類別} +類申購單產生成功}
            //return MAT[mm_pr_m.MAT_CLASS] + "類申購單產生成功";

            int effRows = DBWork.Connection.Execute(sql, mm_pr_m, DBWork.Transaction);

            if (effRows >= 1)
                return "申購單產生成功";
            else
                return "申購單產生失敗";

            ////sql = @" select * from mmsadm.MM_PR_M where SUBSTR(PR_NO, 20, 2) = :MAT_CLASS
            ////        and substr(PR_NO,7,7) >= TWN_DATE(sysdate)
            ////        and substr(PR_NO,7,7) <= TWN_DATE(sysdate)
            ////        and PR_DEPT = :PR_DEPT
            ////        and XACTION = '0' ";

            ////if ((DBWork.Connection.ExecuteScalar(sql, mm_pr_m, DBWork.Transaction)) == null)
            ////{
            ////    //v_pr_no = {user inid} +TO_CHAR(TO_NUMBER(TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'))-19110000000000)+ {物料類別}
            ////    string v_pr_no = ":PR_DEPT||''||TO_CHAR(TO_NUMBER(TO_CHAR(SYSDATE, 'YYYYMMDDHH24MISS')) - 19110000000000)||''||:MAT_CLASS";
            ////    sql = @" insert into MM_PR_M(PR_NO, PR_DEPT, PR_TIME,  PR_USER , MAT_CLASS, PR_STATUS, CREATE_TIME, CREATE_USER, UPDATE_IP,XACTION) 
            ////                                    values(" + v_pr_no + ", :PR_DEPT, sysdate, :PR_USER,:MAT_CLASS, '35', sysdate, :CREATE_USER, :UPDATE_IP,'0')";

            ////    DBWork.Connection.Execute(sql, mm_pr_m, DBWork.Transaction);
            ////    //提示 { { 物料類別} +類申購單產生成功}
            ////    return MAT[mm_pr_m.MAT_CLASS] + "類申購單產生成功";
            ////}
            ////else
            ////{
            ////    //提示 { 本月份此類申購單已存在}
            ////    return "本月份此類申購單已存在";
            ////}
        }
        public int totalCreate(MM_PR_M mm_pr_m)
        {
            string sql = "";


            sql = @" insert into MM_PR_M (PR_NO, PR_DEPT, PR_TIME,  PR_USER , MAT_CLASS, PR_STATUS, CREATE_TIME, CREATE_USER, UPDATE_IP, XACTION) 
                     values ( :PR_NO, :PR_DEPT, sysdate, :PR_USER, :MAT_CLASS, '35', sysdate, :CREATE_USER, :UPDATE_IP, '0') ";


            return DBWork.Connection.Execute(sql, mm_pr_m, DBWork.Transaction);

        }


        public int MasterDelete(string PR_NO)
        {
            var sql = @" Delete from MM_PR_M  where PR_NO=:PR_NO and PR_STATUS='35'";
            return DBWork.Connection.Execute(sql, new { PR_NO = PR_NO }, DBWork.Transaction);
        }
        public int MasterUpdate(MM_PR_M mm_pr_m)
        {
            var sql = @"update MM_PR_M
                         SET MAT_CLASS =:MAT_CLASS,M_STOREID=:M_STOREID,MEMO=:MEMO,UPDATE_USER=:UPDATE_USER,UPDATE_IP=:UPDATE_IP 
                        Where   PR_NO=:PR_NO ";
            return DBWork.Connection.Execute(sql, mm_pr_m, DBWork.Transaction);
        }

        public MI_MAST GetMiMast(string mmcode)
        {
            string sql = @"  select * from MI_MAST where mmcode = :mmcode";
            return DBWork.Connection.QueryFirstOrDefault<MI_MAST>(sql, new { mmcode }, DBWork.Transaction);
        }
        public int DetailCreate(MM_PR_D mm_pr_d)
        {
            string sql = @"INSERT INTO mm_pr_d ( pr_no, mmcode, pr_qty, pr_price,  m_purun,
                                       m_contprice,  agen_no,  disc, m_contid,  create_time,
                                       create_user,  update_ip,  agen_name,  agen_fax, memo,
                                       disc_cprice, disc_uprice, uprice, m_discperc, 
                                       mmname_c, mmname_e,
                                       wexp_id, base_unit, orderkind, caseno, e_sourcecode, m_nhikey, unitrate, SEQ,chinname,chartno,
                                       unit_swap, e_codate, e_itemarmyno )
                           select :pr_no, :mmcode, :pr_qty,
                                (case when 
                                       (select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode') ='805'
                                            then a.disc_cprice
                                             else  (case when a.m_contid ='0'  then a.m_contprice else a.disc_cprice  end)
                                 end) as pr_price, a.m_purun,
                                  a.m_contprice, a.m_agenno, a.m_discperc, a.m_contid, sysdate,
                                  :create_user, :update_ip, b.agen_namec, b.agen_fax, :memo,
                                  a.disc_cprice, a.disc_uprice, a.uprice, a.m_discperc,
                                  a.mmname_c, a.mmname_e, 
                                  a.wexp_id, a.base_unit, a.orderkind, a.caseno, a.e_sourcecode, a.m_nhikey, a.unitrate,MM_PR_D_SEQ.nextval,
                                  :chinname,:chartno,
                                  a.unitrate, a.e_codate, a.e_itemarmyno
                             from MI_MAST a, PH_VENDER b
                            where a.mmcode = :mmcode
                              and a.m_agenno = b.agen_no
            ";
            return DBWork.Connection.Execute(sql, mm_pr_d, DBWork.Transaction);
        }

        public int DetailUpdate(MM_PR_D mm_pr_d)
        {
            string sql = @"UPDATE mm_pr_d 
                   SET
                    PR_QTY=:PR_QTY,                    
                    MEMO = :MEMO,
                    CHINNAME = :CHINNAME,
                    CHARTNO = :CHARTNO,
                    UPDATE_TIME = SYSDATE,
                    UPDATE_USER = :UPDATE_USER,
                    UPDATE_IP = :UPDATE_IP
                    WHERE PR_NO=:PR_NO AND MMCODE=:MMCODE AND SEQ=:SEQ  ";

            return DBWork.Connection.Execute(sql, mm_pr_d, DBWork.Transaction);
        }

        public bool CheckDetailMmcodedExists(string pr_no, string mmcode)
        {
            string sql = @"SELECT 1 FROM MM_PR_D WHERE PR_NO=:PR_NO AND MMCODE=:MMCODE";
            return DBWork.Connection.ExecuteScalar(sql, new { PR_NO = pr_no, MMCODE = mmcode }, DBWork.Transaction) == null;
        }

        public int DetailDelete(string pr_no, string mmcode, int? seq)
        {
            var sql = @" DELETE from MM_PR_D WHERE PR_NO = :PR_NO AND MMCODE = :MMCODE AND SEQ=:SEQ";
            return DBWork.Connection.Execute(sql, new { PR_NO = pr_no, MMCODE = mmcode, SEQ = seq }, DBWork.Transaction);
        }
        public int DetailDeleteAll(string pr_no)
        {
            var sql = @" DELETE from MM_PR_D WHERE PR_NO = :PR_NO 
                            and (select PR_STATUS from MM_PR_M
                                where PR_NO=MM_PR_D.PR_NO)='35' ";
            return DBWork.Connection.Execute(sql, new { PR_NO = pr_no }, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetMATQCombo()
        {
            string sql = @"Select MAT_CLASS VALUE, MAT_CLASS||' '||'全部'||MAT_CLSNAME TEXT from MI_MATCLASS
                            where MAT_CLSID = '2'
                            union
                            select 'SUB_' || data_value as value, data_value || ' ' || data_desc as text
                            from PARAM_D
	                            where grp_code ='MI_MAST' 
	                            and data_name = 'MAT_CLASS_SUB'
	                            and data_value <> '1'
	                            and trim(data_desc) is not null
                            ORDER BY VALUE";
            return DBWork.Connection.Query<ComboItemModel>(sql, null, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetMATCombo()
        {
            string sql = @"Select MAT_CLASS||' '||MAT_CLSNAME TEXT, MAT_CLASS VALUE from MI_MATCLASS
                            where MAT_CLSID = '2'
                            ORDER BY VALUE";
            return DBWork.Connection.Query<ComboItemModel>(sql, null, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetWh_noCombo()
        {
            var p = new DynamicParameters();


            string sql = @"select wh_no || ' ' || wh_name TEXT ,wh_no VALUE from MI_WHMAST where wh_kind='1' and wh_grade='1' ";

            sql += "order by VALUE ";

            return DBWork.Connection.Query<ComboItemModel>(sql, null, DBWork.Transaction);
        }


        public IEnumerable<MM_PR_D> GetSelectMmcodeDetail(string MMCODE, string MAT_CLASS, string WH_NO)
        {
            var p = new DynamicParameters();

            string sql = @"select m.mmcode, m.mat_class, m.m_storeid, m.mmname_c, m.mmname_e, 0 as  total_price, 0 as tot,
                                  v.agen_no, v.agen_namec, v.agen_fax, m.m_discperc,  disc_cprice, 
                                  (select nvl(inv_qty,0) inv_qty from mi_whinv where wh_no=:wh_no and mmcode=m.mmcode and rownum <= 1) inv_qty , 
                                  (case when m.m_contid ='0' then '合約' else '非合約' end) as m_contid, 
                                  m.base_unit, m.m_purun, nvl(m.uprice, 0) as uprice,  
                                  m.unitRate as unit_swap , nvl(m.m_contprice,0) as m_contprice,
                                  get_param('MI_MAST','ORDERKIND', m.orderkind) as orderkind, m.caseNo, 
                                  (select discount_qty from mi_mast where mmcode=M.mmcode) as discount_qty,
                                  TWN_DATE(M.E_CODATE) as E_CODATE, M.E_ITEMARMYNO,
                                  get_param('MI_MAST','E_SOURCECODE', m.e_sourcecode) as e_sourcecode,
                                 (case when 
                                      (select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode') ='805'
                                           then M.disc_cprice 
                                            else  (case when M.m_contid = '0' then M.m_contprice else M.disc_cprice  end)
                                  end) as pr_price
                             from MI_MAST M, PH_VENDER V
                            where M.m_agenno = V.agen_no 
                              and M.mmcode = :MMCODE
                              and M.mat_class = :MAT_CLASS
            ";

            p.Add(":MMCODE", string.Format("{0}", MMCODE));
            p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
            p.Add(":WH_NO", string.Format("{0}", WH_NO));

            return DBWork.Connection.Query<MM_PR_D>(sql, p, DBWork.Transaction);
        }


        public string GetTot(string mmcode, string totprice)
        {
            string sql = @" with mnset as(
                     select set_btime from MI_MNSET
                     where set_ym = (substr(twn_yyymm(sysdate),1,3) ||'01')
                )
                select sum(paysum) as tot
                from (
                   select a.*,
                        (case when sumqty < 0 
                            then floor(nvl(disc_cprice,po_price) * (sumqty*-1)) *-1
                            else floor(nvl(disc_cprice,po_price) * (sumqty))
                        end) as paysum,
                        (case when sumqty < 0 
                            then floor(po_price * sumqty*-1)*-1
                            else floor(po_price * sumqty)
                             end) as fullsum
                   from (
                        select c.mmcode, disc_cprice, po_price, 
                              sum(c.acc_qty/b.unit_swap) as sumqty    
                            from MM_PO_M a, MM_PO_D b, BC_CS_ACC_LOG c, PH_VENDER p   
                        where a.po_no=b.po_no and a.agen_no=p.agen_no 
                             and b.po_no=c.po_no and b.mmcode=c.mmcode
                             and acc_time >= (select set_btime from mnset)  
                             and c.status='P' 
                             and acc_qty <> 0
                             and b.mmcode = :mmcode      
                           group by   c.mmcode, disc_cprice, po_price
                    ) a
                ) group by  mmcode";
            var str = DBWork.Connection.ExecuteScalar(sql, new { mmcode = mmcode }, DBWork.Transaction);
            if (str == null)
            {
                return totprice;
            }
            else
            {
                Int32 t1 = 0;
                Int32 t2 = 0;
                if (Int32.TryParse(totprice, out t1) == true & Int32.TryParse(str.ToString(), out t2) == true)
                    str = t1 + t2;
                else
                    str = "0";
                return str.ToString();
            }
        }

        public string[] GetReport_PARM(string pr_no, string inid)
        {

            string sql = @"select wh_name from MI_WHMAST where inid =: inid";

            string WH_NAME = DBWork.Connection.ExecuteScalar(sql, new { inid = inid }, DBWork.Transaction) == null ? "" : DBWork.Connection.ExecuteScalar(sql, new { inid = inid }, DBWork.Transaction).ToString();

            sql = @"select mat_clsname
                   from MI_MATCLASS a, MM_PR_M b 
                   where a.mat_class=b.mat_class 
                        and b.pr_no=:pr_no";

            string MAT_CLSNAME = DBWork.Connection.ExecuteScalar(sql, new { pr_no = pr_no }, DBWork.Transaction) == null ? "" : DBWork.Connection.ExecuteScalar(sql, new { pr_no = pr_no }, DBWork.Transaction).ToString();

            string[] str = new string[2];

            str[0] = MAT_CLSNAME;
            str[1] = WH_NAME;

            return str;
        }


        public IEnumerable<ComboItemModel> GetTXTDAY_ED()
        {

            string sql = @"select DATA_NAME TEXT ,DATA_VALUE  VALUE
                            from param_M M left join PARAM_D D on m.grp_code=d.grp_code
                            where  M.GRP_CODE='MM_PR_M'";


            return DBWork.Connection.Query<ComboItemModel>(sql, null, DBWork.Transaction);

        }

        public IEnumerable<MM_PR_D> GetReport_Data(string pr_no)
        {

            string sql = @" Select a.mmcode , b.mmname_c , b.mmname_e , b.m_purun , a.req_qty_t 
                        from MM_PR_D a, MI_MAST b
                        where a.pr_no =:pr_no
                         and a.mmcode = b.mmcode
                        order by a.mmcode";

            return DBWork.Connection.Query<MM_PR_D>(sql, new { pr_no = pr_no }, DBWork.Transaction);
        }
        public IEnumerable<MI_MAST> GetMmcodeCombo(string MAT_CLASS, string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"select {0} MMCODE, MMNAME_C, MMNAME_E
                             from MI_MAST 
                            where mat_class=:mat_class and M_CONTID<>'3' 
                        ";
            p.Add(":mat_class", MAT_CLASS);
            if (p0 == null) p0 = "";
            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10 + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_C_I", p0);
                p.Add(":MMNAME_E_I", p0);

                sql += " AND (upper(MMCODE) LIKE upper(:MMCODE) ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR upper(MMNAME_C) LIKE upper(:MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql += " OR upper(MMNAME_E) LIKE upper(:MMNAME_E)) ";
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

        public int UpdateMedocd(string pr_no, string mmcode)
        {
            string sql = @"
                update ME_DOCD
                   set aplyitem_note = '(衛保室刪除品項)' || chr(10) || aplyitem_note
                 where rdocno = :pr_no
                   and mmcode = :mmcode
            ";
            return DBWork.Connection.Execute(sql, new { pr_no, mmcode }, DBWork.Transaction);
        }

        public bool ChceckPrStatus(string pr_no)
        {
            string sql = @"
                select 1 from MM_PR_M
                 where PR_NO = :PR_NO
                   and PR_STATUS = '35'
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { PR_NO = pr_no }, DBWork.Transaction) != null;
        }

        public bool CheckExistsPR_NO(string pr_no)
        {
            string sql = @" SELECT 1 FROM MM_PR_M 
                          WHERE PR_NO = :PR_NO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { PR_NO = pr_no }, DBWork.Transaction) == null);
        }

        public bool CheckExistsMMCODE(string mmcode, string pr_no)
        {
            string sql = @" SELECT 1 FROM MI_MAST 
                          WHERE MMCODE = :MMCODE AND MAT_CLASS = (select mat_class from MM_PR_M where pr_no = :pr_no) ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode, pr_no }, DBWork.Transaction) == null);
        }

        public bool CheckFlagMMCODE(string mmcode)
        {
            string sql = @" SELECT nvl(cancel_id,'N') FROM MI_MAST 
                          WHERE MMCODE = :MMCODE ";
            return DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode }, DBWork.Transaction).ToString() == "N";
        }

        public bool CheckPrExistsMMCODE(string pr_no, string mmcode)
        {
            string sql = @"SELECT 1 FROM MM_PR_D WHERE PR_NO=:PR_NO AND MMCODE=:MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { PR_NO = pr_no, MMCODE = mmcode }, DBWork.Transaction) == null);
        }

        public bool CheckExistsAGENNO(string mmcode)
        {
            string sql = @" SELECT 1 FROM MI_MAST a, PH_VENDER b 
                          WHERE a.MMCODE = :MMCODE AND a.M_AGENNO = b.AGEN_NO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode }, DBWork.Transaction) == null);
        }

        public int DetailCreateFromExcel(MM_PR_D mm_pr_d)
        {
            string sql = @"
                insert into MM_PR_D ( pr_no, mmcode, pr_qty, pr_price,  m_purun,
                           m_contprice, 
                           agen_no,  disc, m_contid,  
                           create_time, create_user,  update_ip,  
                           agen_name,  agen_fax, disc_cprice, disc_uprice, uprice, m_discperc, chinname, chartno)
                select :PR_NO, a.mmcode, :PR_QTY, 
                       (case when 
                             (select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode') ='805'
                                  then a.disc_cprice
                                   else  (case when a.m_contid ='0'  then a.m_contprice else a.disc_cprice end)
                       end) as pr_price,
                       a.m_purun, a.m_contprice, 
                       a.m_agenno, a.m_discperc as disc, a.m_contid as m_contid,
                       sysdate, :CREATE_USER, :UPDATE_IP,
                       c.agen_namec, c.agen_fax, a.disc_cprice, a.disc_uprice, a.uprice, a.m_discperc, :CHINNAME, :CHARTNO
                  from MI_MAST a, PH_VENDER c
                 where a.mmcode = :MMCODE
                   and c.agen_no = a.m_agenno
            ";
            return DBWork.Connection.Execute(sql, mm_pr_d, DBWork.Transaction);
        }
        public IEnumerable<string> CheckPrStatus(string pr_no_list)
        {
            string sql = string.Format(@"
                select pr_no from MM_PR_M
                 where pr_no in ( {0} )
                   and pr_status <> '35'
            ", pr_no_list);
            return DBWork.Connection.Query<string>(sql, new { pr_no_list }, DBWork.Transaction);
        }
        public IEnumerable<MM_PR_D> GetAgaennoMContids(string pr_no_string)
        {
            string sql = string.Format(@"
                select distinct b.agen_no, b.m_contid, a.mat_class 
                  from MM_PR_M a, MM_PR_D b
                 where a.pr_no in ( {0} )
                   and b.pr_no = a.pr_no
                 order by a.mat_class, b.agen_no, b.m_contid 
            ", pr_no_string);
            return DBWork.Connection.Query<MM_PR_D>(sql, DBWork.Transaction);
        }
        public string GetTodayDate()
        {
            string sql = @"select twn_date(sysdate) from dual";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }
        public string GetPoMSeq(string pre, string m_contid, string today)
        {
            string sql = string.Format(@"
                select count(*)+1 from MM_PO_M where po_no like '{0}%'
            ", string.Format("{0}{1}{2}", pre, m_contid, today));
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }
        public PH_MAILSP GetMemo(string m_contid, string mat_class)
        {
            string sql = @"
                select memo, smemo, twn_date(dline_dt) as dline_dt 
                  from PH_MAILSP
                 where inid = wh_inid(whno_mm1) 
                   and m_contid = :m_contid
                   and mat_class = :mat_class
            ";
            return DBWork.Connection.QueryFirstOrDefault<PH_MAILSP>(sql, new { m_contid, mat_class }, DBWork.Transaction);
        }
        public int InsertMmPoM(string po_no, string mat_class, string agen_no, string m_contid,
                                  string memo, string smemo, string dline_dt,
                                  string userId, string updateIp)
        {
            string sql = @"
                insert into MM_PO_M ( PO_NO, PO_TIME, PR_DEPT, WH_NO, 
                             MAT_CLASS, AGEN_NO, M_CONTID, PO_STATUS, 
                             PHONE, MEMO, smemo,
                             CREATE_TIME, CREATE_USER,  UPDATE_IP, 
                             XACTION)
                select :po_no as po_no, sysdate as po_time, wh_inid(whno_mm1) as pr_dept, whno_mm1 as wh_no,
                       :mat_class as mat_class, a.agen_no, :m_contid as m_contid, '80' as po_status,
                       a.agen_tel, :memo as memo, :smemo as smemo, 
                       sysdate, :userId, :updateIp, 
                      '0' as xaction
                 from PH_VENDER a
                where a.agen_no = :agen_no
            ";
            return DBWork.Connection.Execute(sql, new { po_no, mat_class, agen_no, m_contid, memo, smemo, dline_dt, userId, updateIp }, DBWork.Transaction);
        }
        public int InsertMmPoD(string po_no, string pr_no_list, string mat_class, string agen_no, string m_contid, string userId, string updateIp)
        {
            string sql = string.Format(@"
                insert into MM_PO_D ( po_no, mmcode, po_qty, po_price, m_purun, po_amt, m_discperc,
                       unit_swap, uprice, disc_cprice, disc_uprice, 
                       create_time, create_user, update_ip, storeid, 
                       base_unit, memo,
                       orderkind, caseno, e_sourcecode, unitrate, m_nhikey,
                       mmname_c, mmname_e, wexp_id, e_codate,SEQ,chinname,chartno, m_contprice)
                select po_no, mmcode, po_qty, po_price, m_purun, po_amt, m_discperc,
                       unit_swap, uprice, disc_cprice, disc_uprice, 
                       create_time, create_user, update_ip, storeid, 
                       base_unit, memo,
                       orderkind, caseno, e_sourcecode, unitrate, m_nhikey,
                       mmname_c, mmname_e, wexp_id, e_codate,MM_PO_D_SEQ.nextval,chinname,chartno, m_contprice
                  from ( with temp_memo as (
                           select mmcode, listagg(memo, '、') within group (order by pr_no)  as note 
                             from MM_PR_D where pr_no in ( {0} ) and memo is not null
                            group by mmcode
                         )
                         select :po_no as po_no, a.mmcode, sum(a.pr_qty) as po_qty, a.pr_price as po_price, a.m_purun, sum(a.pr_qty * a.pr_price) as po_amt, a.m_discperc, 
                                 a.unit_swap, a.uprice, a.disc_cprice, a.disc_uprice, 
                                 sysdate as create_time, :userId as create_user, :updateIp as update_ip, '1' as storeid, 
                                 a.base_unit,
                                 (select  note from temp_memo where mmcode = a.mmcode) as memo,
                                 a.orderkind, a.caseno, a.e_sourcecode, a.unitrate, a.m_nhikey,
                                 a.mmname_c, a.mmname_e, a.wexp_id, a.e_codate,a.chinname,a.chartno, a.m_contprice
                            from MM_PR_D a, MM_PR_M b
                           where b.pr_no in ( {0} ) 
                             and a.pr_no = b.pr_no
                             and a.agen_no = :agen_no 
                             and a.m_contid = :m_contid 
                             and b.mat_class = :mat_class
                           group by a.mmcode, a.pr_price, a.m_purun, a.m_discperc, a.unit_swap, a.uprice, a.disc_cprice, a.disc_uprice,
                                    a.base_unit, a.orderkind, a.caseno, a.e_sourcecode, a.unitrate, a.m_nhikey,
                                    a.mmname_c, a.mmname_e, a.wexp_id, a.e_codate,a.chinname,a.chartno, a.m_contprice
                       )
            ", pr_no_list);

            return DBWork.Connection.Execute(sql, new { po_no, pr_no_list, mat_class, agen_no, m_contid, userId, updateIp }, DBWork.Transaction);
        }
        public int InsertPhInvoice(string po_no, string pr_no_list, string mat_class, string agen_no, string m_contid, string userId, string updateIp)
        {
            string sql = string.Format(@"
                insert into PH_INVOICE ( po_no, mmcode, transno, po_qty, po_price, m_purun, po_amt, 
                            m_discperc ,unit_swap, uprice, disc_cprice, disc_uprice, create_time, create_user, update_ip, 
                            deli_qty, ckin_qty, bw_sqty)
                select :po_no, a.mmcode, to_char(sysdate,'yyyymmddhh24miss'), sum(a.pr_qty), a.pr_price, a.m_purun, sum(a.pr_qty * a.pr_price), 
                       a.m_discperc, a.unit_swap, a.uprice, a.disc_cprice, a.disc_uprice, sysdate, :userId, :updateIp, 
                       0,0,0
                  from MM_PR_D a, MM_PR_M b
                 where b.pr_no in ( {0} )
                   and a.pr_no = b.pr_no
                   and a.agen_no = :agen_no 
                   and a.m_contid = :m_contid 
                 group by mmcode, pr_price, m_purun, m_discperc, unit_swap, uprice, disc_cprice, disc_uprice 
            ", pr_no_list);

            return DBWork.Connection.Execute(sql, new { po_no, pr_no_list, mat_class, agen_no, m_contid, userId, updateIp }, DBWork.Transaction);
        }

        public int InsertMmPrD(string po_no, string pr_no_list, string userId, string updateIp)
        {
            string sql = string.Format(@"
                insert into MM_PR_D ( pr_no, mmcode, pr_qty, pr_price,  m_purun, m_contprice, unit_swap, agen_no,  disc, m_contid, agen_name,  agen_fax, 
                                      disc_cprice, disc_uprice, uprice, m_discperc,mmname_c, mmname_e, wexp_id, base_unit,  orderkind, caseno, memo,
                                      e_sourcecode, m_nhikey, unitrate, e_codate, create_time, create_user,  update_ip, e_itemarmyno)
                select :po_no as po_no, MMCODE, sum(PR_QTY), PR_PRICE, M_PURUN, M_CONTPRICE, UNIT_SWAP, AGEN_NO, DISC, M_CONTID, AGEN_NAME, AGEN_FAX, 
                                                DISC_CPRICE, DISC_UPRICE, UPRICE, M_DISCPERC, MMNAME_C, MMNAME_E, WEXP_ID, BASE_UNIT, ORDERKIND, CASENO,
                                                listagg(MEMO,'、') within group (order by PR_NO) as MEMO,
                                                E_SOURCECODE, M_NHIKEY, UNITRATE, E_CODATE, sysdate, :userId, :updateIp, e_itemarmyno
                   from MM_PR_D WHERE PR_NO  in ( {0} )
                   group by MMCODE,PR_PRICE, M_PURUN, M_CONTPRICE, UNIT_SWAP, AGEN_NO, DISC, M_CONTID, AGEN_NAME, AGEN_FAX, DISC_CPRICE, DISC_UPRICE, UPRICE, M_DISCPERC, MMNAME_C, MMNAME_E, WEXP_ID, BASE_UNIT, ORDERKIND, CASENO,
                    E_SOURCECODE, M_NHIKEY, UNITRATE, E_CODATE, E_ITEMARMYNO
                ", pr_no_list);

            return DBWork.Connection.Execute(sql, new { po_no, pr_no_list, userId, updateIp }, DBWork.Transaction);
        }

        public int InsertMmPrDLog(string pr_no_list)
        {
            string sql = string.Format(@"
                insert into MM_PR_D_LOG (PR_NO,MMCODE,PR_QTY,PR_PRICE,M_PURUN,M_CONTPRICE,UNIT_SWAP,REQ_QTY_T,DELI_QTY,BW_SQTY,AGEN_NO,DISC,
                                         IS_FAX,M_CONTID,CREATE_TIME,CREATE_USER,UPDATE_TIME,UPDATE_USER,UPDATE_IP,REC_STATUS,IS_EMAIL,
                                         AGEN_NAME,AGEN_FAX,PR_PO_NO,MEMO,DISC_CPRICE,DISC_UPRICE,UPRICE,M_DISCPERC,SRC_PR_QTY,M_NHIKEY,MMNAME_C,
                                         MMNAME_E,WEXP_ID,BASE_UNIT,ORDERKIND,CASENO,E_SOURCECODE,UNITRATE,E_CODATE,SEQ,CHINNAME,CHARTNO, E_ITEMARMYNO)
                select PR_NO,MMCODE,PR_QTY,PR_PRICE,M_PURUN,M_CONTPRICE,UNIT_SWAP,REQ_QTY_T,DELI_QTY,BW_SQTY,AGEN_NO,DISC,
                       IS_FAX,M_CONTID,CREATE_TIME,CREATE_USER,UPDATE_TIME,UPDATE_USER,UPDATE_IP,REC_STATUS,IS_EMAIL,
                       AGEN_NAME,AGEN_FAX,PR_PO_NO,MEMO,DISC_CPRICE,DISC_UPRICE,UPRICE,M_DISCPERC,SRC_PR_QTY,M_NHIKEY,MMNAME_C,
                       MMNAME_E,WEXP_ID,BASE_UNIT,ORDERKIND,CASENO,E_SOURCECODE,UNITRATE,E_CODATE,SEQ,CHINNAME,CHARTNO, E_ITEMARMYNO
                   from MM_PR_D WHERE PR_NO  in ({0})
                   ", pr_no_list);

            return DBWork.Connection.Execute(sql, new { pr_no_list }, DBWork.Transaction);
        }

        public int DeleteMmPrD(string pr_no_list)
        {
            string sql = string.Format(@"
                Delete MM_PR_D 
                 where pr_no in  ( {0} )
                  ", pr_no_list);
            return DBWork.Connection.Execute(sql, new { pr_no_list }, DBWork.Transaction);
        }


        public int InsertMmPrMLog(string po_no, string pr_no_list, string userId)
        {
            string sql = string.Format(@"
                insert into MM_PR_M_LOG ( PR_NO, PR_DEPT, PR_TIME, PR_USER, MAT_CLASS, M_STOREID, PR_STATUS, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, XACTION, SDN, MEMO, ISFROMDOCM, SUMMARY_PR_NO, SUMMARY_TIME, SUMMARY_USER)
                select PR_NO, PR_DEPT, PR_TIME, PR_USER, MAT_CLASS, M_STOREID, PR_STATUS, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP, XACTION, SDN, MEMO, ISFROMDOCM, :po_no, sysdate,  :userId
                   from MM_PR_M WHERE PR_NO  in ( {0} )
                 ", pr_no_list);

            return DBWork.Connection.Execute(sql, new { po_no, pr_no_list, userId }, DBWork.Transaction);
        }
        public int DeleteMmPrM(string pr_no_list)
        {
            string sql = string.Format(@"
                Delete MM_PR_M 
                 where pr_no in  ( {0} )
                  ", pr_no_list);
            return DBWork.Connection.Execute(sql, new { pr_no_list }, DBWork.Transaction);
        }
        public int UpdateMmPrD(string po_no, string pr_no_list, string mat_class, string agen_no, string m_contid, string userId, string updateIp)
        {
            string sql = string.Format(@"
                update MM_PR_D a
                   set pr_po_no = :po_no, update_time = sysdate, update_user = :userId, update_ip = :updateIp
                 where pr_no in  ( {0} )
                   and exists (select 1 from MM_PR_M where pr_no = a.pr_no and mat_class = :mat_class)
                   and a.agen_no = :agen_no
                   and a.m_contid = :m_contid
            ", pr_no_list);
            return DBWork.Connection.Execute(sql, new { po_no, pr_no_list, mat_class, agen_no, m_contid, userId, updateIp }, DBWork.Transaction);
        }
        public int UpdateMmPrM(string pr_no_list, string userId, string updateIp)
        {
            string sql = string.Format(@"
                update MM_PR_M
                   set pr_status = '36', update_user = :userId, update_ip = :updateIp
                 where pr_status = '35' and pr_no in ( {0} )
            ", pr_no_list);
            return DBWork.Connection.Execute(sql, new { pr_no_list, userId, updateIp }, DBWork.Transaction);
        }

        public int GetUnitRate(string mmcode)
        {
            string sql = @"
                select unitRate from MI_MAST where mmcode = :mmcode
            ";
            return DBWork.Connection.QueryFirstOrDefault<int>(sql, new { mmcode }, DBWork.Transaction);
        }

        public string GetPono()
        {
            var p = new OracleDynamicParameters();
            p.Add("O_PONO", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 12); // 務必要填上size,不然取值都會是null

            DBWork.Connection.Query("GET_PONO", p, commandType: CommandType.StoredProcedure);
            return p.Get<OracleString>("O_PONO").Value;
        }

        public string Getsumno(string userId, string mat_class)
        {
            string sql = @"select :userId||''||TO_CHAR(TO_NUMBER(TO_CHAR(SYSDATE, 'YYYYMMDDHH24MISS')) - 19110000000000)||''||:mat_class from dual";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, new { userId, mat_class }, DBWork.Transaction);
        }

        public string GetMmPrMMemos(string pr_no_list, string agen_no, string m_contid, string mat_class)
        {
            string sql = string.Format(@"
                select distinct a.memo
                  from MM_PR_M a, MM_PR_D b
                 where a.pr_no in ( {0} )
                   and b.pr_no = a.pr_no
                   and b.agen_no = :agen_no
                   and b.m_contid = :m_contid
                   and a.mat_class = :mat_class
                   and a.memo is not null
            ", pr_no_list);

            IEnumerable<string> temp_result = DBWork.Connection.Query<string>(sql, new { pr_no_list, agen_no, m_contid, mat_class }, DBWork.Transaction);

            string result = string.Empty;
            foreach (string temp in temp_result)
            {
                if (string.IsNullOrEmpty(result) == false)
                {
                    result += "、";
                }
                result += temp;
            }
            return result;
        }

        public MM_PR_M GetChkPrStatus(string PR_NO)
        {

            var p = new DynamicParameters();

            string sql = @"select PR_STATUS, nvl(ISFROMDOCM,'N') as ISFROMDOCM
                            from MM_PR_M
                            where PR_NO = :PR_NO
                            ";

            p.Add(":PR_NO", string.Format("{0}", PR_NO));

            return DBWork.Connection.QueryFirst<MM_PR_M>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<MM_PR_D> GetExceedAmtMMCodes(string strPrNo)
        {
            //訂單已進貨量+訂單未進貨量+本次採購量
            string strSql = @"with mnset as(
                     select set_btime from MI_MNSET
                     where set_ym = (substr(twn_yyymm(sysdate),1,3) ||'01')
                    )
                    select mmcode
                    from (
                       select a.*,
                            (case when sumqty < 0 
                                then floor(nvl(disc_cprice,po_price) * (sumqty*-1)) *-1
                                else floor(nvl(disc_cprice,po_price) * (sumqty))
                            end) as paysum
                       from (
                    select c.mmcode, disc_cprice, po_price, 
                          sum(c.acc_qty) as sumqty    
                        from MM_PO_M a, MM_PO_D b, BC_CS_ACC_LOG c, PH_VENDER p   
                    where a.po_no=b.po_no and a.agen_no=p.agen_no 
                         and b.po_no=c.po_no and b.mmcode=c.mmcode
                         and acc_time >= (select set_btime from mnset)  
                         --and c.status='P' 
                         and acc_qty <> 0 
                         and b.mmcode in (select mmcode from MM_PR_D where pr_no=:pr_no and m_contid='2')
                       group by c.mmcode, disc_cprice, po_price
                    union  --增加無BC_CS_ACC_LOG
                         select b.mmcode, disc_cprice, po_price, 
                               sum(b.po_qty) as sumqty    
                          from MM_PO_M a, MM_PO_D b, PH_VENDER p  
                         where a.po_no=b.po_no and a.agen_no=p.agen_no 
                           and not exists    --無BC_CS_ACC_LOG
                               (select 1 from BC_CS_ACC_LOG c
                                 where b.po_no=c.po_no and b.mmcode=c.mmcode
                               )
                           and a.po_status in('80','82','83','84','85','88') --排除MM_PO_M.po_statud=87作廢
                           and a.po_time >= (select set_btime from mnset) --採購時間
                           and b.mmcode in (select mmcode from MM_PR_D 
                                             where pr_no=:pr_no and m_contid='2')
                         group by b.mmcode, disc_cprice, po_price
                    union -- 這次的單子
                        select b.mmcode, disc_cprice, disc_cprice as po_price,
                                sum(pr_qty) as sumqty
                         from MM_PR_M a, MM_PR_D b, PH_VENDER p
                         where a.pr_no = b.pr_no and b.agen_no=p.agen_no 
                           and a.pr_no = :pr_no
                           and b.m_contid = '2'
                        group by b.mmcode, disc_cprice
                    ) a
                    ) group by mmcode 
                    having sum(paysum) >=(select data_value from PARAM_D where grp_code='M_CONTID2_LIMIT' and data_name='LIMIT')
                    ";

            return DBWork.Connection.Query<MM_PR_D>(strSql, new { pr_no = strPrNo });
        }

        public DataTable GetExcel(string pr_no)
        {
            var sql = @" SELECT '' as 院內碼,'' as 申購數量, '' as 備註, '' as 病患姓名, '' as 病歷號  FROM DUAL ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, new { PR_NO = pr_no }, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<MM_PR_D> GetUnitrateMMCodes(string strPrNo)
        {
            string strSql = @" select mmcode from MM_PR_D   where PR_NO=:pr_no and MOD(PR_QTY,UNITRATE)<>0";
            return DBWork.Connection.Query<MM_PR_D>(strSql, new { pr_no = strPrNo });
        }
        public bool CheckMmPrDExists(string pr_no, string mmcode, string chinname, string chartno, string memo) {
            string sql = @"
                select 1 from MM_PR_D 
                 where pr_no=:pr_no
                     and nvl(chinname,'CHINNAME')=nvl(trim(:chinname),'CHINNAME')
                     and nvl(chartno,'CHARTNO')=nvl(trim(:chartno),'CHARTNO')
                     and nvl(memo,'MEMO')=nvl(trim(:memo),'MEMO')
                     and mmcode = :mmcode
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { pr_no, mmcode, chinname, chartno, memo }, DBWork.Transaction) != null;
        }

        public double GetTotalPrice(List<MM_PR01_D> ori_list)
        {
            double total = 0;
            ori_list.RemoveAll(item => item.PR_QTY == "0");


            if (ori_list.Count > 1000)
            {
                var sql = @" SELECT MMCODE,disc_cprice from MI_MAST 
                        WHERE MMCODE in  (:mmcodeP)";
                string mmcodeP = "";
                bool isFirst = true;
                foreach (MM_PR01_D item in ori_list)
                {
                    if (isFirst)
                    {
                        mmcodeP = "'"+item.MMCODE+"'";
                        isFirst = false;
                    }
                    else
                    {
                        mmcodeP += "," + "'" + item.MMCODE + "'";
                    }
                }
                var parameters = new DynamicParameters();
                parameters.Add("mmcodeP", mmcodeP);

                var data = DBWork.Connection.Query<MM_PR01_D>(sql, parameters, DBWork.Transaction);
                foreach (var itemA in ori_list)
                {
                    string MMCODE = itemA.MMCODE;
                    string PR_QTY = itemA.PR_QTY;

                    // 查找匹配的 B 集合项
                    var itemB = data.FirstOrDefault(b => b.MMCODE == MMCODE);

                    if (itemB != null)
                    {
                        string disc_cprice = itemB.DISC_CPRICE;
                        total += Convert.ToDouble(PR_QTY) * Convert.ToDouble(disc_cprice);
                    }
                }
            }
            else
            {
                var sql = @" SELECT MMCODE,disc_cprice from MI_MAST 
                        WHERE MMCODE in :MmcodeList ";


                List<string> mmcodeList = new List<string>();

                foreach (MM_PR01_D item in ori_list)
                {
                    mmcodeList.Add(item.MMCODE);
                }
                var parameters = new DynamicParameters();
                parameters.Add("MmcodeList", mmcodeList);
                var data = DBWork.Connection.Query<MM_PR01_D>(sql, parameters, DBWork.Transaction);

                foreach (var itemA in ori_list)
                {
                    string MMCODE = itemA.MMCODE;
                    string PR_QTY = itemA.PR_QTY;

                    // 查找匹配的 B 集合项
                    var itemB = data.FirstOrDefault(b => b.MMCODE == MMCODE);

                    if (itemB != null)
                    {
                        string disc_cprice = itemB.DISC_CPRICE;
                        total += Convert.ToDouble(PR_QTY) * Convert.ToDouble(disc_cprice);
                    }
                }
            }


            return total;
        }
        public bool CheckExistsM(string id)
        {
            string sql = @"SELECT 1 FROM MM_PR_M WHERE PR_NO=:PR_NO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { PR_NO = id }, DBWork.Transaction) == null);
        }
    }
}