using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using WebApp.Models;
using JCLib.DB.Tool;

namespace WebApp.Repository.BG
{
    public class BG0002Repository : JCLib.Mvc.BaseRepository
    {
        FL l = new FL("WebApp.Repository.B.BG0002Repository");
        String sBr = "\r\n";


        public BG0002Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<WebApp.Models.BG0002> GetAll(
            BG0002 v,
            int page_index, int page_size, string sorters)
        {
            int iP = 0;
            String sP = "";
            var p = new DynamicParameters();
            var sql = "";
            sql += " select" + sBr;
            //sql += " rownum ROWNUMBERER, " + sBr; // 項次
            sql += " a.mmcode, " + sBr; // 院內碼    MMCODE    VARCHAR2
            sql += " c.mmname_e, " + sBr; // 中文品名    MMNAME_C    VARCHAR2    200
            sql += " c.mmname_c, " + sBr; // 英文品名    MMNAME_C    VARCHAR2    200
            sql += " c.base_unit,  " + sBr; // 計量單位代碼
            sql += " c.disc_uprice, " + sBr; // -- 優惠最小單價
            sql += " b.data_ym, " + sBr; // varchar2(5)
            sql += " b.apl_inqty, " + sBr; // 入庫總量    APL_INQTY    NUMBER
            sql += " c.disc_uprice* b.apl_inqty as tot " + sBr; // -- 優惠最小單價 * 入庫總量
            sql += " from MI_WHINV a, MI_WINVMON b, MI_MAST c" + sBr;
            sql += " where 1=1 " + sBr;
            sql += " and a.wh_no=b.wh_no " + sBr; // 庫房代碼    WH_NO    VARCHAR2    8
            sql += " and a.mmcode=b.mmcode " + sBr; // -- 院內碼    MMCODE    VARCHAR2
            sql += " and a.mmcode=c.mmcode " + sBr;

            if (!String.IsNullOrEmpty(v.WH_NO)) // 庫房代碼
            {
                //sql += " and a.wh_no='560000' " + sBr; // {庫房別listbox}-- '560000'    庫房代碼    WH_NO    VARCHAR2    8

                sP = ":p" + iP++;
                sql += " and a.wh_no = " + sP + " " + sBr;
                p.Add(sP, v.WH_NO);
            }

            ////sql += " --and b.data_ym between {年月(起)} and {年月(迄)} --'10804' ~  '10805'   年月" + sBr;
            ////sql += " --and b.data_ym between sysdate-100 and sysdate --'10804' ~  '10805'" + sBr;
            l.getTwoDateConditionBy民國年月("data_ym", v.DATA_YM_START, v.DATA_YM_END, ref sql, ref iP, ref p); // 庫房物料月結年月

            //if (!String.IsNullOrEmpty(v.M_CONTID)) // 合約識別碼
            //{
            //    //sql += " and c.m_contid='2'  " + sBr; // 合約識別碼    M_CONTID    VARCHAR2    1
            //    sP = ":p" + iP++;
            //    sql += " c.m_contid = " + sP + " ";
            //    p.Add(sP, string.Format("{0}", v.M_CONTID));
            //}

            if (!String.IsNullOrEmpty(v.MAT_CLASS)) // 物料分類
            {
                //sql += " and c.mat_class='02' " + sBr; // {物料分類listbox } --'02'    物料分類代碼    MAT_CLASS    VARCHAR2    2
                sP = ":p" + iP++;
                sql += sBr + " and c.mat_class = " + sP + " " + sBr;
                p.Add(sP, v.MAT_CLASS);
            }

            if (!String.IsNullOrEmpty(v.RADIO_BUTTON)) // 管控項目
            {
                //sql += " -- IF (radioButton ='0') and c.m_storeid='1' and c.m_applyid <>'E' and c.m_applyid <>'P'" + sBr;
                //sql += " -- IF (radioButton ='1') and c.m_storeid='0' and c.m_applyid <>'E'" + sBr;
                //sql += " -- IF (radioButton ='2') and c.m_storeid='1' and (c.m_applyid ='E' or c.m_applyid ='P')" + sBr;
                sP = ":p" + iP++;
                sql += sBr;
                sql += " and case " + sBr;
                sql += "           when 1=1 " + sBr;
                sql += "               and c.m_storeid='1' " + sBr; // 庫備識別碼    M_STOREID    VARCHAR2    1
                sql += "               and c.m_applyid <>'E' " + sBr; // 申請申購識別碼    M_APPLYID    VARCHAR2    1
                sql += "               and c.m_applyid <>'P' " + sBr; // 申請申購識別碼    M_APPLYID    VARCHAR2    1
                sql += "               then '0' " + sBr;
                sql += "           when 1=1 " + sBr;
                sql += "               and c.m_storeid='0' " + sBr; // --  庫備識別碼    M_STOREID    VARCHAR2    1
                sql += "               and c.m_applyid <>'E' " + sBr; // -- 申請申購識別碼    M_APPLYID    VARCHAR2    1
                sql += "               then '1' " + sBr;
                sql += "           when 1=1 " + sBr;
                sql += "               and c.m_storeid='1' " + sBr; // --  庫備識別碼    M_STOREID    VARCHAR2    1
                sql += "               and " + sBr;
                sql += "               (c.m_applyid ='E' or c.m_applyid='P' " + sBr; // -- 申請申購識別碼    M_APPLYID    VARCHAR2    1
                sql += "               )" + sBr;
                sql += "               then '2' " + sBr;
                sql += "           else '-1' " + sBr;
                sql += "       end = " + sP + " " + sBr; // 管控項目 radioButton
                p.Add(sP, v.RADIO_BUTTON);
            }
            l.lg("GetAll()", l.getDebugSql(sql, p));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);
            return DBWork.Connection.Query<BG0002>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        // 查庫房別
        public IEnumerable<ComboItemModel> GetWhnoCombo(string wh_userId)
        {
            string sql = "";
            sql += " select " + sBr;
            sql += " a.WH_NO AS VALUE, " + sBr;
            sql += " a.WH_NO || ' ' || a.WH_NAME AS TEXT " + sBr;
            sql += " from MI_WHMAST a " + sBr;
            sql += " where 1=1 " + sBr;
            sql += " and a.wh_grade in ('1') " + sBr;
            sql += " and a.wh_kind in ('0','1') " + sBr;
            sql += " order by TEXT " + sBr;
            l.lg("GetWhnoCombo()", l.getDebugSql(sql, null));

            return DBWork.Connection.Query<ComboItemModel>(sql, new { WH_USERID = wh_userId }, DBWork.Transaction);
        }

        // 查物料分類
        public IEnumerable<ComboItemModel> GetMatClassCombo(string wh_userId)
        {
            string sql = "";

            sql += " select " + sBr;
            sql += " mat_class as value, ";
            sql += " mat_class ||' ' || mat_clsname as text " + sBr;
            sql += " from MI_MATCLASS  " + sBr;
            sql += " where mat_clsid in ('1','2','3','6') " + sBr;
            sql += " order by value " + sBr;
            l.lg("GetMatClassCombo()", l.getDebugSql(sql, null));

            return DBWork.Connection.Query<ComboItemModel>(sql, null, DBWork.Transaction);
        } // 

        public DataTable GetExcel(BG0002 v)
        {
            int iP = 0;
            String sP = "";
            var p = new DynamicParameters();
            var sql = "";
            sql += " select" + sBr;
            sql += " '' 項次, " + sBr; // 項次
            sql += " a.mmcode 院內碼, " + sBr; // 院內碼    MMCODE    VARCHAR2
            sql += " c.mmname_c 英文品名, " + sBr; // 英文品名    MMNAME_C    VARCHAR2    200
            sql += " c.mmname_e 中文品名, " + sBr; // 中文品名    MMNAME_C    VARCHAR2    200
            sql += " c.base_unit 計量單位,  " + sBr; // 計量單位代碼
            sql += " b.data_ym 年月, " + sBr; // varchar2(5)
            sql += " b.apl_inqty 本期增加, " + sBr; // 入庫總量    APL_INQTY    NUMBER
            sql += " c.disc_uprice 單價, " + sBr; // -- 優惠最小單價
            sql += " c.disc_uprice* b.apl_inqty as 總價, " + sBr; // -- 優惠最小單價 * 入庫總量
            sql += " '' 累積進貨金額 " + sBr; // -- 優惠最小單價 * 入庫總量
            sql += " from MI_WHINV a, MI_WINVMON b, MI_MAST c" + sBr;
            sql += " where 1=1 " + sBr;
            sql += " and a.wh_no=b.wh_no " + sBr; // 庫房代碼    WH_NO    VARCHAR2    8
            sql += " and a.mmcode=b.mmcode " + sBr; // -- 院內碼    MMCODE    VARCHAR2
            sql += " and a.mmcode=c.mmcode " + sBr;

            if (!String.IsNullOrEmpty(v.WH_NO)) // 庫房代碼
            {
                //sql += " and a.wh_no='560000' " + sBr; // {庫房別listbox}-- '560000'    庫房代碼    WH_NO    VARCHAR2    8

                sP = ":p" + iP++;
                sql += " and a.wh_no = " + sP + " " + sBr;
                p.Add(sP, v.WH_NO);
            }

            ////sql += " --and b.data_ym between {年月(起)} and {年月(迄)} --'10804' ~  '10805'   年月" + sBr;
            ////sql += " --and b.data_ym between sysdate-100 and sysdate --'10804' ~  '10805'" + sBr;
            l.getTwoDateConditionBy民國年月("b.data_ym", v.DATA_YM_START, v.DATA_YM_END, ref sql, ref iP, ref p); // 庫房物料月結年月

            //if (!String.IsNullOrEmpty(v.M_CONTID)) // 合約識別碼
            //{
            //    //sql += " and c.m_contid='2'  " + sBr; // 合約識別碼    M_CONTID    VARCHAR2    1
            //    sP = ":p" + iP++;
            //    sql += " c.m_contid = " + sP + " ";
            //    p.Add(sP, string.Format("{0}", v.M_CONTID));
            //}

            if (!String.IsNullOrEmpty(v.MAT_CLASS)) // 物料分類
            {
                //sql += " and c.mat_class='02' " + sBr; // {物料分類listbox } --'02'    物料分類代碼    MAT_CLASS    VARCHAR2    2
                sP = ":p" + iP++;
                sql += sBr + " and c.mat_class = " + sP + " " + sBr;
                p.Add(sP, v.MAT_CLASS);
            }

            if (!String.IsNullOrEmpty(v.RADIO_BUTTON)) // 管控項目
            {
                //sql += " -- IF (radioButton ='0') and c.m_storeid='1' and c.m_applyid <>'E' and c.m_applyid <>'P'" + sBr;
                //sql += " -- IF (radioButton ='1') and c.m_storeid='0' and c.m_applyid <>'E'" + sBr;
                //sql += " -- IF (radioButton ='2') and c.m_storeid='1' and (c.m_applyid ='E' or c.m_applyid ='P')" + sBr;
                sP = ":p" + iP++;
                sql += sBr;
                sql += " and case " + sBr;
                sql += "           when 1=1 " + sBr;
                sql += "               and c.m_storeid='1' " + sBr; // 庫備識別碼    M_STOREID    VARCHAR2    1
                sql += "               and c.m_applyid <>'E' " + sBr; // 申請申購識別碼    M_APPLYID    VARCHAR2    1
                sql += "               and c.m_applyid <>'P' " + sBr; // 申請申購識別碼    M_APPLYID    VARCHAR2    1
                sql += "               then '0' " + sBr;
                sql += "           when 1=1 " + sBr;
                sql += "               and c.m_storeid='0' " + sBr; // --  庫備識別碼    M_STOREID    VARCHAR2    1
                sql += "               and c.m_applyid <>'E' " + sBr; // -- 申請申購識別碼    M_APPLYID    VARCHAR2    1
                sql += "               then '1' " + sBr;
                sql += "           when 1=1 " + sBr;
                sql += "               and c.m_storeid='1' " + sBr; // --  庫備識別碼    M_STOREID    VARCHAR2    1
                sql += "               and " + sBr;
                sql += "               (c.m_applyid ='E' or c.m_applyid='P' " + sBr; // -- 申請申購識別碼    M_APPLYID    VARCHAR2    1
                sql += "               )" + sBr;
                sql += "               then '2' " + sBr;
                sql += "           else '-1' " + sBr;
                sql += "       end = " + sP + " " + sBr; // 管控項目 radioButton
                p.Add(sP, v.RADIO_BUTTON);
            }
            sql += " order by a.mmcode, b.data_ym desc " + sBr; // 管控項目 radioButton
            l.lg("GetExcel()", l.getDebugSql(sql, p));
            DataTable dtSos = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dtSos.Load(rdr);
            DataTable dt = copyTableWithData(dtSos);
            double tot = 0d;
            double acttot = 0d;
            DataRow dr;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dr = (DataRow)dt.Rows[i];
                dr["項次"] = (i+1).ToString();
                if (Double.TryParse(dr["總價"].ToString(), out tot))
                {
                    acttot += tot;
                    dr["累積進貨金額"] = acttot;
                }
            }
            return dt;
        } // 

        DataTable copyTableWithData(DataTable dtS)
        {
            DataTable dt = new DataTable();
            foreach (DataColumn c in dtS.Columns)
            {
                DataColumn cNew = new DataColumn(c.ColumnName);
                dt.Columns.Add(cNew);
            }
            DataRow dr;
            foreach (DataRow r in dtS.Rows)
            {
                dr = dt.NewRow();
                foreach (DataColumn c in dtS.Columns)
                {
                    dr[c.ColumnName] =  r[c.ColumnName].ToString();
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        public IEnumerable<BG0002> GetPrintData(BG0002 v)
        {
            IEnumerable<BG0002> lst = GetAll(v, 1, 999999, "[{'property':'MMCODE, data_ym','direction':'DESC'}]");
            double tot = 0d;
            double acctot = 0d;
            int i = 1;
            String mmcode = "";
            foreach (BG0002 eV in lst)
            {
                eV.ROWNUMBERER = i.ToString();
                i++;
                if (double.TryParse(eV.TOT, out tot))
                {
                    //if (!mmcode.ToLower().Equals(eV.MMCODE.ToLower())) 
                    //{
                    //    mmcode = eV.MMCODE.ToLower();
                    //    acctot = 0d;
                    //}
                    //if (mmcode.ToLower().Equals(eV.MMCODE.ToLower()))
                    //{   // 依不同【院內碼MMCODE】累計【累計進貨金額】
                    acctot += tot;
                    eV.ACCTOT = acctot.ToString();
                    //}
                }
            }
            return lst;
        } // 
    }
}