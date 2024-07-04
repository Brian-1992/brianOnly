using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using System.Text;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WebApp.Repository.AA
{
    public class AA0044Repository : JCLib.Mvc.BaseRepository
    {
        public AA0044Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0044M> GetAllMN(string userId, string set_ym, string wh_kind, string wh_grade, string wh_no, string mat_class, string m_storeid, string mmcode,string menuLink, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"
                select a.wh_no, a.mmcode,
                       pmn_invqty(:set_ym,a.wh_no,a.mmcode) inv_qty_l,
                       a.inv_qty as inv_qty_n,
                       a.apl_inqty apl_inqty_n,
                       a.apl_outqty apl_outqty_n,
                       a.trn_inqty trn_inqty_n,
                       a.trn_outqty trn_outqty_n,
                       a.adj_inqty adj_inqty_n,
                       a.adj_outqty adj_outqty_n,
                       a.bak_inqty bak_inqty_n,
                       a.bak_outqty bak_outqty_n,
                       a.exg_inqty exg_inqty_n,
                       a.exg_outqty exg_outqty_n,
                       a.mil_inqty mil_inqty_n,
                       a.mil_outqty mil_outqty_n,
                       a.onway_qty onway_qty_n,
                       a.rej_outqty rej_outqty_n,
                       a.dis_outqty dis_outqty_n,
                       (case when inventoryqty > 0 then a.inventoryqty else 0 end) inventoryqty1_n,
                       (case when inventoryqty < 0 then a.inventoryqty else 0 end) inventoryqty2_n,
                       a.use_qty use_qty_n
                  from MI_WHINV a, MI_WHMAST c, MI_MAST d
                 where a.wh_no = c.wh_no
                   and a.mmcode = d.mmcode
            ";
            if (wh_kind != "")
            {
                sql += " and c.wh_kind = :p1 ";

            }
            if (wh_grade != "")
            {
                sql += " and c.wh_grade = :p2 ";

            }
            if (wh_no != "")
            {
                sql += " and a.wh_no = :p3 ";

            }
            else
            {
                if (menuLink == "AB0035")
                {
                    sql += " and a.wh_no in";
                    sql += " ( select e.wh_no from MI_WHMAST e,MI_WHID f ";
                    sql += " where e.wh_no=f.wh_no  and e.wh_grade='2' and wh_userid=:wh_userid ";
                    sql += " union ";
                    sql += " select wh_no from mi_whmast e ";
                    sql += " where  e.wh_grade='2' and exists(select 'x' from ur_id b where e.supply_inid=b.inid and tuser=:wh_userid) ) ";
                }
                else
                {
                    switch (GetUserKind(userId))
                    {
                        case "4":
                        case "5":
                        case "6":
                        case "S"://衛星
                            sql += " and a.wh_no in ";
                            sql += " ( select e.wh_no from MI_WHMAST e,MI_WHID f ";
                            sql += " where e.wh_no=f.wh_no and wh_userid=:wh_userid ";
                            sql += " union ";
                            sql += " select wh_no from mi_whmast e ";
                            sql += " where  wh_kind='1' and exists(select 'x' from ur_id b where e.supply_inid=b.inid and tuser=:wh_userid) ) ";

                            break;
                        case "1"://藥
                            sql += " and a.wh_no in ";
                            sql += " (select wh_no from MI_WHMAST a where wh_kind='0') ";
                            break;
                        default:
                            sql += " and a.wh_no in ";
                            sql += " (select wh_no from MI_WHMAST where wh_kind='1') ";
                            break;
                    }
                }
            }
            if (mat_class != "")
            {
                sql += " and d.mat_class = :p4 ";

            }
            if (m_storeid != "")
            {
                sql += " and d.m_storeid = :p5 ";

            }
            if (mmcode != "")
            {
                sql += " and a.mmcode = :p6 ";

            }

            p.Add("set_ym", set_ym);
            p.Add(":p1", string.Format("{0}", wh_kind));
            p.Add(":p2", string.Format("{0}", wh_grade));
            p.Add(":p3", string.Format("{0}", wh_no));
            p.Add(":wh_userid", string.Format("{0}", userId));
            p.Add(":p4", string.Format("{0}", mat_class));
            p.Add(":p5", string.Format("{0}", m_storeid));
            p.Add(":p6", string.Format("{0}", mmcode));
            
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0044M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<AA0044M> GetAllML(string userId, string set_ym, string wh_kind, string wh_grade, string wh_no, string mat_class, string m_storeid, string mmcode, string menuLink, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = @"
                select a.wh_no, a.mmcode,
                       pmn_invqty(:set_ym,a.wh_no,a.mmcode) inv_qty_l,
                       a.inv_qty as inv_qty_n,
                       a.apl_inqty apl_inqty_n,
                       a.apl_outqty apl_outqty_n,
                       a.trn_inqty trn_inqty_n,
                       a.trn_outqty trn_outqty_n,
                       a.adj_inqty adj_inqty_n,
                       a.adj_outqty adj_outqty_n,
                       a.bak_inqty bak_inqty_n,
                       a.bak_outqty bak_outqty_n,
                       a.exg_inqty exg_inqty_n,
                       a.exg_outqty exg_outqty_n,
                       a.mil_inqty mil_inqty_n,
                       a.mil_outqty mil_outqty_n,
                       a.onway_qty onway_qty_n,
                       a.rej_outqty rej_outqty_n,
                       a.dis_outqty dis_outqty_n,
                       (case when inventoryqty > 0 then a.inventoryqty else 0 end) inventoryqty1_n,
                       (case when inventoryqty < 0 then a.inventoryqty else 0 end) inventoryqty2_n,
                       a.use_qty use_qty_n
                  from MI_WINVMON a, MI_WHMAST c, MI_MAST d
                 where a.data_ym = :set_ym
                   and a.wh_no = c.wh_no
                   and a.mmcode = d.mmcode
            ";
            if (wh_kind != "")
            {
                sql += " and c.wh_kind = :p1 ";

            }
            if (wh_grade != "")
            {
                sql += " and c.wh_grade = :p2 ";

            }
            if (wh_no != "")
            {
                sql += " and a.wh_no = :p3 ";

            }
            else
            {
                if (menuLink == "AB0035")
                {
                    sql += " and a.wh_no in";
                    sql += " ( select e.wh_no from MI_WHMAST e,MI_WHID f ";
                    sql += " where e.wh_no=f.wh_no  and e.wh_grade='2' and wh_userid=:wh_userid ";
                    sql += " union ";
                    sql += " select wh_no from mi_whmast e ";
                    sql += " where  e.wh_grade='2' and exists(select 'x' from ur_id b where e.supply_inid=b.inid and tuser=:wh_userid) ) ";
                }
                else
                {
                    switch (GetUserKind(userId))
                    {
                        case "4":
                        case "5":
                        case "6":
                        case "S"://衛星
                            sql += " and a.wh_no in ";
                            sql += " ( select e.wh_no from MI_WHMAST e,MI_WHID f ";
                            sql += " where e.wh_no=f.wh_no and wh_userid=:wh_userid ";
                            sql += " union ";
                            sql += " select wh_no from mi_whmast e ";
                            sql += " where  wh_kind='1' and exists(select 'x' from ur_id b where e.supply_inid=b.inid and tuser=:wh_userid) ) ";

                            break;
                        case "1"://藥
                            sql += " and a.wh_no in ";
                            sql += " (select wh_no from MI_WHMAST a where wh_kind='0') ";
                            break;
                        default:
                            sql += " and a.wh_no in ";
                            sql += " (select wh_no from MI_WHMAST where wh_kind='1') ";
                            break;
                    }
                }
            }
            if (mat_class != "")
            {
                sql += " and d.mat_class = :p4 ";

            }
            if (m_storeid != "")
            {
                sql += " and d.m_storeid = :p5 ";

            }
            if (mmcode != "")
            {
                sql += " and a.mmcode = :p6 ";

            }

            p.Add("set_ym", set_ym);
            p.Add(":p1", string.Format("{0}", wh_kind));
            p.Add(":p2", string.Format("{0}", wh_grade));
            p.Add(":p3", string.Format("{0}", wh_no));
            p.Add(":wh_userid", string.Format("{0}", userId));
            p.Add(":p4", string.Format("{0}", mat_class));
            p.Add(":p5", string.Format("{0}", m_storeid));
            p.Add(":p6", string.Format("{0}", mmcode));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0044M>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<AA0044D> GetAllD(string whno, string mmcode, string mcode, string dataymn, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.*,TWN_DATE(A.TR_DATE)TR_DATE_T,B.MMNAME_C,B.MMNAME_E,B.BASE_UNIT,B.M_CONTPRICE,
                (SELECT DOCTYPE_NAME FROM MI_DOCTYPE WHERE DOCTYPE=A.TR_DOCTYPE)TR_DOCTYPE_N,
                (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='MI_WHTRNS' AND DATA_NAME='TR_MCODE' AND DATA_VALUE=A.TR_MCODE)TR_MCODE_N,
                (SELECT DATA_DESC FROM PARAM_D WHERE GRP_CODE='MI_WHTRNS' AND DATA_NAME='TR_IO' AND DATA_VALUE=A.TR_IO)TR_IO_N
                FROM MI_WHTRNS A
                INNER JOIN MI_MAST B ON B.MMCODE=A.MMCODE
                WHERE 1=1 ";

            if (whno != "")
            {
                sql += " AND A.WH_NO = :p0 ";
                p.Add(":p0", string.Format("{0}", whno));
            }
            else
            {
                sql += " AND 1=2 ";
            }
            if (mmcode != "")
            {
                sql += " AND A.MMCODE = :p1 ";
                p.Add(":p1", string.Format("{0}", mmcode));
            }
            if (mcode != "")
            {
                sql += " AND A.TR_MCODE = :p2 ";
                p.Add(":p2", string.Format("{0}", mcode));
            }
            if (dataymn != "")
            {
                sql += " AND SUBSTR(TWN_DATE(A.TR_DATE),0,5) = :p3 ";
                p.Add(":p3", string.Format("{0}", dataymn));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0044D>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        
        public string GetTaskid(string id)
        {
            string sql = @"SELECT WHM1_TASK(:WH_USERID) TASK_ID FROM DUAL";
            string rtn = DBWork.Connection.QueryFirstOrDefault(sql, new { WH_USERID = id }, DBWork.Transaction).ToString();
            return rtn;
        }
        public IEnumerable<COMBO_MODEL> GetSetymCombo()
        {
            string sql = @"select SET_YM as VALUE, SET_YM as TEXT ,
                        SET_YM as COMBITEM from MI_MNSET
                        order by set_ym desc";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetWhkindCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_WHMAST' AND DATA_NAME='WH_KIND44' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetWhgradeCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_WHMAST' AND DATA_NAME='WH_GRADE440' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetWhgrade1Combo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_WHMAST' AND DATA_NAME='WH_GRADE441' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetWhnoCombo(string wh_kind, string wh_grade, string kind, bool isAuth, string userid)
        {

            //var p = new DynamicParameters();

            //string sql = @"SELECT WH_NO AS VALUE,WH_NAME AS TEXT, WH_NO || ' ' || WH_NAME as COMBITEM  
            //            FROM MI_WHMAST  
            //            WHERE 1 = 1";
            //if (wh_kind != "")
            //{
            //    sql += " AND WH_KIND = :wh_kind ";
            //    p.Add(":wh_kind", string.Format("{0}", wh_kind));
            //}
            //if (wh_grade != "")
            //{
            //    sql += " AND WH_GRADE = :wh_grade ";
            //    p.Add(":wh_grade", string.Format("{0}", wh_grade));
            //}
            //return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
            StringBuilder sql = new StringBuilder();
            DynamicParameters p = new DynamicParameters();
            if (isAuth)
            {
                sql.Append("SELECT WH_NO VALUE,WH_NO||' '||WH_NAME TEXT,WH_KIND COMBITEM FROM MI_WHMAST");
            }
            else
            {
                switch (kind)
                {
                    case "4":
                    case "5":
                    case "6":
                    case "S"://衛星
                        sql.Append("SELECT A.WH_NO VALUE ,A.WH_NO||' '||WH_NAME TEXT ,A.WH_KIND COMBITEM FROM MI_WHMAST A,MI_WHID B");
                        sql.Append(" WHERE A.WH_NO=B.WH_NO AND WH_USERID=:WH_USERID ");
                        sql.Append(" UNION ");
                        sql.Append(" SELECT WH_NO ,WH_NO||' '||WH_NAME,WH_KIND COMBITEM FROM MI_WHMAST A");
                        sql.Append(" WHERE  WH_KIND='1' AND EXISTS(SELECT 'X' FROM UR_ID B WHERE A.INID=B.INID AND TUSER=:WH_USERID)");
                        p.Add(":WH_USERID", string.Format("{0}", userid));
                        break;
                    case "1"://藥
                        sql.Append("SELECT WH_NO VALUE,WH_NO||' '||WH_NAME TEXT,WH_KIND COMBITEM FROM MI_WHMAST A WHERE WH_KIND='0'");
                        break;
                    default:
                        sql.Append("SELECT WH_NO VALUE,WH_NO||' '||WH_NAME TEXT,WH_KIND COMBITEM FROM MI_WHMAST WHERE WH_KIND='1'");
                        break;
                }
            }

            sql.Append(" ORDER BY 1");

            return DBWork.Connection.Query<COMBO_MODEL>(sql.ToString(), p);
        }
        public IEnumerable<COMBO_MODEL> GetStoreidCombo()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_MAST' AND DATA_NAME='M_STOREID' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<MI_MAST> GetMmCodeCombo(string p0, string p1, string p2, int page_index, int page_size, string sorters)
        {

            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT,A.M_CONTPRICE,
                        ( SELECT AVG_PRICE FROM MI_WHCOST WHERE MMCODE = A.MMCODE AND ROWNUM=1 ) AS AVG_PRICE,
                        ( SELECT INV_QTY FROM MI_WHINV WHERE MMCODE = A.MMCODE AND WH_NO=:TOWH ) AS INV_QTY,
                        ( SELECT AVG_APLQTY FROM V_MM_AVGAPL WHERE MMCODE = A.MMCODE AND WH_NO=:TOWH ) AS AVG_APLQTY 
                        FROM MI_MAST A WHERE 1=1 AND A.MAT_CLASS = :MAT_CLASS 
                          AND A.M_CONTID <> '3'
                          AND A.M_APPLYID <> 'E' ";

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR MMNAME_E LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY MMCODE ";
            }
            p.Add(":MAT_CLASS", p1);
            p.Add(":TOWH", p2);

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
        public IEnumerable<MI_MAST> GetMMCodeDocd(string p0, string mat_class, string m_storeid, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E  
                        FROM MI_MAST A WHERE 1=1  ";
            if (mat_class != "")
            {
                sql += " AND mat_class = :mat_class ";
                p.Add(":mat_class", string.Format("{0}", mat_class));
            }
            if (m_storeid != "")
            {
                sql += " AND m_storeid = :m_storeid ";
                p.Add(":m_storeid", string.Format("{0}", m_storeid));
            }
            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}%", p0));

                sql += " OR MMNAME_E LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
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
        public string GetTwnsystime()
        {
            string sql = @"SELECT TWN_SYSTIME FROM DUAL ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
            return rtn;
        }

        public IEnumerable<COMBO_MODEL> GetMatclass1Combo()
        {
            string sql = @"select MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
                        FROM MI_MATCLASS WHERE MAT_CLSID='1'  
                        ORDER BY MAT_CLASS";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetMatclass2Combo()
        {
            string sql = @"select MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
                        FROM MI_MATCLASS WHERE MAT_CLSID='2'  
                        ORDER BY MAT_CLASS";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetMatclass3Combo()
        {
            string sql = @"select MAT_CLASS AS VALUE, MAT_CLSNAME AS TEXT, 
                        MAT_CLASS || ' ' || MAT_CLSNAME AS COMBITEM 
                        FROM MI_MATCLASS WHERE MAT_CLSID='3'   
                        ORDER BY MAT_CLASS";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetYN()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='Y_OR_N' AND DATA_NAME='YN' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetWhGrade()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='WH_GRADE' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<COMBO_MODEL> GetWhKind()
        {
            string sql = @"SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE || ' ' || DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='ME_DOCM' AND DATA_NAME='WH_KIND' 
                        ORDER BY DATA_VALUE";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public string GetMnset(string id)
        {
            string sql = @"SELECT SET_STATUS FROM MI_MNSET WHERE SET_YM=:SET_YM ";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { SET_YM = id }, DBWork.Transaction).ToString();
            return rtn;
        }

        public string GetUserKind(string id)
        {
            string sql = @"SELECT USER_KIND(:ID) FROM DUAL";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { ID = id }, DBWork.Transaction).ToString();
            return rtn;
        }

        public IEnumerable<COMBO_MODEL> GetWhCombo(bool isAuth, string userid, string kind,string menuLink)
        {
            StringBuilder sql = new StringBuilder();
            DynamicParameters p = new DynamicParameters();
            if (isAuth)
            {
                sql.Append("SELECT WH_NO VALUE,WH_NO||' '||WH_NAME TEXT,WH_KIND COMBITEM FROM MI_WHMAST");
            }
            else if (menuLink == "AB0035") //衛星庫房
            {
                sql.Append("SELECT A.WH_NO VALUE ,A.WH_NO||' '||WH_NAME TEXT ,A.WH_KIND COMBITEM FROM MI_WHMAST A,MI_WHID B");
                sql.Append(" WHERE A.WH_NO=B.WH_NO AND WH_GRADE='2' AND WH_USERID=:WH_USERID ");
                sql.Append(" UNION ");
                sql.Append(" SELECT WH_NO ,WH_NO||' '||WH_NAME,WH_KIND COMBITEM FROM MI_WHMAST A");
                sql.Append(" WHERE  WH_GRADE='2' AND EXISTS(SELECT 'X' FROM UR_ID B WHERE A.SUPPLY_INID=B.INID AND TUSER=:WH_USERID)");
                p.Add(":WH_USERID", string.Format("{0}", userid));
            }
            else
            {
                // 主畫面的AllM在未選擇WH_NO時,會依登入者可選WH_NO限制查詢內容,若這邊可選WH_NO規則有改.AllM那邊也要跟著改
                switch (kind)
                {
                    case "4":
                    case "5":
                    case "6":
                    case "S"://衛星
                        sql.Append("SELECT A.WH_NO VALUE ,A.WH_NO||' '||WH_NAME TEXT ,A.WH_KIND COMBITEM FROM MI_WHMAST A,MI_WHID B");
                        sql.Append(" WHERE A.WH_NO=B.WH_NO AND WH_USERID=:WH_USERID ");
                        sql.Append(" UNION ");
                        sql.Append(" SELECT WH_NO ,WH_NO||' '||WH_NAME,WH_KIND COMBITEM FROM MI_WHMAST A");
                        sql.Append(" WHERE  WH_KIND='1' AND EXISTS(SELECT 'X' FROM UR_ID B WHERE A.SUPPLY_INID=B.INID AND TUSER=:WH_USERID)");
                        p.Add(":WH_USERID", string.Format("{0}", userid));
                        break;
                    case "1"://藥
                        sql.Append("SELECT WH_NO VALUE,WH_NO||' '||WH_NAME TEXT,WH_KIND COMBITEM FROM MI_WHMAST A WHERE WH_KIND='0'");
                        break;
                    default:
                        sql.Append("SELECT WH_NO VALUE,WH_NO||' '||WH_NAME TEXT,WH_KIND COMBITEM FROM MI_WHMAST WHERE WH_KIND='1'");
                        break;
                }
            }

            sql.Append(" ORDER BY 1");

            return DBWork.Connection.Query<COMBO_MODEL>(sql.ToString(), p);
        }
    }
}
