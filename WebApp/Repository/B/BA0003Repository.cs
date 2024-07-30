using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Data;
using System;
using System.Data.SqlClient;

namespace WebApp.Repository.B
{
    public class BA0003Repository : JCLib.Mvc.BaseRepository
    {
        public BA0003Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<MM_PR_M> GetMST(string MAT_CLASS, string PR_TIME_B, string PR_TIME_E, string M_STOREID, string PR_DEPT, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            string sql = @"Select * from mmsadm.MM_PR_M 
                            where PR_DEPT= :PR_DEPT and XACTION = '0' ";

            p.Add(":PR_DEPT", string.Format("{0}", PR_DEPT));

            if (!string.IsNullOrWhiteSpace(MAT_CLASS))
            {
                sql += " AND SUBSTR(PR_NO,20,2) = :MAT_CLASS ";
                p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
            }
            if (!string.IsNullOrWhiteSpace(PR_TIME_B))
            {
                sql += " AND substr(PR_NO,7,7) >= TWN_DATE(TO_DATE(:PR_TIME_B,'YYYY/mm/dd')) ";
                p.Add(":PR_TIME_B", string.Format("{0}", DateTime.Parse(PR_TIME_B).ToString("yyyy/MM/dd")));
            }

            if (!string.IsNullOrWhiteSpace(PR_TIME_E))
            {
                sql += " AND substr(PR_NO,7,7) <= TWN_DATE(TO_DATE(:PR_TIME_E,'YYYY/mm/dd')) ";
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

            string sql = @"select D.mmcode, nvl(D.M_CONTPRICE,0) as M_CONTPRICE, D.unit_swap, PR_PRICE, D.agen_no, D.agen_name, D.agen_fax ,  D.PR_NO, D.PR_QTY, D.REQ_QTY_T,
                            A.mmname_c, A.mmname_e,A.M_CONTID,A.M_STOREID, A.BASE_UNIT, A.M_PURUN , floor(nvl(D.M_CONTPRICE,0)*D.REQ_QTY_T) as TOTAL_PRICE, 0 as  TOT,
                            D.DISC, (select inv_qty from MI_WHINV where wh_no=:WH_NO and mmcode=d.mmcode and ROWNUM <= 1) inv_qty, nvl(D.SRC_PR_QTY,0) as SRC_PR_QTY,
                            (D.PR_QTY-nvl(D.SRC_PR_QTY,0)) as DIFF_PR_QTY 
                            from MM_PR_M M, MM_PR_D D , MI_MAST A
                            where  M.PR_NO=D.PR_NO and D.mmcode=A.mmcode 
                            and M.pr_no= : PR_NO
                            ";

            p.Add(":PR_NO", string.Format("{0}", PR_NO));
            p.Add(":WH_NO", string.Format("{0}", WH_NO));
            if (!string.IsNullOrWhiteSpace(MMCODE))
            {
                sql += " AND A.mmcode like :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", MMCODE));
            }

            sql += " order by A.mmcode ";

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

            Dictionary<string, int> TXTDAY = new Dictionary<string, int>();
            Dictionary<string, string> MAT = new Dictionary<string, string>();
            foreach (ComboItemModel data in GetTXTDAY_ED())
            {
                TXTDAY.Add(data.TEXT, int.Parse(data.VALUE));
            }
            foreach (ComboItemModel data in GetMATCombo())
            {
                MAT.Add(data.VALUE, data.TEXT);
            }

            sql = @" select * from mmsadm.MM_PR_M where SUBSTR(PR_NO, 20, 2) = :MAT_CLASS
                    and substr(PR_NO,7,7) >= TWN_DATE(sysdate)
                    and substr(PR_NO,7,7) <= TWN_DATE(sysdate)
                    and PR_DEPT = :PR_DEPT
                    and M_STOREID = :M_STOREID
                    and XACTION = '0' ";

            if ((DBWork.Connection.ExecuteScalar(sql, mm_pr_m, DBWork.Transaction)) == null)
            {
                //v_pr_no = {user inid} +TO_CHAR(TO_NUMBER(TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'))-19110000000000)+ {物料類別}
                string v_pr_no = ":PR_DEPT||''||TO_CHAR(TO_NUMBER(TO_CHAR(SYSDATE, 'YYYYMMDDHH24MISS')) - 19110000000000)||''||:MAT_CLASS";
                sql = @" insert into MM_PR_M(PR_NO         , PR_DEPT, PR_TIME,  PR_USER , MAT_CLASS, M_STOREID, PR_STATUS, CREATE_TIME, CREATE_USER, UPDATE_IP,XACTION) 
                                                values(" + v_pr_no + ", :PR_DEPT, sysdate, :PR_USER,:MAT_CLASS,:M_STOREID, '35'     , sysdate,    :CREATE_USER, :UPDATE_IP,'0')";

                DBWork.Connection.Execute(sql, mm_pr_m, DBWork.Transaction);
                //提示 { { 物料類別} +類申購單產生成功}
                return MAT[mm_pr_m.MAT_CLASS] + "類申購單產生成功";
            }
            else
            {
                //提示 { 本月份此類申購單已存在}
                return "本月份此類申購單已存在";
            }
        }


        public int MasterDelete(string PR_NO)
        {
            var sql = @" Delete from MM_PR_M  where PR_NO=:PR_NO";
            return DBWork.Connection.Execute(sql, new { PR_NO = PR_NO }, DBWork.Transaction);
        }
        public int MasterUpdate(MM_PR_M mm_pr_m)
        {
            var sql = @"update MM_PR_M
                         SET MAT_CLASS =:MAT_CLASS,M_STOREID=:M_STOREID,UPDATE_USER=:UPDATE_USER,UPDATE_IP=:UPDATE_IP 
                        Where   PR_NO=:PR_NO ";
            return DBWork.Connection.Execute(sql, mm_pr_m, DBWork.Transaction);
        }


        public int DetailCreate(MM_PR_D mm_pr_d)
        {
            string sql = @"INSERT INTO mm_pr_d ( pr_no, mmcode, pr_qty, pr_price,  m_purun,
                           m_contprice,  unit_swap, req_qty_t,  agen_no,  disc, m_contid,  create_time,
                           create_user,  update_ip,  agen_name,  agen_fax)
                     VALUES (:PR_NO, :MMCODE, :PR_QTY,  :PR_PRICE,   :M_PURUN,
                          :M_CONTPRICE, :UNIT_SWAP,  (:PR_QTY / nvl(:UNIT_SWAP, 1)),  :AGEN_NO, :DISC, :M_CONTID, SYSDATE,
                          :CREATE_USER, :UPDATE_IP, :AGEN_NAMEC, :AGEN_FAX)";
            return DBWork.Connection.Execute(sql, mm_pr_d, DBWork.Transaction);
        }

        public int DetailUpdate(MM_PR_D mm_pr_d)
        {
            string sql = @"UPDATE mm_pr_d 
                   SET
                    PR_QTY=:PR_QTY,
                    REQ_QTY_T=:REQ_QTY_T     
                    WHERE PR_NO=:PR_NO AND MMCODE=:MMCODE   ";

            return DBWork.Connection.Execute(sql, mm_pr_d, DBWork.Transaction);
        }
        
        public bool CheckDetailMmcodedExists(string pr_no, string mmcode)
        {
            string sql = @"SELECT 1 FROM MM_PR_D WHERE PR_NO=:PR_NO AND MMCODE=:MMCODE";
            return DBWork.Connection.ExecuteScalar(sql, new { PR_NO = pr_no, MMCODE = mmcode }, DBWork.Transaction) == null;
        }

        public int DetailDelete(string pr_no, string mmcode)
        {
            var sql = @" DELETE from MM_PR_D WHERE PR_NO = :PR_NO AND MMCODE = :MMCODE";
            return DBWork.Connection.Execute(sql, new { PR_NO = pr_no, MMCODE = mmcode }, DBWork.Transaction);
        }
        public int DetailDeleteAll(string pr_no)
        {
            var sql = @" DELETE from MM_PR_D WHERE PR_NO = :PR_NO ";
            return DBWork.Connection.Execute(sql, new { PR_NO = pr_no }, DBWork.Transaction);
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


        public IEnumerable<MM_PR_D> GetSelectMmcodeDetail(string MMCODE, string MAT_CLASS, string WH_NO, string M_STOREID)
        {
            var p = new DynamicParameters();

            string sql = @"select M.mmcode, M.mat_class,M.M_STOREID, M.mmname_c, M.mmname_e, 0 as  TOTAL_PRICE, 0 as  TOT,
                            V.agen_no, V.agen_namec, V.agen_fax, M.M_DISCPERC, (select nvl(inv_qty,0) inv_qty from MI_WHINV where wh_no=:WH_NO and mmcode=m.mmcode and ROWNUM <= 1) inv_qty , 
                            M.M_CONTID, '' as PR_PRICE, M.base_unit,M.M_PURUN, M.UPRICE,  
                            (select nvl(EXCH_RATIO,1) EXCH_RATIO from MI_UNITEXCH where unit_code =M.M_PURUN and agen_no=M.m_agenno and mmcode = :MMCODE) as unit_swap , nvl(M.M_CONTPRICE,0) as M_CONTPRICE
                            from MI_MAST M, PH_VENDER V
                            where M.m_agenno = V.agen_no 
                            and M.mmcode = :MMCODE
                            and M.mat_class = :MAT_CLASS  and M.M_CONTID<>'3' 
                            and M.m_storeid = :M_STOREID";

            p.Add(":MMCODE", string.Format("{0}", MMCODE));
            p.Add(":MAT_CLASS", string.Format("{0}", MAT_CLASS));
            p.Add(":WH_NO", string.Format("{0}", WH_NO));
            p.Add(":M_STOREID", string.Format("{0}", M_STOREID));

            return DBWork.Connection.Query<MM_PR_D>(sql, p, DBWork.Transaction);
        }


        public string GetTot(string wh_no, string mat_class, string mmcode, string totprice)
        {
            string sql = @"select sum( floor(APL_INQTY*b.uprice))
                            from MI_WHINV a, MI_MAST b 
                            where 
                            a.wh_no=:wh_no
                            and
                            a.mmcode=b.mmcode and a.mmcode=:mmcode and b.m_contid='2' and b.mat_class=:mat_class";
            var str = DBWork.Connection.ExecuteScalar(sql, new { wh_no = wh_no, mat_class = mat_class, mmcode = mmcode }, DBWork.Transaction);
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
        public IEnumerable<MI_MAST> GetMmcodeCombo(string MAT_CLASS, string M_STOREID, string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"select {0} MMCODE, MMNAME_C, MMNAME_E
                             from MI_MAST 
                            where mat_class=:mat_class and M_CONTID<>'3' 
                              and m_storeid = :m_storeid
                        ";
            p.Add(":mat_class", MAT_CLASS);
            p.Add(":m_storeid", M_STOREID);
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

    }
}