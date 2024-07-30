using System;
using System.Data;
using JCLib.DB;
using Dapper;
using System.Collections.Generic;
using WebApp.Models.AA;
using System.Text;
using WebApp.Models;

namespace WebApp.Repository.AA
{
    public class AA0129Repository : JCLib.Mvc.BaseRepository
    {
        public AA0129Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0129> GetAll(bool isAuth,string user_kind, string tuser, string wh_no, string mat_class, string mmcode, string mmname_c, string mmname_e, string ctdmdccodes, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT B.WH_NO||' '||B.WH_NAME WH_NAME,A.MMCODE,C.MMNAME_C,C.MMNAME_E ");
            sql.Append(",(SELECT UI_CHANAME FROM MI_UNITCODE D WHERE C.BASE_UNIT=D.UNIT_CODE) BASE_UNIT");
            sql.Append(",INV_QTY,ONWAY_QTY,SAFE_QTY,OPER_QTY ,SHIP_QTY,HIGH_QTY");
            sql.Append("  FROM MI_WHINV A,MI_WHMAST B ,MI_MAST C,MI_WINVCTL E");
            sql.Append(" WHERE A.WH_NO=B.WH_NO AND A.MMCODE=C.MMCODE");
            sql.Append(" AND A.WH_NO=E.WH_NO(+) AND A.MMCODE=E.MMCODE(+)");
            sql.Append(@"  and (
                                (c.mat_class = '01' and a.inv_qty <> 0)
                                or
                                (c.mat_class not in  ('01') )
                               )");
            if (isAuth == false)
            {
                switch (user_kind)
                {
                    case "4":
                    case "5":
                    case "S"://衛星
                        sql.Append(" AND (EXISTS(SELECT 'X' FROM MI_WHID D WHERE D.WH_NO=A.WH_NO AND D.WH_USERID=:TUSER");
                        sql.Append("  OR (B.SUPPLY_INID =(SELECT INID FROM UR_ID WHERE TUSER=:TUSER) AND WH_KIND='1')");
                        sql.Append("))");
                        p.Add(":TUSER", tuser);
                        break;
                    case "6":
                        sql.Append(" AND EXISTS(SELECT 'X' FROM MI_MATCLASS D WHERE D.MAT_CLSID='6' AND C.MAT_CLASS=D.MAT_CLASS)");
                        sql.Append(" AND (EXISTS(SELECT 'X' FROM MI_WHID D WHERE D.WH_NO=A.WH_NO AND D.WH_USERID=:TUSER");
                        sql.Append("  OR (B.INID =(SELECT INID FROM UR_ID WHERE TUSER=:TUSER) AND WH_KIND='1')");
                        sql.Append("))");
                        p.Add(":TUSER", tuser);
                        break;
                    case "1"://藥
                        sql.Append(@" AND EXISTS(SELECT 'X' FROM MI_WHMAST D WHERE D.WH_NO=A.WH_NO AND WH_KIND='0')");
                        break;
                    default://衛,一般物品
                        sql.Append(" AND EXISTS(SELECT 'X' FROM MI_WHMAST D WHERE D.WH_NO=A.WH_NO AND WH_KIND='1')");
                        sql.Append(" AND EXISTS(SELECT 'X' FROM MI_MATCLASS D WHERE D.MAT_CLSID IN('2','3') AND C.MAT_CLASS=D.MAT_CLASS)");
                        break;
                }
            }

            if (wh_no != "")
            {
                sql.Append(" AND A.WH_NO=:WH_NO");
                p.Add(":WH_NO", wh_no);
            }
            if (mat_class != "")
            {
                sql.Append(" AND C.MAT_CLASS=:MAT_CLASS");
                p.Add(":MAT_CLASS", mat_class);
            }
            if (mmcode != "")
            {
                sql.Append(" AND A.MMCODE LIKE :MMCODE");
                p.Add(":MMCODE", mmcode + '%');
            }
            if (mmname_c != "")
            {
                sql.Append(" AND MMNAME_C LIKE :MMNAME_C");
                p.Add(":MMNAME_C", '%' + mmname_c + '%');
            }
            if (mmname_e != "")
            {
                sql.Append(" AND MMNAME_E LIKE :MMNAME_E");
                p.Add(":MMNAME_E", '%' + mmname_e + '%');
            }
            if (ctdmdccodes != "") {
                sql.Append(string.Format("  and e.ctdmdccode in ( {0} )", ctdmdccodes));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0129>(GetPagingStatement(sql.ToString(), sorters), p, DBWork.Transaction);
        }
        public string GetUserKind(string id)
        {
            string sql = @"SELECT USER_KIND(:ID) FROM DUAL";
            string rtn = DBWork.Connection.ExecuteScalar(sql, new { ID = id }, DBWork.Transaction).ToString();
            return rtn;
        }

        public IEnumerable<AA01291> GetAllD1(string wh_no, string mmcode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            string sql = "select MMCODE,STORE_LOC,INV_QTY from MI_WLOCINV where WH_NO=:WH_NO and MMCODE=:MMCODE";
            p.Add(":wh_no", wh_no);
            p.Add(":mmcode", mmcode);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<AA01291>(GetPagingStatement(sql.ToString(), sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA01292> GetAllD2(string wh_no, string mmcode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string sql = "select mmcode,TWN_DATE(exp_date) as exp_date,lot_no,inv_qty from MI_WEXPINV where wh_no=:wh_no and mmcode=:mmcode";
            p.Add(":wh_no", wh_no);
            p.Add(":mmcode", mmcode);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<AA01292>(GetPagingStatement(sql.ToString(), sorters), p, DBWork.Transaction);
        }

        public IEnumerable<AA01293> GetAllD3(string wh_no, string mmcode, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT A.MMCODE, INV_QTY, ONWAY_QTY, APL_INQTY, APL_OUTQTY, TRN_INQTY, TRN_OUTQTY, ADJ_INQTY, ADJ_OUTQTY");
            sql.Append(",BAK_INQTY, BAK_OUTQTY, REJ_OUTQTY, DIS_OUTQTY, EXG_INQTY, EXG_OUTQTY, MIL_INQTY, MIL_OUTQTY");
            sql.Append(",INVENTORYQTY, TUNEAMOUNT, USE_QTY");
            sql.Append(",B.MMNAME_C,B.MMNAME_E,B.WEXP_ID,(SELECT A.WH_NO||' '||WH_NAME FROM MI_WHMAST C WHERE A.WH_NO=C.WH_NO) WH_NAME");
            sql.Append(@",NVL((SELECT INV_QTY FROM MI_WINVMON C WHERE DATA_YM=  TWN_YYYMM(ADD_MONTHS(SYSDATE,-1)) AND C.WH_NO=A.WH_NO AND C.MMCODE=A.MMCODE),0)PRE_QTY
                         ,(select data_value||' '||data_desc 
                             from PARAM_D 
                            where grp_code = 'MI_WINVCTL'
                              and data_name = 'CTDMDCCODE'
                              and data_value = (select ctdmdccode from MI_WINVCTL where wh_no = a.wh_no and mmcode = a.mmcode)) as ctdmdccode");
            sql.Append(" FROM MI_WHINV A,MI_MAST B");
            sql.Append(" WHERE A.WH_NO=:WH_NO AND A.MMCODE=:MMCODE AND A.MMCODE=B.MMCODE");
            p.Add(":WH_NO", wh_no);
            p.Add(":MMCODE", mmcode);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<AA01293>(GetPagingStatement(sql.ToString(), sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetMAT_CLASSCombo(bool isAuth,string wh_kind, string user_kind)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT MAT_CLASS AS VALUE, MAT_CLASS || ' ' || MAT_CLSNAME AS TEXT");
            sql.Append(" FROM MI_MATCLASS");
            if (isAuth == false) {
                switch (user_kind)
                {
                    case "1"://藥
                        sql.Append(" WHERE MAT_CLSID ='1'");
                        break;
                    case "4"://能
                        sql.Append(" WHERE MAT_CLSID ='4'");
                        break;
                    case "5":
                        sql.Append(" WHERE MAT_CLSID ='5'");
                        break;
                    case "6":
                        sql.Append(" WHERE MAT_CLSID ='6'");
                        break;
                    case "S":
                        if (wh_kind == "0")
                            sql.Append(" WHERE MAT_CLSID ='1'");
                        else if (wh_kind == "1")
                            sql.Append(" WHERE MAT_CLSID IN('2','3')");
                        else sql.Append(" WHERE MAT_CLSID IN('1','2','3')");
                        break;
                    default:
                        sql.Append("  WHERE MAT_CLSID IN('2','3')");
                        break;
                }
            }
            sql.Append(" ORDER BY 1");

            return DBWork.Connection.Query<COMBO_MODEL>(sql.ToString());
        }
        public IEnumerable<COMBO_MODEL> GetWhCombo(bool isAuth,string userid, string kind)
        {
            StringBuilder sql = new StringBuilder();
            DynamicParameters p = new DynamicParameters();
            if (isAuth)
            {
                sql.Append("SELECT WH_NO VALUE,WH_NO||' '||WH_NAME TEXT,WH_KIND COMBITEM FROM MI_WHMAST");
            }
            else {
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

        public IEnumerable<MI_MAST> GetMmcodeCombo(string p0, string mat_class, int page_index, int page_size, string sorters)
        {
            DynamicParameters p = new DynamicParameters();
            StringBuilder sql = new StringBuilder();
            sql.Append("select DISTINCT A.MMCODE,MMNAME_C,MMNAME_E");
            sql.Append(",(SELECT UI_CHANAME FROM MI_UNITCODE D WHERE B.BASE_UNIT=D.UNIT_CODE) BASE_UNIT");
            sql.Append(" from MI_WHINV A,MI_MAST B");
            sql.Append(" where A.MMCODE=B.MMCODE");
            sql.Append(" and (A.MMCODE LIKE :MMCODE or MMNAME_C LIKE :MMNAME_C or UPPER(MMNAME_E) LIKE :MMNAME_E )");


            p.Add(":MMCODE", string.Format("{0}%", p0));
            p.Add(":MMNAME_C", string.Format("%{0}%", p0));
            p.Add(":MMNAME_E", string.Format("%{0}%", p0.ToUpper()));
            if (mat_class != "")
            {
                sql.Append(" AND B.MAT_CLASS=:MAT_CLASS");
                p.Add(":MAT_CLASS", mat_class);
            }
            sql.Append(" ORDER BY 1");
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql.ToString(), sorters), p, DBWork.Transaction);
        }

        #region 2021-11-04 管制表序81
        public IEnumerable<COMBO_MODEL> GetCtdmdccodes() {
            string sql = @"
                select data_value as value, data_value||' '||data_desc as text
                  from PARAM_D
                 where grp_code = 'MI_WINVCTL'
                   and data_name = 'CTDMDCCODE'
                 order by data_value
            ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }
        #endregion
    }
}
