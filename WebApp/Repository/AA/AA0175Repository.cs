using System;
using System.Collections.Generic;
using System.Web;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using WebApp.Models.AA;

namespace WebApp.Repository.AA
{
    public class AA0175Repository : JCLib.Mvc.BaseRepository
    {
        public AA0175Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AA0175MasterMODEL> GetAll(string p0, string p1, string p2, string p3, string p4, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"
                select a.wh_no || ' '|| wh_name(a.wh_no) f1,
                       a.wh_no,
                       a.mmcode f2,
                       b.mmname_c f3,
                       a.store_loc f4,
                       a.store_loc as ori_store_loc,
                       DECODE(b.e_sourcecode,'P','買斷','C','寄售','') f6 --買斷寄庫
                from MI_WLOCINV a, MI_MAST b
               where a.mmcode = b.mmcode
                 and a.wh_no in (select wh_no from MI_WHMAST where wh_grade='1' and wh_kind in ('0','1'))
            ";

            //var sql = @"
            //    with temp_mi_wexpinv as (
            //        select mmcode, exp_date, listagg(lot_no, ',') within group (order by exp_date) as lotList
            //        from mi_wexpinv
            //        group by mmcode, exp_date
            //    )
            //    SELECT
            //        a.wh_no || ' '||wh_name(a.wh_no) f1,
            //        a.wh_no, 
            //        a.mmcode f2,        --藥材代碼
            //        b.mmname_c f3,      --藥材名稱
            //        a.store_loc f4,     --儲位位置
            //        a.store_loc as ori_store_loc,
            //        round(a.inv_qty,0) f5,   --現存數量
            //        DECODE(b.e_sourcecode,'P','買斷','C','寄售','') f6, --買斷寄庫
            //        a.loc_note f7,      --備註
            //        twn_date(c.exp_date) f8,      --末效期
            //        c.lotList f9         --批號
            //    FROM
            //        mi_wlocinv a, mi_mast b
            //        left join temp_mi_wexpinv c on(b.mmcode = c.mmcode)
            //    WHERE
            //        a.mmcode = b.mmcode
            //        AND   a.wh_no IN (
            //            SELECT
            //                wh_no
            //            FROM
            //                mi_whmast
            //            WHERE
            //                wh_grade = '1'
            //                AND   wh_kind IN (
            //                    '0',
            //                    '1'
            //                )
            //        )";

            if (p0.Trim() != "")
            {
                sql += " AND B.M_AGENNO like :M_AGENNO ";
                p.Add(":M_AGENNO", string.Format("%{0}%", p0));
            }

            if (p1.Trim() != "")
            {
                sql += " AND A.MMCODE like :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", p1));
            }

            if (p2.Trim() != "")
            {
                if (p2.Trim().Length == 2)
                {
                    sql += " and B.MAT_CLASS = :MAT_CLASS ";
                    p.Add(":MAT_CLASS", p2.Trim());
                }
                else
                {
                    sql += " AND B.MAT_CLASS_SUB like :MAT_CLASS_SUB ";
                    p.Add(":MAT_CLASS_SUB", string.Format("%{0}%", p2.Trim()));
                }
            }

            if (p3.Trim() != "")
            {
                sql += " AND A.WH_NO = :WH_NO";
                p.Add(":WH_NO", p3);
            }

            if (p4 == "Y")
            {
                sql += " and (a.STORE_LOC in ('TMPLOC','NEWPOS') or trim(a.STORE_LOC) is null) ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0175MasterMODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<PH_VENDER> GetAgenNoCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.AGEN_NO, A.AGEN_NAMEC, A.EASYNAME
                          FROM PH_VENDER A 
                         WHERE 1=1 ";

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.AGEN_NO, :AGEN_NO_I), 1000) + NVL(INSTR(A.EASYNAME, :EASYNAME_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大                             
                p.Add(":AGEN_NO_I", p0);
                p.Add(":EASYNAME_I", p0);

                sql += " AND (A.AGEN_NO LIKE :AGEN_NO";
                p.Add(":AGEN_NO", string.Format("%{0}%", p0));

                sql += " OR A.EASYNAME LIKE :EASYNAME) ";
                p.Add(":EASYNAME", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, AGEN_NO", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.AGEN_NO ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<PH_VENDER>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);

        }

        public IEnumerable<COMBO_MODEL> GetWhnoCombo()
        {
            string sql = string.Empty;
            sql = @"
                    select wh_no as value,wh_name as text 
                      from MI_WHMAST 
                     where nvl(cancel_id, 'N')='N'
                       and wh_grade ='1' and wh_kind in ('0','1')
                     order by wh_grade, wh_no
                ";
            return DBWork.Connection.Query<COMBO_MODEL>(sql, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"
                                    SELECT {0} 
                                           a.mmcode,
                                           a.mmname_c,
                                           a.mmname_e
                                     FROM  MI_MAST a
                                    WHERE  1 = 1
                                      AND  nvl(cancel_id, 'N') = 'N'
                                ";

            if (!string.IsNullOrWhiteSpace(p0))
            {
                sql = string.Format(sql, "(NVL(INSTR(UPPER(A.MMCODE), UPPER(:MMCODE_I)), 1000) + NVL(INSTR(UPPER(A.MMNAME_E), UPPER(:MMNAME_E_I)), 100) * 10 + NVL(INSTR(UPPER(A.MMNAME_C), UPPER(:MMNAME_C_I)), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (UPPER(A.MMCODE) LIKE UPPER(:MMCODE) ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

                sql += " OR UPPER(A.MMNAME_E) LIKE UPPER(:MMNAME_E) ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR UPPER(A.MMNAME_C) LIKE UPPER(:MMNAME_C)) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " ORDER BY A.MMCODE ";
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<COMBO_MODEL> GetMatClassSubCombo()
        {
            string sql = @" select '01' as value, '全部藥品' as text from dual
                    union
                    select '02' as value, '全部衛材' as text from dual
                    union
                    SELECT
                        data_value AS value, data_desc AS text
                    FROM
                        param_d
                    WHERE
                        data_name = 'MAT_CLASS_SUB'
                    ORDER BY
                        value";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public string MasterCreate(AA0175 input)
        {
            string sql = @" insert into MI_WLOCINV(WH_NO, MMCODE, STORE_LOC, INV_QTY, LOC_NOTE, CREATE_TIME, CREATE_USER) 
                                                values(:WH_NO, :MMCODE, :STORE_LOC, :INV_QTY, :LOC_NOTE, sysdate, :CREATE_USER)";

            int effRows = DBWork.Connection.Execute(sql, input, DBWork.Transaction);

            if (effRows >= 1)
                return "新增成功";
            else
                return "新增失敗";
        }

        public int CreateImport(AA0175 input)
        {
            string sql = @" insert into MI_WLOCINV(WH_NO, MMCODE, STORE_LOC, INV_QTY, LOC_NOTE, CREATE_TIME, CREATE_USER) 
                                                values(:WH_NO, :MMCODE, :STORE_LOC, :INV_QTY, :LOC_NOTE, sysdate, :CREATE_USER)";

            return DBWork.Connection.Execute(sql, input, DBWork.Transaction);
        }

        public int MasterUpdate(AA0175 input)
        {
            var sql = @"update MI_WLOCINV
                         SET INV_QTY=:INV_QTY, LOC_NOTE=:LOC_NOTE, UPDATE_USER=:UPDATE_USER, UPDATE_IP=:UPDATE_IP , UPDATE_TIME=SYSDATE
                        where WH_NO=:WH_NO AND MMCODE=:MMCODE AND STORE_LOC=:STORE_LOC ";
            return DBWork.Connection.Execute(sql, input, DBWork.Transaction);
        }

        public int MasterDelete(AA0175 input)
        {
            var sql = @"delete from MI_WLOCINV where WH_NO=:WH_NO AND MMCODE=:MMCODE AND STORE_LOC=:STORE_LOC ";
            return DBWork.Connection.Execute(sql, input, DBWork.Transaction);
        }
        public int MasterDeleteOri(AA0175 input)
        {
            var sql = @"delete from MI_WLOCINV where WH_NO=:WH_NO AND MMCODE=:MMCODE AND STORE_LOC=:ORI_STORE_LOC ";
            return DBWork.Connection.Execute(sql, input, DBWork.Transaction);
        }

        public bool CheckMasterExists(string wh_no, string mmcode, string store_loc)
        {
            string sql = @"SELECT 1 FROM MI_WLOCINV WHERE WH_NO=:WH_NO AND MMCODE=:MMCODE AND STORE_LOC=:STORE_LOC";
            return DBWork.Connection.ExecuteScalar(sql, new { WH_NO = wh_no, MMCODE = mmcode, STORE_LOC = store_loc }, DBWork.Transaction) != null;
        }

        public bool CheckExists_WHmast(string wh_no, string mmcode)
        {
            string sql = @"select 1 
                             from MI_WHMAST 
                            where WH_NO=:WH_NO AND WH_GRADE='1' AND WH_KIND=(CASE WHEN (SELECT MAT_CLASS FROM MI_MAST WHERE MMCODE=:MMCODE) = '01' THEN '0' ELSE '1' END )                            
                          ";
            return (DBWork.Connection.ExecuteScalar(sql, new { WH_NO = wh_no, MMCODE = mmcode }, DBWork.Transaction) == null);
        }

        public DataTable GetExcel(string p0, string p1, string p2, string p3, string p4)
        {
            DynamicParameters p = new DynamicParameters();

            var sql = @"
                select a.wh_no || ' '|| wh_name(a.wh_no) 庫房代碼,
                       a.mmcode 藥材代碼,
                       b.mmname_c 藥材名稱,
                       a.store_loc 儲位位置,
                       DECODE(b.e_sourcecode,'P','買斷','C','寄售','') 買斷寄庫,
                       a.loc_note 備註 
                from MI_WLOCINV a, MI_MAST b
               where a.mmcode = b.mmcode
                 and a.wh_no in (select wh_no from MI_WHMAST where wh_grade='1' and wh_kind in ('0','1'))
            ";

            //var sql = @"WITH temp_mi_wexpinv AS (
            //        SELECT
            //            mmcode, exp_date, LISTAGG(lot_no, ',') WITHIN GROUP(ORDER BY exp_date ) AS lotlist
            //        FROM
            //            mi_wexpinv
            //        GROUP BY
            //            mmcode, exp_date
            //    ) SELECT
            //        a.wh_no 庫房代碼,
            //        a.mmcode 藥材代碼,
            //        b.mmname_c 藥材名稱,
            //        a.store_loc 儲位位置,
            //        round(a.inv_qty,0) 儲位數量,
            //        DECODE(b.e_sourcecode,'P','買斷','C','寄售','') 買斷寄庫,
            //        a.loc_note 備註,
            //        twn_date(c.exp_date) 末效期,
            //        c.lotlist 批號
            //    FROM
            //        mi_wlocinv a,
            //        mi_mast b
            //        left join temp_mi_wexpinv c on(b.mmcode = c.mmcode)
            //    WHERE
            //        a.mmcode = b.mmcode
            //        AND   a.wh_no IN (
            //            SELECT
            //                wh_no
            //            FROM
            //                mi_whmast
            //            WHERE
            //                wh_grade = '1'
            //                AND   wh_kind IN (
            //                    '0',
            //                    '1'
            //                )
            //        )";

            if (p0.Trim() != "")
            {
                sql += " AND B.M_AGENNO like :M_AGENNO ";
                p.Add(":M_AGENNO", string.Format("%{0}%", p0));
            }

            if (p1.Trim() != "")
            {
                sql += " AND A.MMCODE like :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", p1));
            }

            if (p2.Trim() != "")
            {
                if (p2.Trim().Length == 2)
                {
                    sql += " and B.MAT_CLASS = :MAT_CLASS ";
                    p.Add(":MAT_CLASS", p2.Trim());
                }
                else
                {
                    sql += " AND B.MAT_CLASS_SUB like :MAT_CLASS_SUB ";
                    p.Add(":MAT_CLASS_SUB", string.Format("%{0}%", p2.Trim()));
                }
            }

            if (p3.Trim() != "")
            {
                sql += " AND A.WH_NO = :WH_NO ";
                p.Add(":WH_NO", p3);
            }

            if (p4 == "Y")
            {
                sql += " and (a.STORE_LOC in ('TMPLOC','NEWPOS') or trim(a.STORE_LOC) is null) ";
            }

            sql += @" ORDER BY A.MMCODE";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public class AA0175MasterMODEL : JCLib.Mvc.BaseModel
        {
            public string rnm { get; set; }
            public string F1 { get; set; }
            public string F2 { get; set; }
            public string F3 { get; set; }
            public string F4 { get; set; }
            public string F5 { get; set; }
            public string F6 { get; set; }
            public string F7 { get; set; }
            public string F8 { get; set; }
            public string F9 { get; set; }

            public string ORI_STORE_LOC { get; set; }
            public string WH_NO { get; set; }
        }
    }
}