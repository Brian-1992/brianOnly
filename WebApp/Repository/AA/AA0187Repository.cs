using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using JCLib.Mvc;

namespace WebApp.Repository.AA
{
    public class AA0187Repository : JCLib.Mvc.BaseRepository
    {
        public AA0187Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        private string GetSql(ref DynamicParameters p,
                              string wh_no, string mmcode, string warbak, string mat_class,
                              string m_contid, string e_sourcecode, string e_restrictcode,
                              string common, string spdrug, string fastdrug, string isiv,
                              string orderkind, string spxfee, string drugkind, string agen_no,
                              string agen_name, string touchcase,
                              string qtyiszero, string qtyisnotzero, string qtyischanged, string qtyisnotchanged,
                              bool isAA, string userId)
        {
            string sql = string.Format(@"
                select a.wh_no, a.mmcode, b.mmname_c ||' '||b.mmname_e as mmname,
                       a.inv_qty, pmn_invqty(cur_setym, a.wh_no, a.mmcode) as P_INVQTY,
                       a.apl_inqty, a.apl_outqty, a.trn_inqty, a.trn_outqty,a.adj_inqty,a.adj_outqty,
                       a.bak_inqty, a.bak_outqty, a.rej_outqty, a.dis_outqty,
                       a.exg_inqty, a.exg_outqty, a.inventoryqty,a.use_qty,
                       b.unitrate, b.trutrate,
                       b.base_unit, c.easyname, b.m_agenno as agen_no,
                       get_param('MI_MAST','E_RESTRICTCODE',b.e_restrictcode) as e_restrictcode,
                       get_param('MI_MAST','E_SOURCECODE',b.e_sourcecode) as e_sourcecode,
                       trim(b.caseno) as caseno,
                       b.disc_cprice, nvl(b.disc_cprice * a.inv_qty, 0) as amount
                  from MI_WHINV a, MI_MAST b
                  left join PH_VENDER c on (c.agen_no = b.m_agenno)
                 where 1=1
                   and a.mmcode = b.mmcode
                   and a.wh_no = nvl(trim(:wh_no), a.wh_no)
                   and a.mmcode = nvl(trim(:mmcode), a.mmcode)
                   and b.warbak = nvl(trim(:warbak), b.warbak) 
                   and b.m_contid = nvl(trim(:m_contid), b.m_contid) 
                   and b.e_sourcecode = nvl(trim(:e_sourcecode), b.e_sourcecode) 
                   and b.e_restrictcode = nvl(trim(:e_restrictcode), b.e_restrictcode) 
                   and b.common = nvl(trim(:common), b.common) 
                   and b.spdrug = nvl(trim(:spdrug), b.spdrug) 
                   and b.fastdrug = nvl(trim(:fastdrug), b.fastdrug) 
                   and b.isiv = nvl(trim(:isiv), b.isiv)
                   and b.orderkind = nvl(trim(:orderkind), b.orderkind)
                   and b.spxfee = nvl(trim(:spxfee), b.spxfee)
                   and b.drugkind = nvl(trim(:drugkind), b.drugkind)
                   and nvl(b.m_agenno, 'N') = nvl(nvl(trim(:agen_no), b.m_agenno),'N')
                   and b.touchcase = nvl(trim(:touchcase), b.touchcase)
                   and nvl(b.cancel_id, 'N') = 'N'
            ");
            if (string.IsNullOrEmpty(mat_class) == false)
            {
                if (mat_class == "A" || mat_class == "B")
                {
                    sql += @"  and b.mat_class=nvl(DECODE(:mat_class,'A','01','B','02'), b.mat_class)";
                }
                else
                {
                    sql += @"  and b.mat_class_sub=nvl(:mat_class, b.mat_class_sub)";
                }
            }
            if (isAA == false)
            {
                sql += @"
                    and a.wh_no in (select wh_no from MI_WHID where wh_userid=:userId )
                ";
            }
            if (!string.IsNullOrEmpty(agen_name))
            {
                sql += @" AND c.agen_namec LIKE CONCAT(CONCAT('%', :agen_name),'%')";
                p.Add(":agen_name", agen_name);
            }
            if (bool.Parse(qtyiszero))
            {
                sql += @" AND a.inv_qty = '0' ";
            }
            if (bool.Parse(qtyisnotzero))
            {
                sql += @" AND a.inv_qty <> '0' ";
            }
            if (bool.Parse(qtyischanged))
            {
                sql += @"
                    and not (
                        a.apl_inqty = '0' and
                        a.apl_outqty = '0' and
                        a.trn_inqty = '0' and
                        a.trn_outqty = '0' and
                        a.adj_inqty = '0' and
                        a.adj_outqty = '0'  and
                        a.bak_inqty = '0' and
                        a.bak_outqty = '0' and
                        a.rej_outqty = '0' and
                        a.dis_outqty = '0' and
                        a.exg_inqty = '0' and
                        a.exg_outqty = '0' and
                        a.inventoryqty = '0' and
                        a.use_qty = '0'
                    )
                ";
            }
            if (bool.Parse(qtyisnotchanged))
            {
                sql += @"
                   and (
                        a.apl_inqty = '0' and
                        a.apl_outqty = '0' and
                        a.trn_inqty = '0' and
                        a.trn_outqty = '0' and
                        a.adj_inqty = '0' and
                        a.adj_outqty = '0'  and
                        a.bak_inqty = '0' and
                        a.bak_outqty = '0' and
                        a.rej_outqty = '0' and
                        a.dis_outqty = '0' and
                        a.exg_inqty = '0' and
                        a.exg_outqty = '0' and
                        a.inventoryqty = '0' and
                        a.use_qty = '0'
                    )
                ";
            }

            return sql;
        }

        public IEnumerable<AA0187> GetAll(string wh_no, string mmcode, string warbak, string mat_class,
                                          string m_contid, string e_sourcecode, string e_restrictcode,
                                          string common, string spdrug, string fastdrug, string isiv,
                                          string orderkind, string spxfee, string drugkind, string agen_no,
                                          string agen_name, string touchcase,
                                          string qtyiszero, string qtyisnotzero, string qtyischanged, string qtyisnotchanged,
                                          bool isAA, string userId)
        {
            var p = new DynamicParameters();

            string sql = GetSql(ref p, wh_no, mmcode, warbak, mat_class,
                                m_contid, e_sourcecode, e_restrictcode,
                                common, spdrug, fastdrug, isiv,
                                orderkind, spxfee, drugkind, agen_no,
                                agen_name, touchcase,
                                qtyiszero, qtyisnotzero, qtyischanged, qtyisnotchanged,
                                isAA, userId);

            p.Add(":wh_no", wh_no);
            p.Add(":mmcode", mmcode);
            p.Add(":warbak", warbak);
            p.Add(":mat_class", mat_class);
            p.Add(":m_contid", m_contid);
            p.Add(":e_sourcecode", e_sourcecode);
            p.Add(":e_restrictcode", e_restrictcode);
            p.Add(":common", common);
            p.Add(":spdrug", spdrug);
            p.Add(":fastdrug", fastdrug);
            p.Add(":isiv", isiv);
            p.Add(":orderkind", orderkind);
            p.Add(":spxfee", spxfee);
            p.Add(":drugkind", drugkind);
            p.Add(":agen_no", agen_no);
            //p.Add(":agen_name", agen_name);
            p.Add(":touchcase", touchcase);
            p.Add(":isAA", isAA);
            p.Add(":userId", userId);

            return DBWork.PagingQuery<AA0187>(sql, p, DBWork.Transaction);
        }

        public DataTable GetExcel(string wh_no, string mmcode, string warbak, string mat_class,
                                          string m_contid, string e_sourcecode, string e_restrictcode,
                                          string common, string spdrug, string fastdrug, string isiv,
                                          string orderkind, string spxfee, string drugkind, string agen_no,
                                          string agen_name, string touchcase,
                                          string qtyiszero, string qtyisnotzero, string qtyischanged, string qtyisnotchanged,
                                          bool isAA, string userId)
        {
            var p = new DynamicParameters();

            string sql = GetSql(ref p, wh_no, mmcode, warbak, mat_class,
                                m_contid, e_sourcecode, e_restrictcode,
                                common, spdrug, fastdrug, isiv,
                                orderkind, spxfee, drugkind, agen_no,
                                agen_name, touchcase,
                                qtyiszero, qtyisnotzero, qtyischanged, qtyisnotchanged,
                                isAA, userId);

            sql = string.Format(@"
                select wh_no as 庫房代碼, mmcode as 藥材代碼, mmname as 藥材名稱,
                       agen_no as 廠商代碼,inv_qty as 存量, p_invqty as 上月結,
                       apl_inqty as ""進貨/撥發入"", apl_outqty as 撥發出, trn_inqty as 調撥入, trn_outqty as 調撥出,
                       adj_inqty as 調帳入, adj_outqty as 調帳出,
                       bak_inqty as 退料入, bak_outqty as 退料出, rej_outqty as 退貨量, dis_outqty as 報廢量,
                       exg_inqty as 換貨入, exg_outqty as 換貨出, use_qty as 消耗量, inventoryqty as 盤點差異量,  unitrate as 出貨量, 
                       trutrate as 轉換量比, disc_cprice as 庫存成本, base_unit as 單位, easyname as 廠商簡稱,
                       e_restrictcode as 管制品, e_sourcecode as 買斷寄庫, caseno as 案號, amount as 庫存成本
                from (
                    {0}
                ) ORDER BY wh_no ASC, mmcode ASC
            ", sql);

            p.Add(":wh_no", wh_no);
            p.Add(":mmcode", mmcode);
            p.Add(":warbak", warbak);
            p.Add(":mat_class", mat_class);
            p.Add(":m_contid", m_contid);
            p.Add(":e_sourcecode", e_sourcecode);
            p.Add(":e_restrictcode", e_restrictcode);
            p.Add(":common", common);
            p.Add(":spdrug", spdrug);
            p.Add(":fastdrug", fastdrug);
            p.Add(":isiv", isiv);
            p.Add(":orderkind", orderkind);
            p.Add(":spxfee", spxfee);
            p.Add(":drugkind", drugkind);
            p.Add(":agen_no", agen_no);
            p.Add(":agen_name", agen_name);
            p.Add(":touchcase", touchcase);
            p.Add(":isAA", isAA);
            p.Add(":userId", userId);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<AA0187> GetPrint(string p1, string p2, string p3, string p4, string p5, string p6)
        {

            var p = new DynamicParameters();
            string sql = @"select * from (
                        select b.m_agenno, a.mmcode, b.mmname_e || ' '|| b.mmname_c mmname, b.unitrate, a.inv_qty,
                        (select listagg(store_loc, ', ') within group (order by store_loc) from MI_WLOCINV where wh_no = a.wh_no and mmcode = a.mmcode) store_loc
                        from MI_WHINV a, MI_MAST b
                        where a.wh_no in (select wh_no from MI_WHMAST where wh_grade='1')
                        and a.mmcode = b.mmcode
                        ";

            if (!string.IsNullOrEmpty(p1))
            {
                if (p1.Trim() != "")
                {
                    sql += string.Format(@"
                        and (b.mat_class = :mat_class or b.mat_class_sub = :mat_class) ");
                }
            }
            if (!string.IsNullOrEmpty(p2))
            {
                sql += string.Format(@"
                    and b.m_agenno >= :m_agenno_start ");
            }
            if (!string.IsNullOrEmpty(p3))
            {
                sql += string.Format(@"
                    and b.m_agenno <= :m_agenno_end ");
            }
            if (p6 == "true")
            {
                sql += string.Format(@"
                    and a.inv_Qty <> 0 ");
            }
            sql += " ) d ";
            if (!string.IsNullOrEmpty(p4))
            {
                switch (p4)
                {
                    case "mmname":
                        sql += string.Format(@" order by mmname ");
                        break;
                    case "mmcode":
                        sql += string.Format(@" order by mmcode ");
                        break;
                    case "store_loc":
                        sql += string.Format(@" order by store_loc ");
                        break;
                    case "m_agenno":
                        sql += string.Format(@" order by m_agenno ");
                        break;
                    default:
                        break;
                }
                if (!string.IsNullOrEmpty(p5))
                {
                    if (p5 == "DESC")
                    {
                        sql += string.Format(@" DESC ");
                    }
                }
            }

            p.Add(":mat_class", p1);
            p.Add(":m_agenno_start", p2);
            p.Add(":m_agenno_end", p3);

            return DBWork.Connection.Query<AA0187>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetWhnoCombo(bool isAA, string userId)
        {
            string sql = string.Empty;
            if (isAA)
            {
                sql = @"
                    select wh_no as value,wh_no||' '||wh_name as text 
                      from MI_WHMAST 
                     where nvl(cancel_id, 'N')='N'
                     order by wh_grade, wh_no
                ";
            }
            else
            {
                sql = @"
                    select a.wh_no as value, a.wh_no||' '||a.wh_name as text 
                      from MI_WHMAST a, MI_WHID b
                     where nvl(a.cancel_id, 'N')='N'
                       and a.wh_no = b.wh_no
                       and b.wh_userid=:userId
                     order by a.wh_grade, a.wh_no
                ";
            }
            return DBWork.Connection.Query<COMBO_MODEL>(sql, new { isAA, userId }, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetMatClassSubParamCombo()
        {
            var p = new DynamicParameters();
            string sql = @"SELECT ROWNUM+2 AS ORDERNO, DATA_VALUE AS VALUE, DATA_DESC AS TEXT 
                           FROM PARAM_D 
                           WHERE GRP_CODE = 'MI_MAST' AND DATA_NAME = 'MAT_CLASS_SUB'
                           UNION
                           SELECT 0 AS ORDERNO, ' ' AS VALUE , '全部' AS TEXT FROM DUAL
                           UNION
                           SELECT 1 AS ORDERNO,'A' AS VALUE , '全部藥品' AS TEXT FROM DUAL
                           UNION
                           SELECT 2 AS ORDERNO,'B' AS VALUE , '全部衛材' AS TEXT FROM DUAL ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //藥材代碼
        public IEnumerable<COMBO_MODEL> GetMiMastCombo()
        {
            var p = new DynamicParameters();
            string sql = @"SELECT MMCODE VALUE, MMNAME_C TEXT, MMNAME_E 
                           FROM MI_MAST ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //是否合約
        public IEnumerable<COMBO_MODEL> GetContidParamCombo()
        {
            var p = new DynamicParameters();
            string sql = @"SELECT DATA_VALUE AS VALUE, DATA_DESC AS TEXT 
                           FROM PARAM_D 
                           WHERE GRP_CODE = 'MI_MAST' 
                           AND DATA_NAME = 'M_CONTID'
                           UNION
                           SELECT ' ' AS VALUE , '全部' AS TEXT 
                           FROM DUAL ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //買斷寄庫
        public IEnumerable<COMBO_MODEL> GetSourcecodeParamCombo()
        {
            var p = new DynamicParameters();
            string sql = @"SELECT DATA_VALUE AS VALUE, DATA_DESC AS TEXT
                           FROM PARAM_D 
                           WHERE GRP_CODE = 'MI_MAST' 
                           AND DATA_NAME = 'E_SOURCECODE'
                           UNION
                           SELECT ' ' AS VALUE , '全部' AS TEXT
                           FROM DUAL ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //是否戰備
        public IEnumerable<COMBO_MODEL> GetWarbakParamCombo()
        {
            var p = new DynamicParameters();
            string sql = @"SELECT DATA_VALUE AS VALUE, DATA_DESC AS TEXT
                           FROM PARAM_D 
                           WHERE GRP_CODE = 'MI_MAST' 
                           AND DATA_NAME = 'WARBAK'
                           UNION
                           SELECT ' ' AS VALUE , '全部' AS TEXT
                           FROM DUAL ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //管制品項
        public IEnumerable<COMBO_MODEL> GetRestrictcodeParamCombo()
        {
            var p = new DynamicParameters();
            string sql = @"SELECT DATA_VALUE AS VALUE, DATA_DESC AS TEXT
                           FROM PARAM_D 
                           WHERE GRP_CODE = 'MI_MAST' 
                           AND DATA_NAME = 'E_RESTRICTCODE'
                           UNION
                           SELECT ' ' AS VALUE , '全部' AS TEXT
                           FROM DUAL ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //是否常用品項
        public IEnumerable<COMBO_MODEL> GetCommonParamCombo()
        {
            var p = new DynamicParameters();
            string sql = @"SELECT DATA_VALUE AS VALUE, DATA_DESC AS TEXT
                           FROM PARAM_D 
                           WHERE GRP_CODE = 'MI_MAST' 
                           AND DATA_NAME = 'COMMON'
                           UNION
                           SELECT ' ' AS VALUE , '全部' AS TEXT
                           FROM DUAL ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //急救品項
        public IEnumerable<COMBO_MODEL> GetFastdrugParamCombo()
        {
            var p = new DynamicParameters();
            string sql = @"SELECT DATA_VALUE AS VALUE, DATA_DESC AS TEXT
                           FROM PARAM_D 
                           WHERE GRP_CODE = 'MI_MAST' 
                           AND DATA_NAME = 'FASTDRUG'
                           UNION
                           SELECT ' ' AS VALUE , '全部' AS TEXT
                           FROM DUAL ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //中西藥別
        public IEnumerable<COMBO_MODEL> GetDrugkindParamCombo()
        {
            var p = new DynamicParameters();
            string sql = @"SELECT DATA_VALUE AS VALUE, DATA_DESC AS TEXT
                           FROM PARAM_D 
                           WHERE GRP_CODE = 'MI_MAST' 
                           AND DATA_NAME = 'DRUGKIND'
                           UNION
                           SELECT ' ' AS VALUE , '全部' AS TEXT
                           FROM DUAL ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //合約類別
        public IEnumerable<COMBO_MODEL> GetTouchcaseParamCombo()
        {
            var p = new DynamicParameters();
            string sql = @"SELECT DATA_VALUE AS VALUE, DATA_DESC AS TEXT
                           FROM PARAM_D 
                           WHERE GRP_CODE = 'MI_MAST' 
                           AND DATA_NAME = 'TOUCHCASE'
                           UNION
                           SELECT ' ' AS VALUE , '全部' AS TEXT
                           FROM DUAL ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //採購類別
        public IEnumerable<COMBO_MODEL> GetOrderkindParamCombo()
        {
            var p = new DynamicParameters();
            string sql = @"SELECT DATA_VALUE AS VALUE, DATA_DESC AS TEXT
                           FROM PARAM_D 
                           WHERE GRP_CODE = 'MI_MAST' 
                           AND DATA_NAME = 'ORDERKIND'
                           UNION
                           SELECT ' ' AS VALUE , '全部' AS TEXT
                           FROM DUAL ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //特殊品項
        public IEnumerable<COMBO_MODEL> GetSpecialorderkindParamCombo()
        {
            var p = new DynamicParameters();
            string sql = @"SELECT DATA_VALUE AS VALUE, DATA_DESC AS TEXT
                           FROM PARAM_D 
                           WHERE GRP_CODE = 'MI_MAST' 
                           AND DATA_NAME = 'SPDRUG'
                           UNION
                           SELECT ' ' AS VALUE , '全部' AS TEXT
                           FROM DUAL ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        //是否特材
        public IEnumerable<COMBO_MODEL> GetSpxfeeParamCombo()
        {
            var p = new DynamicParameters();
            string sql = @"SELECT DATA_VALUE AS VALUE, DATA_DESC AS TEXT
                           FROM PARAM_D 
                           WHERE GRP_CODE = 'MI_MAST' 
                           AND DATA_NAME = 'SPXFEE'
                           UNION
                           SELECT ' ' AS VALUE , '全部' AS TEXT
                           FROM DUAL ";


            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        public IEnumerable<COMBO_MODEL> GetMatClassCombo()
        {
            var p = new DynamicParameters();
            string sql = @"SELECT ROWNUM+2 AS ORDERNO, DATA_VALUE AS VALUE, DATA_DESC AS TEXT FROM PARAM_D 
                                        WHERE GRP_CODE = 'MI_MAST' AND DATA_NAME = 'MAT_CLASS_SUB'
                                        UNION
                                        SELECT 0 AS ORDERNO, ' ' AS VALUE , '全部' AS TEXT FROM DUAL
                                        UNION
                                        SELECT 1 AS ORDERNO,'01' AS VALUE , '全部藥品' AS TEXT FROM DUAL
                                        UNION
                                        SELECT 2 AS ORDERNO,'02' AS VALUE , '全部衛材' AS TEXT FROM DUAL
                                         ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
        public IEnumerable<COMBO_MODEL> GetAgennoCombo()
        {
            string sql = @"
                select agen_No as value, agen_namec as text
                  from PH_VENDER
                UNION
                           SELECT ' ' AS VALUE , '全部' AS TEXT
                           FROM DUAL 
            ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetIsivCombo()
        {
            string sql = @"
                SELECT DATA_VALUE AS VALUE, DATA_DESC AS TEXT 
                FROM PARAM_D 
                WHERE GRP_CODE = 'Y_OR_N' 
                AND DATA_NAME = 'YN'
                UNION
                SELECT ' ' AS VALUE , '全部' AS TEXT 
                FROM DUAL
            ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }
        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"
                        SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E
                          FROM MI_MAST A 
                         WHERE 1=1 
                           AND nvl(A.CANCEL_ID, 'N') <> 'Y' ";
            sql += " {1} ";
            if (p0 != "")
            {
                sql = string.Format(sql,
                     "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(MMNAME_E), UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,",
                     @"   AND (UPPER(A.MMCODE) LIKE UPPER(:MMCODE) OR UPPER(MMNAME_E) LIKE UPPER(:MMNAME_E) OR UPPER(MMNAME_C) LIKE UPPER(:MMNAME_C))");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);
                p.Add(":MMCODE", string.Format("{0}%", p0));
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, mmcode", sql);
            }
            else
            {
                sql = string.Format(sql, "", "");
                sql += " ORDER BY MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<string> DefaultWhNo(string menuCode)
        {
            string sql = "";
            if (menuCode == "AA0212")
            {
                sql = @"SELECT WHNO_ME1 FROM DUAL";
            }
            else if (menuCode == "FA0083")
            {
                sql = @"SELECT WHNO_MM1 FROM DUAL";
            }
            return DBWork.Connection.Query<string>(sql, DBWork.Transaction);
        }
    }

}