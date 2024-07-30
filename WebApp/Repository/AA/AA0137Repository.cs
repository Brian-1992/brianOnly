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
    public class AA0137Repository : JCLib.Mvc.BaseRepository
    {
        public AA0137Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0129> GetAll(string tuser, string wh_no, string mmcode, string mmname_c, string mmname_e, string rest,string ctdmdccodes, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT B.WH_NO||' '||B.WH_NAME WH_NAME,A.MMCODE,C.MMNAME_C,C.MMNAME_E ");
            sql.Append(",(SELECT UI_CHANAME FROM MI_UNITCODE D WHERE C.BASE_UNIT=D.UNIT_CODE) BASE_UNIT");
            sql.Append(",INV_QTY,ONWAY_QTY,SAFE_QTY,OPER_QTY ,SHIP_QTY,HIGH_QTY   ");
            sql.Append("  FROM MI_WHINV A,MI_WHMAST B ,MI_MAST C,MI_WINVCTL E");
            sql.Append(" WHERE A.WH_NO=B.WH_NO AND A.MMCODE=C.MMCODE");
            sql.Append(" AND A.WH_NO=E.WH_NO(+) AND A.MMCODE=E.MMCODE(+)");
            sql.Append(" AND B.WH_KIND='0'");
            sql.Append(@"  and (
                                (c.mat_class = '01' and (e.ctdmdccode in ('0', '2', '3', '4') or a.inv_qty <> 0))
                                or
                                (c.mat_class not in  ('01') )
                               )");

            if (wh_no != "")
            {
                sql.Append(" AND A.WH_NO=:WH_NO");
                p.Add(":WH_NO", wh_no);
            }
            if (mmcode != "")
            {
                sql.Append(" AND A.MMCODE LIKE :MMCODE");
                p.Add(":MMCODE", mmcode + '%');
            }
            if (mmname_c != "")
            {
                sql.Append(" AND MMNAME_C LIKE :MMNAME_C");
                p.Add(":MMNAME_C", mmname_c + '%');
            }
            if (mmname_e != "")
            {
                sql.Append(" AND MMNAME_E LIKE :MMNAME_E");
                p.Add(":MMNAME_E", mmname_e + '%');
            }
            if (rest != "")
            {
                if (rest == "0") // 1~3級管制用藥
                    sql.Append(" and C.E_RESTRICTCODE in ('1','2','3') ");
                else if (rest == "1") // 第四級管制用藥
                    sql.Append(" and C.E_RESTRICTCODE = '4' ");
                else if (rest == "2") // N非管制用藥
                    sql.Append(" and C.E_RESTRICTCODE = 'N' ");
                else if (rest == "3") // 0其它列管藥品
                    sql.Append(" and C.E_RESTRICTCODE = '0' ");
            }
            if (ctdmdccodes != "")
            {
                sql.Append(string.Format("  and e.ctdmdccode in ( {0} )", ctdmdccodes));
            }
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0129>(GetPagingStatement(sql.ToString(), sorters), p, DBWork.Transaction);
        }

        public DataTable GetExcel(string tuser, string wh_no, string mmcode, string mmname_c, string mmname_e, string rest)
        {
            DynamicParameters p = new DynamicParameters();

            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT B.WH_NO||' '||B.WH_NAME as 庫房,A.MMCODE as 院內碼,C.MMNAME_C as 中文品名,C.MMNAME_E as 英文品名 ");
            sql.Append(",(SELECT UI_CHANAME FROM MI_UNITCODE D WHERE C.BASE_UNIT=D.UNIT_CODE) as 單位 ");
            sql.Append(",INV_QTY as 庫存量,ONWAY_QTY as 在途量,SAFE_QTY as 安全量,OPER_QTY as 作業量 ,SHIP_QTY as 運補量,HIGH_QTY as 基準量  ");
            sql.Append("  FROM MI_WHINV A,MI_WHMAST B ,MI_MAST C,MI_WINVCTL E");
            sql.Append(" WHERE A.WH_NO=B.WH_NO AND A.MMCODE=C.MMCODE");
            sql.Append(" AND A.WH_NO=E.WH_NO(+) AND A.MMCODE=E.MMCODE(+)");
            sql.Append(" AND B.WH_KIND='0'");
            sql.Append(@"  and (
                                (c.mat_class = '01' and (e.ctdmdccode in ('0', '2', '3', '4') or a.inv_qty <> 0))
                                or
                                (c.mat_class not in  ('01') )
                               )");

            if (wh_no != "")
            {
                sql.Append(" AND A.WH_NO=:WH_NO");
                p.Add(":WH_NO", wh_no);
            }
            if (mmcode != "")
            {
                sql.Append(" AND A.MMCODE LIKE :MMCODE");
                p.Add(":MMCODE", mmcode + '%');
            }
            if (mmname_c != "")
            {
                sql.Append(" AND MMNAME_C LIKE :MMNAME_C");
                p.Add(":MMNAME_C", mmname_c + '%');
            }
            if (mmname_e != "")
            {
                sql.Append(" AND MMNAME_E LIKE :MMNAME_E");
                p.Add(":MMNAME_E", mmname_e + '%');
            }
            if (rest != "")
            {
                if (rest == "0") // 1~3級管制用藥
                    sql.Append(" and C.E_RESTRICTCODE in ('1','2','3') ");
                else if (rest == "1") // 第四級管制用藥
                    sql.Append(" and C.E_RESTRICTCODE = '4' ");
                else if (rest == "2") // N非管制用藥
                    sql.Append(" and C.E_RESTRICTCODE = 'N' ");
                else if (rest == "3") // 0其它列管藥品
                    sql.Append(" and C.E_RESTRICTCODE = '0' ");
            }

            sql.Append(" ORDER BY B.WH_NO, A.MMCODE ");

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql.ToString(), p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<COMBO_MODEL> GetWhCombo()
        {
            StringBuilder sql = new StringBuilder();
            DynamicParameters p = new DynamicParameters();
            sql.Append(" select WH_NO VALUE, WH_NO||' '||WH_NAME TEXT from MI_WHMAST where WH_KIND = '0' order by WH_NO ");

            return DBWork.Connection.Query<COMBO_MODEL>(sql.ToString(), p);
        }

        public IEnumerable<MI_MAST> GetMmcodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            DynamicParameters p = new DynamicParameters();
            StringBuilder sql = new StringBuilder();
            sql.Append("select DISTINCT A.MMCODE,MMNAME_C,MMNAME_E");
            sql.Append(",(SELECT UI_CHANAME FROM MI_UNITCODE D WHERE B.BASE_UNIT=D.UNIT_CODE) BASE_UNIT");
            sql.Append(" from MI_WHINV A,MI_MAST B");
            sql.Append(" where A.MMCODE=B.MMCODE and B.MAT_CLASS = '01' ");
            sql.Append(" and (A.MMCODE LIKE :MMCODE or MMNAME_C LIKE :MMNAME_C or UPPER(MMNAME_E) LIKE :MMNAME_E )");


            p.Add(":MMCODE", string.Format("{0}%", p0));
            p.Add(":MMNAME_C", string.Format("%{0}%", p0));
            p.Add(":MMNAME_E", string.Format("%{0}%", p0.ToUpper()));

            sql.Append(" ORDER BY 1");
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql.ToString(), sorters), p, DBWork.Transaction);
        }
    }
}