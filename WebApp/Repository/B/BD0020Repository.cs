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
    public class BD0020Repository : JCLib.Mvc.BaseRepository
    {
        public BD0020Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MM_PR01_M> GetM(string MAT_CLASS, string PR_TIME_B, string PR_TIME_E, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            string sql = @"Select PR_NO, MAT_CLASS, PR_STATUS, PR_TIME, MEMO, (select UNA from UR_ID where TUSER = a.CREATE_USER) as CREATE_USER, ISFROMDOCM,
                                                    (select sum(pr_amt) from MM_PR01_D where pr_no= a.pr_no) as pr_amt 
                                          from MM_PR01_M a
                            where 1=1 ";

            if (!string.IsNullOrWhiteSpace(MAT_CLASS))
            {
                sql += " AND MAT_CLASS = :MAT_CLASS ";
                p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
            }
            if (!string.IsNullOrWhiteSpace(PR_TIME_B))
            {
                sql += " AND TWN_DATE(PR_TIME) >= TWN_DATE(TO_DATE(:PR_TIME_B,'YYYY/mm/dd')) ";
                p.Add(":PR_TIME_B", string.Format("{0}", DateTime.Parse(PR_TIME_B).ToString("yyyy/MM/dd")));
            }

            if (!string.IsNullOrWhiteSpace(PR_TIME_E))
            {
                sql += " AND TWN_DATE(PR_TIME) <= TWN_DATE(TO_DATE(:PR_TIME_E,'YYYY/mm/dd')) ";
                p.Add(":PR_TIME_E", string.Format("{0}", DateTime.Parse(PR_TIME_E).ToString("yyyy/MM/dd")));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MM_PR01_M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<MM_PR01_D> GetD(string PR_NO, string MMCODE, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            string sql = @"select d.PR_NO, d.MMCODE, d.mmname_c, d.mmname_e,
                                  d.PR_QTY, d.BASE_UNIT, d.M_CONTPRICE, d.M_PURUN,
                                  d.ISWILLING, d.DISCOUNT_QTY, d.DISC_COST_UPRICE, d.M_DISCPERC, d.DISC_CPRICE,
                                  (case when d.M_CONTID = '0' then '合約' when d.M_CONTID = '2' then '非合約' else d.M_CONTID end) as M_CONTID, 
                                  d.AGEN_NO, d.AGEN_NAME, d.AGEN_TEL, d.AGEN_FAX, d.PR_AMT, d.SAFE_QTY, d.OPER_QTY, d.SHIP_QTY, d.PR_QTY as ORI_PR_QTY,
                                  get_param('MI_MAST','ORDERKIND', d.orderkind) as orderkind, d.caseNo, d.unitRate as unit_swap,
                                  (select discount_qty from mi_mast where mmcode=a.mmcode) as discount_qty_2,
                                  get_param('MI_MAST','E_SOURCECODE', d.e_sourcecode) as e_sourcecode, d.memo, twn_date(a.e_codate) as e_codate, 
                                  a.e_itemarmyno,d.pr_price
                             from MM_PR01_D d, MI_MAST a
                            where d.PR_NO = :PR_NO AND d.mmcode = a.mmcode
                            ";

            p.Add(":PR_NO", string.Format("{0}", PR_NO));
            if (!string.IsNullOrWhiteSpace(MMCODE))
            {
                sql += " AND d.MMCODE like :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", MMCODE));
            }

            sql += " order by MMCODE ";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MM_PR01_D>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public string MasterCreate(MM_PR01_M mm_pr01_m)
        {
            string sql = @" insert into MM_PR01_M(PR_NO, PR_DEPT, PR_TIME, PR_USER, MEMO, CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP) 
                                                values(twn_systime, user_inid(:PR_USER), sysdate, :PR_USER, :MEMO, sysdate, :CREATE_USER, sysdate, :UPDATE_USER, :UPDATE_IP)";

            int effRows = DBWork.Connection.Execute(sql, mm_pr01_m, DBWork.Transaction);

            if (effRows >= 1)
                return "申購單產生成功";
            else
                return "申購單產生失敗";
        }


        public int MasterDelete(string PR_NO)
        {
            var sql = @" Delete from MM_PR01_M  where PR_NO=:PR_NO and PR_STATUS='35'";
            return DBWork.Connection.Execute(sql, new { PR_NO = PR_NO }, DBWork.Transaction);
        }
        public int MasterUpdate(MM_PR01_M mm_pr01_m)
        {
            var sql = @"update MM_PR01_M
                         SET MEMO=:MEMO,UPDATE_USER=:UPDATE_USER,UPDATE_IP=:UPDATE_IP 
                        where   PR_NO=:PR_NO ";
            return DBWork.Connection.Execute(sql, mm_pr01_m, DBWork.Transaction);
        }


        public int DetailCreate(MM_PR01_D mm_pr01_d)
        {
            string sql = @"
                insert into MM_PR01_D ( PR_NO, MMCODE, PR_QTY, PR_PRICE, M_PURUN, BASE_UNIT, 
                       M_CONTPRICE, M_DISCPERC, UNIT_SWAP, AGEN_NO, CONTRACNO,  M_CONTID, 
                       AGEN_NAME, AGEN_TEL, EMAIL, AGEN_FAX, PR_AMT,  MIN_ORDQTY, DISC_CPRICE, DISC_UPRICE, UPRICE, --ISWILLING, 
                       DISCOUNT_QTY, DISC_COST_UPRICE, 
                       --SAFE_QTY, OPER_QTY,  安全存量、最大存量
                       CREATE_TIME, CREATE_USER, UPDATE_TIME, UPDATE_USER, UPDATE_IP,
                       orderKind, caseNo, e_sourcecode, memo, unitRate, m_nhikey, e_codate, 
                       mmname_c, mmname_e, wexp_id, e_itemarmyno)
                select :PR_NO, A.MMCODE, :PR_QTY, 
                       (case when 
                             (select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode') ='805'
                                  then a.disc_cprice
                                  else (case when a.m_contid ='0' then a.m_contprice else a.disc_cprice end)
                       end) as pr_price,
                       A.M_PURUN, A.BASE_UNIT, A.M_CONTPRICE, A.M_DISCPERC, 1, A.M_AGENNO, A.CONTRACNO, A.M_CONTID, 
                       B.AGEN_NAMEC, B.AGEN_TEL, B.EMAIL, B.AGEN_FAX, (:PR_QTY*a.disc_cprice ),
                       NVL(A.UnitRate,1), A.DISC_CPRICE, A.DISC_UPRICE, A.UPRICE,
                       --(SELECT ISWILLING FROM MILMED_JBID_LIST WHERE 1=1 AND SUBSTR(A.E_YRARMYNO,1,3)= JBID_STYR AND A.E_ITEMARMYNO=BID_NO), 
                       --(SELECT DISCOUNT_QTY FROM MILMED_JBID_LIST WHERE 1=1 AND SUBSTR(A.E_YRARMYNO,1,3)= JBID_STYR AND A.E_ITEMARMYNO=BID_NO), 
                        A.DISCOUNT_QTY, A.DISC_COST_UPRICE,
                       --(SELECT DISC_COST_UPRICE FROM MILMED_JBID_LIST WHERE 1=1 AND SUBSTR(A.E_YRARMYNO,1,3)= JBID_STYR AND A.E_ITEMARMYNO=BID_NO), 
                       --NVL(C.SAFE_QTY,0), NVL(C.OPER_QTY,0),  安全存量、最大存量
                       sysdate, :CREATE_USER, sysdate, :CREATE_USER, :UPDATE_IP,
                       a.orderKind, a.caseNo, a.e_sourcecode, :memo, a.unitRate, a.m_nhikey, a.e_codate,
                       a.mmname_c, a.mmname_e, a.wexp_id, e_itemarmyno
                 FROM MI_MAST A, PH_VENDER B
                WHERE A.MMCODE = :MMCODE
                  AND B.AGEN_NO = A.M_AGENNO
                  AND NVL(A.E_ORDERDCFLAG,'N')='N'
                    ";
            return DBWork.Connection.Execute(sql, mm_pr01_d, DBWork.Transaction);
        }

        public int DetailUpdate(MM_PR01_D mm_pr01_d)
        {
            string sql = @"UPDATE MM_PR01_D 
                   SET
                    PR_QTY=:PR_QTY,
                    PR_AMT=:PR_AMT,
                    memo = :memo,
                    UPDATE_TIME=sysdate,
                    UPDATE_USER=:UPDATE_USER,
                    UPDATE_IP=:UPDATE_IP
                    WHERE PR_NO=:PR_NO AND MMCODE=:MMCODE   ";

            return DBWork.Connection.Execute(sql, mm_pr01_d, DBWork.Transaction);
        }

        public bool CheckDetailMmcodedExists(string pr_no, string mmcode)
        {
            string sql = @"SELECT 1 FROM MM_PR01_D WHERE PR_NO=:PR_NO AND MMCODE=:MMCODE";
            return DBWork.Connection.ExecuteScalar(sql, new { PR_NO = pr_no, MMCODE = mmcode }, DBWork.Transaction) == null;
        }

        public bool CheckMastMmcodedExists(string mmcode)
        {
            string sql = @"SELECT 1 FROM MI_MAST WHERE MMCODE=:MMCODE and nvl(E_ORDERDCFLAG,'N')='N' ";
            return DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode }, DBWork.Transaction) != null;
        }

        public bool CheckMastAgennoExists(string mmcode)
        {
            string sql = @"SELECT A.M_AGENNO FROM MI_MAST A, PH_VENDER B WHERE A.MMCODE = :MMCODE and A.M_AGENNO = B.AGEN_NO ";

            string agenno = DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode }, DBWork.Transaction) == null ? "": DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode }, DBWork.Transaction).ToString();
            if (agenno == null || agenno == "")
                return false;
            else
                return true;
        }

        public int DetailDelete(string pr_no, string mmcode)
        {
            var sql = @" DELETE from MM_PR01_D WHERE PR_NO = :PR_NO AND MMCODE = :MMCODE";
            return DBWork.Connection.Execute(sql, new { PR_NO = pr_no, MMCODE = mmcode }, DBWork.Transaction);
        }
        public int DetailDeleteAll(string pr_no)
        {
            var sql = @" DELETE from MM_PR01_D WHERE PR_NO = :PR_NO 
                            and (select PR_STATUS from MM_PR01_M
                                where PR_NO=:PR_NO)='35' ";
            return DBWork.Connection.Execute(sql, new { PR_NO = pr_no }, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetMATCombo()
        {
            string sql = @"Select MAT_CLASS||' '||MAT_CLSNAME TEXT, MAT_CLASS VALUE from MI_MATCLASS
                            where MAT_CLSID in ('1')
                            ORDER BY VALUE";
            return DBWork.Connection.Query<ComboItemModel>(sql, null, DBWork.Transaction);
        }

        public IEnumerable<ComboItemModel> GetWh_noCombo()
        {
            var p = new DynamicParameters();


            string sql = @"select wh_no || ' ' || wh_name TEXT ,wh_no VALUE from MI_WHMAST where wh_kind='0' and wh_grade='1' ";

            sql += "order by VALUE ";

            return DBWork.Connection.Query<ComboItemModel>(sql, null, DBWork.Transaction);
        }


        public IEnumerable<MM_PR01_D> GetSelectMmcodeDetail(string MMCODE, string MAT_CLASS)
        {
            var p = new DynamicParameters();

            string sql = @"select MMCODE, MMNAME_C, MMNAME_E, M_AGENNO as AGEN_NO,
                                  (SELECT AGEN_NAMEC FROM PH_VENDER WHERE AGEN_NO = A.M_AGENNO) as AGEN_NAME,
                                  BASE_UNIT, M_CONTPRICE, 
                                  (SELECT ISWILLING FROM MILMED_JBID_LIST WHERE MMCODE=A.MMCODE AND SUBSTR(A.E_YRARMYNO,1,3)= JBID_STYR AND A.E_ITEMARMYNO=BID_NO) as ISWILLING,
                                  (SELECT DISCOUNT_QTY FROM MILMED_JBID_LIST WHERE MMCODE=A.MMCODE AND SUBSTR(A.E_YRARMYNO,1,3)= JBID_STYR AND A.E_ITEMARMYNO=BID_NO) as DISCOUNT_QTY,
                                  (SELECT DISC_COST_UPRICE FROM MILMED_JBID_LIST WHERE MMCODE=A.MMCODE AND SUBSTR(A.E_YRARMYNO,1,3)= JBID_STYR AND A.E_ITEMARMYNO=BID_NO) as DISC_COST_UPRICE, 
                                  M_DISCPERC, DISC_CPRICE, CONTRACNO,
                                  (CASE WHEN M_CONTID = '0' THEN '合約' ELSE '非合約' END) as M_CONTID,
                                  (SELECT SAFE_QTY FROM MI_WINVCTL WHERE WH_NO = WHNO_ME1 AND MMCODE = A.MMCODE) as SAFE_QTY, 
                                  (SELECT OPER_QTY FROM MI_WINVCTL WHERE WH_NO = WHNO_ME1 AND MMCODE = A.MMCODE) as OPER_QTY,
                                  (SELECT SHIP_QTY FROM MI_WINVCTL WHERE WH_NO = WHNO_ME1 AND MMCODE = A.MMCODE) as SHIP_QTY,
                                  (select discount_qty from mi_mast where mmcode=a.mmcode) as discount_qty_2,
                                  get_param('MI_MAST','ORDERKIND', orderkind) as orderkind, caseNo, unitRate as unit_swap,
                                  get_param('MI_MAST','E_SOURCECODE', e_sourcecode) as e_sourcecode, e_codate, e_itemarmyno,
                                   (case when 
                                         (select data_value from PARAM_D where grp_code='HOSP_INFO' and data_name='HospCode') ='805'
                                             then A.disc_cprice
                                              else (case when A.m_contid ='0'  then A.m_contprice else A.disc_cprice end)
                                    end) as pr_price
                             from MI_MAST A
                            where MMCODE = :MMCODE
                              and MAT_CLASS = :MAT_CLASS";

            p.Add(":MMCODE", string.Format("{0}", MMCODE));
            p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));

            return DBWork.Connection.Query<MM_PR01_D>(sql, p, DBWork.Transaction);
        }

       
        public IEnumerable<MI_MAST> GetMmcodeCombo(string MAT_CLASS, string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"select {0} MMCODE, MMNAME_C, MMNAME_E
                             from MI_MAST 
                            where mat_class=:mat_class
                              and nvl(e_orderdcflag, 'N') = 'N'
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

        public MM_PR01_M GetChkPrStatus(string PR_NO)
        {

            var p = new DynamicParameters();

            string sql = @"select PR_STATUS, nvl(ISFROMDOCM,'N') as ISFROMDOCM
                            from MM_PR01_M
                            where PR_NO = :PR_NO
                            ";

            p.Add(":PR_NO", string.Format("{0}", PR_NO));

            return DBWork.Connection.QueryFirst<MM_PR01_M>(sql, p, DBWork.Transaction);
        }

        public bool ChceckPrStatus(string pr_no)
        {
            string sql = @"
                select 1 from MM_PR01_M
                 where PR_NO = :PR_NO
                   and PR_STATUS = '35'
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { PR_NO = pr_no }, DBWork.Transaction) != null;
        }

        public bool CheckExistsPR_NO(string pr_no)
        {
            string sql = @" SELECT 1 FROM MM_PR01_M 
                          WHERE PR_NO = :PR_NO";
            return !(DBWork.Connection.ExecuteScalar(sql, new { PR_NO = pr_no }, DBWork.Transaction) == null);
        }

        public bool CheckExistsMMCODE(string mmcode)
        {
            string sql = @" SELECT 1 FROM MI_MAST 
                          WHERE MMCODE = :MMCODE AND MAT_CLASS = '01' ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode }, DBWork.Transaction) == null);
        }

        public bool CheckFlagMMCODE(string mmcode)
        {
            string sql = @" SELECT nvl(E_ORDERDCFLAG,'N') FROM MI_MAST 
                          WHERE MMCODE = :MMCODE ";
            return DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode }, DBWork.Transaction).ToString() == "N";
        }

        public bool CheckPrExistsMMCODE(string pr_no, string mmcode)
        {
            string sql = @"SELECT 1 FROM MM_PR01_D WHERE PR_NO=:PR_NO AND MMCODE=:MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { PR_NO = pr_no, MMCODE = mmcode }, DBWork.Transaction) == null);
        }

        public bool CheckExistsAGENNO(string mmcode)
        {
            string sql = @" SELECT 1 FROM MI_MAST a, PH_VENDER b 
                          WHERE a.MMCODE = :MMCODE AND a.M_AGENNO = b.AGEN_NO ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = mmcode }, DBWork.Transaction) == null);
        }

        //匯出範本檔
        public DataTable GetExcel(string pr_no)
        {
            var sql = @" SELECT '' as 院內碼,'' as 申購數量, '' as 備註  FROM DUAL ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, new { PR_NO = pr_no }, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<string> FindContracno(string[] pr_no_list)
        {
            var p = new DynamicParameters();

            var sql = @"select distinct contracno from MM_PR01_D where PR_NO in :PR_NO and PR_QTY <> 0";

            p.Add(":PR_NO", pr_no_list);
            
            return DBWork.Connection.Query<string>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<MM_PR01_D> FindContracnoRec(string[] pr_no_list, string v_contracno)
        {
            var p = new DynamicParameters();
            var sql = string.Empty;
            if (string.IsNullOrEmpty(v_contracno))
            {
                sql = @"
                    select distinct AGEN_NO, AGEN_TEL, M_CONTID 
                    from MM_PR01_D 
                    where PR_NO in :PR_NO
                    and CONTRACNO is null
                ";
            }
            else {
                sql = @"select distinct AGEN_NO, AGEN_TEL, M_CONTID 
                    from MM_PR01_D 
                    where PR_NO in :PR_NO
                    and CONTRACNO = :v_contracno";
            }
            
            p.Add(":PR_NO", pr_no_list);
            p.Add(":v_contracno", v_contracno);

            return DBWork.Connection.Query<MM_PR01_D>(sql, p, DBWork.Transaction);
        }

        public string GetPoNo()
        {
            string sql = @" select twn_date(sysdate)|| LPAD(TO_CHAR((count(*)+1)),4,'0') from MM_PO_M where po_no like twn_date(sysdate)||'%' ";

            string po_no = DBWork.Connection.ExecuteScalar(sql, null, DBWork.Transaction) == null ? "" : DBWork.Connection.ExecuteScalar(sql, null, DBWork.Transaction).ToString();
            return po_no;
        }

        public string GetMemo(string agen_no, string m_contid, string v_contracno)
        {
            string sql = @" select getMAILMSG(:AGEN_NO, :M_CONTID, :CONTRACNO) from dual ";

            string memo = DBWork.Connection.ExecuteScalar(sql, new { AGEN_NO = agen_no, M_CONTID = m_contid, CONTRACNO = v_contracno }, DBWork.Transaction) == null 
                ? "" : DBWork.Connection.ExecuteScalar(sql, new { AGEN_NO = agen_no, M_CONTID = m_contid, CONTRACNO = v_contracno }, DBWork.Transaction).ToString();
            return memo;
        }

        public int CreatPoM(string v_po_no, string agen_no, string m_contid, string agen_tel, string v_memo, 
            string username, string userip)
        {
            string sql = @" insert into MM_PO_M ( PO_NO, PO_TIME, PR_DEPT, WH_NO, MAT_CLASS, AGEN_NO, M_CONTID, PO_STATUS, 
                PHONE, MEMO, CREATE_TIME, CREATE_USER,  UPDATE_IP, XACTION)
                VALUES (:V_PO_NO, sysdate, WH_INID(WHNO_ME1), WHNO_ME1, '01', :AGEN_NO, :M_CONTID,'80', 
                :AGEN_TEL, :V_MEMO, sysdate, :USERNAME, :USERIP, '1')";

            return DBWork.Connection.Execute(sql, new
            {
                V_PO_NO = v_po_no,
                AGEN_NO = agen_no,
                M_CONTID = m_contid,
                AGEN_TEL = agen_tel,
                V_MEMO = v_memo,
                USERNAME = username,
                USERIP = userip
            }, DBWork.Transaction);
        }

        public int CreatPoD(string v_po_no, string pr_no_list, string agen_no, string m_contid, 
            string username, string userip)
        {
            string sql = string.Format(@" 
             insert into MM_PO_D ( po_no, mmcode, po_qty, 
                    po_price, 
                    m_purun, po_amt, m_discperc,
                    unit_swap, uprice, disc_cprice, disc_uprice, seq, create_time, create_user, update_ip, storeid, iswilling, 
                    discount_qty, disc_cost_uprice, base_unit, 
                    memo,
                    orderkind, caseno, e_sourcecode, unitrate, m_nhikey,
                    mmname_c, mmname_e, wexp_id, e_codate, m_contprice)
             select po_no, mmcode, po_qty, 
                    (
                        case when (select nvl(DISCOUNT_QTY, 0) from MI_MAST where MMCODE = T.MMCODE) > 0 and T.po_qty >= (select nvl(DISCOUNT_QTY, 0) from MI_MAST where MMCODE = T.MMCODE) then (select nvl(DISC_COST_UPRICE, 0) from MI_MAST where MMCODE = T.MMCODE)
                        else T.po_price end
                    ) as po_price, 
                    m_purun, po_amt, m_discperc,
                    unit_swap, uprice, disc_cprice, disc_uprice, MM_PO_D_SEQ.nextval,
                    create_time, create_user, update_ip, storeid, iswilling, 
                    discount_qty, disc_cost_uprice, base_unit, 
                    memo,
                    orderkind, caseno, e_sourcecode, unitrate, m_nhikey,
                    mmname_c, mmname_e, wexp_id, e_codate, m_contprice
               from ( with temp_memo as (
                           select mmcode, listagg(memo, '、') within group (order by pr_no)  as note 
                             from MM_PR01_D where pr_no in ( {0} ) and memo is not null
                            group by mmcode
                      )
                      select :v_po_no as po_no, mmcode, sum(pr_qty) as po_qty, pr_price as po_price, m_purun, sum(pr_amt) as po_amt, m_discperc, unit_swap, 
                             uprice, disc_cprice, disc_uprice, 
                             sysdate as create_time, :username as create_user, :userip as update_ip, '1' as storeid, iswilling, 
                             discount_qty, disc_cost_uprice, base_unit,
                             (select note from temp_memo where mmcode = a.mmcode) as memo,
                             a.orderkind, a.caseno, a.e_sourcecode, a.unitrate, m_nhikey,
                             a.mmname_c, a.mmname_e, a.wexp_id, a.e_codate, a.m_contprice
                        from mm_pr01_d a
                       where a.pr_no in ( {0} ) and a.agen_no = :agen_no and a.m_contid = :m_contid
                       group by a.mmcode, a.pr_price, a.m_purun, a.m_discperc, a.unit_swap, a.uprice, a.disc_cprice, a.disc_uprice, a.iswilling, 
                             a.discount_qty, a.disc_cost_uprice, a.base_unit,
                             a.orderkind, a.caseno, a.e_sourcecode, a.unitrate, m_nhikey,
                             a.mmname_c, a.mmname_e, a.wexp_id, a.e_codate, a.m_contprice
                    ) T
             "
            , pr_no_list);

            return DBWork.Connection.Execute(sql, new
            {
                V_PO_NO = v_po_no,
                PR_NO_LIST = pr_no_list,
                AGEN_NO = agen_no,
                M_CONTID = m_contid,
                USERNAME = username,
                USERIP = userip
            }, DBWork.Transaction);
        }

        public int UpdatePr01D(string v_po_no, string pr_no_list, string agen_no, string m_contid)
        {
            string sql = string.Format(@" 
                update MM_PR01_D set PR01_PO_NO = :V_PO_NO
                where PR_NO in ( {0} )
                and AGEN_NO = :AGEN_NO 
                and M_CONTID = :M_CONTID 
            "
            , pr_no_list);

            return DBWork.Connection.Execute(sql, new
            {
                V_PO_NO = v_po_no,
                PR_NO_LIST = pr_no_list,
                AGEN_NO = agen_no,
                M_CONTID = m_contid,
            }, DBWork.Transaction);
        }

        public int CreatPhInvoice(string v_po_no, string pr_no_list, string agen_no, string m_contid, 
            string username, string userip)
        {
            string sql = string.Format(@" 
                insert into PH_INVOICE ( PO_NO, MMCODE, TRANSNO, PO_QTY, PO_PRICE, M_PURUN,  PO_AMT, 
                            M_DISCPERC ,UNIT_SWAP, UPRICE, DISC_CPRICE, DISC_UPRICE, CREATE_TIME, CREATE_USER, UPDATE_IP, 
                            DELI_QTY, CKIN_QTY, BW_SQTY)
                select :V_PO_NO, MMCODE, to_char(sysdate,'yyyymmddhh24miss'), sum(PR_QTY), PR_PRICE, M_PURUN, sum(PR_AMT), 
                       M_DISCPERC, UNIT_SWAP, UPRICE, DISC_CPRICE, DISC_UPRICE, sysdate, :USERNAME, :USERIP, 
                       0,0,0
                  from MM_PR01_D
                 where PR_NO in ( {0} ) and AGEN_NO = :AGEN_NO and M_CONTID = :M_CONTID 
                 group by MMCODE, PR_PRICE, M_PURUN, M_DISCPERC, UNIT_SWAP, UPRICE, DISC_CPRICE, DISC_UPRICE "
            , pr_no_list);

            return DBWork.Connection.Execute(sql, new
            {
                V_PO_NO = v_po_no,
                PR_NO_LIST = pr_no_list,
                AGEN_NO = agen_no,
                M_CONTID = m_contid,
                USERNAME = username,
                USERIP = userip
            }, DBWork.Transaction);
        }

        public int CreatPoInrec(string v_po_no, string pr_no_list, string agen_no, string m_contid, 
            string username, string userip)
        {
            string sql = string.Format(@" 
                insert into MM_PO_INREC ( SEQ, WH_NO, PO_NO, MMCODE, PURDATE, AGEN_NO, PO_QTY, PO_PRICE, 
                    STATUS, M_PURUN, PO_AMT, M_DISCPERC, UNIT_SWAP, UPRICE,  DISC_CPRICE, DISC_UPRICE, 
                    ISWILLING, DISCOUNT_QTY, DISC_COST_UPRICE, CREATE_TIME, CREATE_USER, UPDATE_IP)
                select MM_PO_INREC_SEQ.NEXTVAL, WHNO_ME1, :V_PO_NO, MMCODE, twn_date(sysdate), :AGEN_NO, PO_QTY, PR_PRICE, 
                    'N', M_PURUN, PO_AMT, M_DISCPERC, UNIT_SWAP, UPRICE,  DISC_CPRICE, DISC_UPRICE, 
                    ISWILLING, DISCOUNT_QTY, DISC_COST_UPRICE, sysdate, :USERNAME, :USERIP
                from (
                    select MMCODE, sum(PR_QTY) as PO_QTY, PR_PRICE, 
                        M_PURUN, sum(PR_AMT) as PO_AMT, M_DISCPERC, UNIT_SWAP, UPRICE, DISC_CPRICE, DISC_UPRICE, 
                        ISWILLING, DISCOUNT_QTY, DISC_COST_UPRICE
                    from MM_PR01_D
                    where PR_NO IN ( {0} ) and AGEN_NO = :AGEN_NO and M_CONTID = :M_CONTID 
                    group by MMCODE, PR_PRICE, M_PURUN, M_DISCPERC, UNIT_SWAP, UPRICE, DISC_CPRICE, DISC_UPRICE, ISWILLING, 
                    DISCOUNT_QTY, DISC_COST_UPRICE 
                ) "
                , pr_no_list);

            return DBWork.Connection.Execute(sql, new
            {
                V_PO_NO = v_po_no,
                PR_NO_LIST = pr_no_list,
                AGEN_NO = agen_no,
                M_CONTID = m_contid,
                USERNAME = username,
                USERIP = userip
            }, DBWork.Transaction);
        }

        public int UpdateMM_PR01_M_STATUS(string pr_no_list, string username, string userip)
        {
            string sql = string.Format(@"
                update MM_PR01_M 
                   set PR_STATUS = '36', UPDATE_TIME = sysdate, UPDATE_USER = :USERNAME, UPDATE_IP = :USERIP
                 where PR_NO in ( {0} ) "
                , pr_no_list);

            return DBWork.Connection.Execute(sql, new { PR_NO_LIST = pr_no_list, USERNAME = username, USERIP = userip}, DBWork.Transaction);
        }


        public IEnumerable<MM_PR01_D> GetAgaennoMContids(string pr_no_string)
        {
            string sql = string.Format(@"
                select distinct b.agen_no, b.m_contid 
                  from MM_PR01_M a, MM_PR01_D b
                 where a.pr_no in ( {0} )
                   and b.pr_no = a.pr_no
                   and b.pr_qty > 0
                 order by b.agen_no, b.m_contid 
            ", pr_no_string);
            return DBWork.Connection.Query<MM_PR01_D>(sql, DBWork.Transaction);
        }
        public string GetPono()
        {
            var p = new OracleDynamicParameters();
            p.Add("O_PONO", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 12); // 務必要填上size,不然取值都會是null

            DBWork.Connection.Query("GET_PONO", p, commandType: CommandType.StoredProcedure);
            return p.Get<OracleString>("O_PONO").Value;
        }

        public string GetMmPr01MMemos(string pr_no_list, string agen_no, string m_contid)
        {
            string sql = string.Empty;
            if (string.IsNullOrEmpty(m_contid))
            {
                sql = string.Format(@"
                select distinct a.memo
                  from MM_PR01_M a, MM_PR01_D b
                 where a.pr_no in ( {0} )
                   and b.pr_no = a.pr_no
                   and b.agen_no = :agen_no
                   and b.m_contid is null
                   and a.memo is not null
            ", pr_no_list);
            }
            else {
                 sql = string.Format(@"
                select distinct a.memo
                  from MM_PR01_M a, MM_PR01_D b
                 where a.pr_no in ( {0} )
                   and b.pr_no = a.pr_no
                   and b.agen_no = :agen_no
                   and b.m_contid = :m_contid
                   and a.memo is not null
            ", pr_no_list);
            }

            IEnumerable<string> temp_result = DBWork.Connection.Query<string>(sql, new { pr_no_list, agen_no, m_contid }, DBWork.Transaction);

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

        public int GetUnitRate(string mmcode)
        {
            string sql = @"
                select unitRate from MI_MAST where mmcode = :mmcode
            ";
            return DBWork.Connection.QueryFirstOrDefault<int>(sql, new { mmcode }, DBWork.Transaction);
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
                       --  and c.status='P' 
                         and acc_qty <> 0 
                         and b.mmcode in (select mmcode from MM_PR01_D  where pr_no=:pr_no and m_contid='2')
                       group by c.mmcode, disc_cprice, po_price
                    union all  --增加無BC_CS_ACC_LOG
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
                           and b.mmcode in (select mmcode from MM_PR01_D 
                                             where pr_no=:pr_no and m_contid='2')
                         group by b.mmcode, disc_cprice, po_price
                    union all
                        select b.mmcode, disc_cprice, disc_cprice as po_price,
                                sum(pr_qty) as sumqty
                         from MM_PR01_M a, MM_PR01_D b, PH_VENDER p
                         where a.pr_no = b.pr_no and b.agen_no=p.agen_no 
                           and a.pr_no = :pr_no
                           and b.m_contid='2'
                        group by b.mmcode, disc_cprice
                    ) a
                    ) group by mmcode 
                    having sum(paysum) >=(select data_value from PARAM_D where grp_code='M_CONTID2_LIMIT' and data_name='LIMIT')
                    ";

            return DBWork.Connection.Query<MM_PR_D>(strSql, new { pr_no = strPrNo });
        }

        public double GetTotalPrice(List<MM_PR01_D> ori_list)
        {
            double total = 0;
            ori_list.RemoveAll(item => item.PR_QTY == "0");
            if (ori_list.Count() == 0) return 0;

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
                        mmcodeP = "'" + item.MMCODE + "'";
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

    }
}