using System;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using System.Text;
using JCLib.Mvc;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;

namespace WebApp.Repository.F
{
    public class FA0080Repository : JCLib.Mvc.BaseRepository
    {
        public FA0080Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<FA0080MasterMODEL> GetAllM(string p0, string p1, string p4, string p5, string p6, string p7, string menulink, string user, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            //1130124 因his扣庫的TR_DOCNO不存在me_docm需改成left join
            var sql = @"with start_time as (
                            select set_btime as st from MI_MNSET where set_ym = :p0
                            ), end_time as (
                            select nvl(set_ctime, set_etime) as ed from MI_MNSET where set_ym=:p1
                            ) 
                         SELECT 1 F1,twn_date(b.st) F2,0 F3,0 F4,0 F5,0 F6,0 F7,0 F8,0 F9, 0 F10,0 F11,0 F12, 0 F13, 0 F14, 0 F15, 0 F16, 
                                NVL(inv_qty,0) F17,'上月結存' F18, '' update_user, '' update_time 
                           FROM MI_WINVMON a, start_time b
                          where data_ym = twn_pym(:p0) and wh_no = :p4 and mmcode = :p5 
                         UNION
                         SELECT * FROM
                         (
                         select rownum+1 as F1 ,
                                (case when tr_mcode = 'USEO' then substr(A.TR_DOCNO,1,7) else twn_date(A.tr_date) end) F2,
                                case when tr_mcode in ('APLI') then tr_inv_qty else 0 end F3,  --進貨/撥發入
                                case when tr_mcode in ('APLO') then tr_inv_qty else 0 end F4,  --撥發出
                                case when tr_mcode in ('TRNI') then tr_inv_qty else 0 end F5,    --調撥入
                                case when tr_mcode in ('TRNO') then tr_inv_qty else 0 end F6,   --調撥出
                                case when tr_mcode in ('ADJI') then tr_inv_qty else 0 end F7,      --調帳入
                                case when tr_mcode in ('ADJO') then tr_inv_qty else 0 end F8,  --調帳出
                                case when tr_mcode in ('BAKI') then tr_inv_qty else 0 end F9,     --退料入
                                case when tr_mcode in ('BAKO') then tr_inv_qty else 0 end F10,  --退料出
                                case when tr_mcode in ('REJO') then tr_inv_qty else 0 end F11,  --退貨
                                case when tr_mcode in ('DISO') then tr_inv_qty else 0 end F12, --報廢
                                case when tr_mcode in ('EXGI') then tr_inv_qty else 0 end F13,  --換貨入
                                case when tr_mcode in ('EXGO') then tr_inv_qty else 0 end F14,  --換貨出                       
                               -- case when tr_mcode in ('USEI') then tr_inv_qty else 0 end F,    
                                case when tr_mcode in ('USEO') then tr_inv_qty else 0 end F15, --消耗          
                                case when tr_mcode in ('CHIO') then tr_inv_qty else 0 end F16,  --盤點
                                af_tr_invqty F17,
                                wh_name(case when tr_io = 'I' then frwh else towh end) ||'('||tr_docno||')' F18,
                                user_name(b.update_user) as UPDATE_USER, 
                                twn_time(b.update_time) as UPDATE_TIME
                         from mi_whtrns a
                         left join me_docm b on a.tr_docno = b.docno 
                           where (case when tr_mcode='USEO' then TWN_date(A.TR_DATE-1) else TWN_date(A.TR_DATE) end) BETWEEN twn_date((select st from start_time)) AND twn_date((select ed from end_time)) 
                           AND A.WH_NO = :p4
                           AND A.MMCODE = :p5
                           and a.tr_mcode not like 'WA%' ";

            if (menulink == "AB0147")
            {
                sql += " AND EXISTS ( SELECT 1 FROM MI_MAST WHERE MMCODE=A.MMCODE AND E_RESTRICTCODE <> 'N' ) ";
                p.Add(":p6", p6);
            }
            if (menulink == "AB0150" || menulink == "AB0154" || menulink == "FA0080")
            {
                sql += @" and ((select count(*) from MI_WHID where WH_NO = A.WH_NO and WH_USERID = :userid ) > 0 
                                or (select count(*) from UR_UIR where RLNO in ('MAT_14', 'MED_14', 'MMSpl_14') and TUSER = :userid) > 0) ";
                p.Add(":userid", user);
            }
            if (menulink == "AB0148" || menulink == "AB0150" || menulink == "AB0154")
            {
                sql += " and b.MAT_CLASS='01' ";  //只可查詢藥品
            }
            //if (p7 == "N")
            //{
            //    sql += " and a.TR_INV_QTY > 0 ";
            //}
            sql += @"             ORDER BY TR_DATE 
                         ) ORDER BY 1  ";

            p.Add(":p0", string.Format("{0}", p0));
            p.Add(":p1", string.Format("{0}", p1));
            p.Add(":p4", string.Format("{0}", p4));
            p.Add(":p5", string.Format("{0}", p5));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<FA0080MasterMODEL>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<FA0080ReportMODEL> GetPrintData(string p0, string p1, string p4, string p5, string p6, string p7, string menulink, string user)
        {
            var p = new DynamicParameters();

            var sql = @" with start_time as (
                            select set_btime as st from MI_MNSET where set_ym = :p0
                            ), end_time as (
                            select nvl(set_ctime, set_etime) as ed from MI_MNSET where set_ym=:p1
                            ) 
                         SELECT 1 F1,twn_date(b.st) F2,0 F3,0 F4,0 F5,0 F6,0 F7,0 F8,0 F9, 0 F10,0 F11,0 F12, 0 F13, 0 F14, 0 F15, 0 F16, 
                                NVL(inv_qty,0) F17,'上月結存' F18
                           FROM MI_WINVMON a, start_time b
                          where data_ym = twn_pym(:p0) and wh_no = :p4 and mmcode = :p5 
                         UNION
                         SELECT * FROM
                         (
                         select rownum+1 as F1 ,
                                (case when tr_mcode = 'USEO' then twn_date(A.tr_date - 1) else twn_date(A.tr_date) end) F2,
                                case when tr_mcode in ('APLI') then tr_inv_qty else 0 end F3,   --進貨/撥發入
                                case when tr_mcode in ('APLO') then tr_inv_qty else 0 end F4,  --撥發出
                                case when tr_mcode in ('TRNI') then tr_inv_qty else 0 end F5,    --調撥入
                                case when tr_mcode in ('TRNO') then tr_inv_qty else 0 end F6,   --調撥出
                                case when tr_mcode in ('ADJI') then tr_inv_qty else 0 end F7,      --調帳入
                                case when tr_mcode in ('ADJO') then tr_inv_qty else 0 end F8,  --調帳出
                                case when tr_mcode in ('BAKI') then tr_inv_qty else 0 end F9,     --退料入
                                case when tr_mcode in ('BAKO') then tr_inv_qty else 0 end F10,  --退料出
                                case when tr_mcode in ('REJO') then tr_inv_qty else 0 end F11,  --退貨
                                case when tr_mcode in ('DISO') then tr_inv_qty else 0 end F12, --報廢
                                case when tr_mcode in ('EXGI') then tr_inv_qty else 0 end F13,  --換貨入
                                case when tr_mcode in ('EXGO') then tr_inv_qty else 0 end F14,  --換貨出                       
                               -- case when tr_mcode in ('USEI') then tr_inv_qty else 0 end F,    
                                case when tr_mcode in ('USEO') then tr_inv_qty else 0 end F15, --消耗              
                                case when tr_mcode in ('CHIO') then tr_inv_qty else 0 end F16,  --盤點
                                af_tr_invqty F17,
                                wh_name(case when tr_io = 'I' then frwh else towh end) ||'('||tr_docno||')' F18
                           from mi_whtrns a
                           left join me_docm b on a.tr_docno = b.docno 
                          where (case when tr_mcode='USEO' then TWN_date(A.TR_DATE-1) else TWN_date(A.TR_DATE) end) BETWEEN twn_date((select st from start_time)) AND twn_date((select ed from end_time))  
                            AND A.WH_NO = :p4
                            AND A.MMCODE = :p5  ";
            if (menulink == "AB0147")
            {
                sql += " AND EXISTS ( SELECT 1 FROM MI_MAST WHERE MMCODE=A.MMCODE AND E_RESTRICTCODE <> 'N' ) ";
                p.Add(":p6", p6);
            }
            if (menulink == "AB0150" || menulink == "AB0154" || menulink == "FA0080")
            {
                sql += @" and ((select count(*) from MI_WHID where WH_NO= A.WH_NO and WH_USERID = :userid ) > 0 
                                    or (select count(*) from UR_UIR where RLNO in ('MAT_14', 'MED_14', 'MMSpl_14') and TUSER = :userid) > 0) ";
                p.Add(":userid", user);
            }
            if (menulink == "AB0148" || menulink == "AB0150" || menulink == "AB0154")
            {
                sql += " and b.MAT_CLASS='01' ";  //只可查詢藥品
            }
            if (p7 == "N")
            {
                sql += " and a.TR_INV_QTY > 0 ";
            }
            sql += @"    
                         ORDER BY TR_DATE 
                         ) ORDER BY 1  ";

            p.Add(":p0", string.Format("{0}", p0));
            p.Add(":p1", string.Format("{0}", p1));
            p.Add(":p4", string.Format("{0}", p4));
            p.Add(":p5", string.Format("{0}", p5));

            return DBWork.Connection.Query<FA0080ReportMODEL>(sql, p, DBWork.Transaction);
        }

        public DataTable GetExcel(string p0, string p1, string p4, string p5, string p6, string p7, string menulink, string user)
        {
            var p = new DynamicParameters();

            var sql = @" with start_time as (
                            select set_btime as st from MI_MNSET where set_ym = :p0
                            ), end_time as (
                            select nvl(set_ctime, set_etime) as ed from MI_MNSET where set_ym=:p1
                            ) 
                         SELECT 1 項次,twn_date(b.st) 日期,0 ""進貨/撥發入"",0 撥發出,0 調撥入,0 調撥出,0 調帳入,0 調帳出,
                                0 退料入, 0 退料出,0 退貨,0 報廢, 0 換貨入, 0 換貨出, 0 消耗, 0 盤點差異, 
                                NVL(inv_qty,0) 結存,'上月結存' 進出單位 
                           FROM MI_WINVMON a, start_time b
                          where data_ym = twn_pym(:p0) and wh_no = :p4 and mmcode = :p5 
                         UNION
                         SELECT * FROM
                         (
                         select rownum+1 as 項次 ,
                                (case when tr_mcode = 'USEO' then twn_date(A.tr_date - 1) else twn_date(A.tr_date) end) 日期,
                                case when tr_mcode in ('APLI') then tr_inv_qty else 0 end  as ""進貨/撥發入"",
                                case when tr_mcode in ('APLO') then tr_inv_qty else 0 end 撥發出,
                                case when tr_mcode in ('TRNI') then tr_inv_qty else 0 end 調撥入,
                                case when tr_mcode in ('TRNO') then tr_inv_qty else 0 end 調撥出,
                                case when tr_mcode in ('ADJI') then tr_inv_qty else 0 end 調帳入,
                                case when tr_mcode in ('ADJO') then tr_inv_qty else 0 end 調帳出,
                                case when tr_mcode in ('BAKI') then tr_inv_qty else 0 end 退料入,
                                case when tr_mcode in ('BAKO') then tr_inv_qty else 0 end 退料出,
                                case when tr_mcode in ('REJO') then tr_inv_qty else 0 end 退貨,
                                case when tr_mcode in ('DISO') then tr_inv_qty else 0 end 報廢,
                                case when tr_mcode in ('EXGI') then tr_inv_qty else 0 end 換貨入,
                                case when tr_mcode in ('EXGO') then tr_inv_qty else 0 end 換貨出,  
                               -- case when tr_mcode in ('USEI') then tr_inv_qty else 0 end,    
                                case when tr_mcode in ('USEO') then tr_inv_qty else 0 end 消耗, 
                                case when tr_mcode in ('CHIO') then tr_inv_qty else 0 end 盤點差異,
                                af_tr_invqty 結存,
                                wh_name(case when tr_io = 'I' then frwh else towh end) ||'('||tr_docno||')' 進出單位
                         from mi_whtrns a
                         left join me_docm b on a.tr_docno = b.docno
                         where (case when tr_mcode='USEO' then TWN_date(A.TR_DATE-1) else TWN_date(A.TR_DATE) end) BETWEEN twn_date((select st from start_time)) AND twn_date((select ed from end_time)) 
                           AND A.WH_NO = :p4
                           AND A.MMCODE = :p5  ";
            if (menulink == "AB0147")
            {
                sql += " AND EXISTS ( SELECT 1 FROM MI_MAST WHERE MMCODE=A.MMCODE AND E_RESTRICTCODE <> 'N' ) ";
                p.Add(":p6", p6);
            }
            if (menulink == "AB0150" || menulink == "AB0154" || menulink == "FA0080")
            {
                sql += @" and ((select count(*) from MI_WHID where WH_NO = A.WH_NO and WH_USERID = :userid ) > 0 
                                            or (select count(*) from UR_UIR where RLNO in ('MAT_14', 'MED_14', 'MMSpl_14') and TUSER = :userid) > 0) ";
                p.Add(":userid", user);
            }
            if (menulink == "AB0148" || menulink == "AB0150" || menulink == "AB0154")
            {
                sql += " and b.MAT_CLASS='01' ";  //只可查詢藥品
            }
            if (p7 == "N")
            {
                sql += " and a.TR_INV_QTY > 0 ";
            }
            sql += @"    
                         ORDER BY TR_DATE 
                         ) ORDER BY 1  ";

            p.Add(":p0", string.Format("{0}", p0));
            p.Add(":p1", string.Format("{0}", p1));
            p.Add(":p4", string.Format("{0}", p4));
            p.Add(":p5", string.Format("{0}", p5));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public DataTable GetLastRec(string p0, string p1, string p4, string p5)
        {
            var p = new DynamicParameters();

            var sql = @" select TWN_YYYMM(A.tr_date) yyymm, wh_no, mmcode  
                           from mi_whtrns a, me_docm b
                          where a.tr_docno = b.docno 
                            and wh_name(case when a.tr_io = 'I' then frwh else towh end) like '藥劑科%' 
                            AND TWN_YYYMM(A.TR_DATE) BETWEEN :p0 AND :p1 
                            AND A.WH_NO = :p4
                            AND A.MMCODE = :p5 
                            ORDER BY TR_DATE
                        ";

            p.Add(":p0", string.Format("{0}", p0));
            p.Add(":p1", string.Format("{0}", p1));
            p.Add(":p4", string.Format("{0}", p4));
            p.Add(":p5", string.Format("{0}", p5));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<COMBO_MODEL> GetMatClassSubCombo()
        {
            string sql = @" select DATA_VALUE as VALUE, DATA_DESC as TEXT
                            from PARAM_D
                            where GRP_CODE ='MI_MAST' 
                            and DATA_NAME = 'MAT_CLASS_SUB'
                            and trim(DATA_DESC) is not null
                            order by VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetCommonCombo()
        {
            string sql = @" SELECT VALUE, TEXT FROM (
                            SELECT '1' VALUE, '非常用品' TEXT FROM DUAL
                            UNION
                            SELECT '2' VALUE, '常用品' TEXT FROM DUAL
                            UNION
                            SELECT '3' VALUE, '藥品' TEXT FROM DUAL
                            UNION
                            SELECT '4' VALUE, '檢驗' TEXT FROM DUAL
                            ) ORDER BY VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }

        public IEnumerable<COMBO_MODEL> GetERestrictCodeCombo()
        {
            string sql = @" SELECT DISTINCT DATA_VALUE as VALUE, DATA_DESC as TEXT ,
                        DATA_VALUE||' '||DATA_DESC as COMBITEM 
                        FROM PARAM_D
                        WHERE GRP_CODE='MI_MAST' AND DATA_NAME='E_RESTRICTCODE' 
                          AND DATA_VALUE <> 'N' 
                        ORDER BY DATA_VALUE ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql);
        }
        public IEnumerable<MI_WHMAST> GetWhnoCombo(string p0, string menulink, string userid, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT DISTINCT {0} A.WH_NO, A.WH_NAME, A.WH_KIND, A.WH_GRADE FROM MI_WHMAST A, MI_WHID b 
                         WHERE b.wh_no = a.wh_no ";

            sql += @" and ( b.wh_userid = :userid 
                           or (select count(*) from UR_UIR where RLNO in ('MAT_14', 'MED_14', 'MMSpl_14') and TUSER = :userid and TUSER = b.wh_userid) > 0 ) ";

            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.WH_NO, :WH_NO_I), 1000) + NVL(INSTR(A.WH_NAME, :WH_NAME_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大                
                p.Add(":WH_NO_I", p0);
                p.Add(":WH_NAME_I", p0);

                sql += " AND (A.WH_NO LIKE :WH_NO ";
                p.Add(":WH_NO", string.Format("%{0}%", p0));

                sql += " OR A.WH_NAME LIKE :WH_NAME) ";
                p.Add(":WH_NAME", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, WH_NO", sql);
            }
            else
            {
                sql = string.Format(sql, "");
                sql += " order by A.WH_GRADE,A.WH_KIND,A.WH_NO ";
            }
            p.Add(":userid", userid);
            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_WHMAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);

        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, string p2, string p3, string p4, string p6, string menulink, string user, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT distinct {0} A.MMCODE, B.MMNAME_C, B.MMNAME_E, B.MAT_CLASS, B.BASE_UNIT 
                        FROM  MI_WHINV A, MI_MAST B 
                        WHERE A.MMCODE = B.MMCODE  
                    {1} ";

            if (p2 != "" && p2 != null)
            {
                sql += " AND B.MAT_CLASS_SUB = :p2 ";
                p.Add(":p2", string.Format("{0}", p2));
            }
            if (p3 != "" && p3 != null)
            {
                sql += " AND B.COMMON = :p3 ";
                p.Add(":p3", string.Format("{0}", p3));
            }
            if (p4 != "" && p4 != null)
            {
                sql += " AND A.WH_NO = :p4 ";
                p.Add(":p4", string.Format("{0}", p4));
            }
            if (menulink == "AB0147")
            {
                sql += " AND B.E_RESTRICTCODE <> 'N' ";
            }
            if (menulink == "AB0150" || menulink == "AB0154" || menulink == "FA0080")
            {
                sql += @" and ((select count(*) from MI_WHID where WH_NO = A.WH_NO and WH_USERID = :userid ) > 0 
                                            or (select count(*) from UR_UIR where RLNO in ('MAT_14', 'MED_14', 'MMSpl_14') and TUSER = :userid) > 0) ";
                p.Add(":userid", user);
            }
            if (menulink == "AB0148" || menulink == "AB0150" || menulink == "AB0154")
            {
                sql += " and b.MAT_CLASS='01' ";  //只可查詢藥品
            }

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

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX", sql);
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


        public string BF_TR_INVQTY(string p0, string p4, string p5)
        {
            string sql = @" SELECT NVL(BF_TR_INVQTY(:P4,:P5,:P0),'') FROM DUAL ";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { P0 = p0, P4 = p4, P5 = p5 }, DBWork.Transaction);
        }

        public string AF_TR_INVQTY1(string p0, string p4, string p5)
        {
            string sql = @" SELECT NVL(AF_TR_INVQTY1(:P4,:P5,:P0),'') FROM DUAL ";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { P0 = p0, P4 = p4, P5 = p5 }, DBWork.Transaction);
        }

        public string AF_TR_INVQTY2(string p0, string p4, string p5)
        {
            string sql = @" SELECT NVL(AF_TR_INVQTY2(:P4,:P5,:P0),'') FROM DUAL ";
            return DBWork.Connection.ExecuteScalar<string>(sql, new { P0 = p0, P4 = p4, P5 = p5 }, DBWork.Transaction);
        }

        public bool CheckIs01MmCode(string mmcode)
        {
            string sql = @"
               select 1 from MI_MAST  a
                where a.mmcode = :mmcode
                  and a.mat_class = '01' 
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { mmcode }, DBWork.Transaction) == null;
        }

        public bool CheckIsRestrictcode(string mmcode)
        {
            string sql = @"
                select 1 from MI_MAST a
                 where a.mmcode = :mmcode
                   and a.mat_class = '01'
                   and a.e_restrictcode in ('0','1','2','3','4')
            ";
            return DBWork.Connection.ExecuteScalar(sql, new { mmcode }, DBWork.Transaction) == null;
        }

        public string GetCursetym()
        {
            string sql = @"
            select cur_setym from dual
        ";
            return DBWork.Connection.QueryFirstOrDefault<string>(sql, DBWork.Transaction);
        }
    }


    public class FA0080ReportMODEL : JCLib.Mvc.BaseModel
    {
        public string F1 { get; set; }
        public string F2 { get; set; }
        public float F3 { get; set; }
        public float F4 { get; set; }
        public float F5 { get; set; }
        public float F6 { get; set; }
        public float F7 { get; set; }
        public float F8 { get; set; }
        public float F9 { get; set; }
        public string F10 { get; set; }
        public string F11 { get; set; }
        public string F12 { get; set; }
        public string F13 { get; set; }
        public string F14 { get; set; }
        public string F15 { get; set; }
        public string F16 { get; set; }
        public string F17 { get; set; }
        public string F18 { get; set; }

    }
    public class FA0080MasterMODEL : JCLib.Mvc.BaseModel
    {
        public string F1 { get; set; }
        public string F2 { get; set; }
        public string F3 { get; set; }
        public string F4 { get; set; }
        public string F5 { get; set; }
        public string F6 { get; set; }
        public string F7 { get; set; }
        public string F8 { get; set; }
        public string F9 { get; set; }
        public string F10 { get; set; }
        public string F11 { get; set; }
        public string F12 { get; set; }
        public string F13 { get; set; }
        public string F14 { get; set; }
        public string F15 { get; set; }
        public string F16 { get; set; }
        public string F17 { get; set; }
        public string F18 { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }

    }



}
