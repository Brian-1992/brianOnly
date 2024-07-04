using System;
using System.Collections.Generic;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Text;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using TSGH.Models;

namespace WebApp.Repository.C
{
    public class CE0032ReportMODEL : JCLib.Mvc.BaseModel
    {
        public string F1 { get; set; }
        public string F2 { get; set; }
        public double F3 { get; set; }
        public string F4 { get; set; }
        public string F5 { get; set; }
        public string F6 { get; set; }
        public double F7 { get; set; }
        public double F8 { get; set; }
    }
    public class CE0032MODEL : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }
        public string MMNAME { get; set; }
        public double STORE_QTY { get; set; }
        public string CHK_UID { get; set; }
        public string CHK_UID_NAME { get; set; }
        public string STORE_LOC_QTY { get; set; }
        public string CHK_QTY { get; set; }
        public string GAP_T { get; set; }
    }
    public class CE0032Repository : JCLib.Mvc.BaseRepository
    {
        public CE0032Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        
        public IEnumerable<CE0032MODEL> GetQueryData(string pFromWhere, string pChkYM, string p1, string p2, int page_index, int page_size, string sorters)
        {
            DynamicParameters sqlParam = new DynamicParameters();
            string sqlStr = GetSqlstr(pChkYM, p1, p2, out sqlParam);

            sqlParam.Add("OFFSET", (page_index - 1) * page_size);
            sqlParam.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<CE0032MODEL>((pFromWhere == "rdlc" ? sqlStr : GetPagingStatement(sqlStr, sorters)), sqlParam, DBWork.Transaction);
        }
        public String GetSqlstr(string pChkYM, string p1, string p2, out DynamicParameters pSqlParam)
        {
            pSqlParam = new DynamicParameters();
            String sqlStr = @"select * from 
                                (Select a.mmcode, 
                                (b.mmname_c ||'<br>'|| b.mmname_e) as mmname, b.MAT_CLASS,
                                a.store_qty,
                                    (select distinct chk_uid from CHK_DETAIL 
                                                         where chk_no = a.chk_no 
                                                           and mmcode= a.mmcode) as chk_uid,
                                       (select una from UR_ID 
                                         where tuser = (select distinct chk_uid from CHK_DETAIL 
                                                         where chk_no = a.chk_no 
                                                           and mmcode= a.mmcode) 
                                        ) as chk_uid_name,
                                (select listagg(store_loc || '-' || chk_qty, '<br>')
                                 within group (order by store_loc)
                                   from chk_detail
                                  where chk_no = a.chk_no
                                    and mmcode = a.mmcode) as store_loc_qty,                    
                                     (case 
		                                When a.status_tot = 1 then a.chk_qty1
		                                When a.status_tot = 2 then a.chk_qty2
                                When a.status_tot = 3 then a.chk_qty3
                                Else 0 end) as chk_qty, 
                                     a.gap_t
                                 from CHK_DETAILTOT a, MI_MAST b
                                where chk_no in (select chk_no from CHK_MAST 
                                                    where chk_ym = :CHK_YM
                                                    and chk_wh_no = '560000'
                                                    and chk_level = '1')
                                  and b.mmcode = a.mmcode ";
            if (p2 != "")
            {
                sqlStr += " AND b.MAT_CLASS=:mat_class ";
                pSqlParam.Add(":mat_class", string.Format("{0}", p2));
            }

            sqlStr += @") A
                                  where 1 = 1 
                                 ";
            if (p1 != "")
            {
                sqlStr += " AND A.chk_uid=:chk_uid ";
                pSqlParam.Add(":chk_uid", string.Format("{0}", p1));
            }
            
            sqlStr += " ORDER BY MMCODE ";

            pSqlParam.Add("CHK_YM", pChkYM);

            return sqlStr.ToString();
        }
        public IEnumerable<CE0032ReportMODEL> GetPrintData(string pChkYM, string p1, string p2)
        {
            var p = new DynamicParameters();

            var sql = @"select A.MMCODE F1,
                                A.MMNAME F2,
                                A.STORE_QTY F3,
                                A.CHK_UID F4,
                                A.CHK_UID_NAME F5,
                                A.STORE_LOC_QTY F6,
                                A.CHK_QTY F7,
                                A.GAP_T F8
                            from 
                                (Select a.mmcode, 
                                (b.mmname_c ||'<br>'|| b.mmname_e) as mmname, b.MAT_CLASS,
                                a.store_qty,
                                    (select distinct chk_uid from CHK_DETAIL 
                                                         where chk_no = a.chk_no 
                                                           and mmcode= a.mmcode) as chk_uid,
                                       (select una from UR_ID 
                                         where tuser = (select distinct chk_uid from CHK_DETAIL 
                                                         where chk_no = a.chk_no 
                                                           and mmcode= a.mmcode) 
                                        ) as chk_uid_name,
                                (select listagg(store_loc || '-' || chk_qty, '<br>')
                                 within group (order by store_loc)
                                   from chk_detail
                                  where chk_no = a.chk_no
                                    and mmcode = a.mmcode) as store_loc_qty,                    
                                     (case 
		                                When a.status_tot = 1 then a.chk_qty1
		                                When a.status_tot = 2 then a.chk_qty2
                                When a.status_tot = 3 then a.chk_qty3
                                Else 0 end) as chk_qty, 
                                     a.gap_t
                                 from CHK_DETAILTOT a, MI_MAST b
                                where chk_no in (select chk_no from CHK_MAST 
                                                    where chk_ym = :CHK_YM
                                                    and chk_wh_no = '560000'
                                                    and chk_level = '1')
                                  and b.mmcode = a.mmcode ";
            if (p2 != "")
            {
                sql += " AND b.MAT_CLASS = :mat_class ";
                p.Add(":mat_class", string.Format("{0}", p2));
            }

            sql += @" ) A
                                  where 1 = 1  ";
            if(p1 != "")
            {
                sql += " AND A.CHK_UID = :UID ";
                p.Add(":UID", string.Format("{0}", p1));
            }
            
            sql += " ORDER BY MMCODE ";
            p.Add("CHK_YM", pChkYM);
            return DBWork.Connection.Query<CE0032ReportMODEL>(sql, p, DBWork.Transaction);
        }


        public IEnumerable<COMBO_MODEL> GetManagerCombo()
        {

            var p = new DynamicParameters();

            string sql = @"select distinct a.managerid VALUE, 
                            (select una from UR_ID where tuser = a.managerid) as TEXT 
                              From BC_ITMANAGER a where wh_no = WHNO_MM1 ORDER BY 2 ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetMatClassCombo()
        {
            var p = new DynamicParameters();

            string sql = @" SELECT Trim(MAT_CLASS) as VALUE, Trim(MAT_CLSNAME) as TEXT, 
                        Trim(MAT_CLASS) || ' ' || Trim(MAT_CLSNAME) as COMBITEM
                        FROM MI_MATCLASS WHERE MAT_CLSID in ('2', '3', '6') 
                        ORDER BY MAT_CLASS ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p, DBWork.Transaction);
        }

        //匯出
        public DataTable GetExcel(string p0, string p1, string p2)
        {
            var p = new DynamicParameters();
            var sql = @"select 
                                A.MMCODE 院內碼,
                                A.MMNAME 品名,
                                A.STORE_QTY 電腦量,
                                A.CHK_UID 盤點人員帳號,
                                A.CHK_UID_NAME 盤點人員,
                                A.STORE_LOC_QTY 儲位量,
                                A.CHK_QTY 實盤量,
                                A.GAP_T 差異量
                        from 
                                (Select a.mmcode, 
                                (b.mmname_c ||'('|| b.mmname_e || ')') as mmname, b.MAT_CLASS,
                                a.store_qty,
                                    (select distinct chk_uid from CHK_DETAIL 
                                                         where chk_no = a.chk_no 
                                                           and mmcode= a.mmcode) as chk_uid,
                                       (select una from UR_ID 
                                         where tuser = (select distinct chk_uid from CHK_DETAIL 
                                                         where chk_no = a.chk_no 
                                                           and mmcode= a.mmcode) 
                                        ) as chk_uid_name,
                                (select listagg(store_loc || '-' || chk_qty, '、')
                                 within group (order by store_loc)
                                   from chk_detail
                                  where chk_no = a.chk_no
                                    and mmcode = a.mmcode) as store_loc_qty,                    
                                     (case 
		                                When a.status_tot = 1 then a.chk_qty1
		                                When a.status_tot = 2 then a.chk_qty2
                                When a.status_tot = 3 then a.chk_qty3
                                Else 0 end) as chk_qty, 
                                     a.gap_t
                                 from CHK_DETAILTOT a, MI_MAST b
                                where chk_no in (select chk_no from CHK_MAST 
                                                    where chk_ym = :CHK_YM
                                                    and chk_wh_no = '560000'
                                                    and chk_level = '1')
                                  and b.mmcode = a.mmcode";
            if (p2 != "")
            {
                sql += " AND b.MAT_CLASS = :mat_class ";
                p.Add(":mat_class", string.Format("{0}", p2));
            }
            sql += @") A
                                  where 1 = 1  ";
            if (p1 != "")
            {
                sql += " AND A.CHK_UID = :UID ";
                p.Add(":UID", string.Format("{0}", p1));
            }
            
            sql += " ORDER BY MMCODE ";
            p.Add("CHK_YM", p0);

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }
    }
}