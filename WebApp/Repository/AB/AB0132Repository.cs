using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using WebApp.Models.AB;
using System.Collections.Generic;

namespace WebApp.Repository.AB
{
    public class AB0132Repository : JCLib.Mvc.BaseRepository
    {
        public AB0132Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<AB0132> GetAll(string start_date, string end_date, string visit_kind, string ordercode, string medno, string sectionno, string det_nrcode_14, string det_bedno_14,string wh_no, int page_index, int page_size, string sorters ,string menuLink="")
        {
            var p = new DynamicParameters();

            #region
            var sql = @" SELECT A.DATA_DATE DATA_DATE,  
                                A.ORDERCODE ORDERCODE,  
                                B.MMNAME_C MMNAME_C,  
                                A.ORDERDR ORDERDR,  
                                A.MEDNO MEDNO,  
                                A.SECTIONNO SECTIONNO,  
                                C.SECTIONNAME SECTIONNAME,  
                                A.DOSE DOSE,  
                                A.SUMQTY SUMQTY,  
                                A.CREATEOPID CREATEOPID,     
                                A.CREATEDATETIME CREATEDATETIME,  
                                A.ORDERUNIT ORDERUNIT,  
                                A.ATTACHUNIT ATTACHUNIT,  
                                DECODE(A.VISIT_KIND, '1','門診消耗','2','住院消耗','3','急診消耗','4','出院帶藥') VISIT_KIND,  
                                A.HIS14_DET_COST  DET_COST_14,  
                                A.HIS14_DET_DEPTCENTER  DET_DEPTCENTER_14,  
                                A.HIS14_DET_NRCODE  DET_NRCODE_14,  
                                A.HIS14_DET_BEDNO  DET_BEDNO_14,
                                A.STOCKCODE STOCKCODE
                         FROM HIS_CONSUME_D A left join SEC_MAST C on A.SECTIONNO = C.SECTIONNO, MI_MAST B
                         WHERE A.ORDERCODE = B.MMCODE ";
            #endregion

            if (menuLink == "AB0149")
            {
                sql += " AND B.MAT_CLASS = '01' ";
            }
            else if (menuLink == "FA0087")
            {
                sql += " AND B.MAT_CLASS != '01' ";
            }

            //判斷是否為消耗結存月報表，如果是要切換成月查詢
            if ((menuLink == "AB0151" || menuLink == "AB0152") && start_date.Trim() != "" && end_date.Trim() != "")
            {
                sql += " AND substr(A.DATA_DATE,1,5) BETWEEN :START_DATE AND :END_DATE ";
                p.Add(":START_DATE", start_date);
                p.Add(":END_DATE", end_date);

            } else if (start_date.Trim() != "" && end_date.Trim() != "")
            {
                sql += " AND A.DATA_DATE BETWEEN :START_DATE AND :END_DATE ";
                p.Add(":START_DATE", start_date);
                p.Add(":END_DATE", end_date);
            }

            if (visit_kind.Trim() != "")
            {
                sql += " AND A.VISIT_KIND = :VISIT_KIND_TEXT ";
                p.Add(":VISIT_KIND_TEXT", visit_kind);
            }
            if (ordercode.Trim() != "")
            {
                sql += " AND A.ORDERCODE = :ORDERCODE_TEXT ";
                p.Add(":ORDERCODE_TEXT", ordercode);
            }
            if (medno.Trim() != "")
            {
                sql += " AND A.MEDNO = :MEDNO_TEXT ";
                p.Add(":MEDNO_TEXT", medno);
            }
            if (sectionno.Trim() != "")
            {
                sql += " AND A.SECTIONNO = :SECTIONNO_TEXT ";
                p.Add(":SECTIONNO_TEXT", sectionno);
            }
            if (det_nrcode_14.Trim() != "")
            {
                sql += " AND A.HIS14_DET_NRCODE= :DET_NRCODE_14_TEXT ";
                p.Add(":DET_NRCODE_14_TEXT", det_nrcode_14);
            }
            if (det_bedno_14.Trim() != "")
            {
                sql += " AND A.HIS14_DET_BEDNO = :DET_BEDNO_14 ";
                p.Add(":DET_BEDNO_14", det_bedno_14);
            }

            if (wh_no.Trim() != "")
            {
                sql += " AND A.STOCKCODE = :STOCKCODE ";
                p.Add(":STOCKCODE", wh_no);

            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AB0132>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public DataTable GetExcel(string start_date, string end_date, string visit_kind, string ordercode, string medno, string sectionno, string det_nrcode_14, string det_bedno_14,string wh_no, string menuLink)
        {
            var p = new DynamicParameters();

            #region
            var sql = @" SELECT A.DATA_DATE AS 日期,  
                                A.ORDERCODE AS 藥品代碼,  
                                B.MMNAME_C AS 藥品名稱,  
                                A.ORDERDR AS 開立醫師,  
                                A.MEDNO AS 病歷號碼,  
                                A.SECTIONNO AS 科別代碼,  
                                C.SECTIONNAME AS 科別名稱,  
                                A.DOSE AS 開立劑量,  
                                A.SUMQTY AS 總量,  
                                A.CREATEOPID AS 建立人員,     
                                A.CREATEDATETIME AS 建立日期時間,  
                                A.ORDERUNIT AS 單位,  
                                A.ATTACHUNIT AS 劑型單位,  
                                DECODE(A.VISIT_KIND, '1','門診消耗','2','住院消耗','3','急診消耗','4','出院帶藥') AS 門急住診別,  
                                A.HIS14_DET_COST AS 成本,  
                                A.HIS14_DET_DEPTCENTER AS 成本中心部門,  
                                A.HIS14_DET_NRCODE AS 護理站代碼,  
                                A.HIS14_DET_BEDNO AS 床位號,
                                A.STOCKCODE AS 扣庫地點
                         FROM HIS_CONSUME_D A left join SEC_MAST C on A.SECTIONNO = C.SECTIONNO, MI_MAST B
                         WHERE A.ORDERCODE = B.MMCODE ";
            #endregion

            if (menuLink == "AB0149")
            {
                sql += " AND B.MAT_CLASS = '01' ";
            }
            else if (menuLink == "FA0087")
            {
                sql += " AND B.MAT_CLASS != '01' ";
            }

            //判斷是否為消耗結存月報表，如果是要切換成月查詢
            if ((menuLink == "AB0151" || menuLink == "AB0152") && start_date.Trim() != "" && end_date.Trim() != "")
            {
                sql += " AND substr(A.DATA_DATE,1,5) BETWEEN :START_DATE AND :END_DATE ";
                p.Add(":START_DATE", start_date);
                p.Add(":END_DATE", end_date);

            }
            else if (start_date.Trim() != "" && end_date.Trim() != "")
            {
                sql += " AND A.DATA_DATE BETWEEN :START_DATE AND :END_DATE ";
                p.Add(":START_DATE", start_date);
                p.Add(":END_DATE", end_date);
            }

            if (visit_kind.Trim() != "")
            {
                sql += " AND A.VISIT_KIND = :VISIT_KIND_TEXT ";
                p.Add(":VISIT_KIND_TEXT", visit_kind);
            }
            if (ordercode.Trim() != "")
            {
                sql += " AND A.ORDERCODE = :ORDERCODE_TEXT ";
                p.Add(":ORDERCODE_TEXT", ordercode);
            }
            if (medno.Trim() != "")
            {
                sql += " AND A.MEDNO = :MEDNO_TEXT ";
                p.Add(":MEDNO_TEXT", medno);
            }
            if (sectionno.Trim() != "")
            {
                sql += " AND A.SECTIONNO = :SECTIONNO_TEXT ";
                p.Add(":SECTIONNO_TEXT", sectionno);
            }
            if (det_nrcode_14.Trim() != "")
            {
                sql += " AND A.HIS14_DET_NRCODE= :DET_NRCODE_14_TEXT ";
                p.Add(":DET_NRCODE_14_TEXT", det_nrcode_14);
            }
            if (det_bedno_14.Trim() != "")
            {
                sql += " AND A.HIS14_DET_BEDNO = :DET_BEDNO_14 ";
                p.Add(":DET_BEDNO_14", det_bedno_14);
            }

            if (wh_no.Trim() != "")
            {
                sql += " AND A.STOCKCODE = :STOCKCODE ";
                p.Add(":STOCKCODE", wh_no);
            }

            //新增排序(先依照日期再依藥品代碼,開立醫師及病歷號碼)
            sql += "ORDER BY DATA_DATE ASC, ORDERCODE ASC, ORDERDR ASC, MEDNO ASC ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public DataTable GetExcel2(string start_date, string end_date, string visit_kind, string ordercode, string menuLink)
        {
            var p = new DynamicParameters();

            #region
            var sql = @"SELECT :START_DATE AS 起始日期 , :END_DATE AS  結束日期, 
                    a.stockcode ||' '|| wh_name(a.stockcode) 庫房, a.PARENT_ORDERCODE 院內碼, 
                    b.mmname_c 中文品名, b.mmname_e 英文品名, sum(a.parent_consume_qty) 消耗數量 
                    --DECODE(A.VISIT_KIND, '1','門診消耗','2','住院消耗','3','急診消耗','4','出院帶藥') 門急住診別
                    from HIS_CONSUME_M a,
                    MI_MAST b
                    where a.PARENT_ORDERCODE = b.mmcode
                     ";
            #endregion

            if (menuLink == "AB0149")
            {
                sql += " AND B.MAT_CLASS = '01' ";
            }
            else if (menuLink == "FA0087")
            {
                sql += " AND B.MAT_CLASS != '01' ";
            }

            //判斷是否為消耗結存月報表，如果是要切換成月查詢
            if ((menuLink == "AB0151" || menuLink == "AB0152") && start_date.Trim() != "" && end_date.Trim() != "")
            {
                sql += " AND substr(A.DATA_DATE,1,5) BETWEEN :START_DATE AND :END_DATE ";
                p.Add(":START_DATE", start_date);
                p.Add(":END_DATE", end_date);

            }
            else if (start_date.Trim() != "" && end_date.Trim() != "")
            {
                sql += " AND A.DATA_DATE BETWEEN :START_DATE AND :END_DATE ";
                p.Add(":START_DATE", start_date);
                p.Add(":END_DATE", end_date);
            }

            if (visit_kind.Trim() != "")
            {
                sql += " AND A.VISIT_KIND = :VISIT_KIND_TEXT ";
                p.Add(":VISIT_KIND_TEXT", visit_kind);
            }
            if (ordercode.Trim() != "")
            {
                sql += " AND A.PARENT_ORDERCODE = :ORDERCODE_TEXT ";
                p.Add(":ORDERCODE_TEXT", ordercode);
            }

            sql += @" group by a.stockcode, a.parent_ordercode,
                    b.mmname_c, b.mmname_e, a.stock_unit
                    order by a.stockcode, a.PARENT_ORDERCODE ";

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }


        //藥品代碼combo
        public IEnumerable<MI_MAST> GetOrdercodeCombo(string p0, int page_index, int page_size, string sorters,string menuLink="")
        {
            var p = new DynamicParameters();

            string sql = @"  SELECT {0}
                                    MMCODE, 
                                    MMNAME_C,  
                                    MMNAME_E 
                             FROM MI_MAST
                             WHERE 1 = 1  ";

            if (menuLink != "")
            {
                if (menuLink == "AB0149")
                {
                    sql += " AND MAT_CLASS = '01' ";
                }
                else if (menuLink == "FA0087")
                {
                    sql += " AND MAT_CLASS != '01' ";
                }
            }
            else
            {
                sql += " AND MAT_CLASS = '01' ";
            }

            if (p0.Trim() != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(MMCODE, :MMCODE_I), 1000) + NVL(INSTR(MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,");
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += @"     AND ( MMCODE LIKE :MMCODE
                                    OR MMNAME_E LIKE UPPER(:MMNAME_E) 
                                    OR MMNAME_C LIKE UPPER(:MMNAME_C) )   ";
                p.Add(":MMCODE", string.Format("{0}%", p0));
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));
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

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p);
        }
        //科別代碼combo
        public IEnumerable<COMBO_MODEL> GetSectionNoCombo()
        {
            var p = new DynamicParameters();

            string sql = @"  SELECT SECTIONNO VALUE,
                                    SECTIONNO || ' ' || SECTIONNAME TEXT
                             FROM SEC_MAST
                             WHERE 1 = 1  
                             ORDER BY SECTIONNO ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }

        public string GetSetBTime()
        {
            string sql = @" SELECT TWN_DATE(SET_BTIME) SET_BTIME FROM MI_MNSET WHERE SET_STATUS = 'N' AND ROWNUM =1 ";
            return DBWork.Connection.ExecuteScalar(sql, DBWork.Transaction).ToString();
        }
        

        //庫房代碼combo
        public IEnumerable<COMBO_MODEL> GetWhnoCombo()
        {
            var p = new DynamicParameters();

            string sql = @"  SELECT  wh_no VALUE,wh_no ||' '|| wh_name TEXT
                             FROM MI_WHMASt 
                             WHERE wh_grade='2' AND wh_kind='0' ";

            return DBWork.Connection.Query<COMBO_MODEL>(sql, p);
        }
    }
}