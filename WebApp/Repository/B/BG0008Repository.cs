using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.B
{
    public class BG0008Repository : JCLib.Mvc.BaseRepository
    {
        public BG0008Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<BG0008> GetAll(string po_time_b, string po_time_e, string mat_class, string agen_no, string m_storeid, string m_contid, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"select a.PO_NO, a.agen_no, p.AGEN_NAMEC, P.AGEN_TEL, b.MMCODE, m.MMNAME_C, m.MMNAME_E, b.M_PURUN,nvl(PO_PRICE,0) PO_PRICE, DELI_QTY, PO_QTY,
                        (PO_QTY-DELI_QTY) NOIN_QTY, floor((PO_QTY-DELI_QTY)* nvl(PO_PRICE,0)) AMOUNT
                        FROM MM_PO_M a,MM_PO_D b, MI_MAST m , PH_VENDER p
                        where a.po_no=b.po_no and b.mmcode=m.mmcode and a.AGEN_NO=p.AGEN_NO
                        and substr(a.po_no,1,3) in ('INV','GEN') 
                        and a.po_time >= to_date(:po_time_b,'yyyy-mm-dd') 
                        and a.po_time <= to_date(:po_time_e,'yyyy-mm-dd')
                        and m.mat_class = :mat_class
                        and PO_QTY-DELI_QTY > 0 ";

            p.Add(":po_time_b", string.Format("{0}", DateTime.Parse(po_time_b).ToString("yyyy-MM-dd")));
            p.Add(":po_time_e", string.Format("{0}", DateTime.Parse(po_time_e).ToString("yyyy-MM-dd")));
            p.Add(":mat_class", string.Format("{0}", mat_class));

            if (agen_no != "")
            {
                sql += "  and a.agen_no =:agen_no ";
                p.Add(":agen_no", string.Format("{0}", agen_no));
            }
            if (m_storeid != "")
            {
                sql += " and b.storeid=:m_storeid ";
                p.Add(":m_storeid", string.Format("{0}", m_storeid));
            }
            if (m_contid != "")
            {
                sql += "  and a.m_contid= :m_contid  ";
                p.Add(":m_contid", string.Format("{0}", m_contid));
            }


            sql += " order by a.AGEN_NO, a.po_no, b.mmcode";

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<BG0008>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.MAT_CLASS, A.BASE_UNIT FROM MI_MAST A WHERE 1=1 ";


            if (p0 != "")
            {
                sql = string.Format(sql, "(NVL(INSTR(A.MMCODE, :MMCODE_I), 1000) + NVL(INSTR(A.MMNAME_E, :MMNAME_E_I), 100) * 10 + NVL(INSTR(A.MMNAME_C, :MMNAME_C_I), 100) * 10) IDX,"); // 設定權重, 值越小權重最大
                p.Add(":MMCODE_I", p0);
                p.Add(":MMNAME_E_I", p0);
                p.Add(":MMNAME_C_I", p0);

                sql += " AND (A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", p0));

                sql += " OR A.MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", p0));

                sql += " OR A.MMNAME_C LIKE :MMNAME_C) ";
                p.Add(":MMNAME_C", string.Format("%{0}%", p0));

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, MMCODE", sql);
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


        public IEnumerable<ComboItemModel> GetMATCombo()
        {
            string sql = @"Select MAT_CLASS||' '||MAT_CLSNAME TEXT, MAT_CLASS VALUE 
                            from MI_MATCLASS 
                            where MAT_CLASS between '02' AND '08' ORDER BY VALUE";
            return DBWork.Connection.Query<ComboItemModel>(sql, null, DBWork.Transaction);
        }

        public DataTable GetExcel(string po_time_b, string po_time_e, string mat_class, string agen_no, string m_storeid, string m_contid)
        {
            var p = new DynamicParameters();
            var sql = "";


            if (m_storeid == "1")
            {
                sql = @"select a.po_no 訂單編號,a.mat_class 物料分類,b.MMCODE 院內碼, b.MMNAME_E 英文品名, b.MMNAME_C 中文品名,'庫備' 是否庫備, 
                        (case when a.m_contid = '0' then '合約' when a.m_contid = '2' then '非合約' when a.m_contid = '3' then '小採' else '' end) 是否合約, 
                        a.agen_no 廠商碼, p.AGEN_NAMEC 廠商名稱, P.AGEN_TEL 廠商電話號碼, b.M_PURUN 包裝單位, 
                        m.m_contprice 單價,b.PO_QTY 訂單量, b.DELI_QTY 進貨量, (b.PO_QTY - b.DELI_QTY) 未進貨量,
                        floor((b.PO_QTY-b.DELI_QTY)* nvl(b.PO_PRICE,0)) 未進貨金額, v.inv_qty 庫存量
                            FROM MM_PO_M a, MM_PO_D b, MI_MAST m, PH_VENDER p , MI_WHINV v
                        where a.po_no = b.po_no and b.mmcode = m.mmcode and a.AGEN_NO = p.AGEN_NO
                        and a.wh_no = v.wh_no and b.mmcode = v.mmcode
                        and substr(a.po_no,1,3) in ('INV','GEN') 
                        and a.po_time >= to_date(:po_time_b, 'yyyy-mm-dd')
                        and a.po_time <= to_date(:po_time_e, 'yyyy-mm-dd')
                        and b.storeid = '1'
                        and m.mat_class = :mat_class 
                        and b.PO_QTY-b.DELI_QTY > 0 ";
            }
            else
            {
                sql = @"select a.po_no 訂單編號,a.mat_class 物料分類, b.MMCODE 院內碼,b.MMNAME_E 英文品名, b.MMNAME_C 中文品名, '非庫備' 是否庫備, 
                        (case when a.m_contid='0' then '合約' when a.m_contid='2' then '非合約' when a.m_contid='3' then '小採' else '' end) 是否合約,
                        a.agen_no 廠商碼, p.AGEN_NAMEC 廠商名稱, P.AGEN_TEL 廠商電話號碼, b.M_PURUN 包裝單位, 
                        m.m_contprice 單價,b.PO_QTY 總申購量,n.appqty 單位申請量, u.inid_name 申請單位,  b.DELI_QTY 進貨量, (b.PO_QTY-b.DELI_QTY) 未進貨量,
                        floor((b.PO_QTY-b.DELI_QTY)* nvl(b.PO_PRICE,0)) 未進貨金額
                        FROM MM_PO_M a,MM_PO_D b, MI_MAST m , PH_VENDER p , PH_PO_N n, UR_INID u
                        where a.po_no=b.po_no and b.mmcode=m.mmcode and a.AGEN_NO=p.AGEN_NO
                        and b.po_no=n.po_no and b.mmcode=n.mmcode
                        and n.inid=u.inid
                        and substr(a.po_no,1,3) in ('INV','GEN') 
                        and a.po_time >= to_date(:po_time_b,'yyyy-mm-dd') 
                        and a.po_time <= to_date(:po_time_e,'yyyy-mm-dd')
                        and b.storeid='0'
                        and m.mat_class = :mat_class
                        and b.PO_QTY-b.DELI_QTY > 0 ";
            }


            p.Add(":po_time_b", string.Format("{0}", DateTime.Parse(po_time_b).ToString("yyyy-MM-dd")));
            p.Add(":po_time_e", string.Format("{0}", DateTime.Parse(po_time_e).ToString("yyyy-MM-dd")));
            p.Add(":mat_class", string.Format("{0}", mat_class));

            if (agen_no != "")
            {
                sql += "  and a.agen_no =:agen_no ";
                p.Add(":agen_no", string.Format("{0}", agen_no));
            }

            if (m_contid != "")
            {
                sql += "  and a.m_contid= :m_contid  ";
                p.Add(":m_contid", string.Format("{0}", m_contid));
            }


            sql += " order by a.AGEN_NO, a.po_no, b.mmcode";




            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public IEnumerable<BG0008> Report(string po_time_b, string po_time_e, string mat_class, string agen_no, string m_storeid, string m_contid)
        {
            var p = new DynamicParameters();

            var sql = @"select a.PO_NO, a.agen_no, p.AGEN_NAMEC, P.AGEN_TEL, b.MMCODE, b.MMNAME_C, b.MMNAME_E, b.M_PURUN,nvl(PO_PRICE,0) PO_PRICE, b.DELI_QTY, b.PO_QTY,
                        (b.PO_QTY-b.DELI_QTY) NOIN_QTY, floor((b.PO_QTY-b.DELI_QTY)* nvl(b.PO_PRICE,0)) AMOUNT
                        ,A.AGEN_NO||'@@'||P.AGEN_NAMEC||'@@'||P.AGEN_TEL||'@@'||A.PO_NO DATA_ARR
                        FROM MM_PO_M a,MM_PO_D b, MI_MAST m , PH_VENDER p
                        where a.po_no=b.po_no and b.mmcode=m.mmcode and a.AGEN_NO=p.AGEN_NO
                        and substr(a.po_no,1,3) in ('INV','GEN')
                        and a.po_time >= to_date(:po_time_b,'yyyy-mm-dd') 
                        and a.po_time <= to_date(:po_time_e,'yyyy-mm-dd')
                        and m.mat_class = :mat_class
                        and b.PO_QTY-DELI_QTY > 0 ";

            p.Add(":po_time_b", string.Format("{0}", DateTime.Parse(po_time_b).ToString("yyyy-MM-dd")));
            p.Add(":po_time_e", string.Format("{0}", DateTime.Parse(po_time_e).ToString("yyyy-MM-dd")));
            p.Add(":mat_class", string.Format("{0}", mat_class));

            if (agen_no != "")
            {
                sql += "  and a.agen_no =:agen_no ";
                p.Add(":agen_no", string.Format("{0}", agen_no));
            }
            if (m_storeid != "")
            {
                sql += " and b.storeid=:m_storeid ";
                p.Add(":m_storeid", string.Format("{0}", m_storeid));
            }
            if (m_contid != "")
            {
                sql += "  and a.m_contid= :m_contid  ";
                p.Add(":m_contid", string.Format("{0}", m_contid));
            }


            sql += " order by a.AGEN_NO, a.po_no, b.mmcode";



            return DBWork.Connection.Query<BG0008>(sql, p, DBWork.Transaction);
        }
    }
}