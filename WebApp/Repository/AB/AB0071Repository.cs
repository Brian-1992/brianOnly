using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;

namespace WebApp.Repository.AB
{
    public class AB0071Repository : JCLib.Mvc.BaseRepository
    {
        public AB0071Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public IEnumerable<ComboItemModel> GetWH_NoCombo(string userID)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT A.WH_NO||' '||A.WH_NAME TEXT,A.WH_NO VALUE
                        FROM MI_WHMAST A, MI_WHID B
                        WHERE A.WH_KIND = '0'
                        AND A.WH_NO = B.WH_NO
                        AND WH_USERID = : userID ";


            return DBWork.Connection.Query<ComboItemModel>(sql, new { userID= userID } , DBWork.Transaction);

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

        public IEnumerable<ME_AB0071> Report(string STOCKCODE, string YYYYMMDD_B, string YYYYMMDD_E, string P4, string MMCODE,bool OrderByRXNO)
        {
            var p = new DynamicParameters();

            var sql = @"Select   a.CREATEDATETIME --日期
                                ,a.NRCODENAME   --病房-床位
                                ,a.RXNO        --處方箋
                                ,a.CHARTNO    --病歷號
                                ,a.ORDERCODE    --院內碼
                                ,a.ORDERENGNAME  --品名
                                ,a.DOSE          --劑量
                                ,a.ORDERUNIT      --劑量單位
                                ,a.PATHNO          --途徑
                                ,a.FREQNO           --頻率
                                ,a.PAYFLAG          --自費
                                ,a.BUYFLAG          --自備
                                ,a.USEQTY           --數量
                                ,a.CHINNAME         --開立醫師
                                ,a.SIGNOPID         --給藥人員
                                ,a.INOUTFLAG        --A:買 D:退
                        From me_ab0071 a, mi_mast b
                        Where a.ORDERCODE = b.mmcode
                        And ( b.E_HighPriceFlag in :p4 or b. E_RestrictCode in :p4 )
                        AND a.STOCKCODE  = :STOCKCODE ";

            string[] p4 = P4.Split(',');

            p.Add(":p4", p4);
            p.Add(":STOCKCODE", string.Format("{0}", STOCKCODE));

            if (YYYYMMDD_B != "")
            {
                sql += " AND SUBSTR(A.CREATEDATETIME,1,7) >=:YYYYMMDD_B ";
                p.Add(":YYYYMMDD_B", string.Format("{0}", YYYYMMDD_B));
            }
            if (YYYYMMDD_E != "")
            {
                sql += " AND SUBSTR(A.CREATEDATETIME,1,7) <=:YYYYMMDD_E ";
                p.Add(":YYYYMMDD_E", string.Format("{0}", YYYYMMDD_E));
            }
            if (MMCODE != "")
            {
                sql += " AND a.ORDERCODE  = :MMCODE ";
                p.Add(":MMCODE", string.Format("{0}", MMCODE));
            }
            if (OrderByRXNO)
            {
                sql += " ORDER BY A.RXNO  ";
            }
            else
            {
                sql += " ORDER BY A.CREATEDATETIME ,A.ORDERENGNAME, A.MEDNO  ";
            }

            return DBWork.Connection.Query<ME_AB0071>(sql, p, DBWork.Transaction);
        }
    }
}