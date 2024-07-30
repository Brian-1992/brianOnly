using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;


namespace WebApp.Repository.AA
{
    public class AA0090Repository : JCLib.Mvc.BaseRepository
    {
        public AA0090Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        //查詢
        public IEnumerable<AA0090> GetAll(string MMCODE, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"  SELECT MWL.WH_NO,
                                    MWT.WH_NAME,
                                    TO_CHAR (ROUND (NVL (MWL.LOW_QTY, 0), 2), 'FM999999990.00') LOW_QTY,
                                    TO_CHAR (ROUND (NVL (MWL.SAFE_DAY, 0), 2), 'FM999999990.00') SAFE_DAY,
                                    TO_CHAR (ROUND (NVL (MWL.SAFE_QTY, 0), 2), 'FM999999990.00') SAFE_QTY,
                                    TO_CHAR (ROUND (NVL (MWL.OPER_DAY, 0), 2), 'FM999999990.00') OPER_DAY,
                                    TO_CHAR (ROUND (NVL (MWL.OPER_QTY, 0), 2), 'FM999999990.00') OPER_QTY,
                                    MWT.CANCEL_ID,
                                    TO_CHAR (ROUND (NVL (MWL.MIN_ORDQTY, 0), 2), 'FM999999990.00') MIN_ORDQTY,
                                    MWV.STORE_LOC,
                                    TO_CHAR (ROUND (NVL (MWV2.INV_QTY, 0), 2), 'FM999999990.00') INV_QTY,
                                    MWT.PWH_NO,
                                    MWL.NOWCONSUMEFLAG,
                                    MWL.RESERVEFLAG
                             FROM  MI_WINVCTL MWL,
                                   MI_WHMAST MWT,
                                   MI_WLOCINV MWV,
                                   MI_WHINV MWV2
                             WHERE  MWL.WH_NO = MWT.WH_NO
                             AND MWL.WH_NO = MWV.WH_NO(+)
                             AND MWL.MMCODE = MWV.MMCODE(+)
                             AND MWL.WH_NO = MWV2.WH_NO(+)
                             AND MWL.MMCODE = MWV2.MMCODE(+)
                             AND MWL.MMCODE = :MMCODE
                             ORDER BY WH_NO";

            p.Add(":MMCODE", string.Format("{0}", MMCODE));

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<AA0090>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }

        //院內碼下拉式選單
        public IEnumerable<MI_MAST> GetMMCodeCombo(string p0, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT {0} A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.INSUORDERCODE, A.ORDERHOSPNAME, A.ORDEREASYNAME, A.SCIENTIFICNAME,
                                   A.CURECONSISTENCY, A.PEAR, A.TROUGH, A.DANGER, A.TDMFLAG, A.TDMMEMO1, A.TDMMEMO2, A.TDMMEMO3, A.UDSERVICEFLAG,
                                   A.UDPOWDERFLAG, A.AIRDELIVERY
                        FROM (SELECT MMT.MMCODE,
                                     MMT.MMNAME_C,
                                     MMT.MMNAME_E,
                                     HBD.INSUORDERCODE,
                                     HBM.ORDERHOSPNAME,
                                     HBM.ORDEREASYNAME,
                                     HBM.SCIENTIFICNAME,
                                     NVL (HST.MINCURECONSISTENCY, '0') || ' ~ ' || NVL (HST.MAXCURECONSISTENCY, '0') CURECONSISTENCY,
                                     NVL (HST.PEARBEGIN, '0') || ' ~ ' || NVL (HST.PEAREND, '0') PEAR,
                                     NVL (HST.TROUGHBEGIN, '0') || ' ~ ' || NVL (HST.TROUGHEND, '0') TROUGH,
                                     NVL (HST.DANGERBEGIN, '0') || ' ~ ' || NVL (HST.DANGEREND, '0') DANGER,
                                     NVL (HBM.TDMFLAG, 'N') TDMFLAG,
                                     HST.TDMMEMO1,
                                     HST.TDMMEMO2,
                                     HST.TDMMEMO3,
                                     NVL (HBM.UDSERVICEFLAG, 'N') UDSERVICEFLAG,
                                     NVL (HBM.UDPOWDERFLAG, 'N') UDPOWDERFLAG,
                                     NVL (HBM.AIRDELIVERY, 'N') AIRDELIVERY
                              FROM MI_MAST MMT,
                                   HIS_BASORDD HBD,
                                   HIS_BASORDM HBM,
                                   HIS_STKDMIT HST
                              WHERE  HBD.ORDERCODE = MMT.MMCODE
                              AND    TO_CHAR (SYSDATE, 'YYYYMMDD') - 19110000 >= HBD.BEGINDATE
                              AND    TO_CHAR (SYSDATE, 'YYYYMMDD') <= HBD.ENDDATE
                              AND    HBM.ORDERCODE = MMT.MMCODE
                              AND    HST.SKORDERCODE = MMT.MMCODE) A
                        WHERE 1=1 ";

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

                sql = string.Format("SELECT * FROM ({0}) SV ORDER BY IDX, MMCODE ", sql);
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

        public class MI_MAST_QUERY_PARAMS
        {
            public string MMCODE;
            public string MMNAME_C;
            public string MMNAME_E;
            public string INSUORDERCODE;
            public string ORDERHOSPNAME;
            public string ORDEREASYNAME;
            public string SCIENTIFICNAME;
            public string MINCURECONSISTENCY;
            public string MAXCURECONSISTENCY;
            public string PEARBEGIN;
            public string PEAREND;
            public string TROUGHBEGIN;
            public string TROUGHEND;
            public string DANGERBEGIN;
            public string DANGEREND;
            public string TDMFLAG;
            public string TDMMEMO1;
            public string TDMMEMO2;
            public string TDMMEMO3;
            public string UDSERVICEFLAG;
            public string UDPOWDERFLAG;
            public string AIRDELIVERY;
        }

        //院內碼彈出式視窗(搜尋)
        public IEnumerable<MI_MAST> GetMmcode(MI_MAST_QUERY_PARAMS query, int page_index, int page_size, string sorters)
        {
            var p = new DynamicParameters();
            string sql = @"SELECT A.MMCODE, A.MMNAME_C, A.MMNAME_E, A.INSUORDERCODE, A.ORDERHOSPNAME, A.ORDEREASYNAME, A.SCIENTIFICNAME,
                                  A.CURECONSISTENCY, A.PEAR, A.TROUGH, A.DANGER, A.TDMFLAG, A.TDMMEMO1, A.TDMMEMO2, A.TDMMEMO3, A.UDSERVICEFLAG,
                                  A.UDPOWDERFLAG, A.AIRDELIVERY
                           FROM (SELECT MMT.MMCODE,
                                        MMT.MMNAME_C,
                                        MMT.MMNAME_E,
                                        HBD.INSUORDERCODE,
                                        HBM.ORDERHOSPNAME,
                                        HBM.ORDEREASYNAME,
                                        HBM.SCIENTIFICNAME,
                                        NVL (HST.MINCURECONSISTENCY, '0') || ' ~ ' || NVL (HST.MAXCURECONSISTENCY, '0') CURECONSISTENCY,
                                        NVL (HST.PEARBEGIN, '0') || ' ~ ' || NVL (HST.PEAREND, '0') PEAR,
                                        NVL (HST.TROUGHBEGIN, '0') || ' ~ ' || NVL (HST.TROUGHEND, '0') TROUGH,
                                        NVL (HST.DANGERBEGIN, '0') || ' ~ ' || NVL (HST.DANGEREND, '0') DANGER,
                                        NVL (HBM.TDMFLAG, 'N') TDMFLAG,
                                        HST.TDMMEMO1,
                                        HST.TDMMEMO2,
                                        HST.TDMMEMO3,
                                        NVL (HBM.UDSERVICEFLAG, 'N') UDSERVICEFLAG,
                                        NVL (HBM.UDPOWDERFLAG, 'N') UDPOWDERFLAG,
                                        NVL (HBM.AIRDELIVERY, 'N') AIRDELIVERY
                                 FROM MI_MAST MMT,
                                      HIS_BASORDD HBD,
                                      HIS_BASORDM HBM,
                                      HIS_STKDMIT HST
                                 WHERE  HBD.ORDERCODE = MMT.MMCODE
                                 AND    TO_CHAR (SYSDATE, 'YYYYMMDD') - 19110000 >= HBD.BEGINDATE
                                 AND    TO_CHAR (SYSDATE, 'YYYYMMDD') <= HBD.ENDDATE
                                 AND    HBM.ORDERCODE = MMT.MMCODE
                                 AND    HST.SKORDERCODE = MMT.MMCODE) A
                           WHERE 1=1  ";

            if (query.MMCODE != "")
            {
                sql += " AND A.MMCODE LIKE :MMCODE ";
                p.Add(":MMCODE", string.Format("%{0}%", query.MMCODE));
            }

            if (query.MMNAME_C != "")
            {
                sql += " AND A.MMNAME_C LIKE :MMNAME_C ";
                p.Add(":MMNAME_C", string.Format("%{0}%", query.MMNAME_C));
            }

            if (query.MMNAME_E != "")
            {
                sql += " AND A.MMNAME_E LIKE :MMNAME_E ";
                p.Add(":MMNAME_E", string.Format("%{0}%", query.MMNAME_E));
            }

            p.Add("OFFSET", (page_index - 1) * page_size);
            p.Add("PAGE_SIZE", page_size);

            return DBWork.Connection.Query<MI_MAST>(GetPagingStatement(sql, sorters), p, DBWork.Transaction);
        }
    }
}