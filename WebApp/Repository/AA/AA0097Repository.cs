using System;
using System.Data;
using JCLib.DB;
using Dapper;
using WebApp.Models;
using System.Collections.Generic;
using JCLib.Mvc;

namespace WebApp.Repository.AA
{
    public class AA0097Repository : JCLib.Mvc.BaseRepository
    {
        public AA0097Repository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public DataTable GetAll(string MMCODE)
        {
            var p = new DynamicParameters();

            var sql = @"SELECT 
                            A.*, 
                            UNIT_EXCHRATIO(A.MMCODE, A.M_PURUN, A.M_AGENNO) AS EXCH_RATIO
                        FROM 
                            MI_MAST A, 
                            MI_UNITEXCH B
                        WHERE 
                            1=1
                            AND A.MMCODE = :MMCODE
                            AND A.MAT_CLASS = '02'
                            AND A.MMCODE = B.MMCODE                            
                            ";

            p.Add(":MMCODE", string.Format("{0}", MMCODE));

            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        public DataTable GetExcel()
        {
            var p = new DynamicParameters();

            string sql = @" SELECT
                                A.MMCODE AS ""院內碼"",
                                A.M_STOREID AS ""庫備識別碼(0非庫備,1庫備)"",
                                '' AS ""庫備識別碼(0非庫備,1庫備)(新)"",
                                A.DISC_UPRICE AS ""優惠最小單價(計量單位單價)"",
                                '' AS ""優惠最小單價(計量單位單價)(新)"",
                                A.M_PURUN AS ""申購計量單位(包裝單位)"",
                                '' AS ""申購計量單位(包裝單位)(新)"",
                                UNIT_EXCHRATIO(A.MMCODE, A.M_PURUN, A.M_AGENNO) AS ""包裝轉換率"",
                                '' AS ""包裝轉換率(新)"",
                                M_VOLL AS ""長度(CM)"",
                                '' AS ""長度(CM)(新)"",
                                M_VOLW AS ""寬度(CM)"",
                                '' AS ""寬度(CM)(新)"",
                                M_VOLH AS ""高度(CM)"",
                                '' AS ""高度(CM)(新)"",
                                M_VOLC AS ""圓周"",
                                '' AS ""圓周(新)"",
                                M_SWAP AS ""材積轉換率"",
                                '' AS ""材積轉換率(新)"",
                                M_IDKEY AS ""ID碼"",
                                '' AS ""ID碼(新)"",
                                M_INVKEY AS ""衛材料號碼"",
                                '' AS ""衛材料號碼(新)"",
                                M_GOVKEY AS ""行政院碼"",
                                '' AS ""行政院碼(新)"",
                                M_CONSUMID AS ""消耗屬性(1消耗,2半消耗)"",
                                '' AS ""消耗屬性(1消耗,2半消耗)(新)"",
                                M_PAYKIND AS ""給付類別(1自費,2健保,3醫院吸收)"",
                                '' AS ""給付類別(1自費,2健保,3醫院吸收)(新)"",
                                M_PAYID AS ""計費方式(1計價,2不計價)"",
                                '' AS ""計費方式(1計價,2不計價)(新)"",
                                M_TRNID AS ""扣庫方式(1扣庫2不扣庫)"",
                                '' AS ""扣庫方式(1扣庫2不扣庫)(新)""   
                            FROM 
                                MI_MAST A, 
                                MI_UNITEXCH B   
                            WHERE 
                                1=1
                                AND A.MAT_CLASS = '02'
                                AND A.MMCODE = B.MMCODE                            
                            ";


            DataTable dt = new DataTable();
            using (var rdr = DBWork.Connection.ExecuteReader(sql, p, DBWork.Transaction))
                dt.Load(rdr);

            return dt;
        }

        //確認更新 於Log表插入新紀錄 MM_MAST_UPD
        public int Insert(AA0097M aa0097m)
        {
            var sql = @"INSERT INTO 
                            MM_MAST_UPD (
                                MMCODE,
                                M_STOREID,
                                DISC_UPRICE,
                                M_PURUN,
                                EXCH_RATIO,
                                M_VOLL,
                                M_VOLW,
                                M_VOLH,
                                M_VOLC,
                                M_SWAP,
                                M_IDKEY,
                                M_INVKEY,
                                M_GOVKEY,
                                M_CONSUMID,
                                M_PAYKIND,
                                M_PAYID,
                                M_TRNID,
                                UPDATE_TIME,
                                UPDATE_USER,
                                UPDATE_IP
                            )
                            VALUES (
                                :MMCODE,
                                :M_STOREID_N,
                                :DISC_UPRICE_N,
                                :M_PURUN_N,
                                :EXCH_RATIO_N,
                                :M_VOLL_N,
                                :M_VOLW_N,
                                :M_VOLH_N,
                                :M_VOLC_N,
                                :M_SWAP_N,
                                :M_IDKEY_N,
                                :M_INVKEY_N,
                                :M_GOVKEY_N,
                                :M_CONSUMID_N,
                                :M_PAYKIND_N,
                                :M_PAYID_N,
                                :M_TRNID_N,
                                :UPDATE_TIME,
                                :UPDATE_USER,
                                :UPDATE_IP
                            )
                       ";

            return DBWork.Connection.Execute(sql, aa0097m, DBWork.Transaction);
        }

        //確認更新 修改基本檔資料 MI_MAST
        public int Update(AA0097M aa0097m)
        {
            var sql = @"UPDATE 
                            MI_MAST
                        SET 
                            M_STOREID = NVL(:M_STOREID_N, M_STOREID),
                            DISC_UPRICE = NVL(:DISC_UPRICE_N, DISC_UPRICE),
                            M_PURUN = NVL(:M_PURUN_N, M_PURUN),                    
                            M_VOLL = NVL(:M_VOLL_N, M_VOLL),
                            M_VOLW = NVL(:M_VOLW_N, M_VOLW),
                            M_VOLH = NVL(:M_VOLH_N, M_VOLH),
                            M_VOLC = NVL(:M_VOLC_N, M_VOLC),
                            M_SWAP = NVL(:M_SWAP_N, M_SWAP),
                            M_IDKEY = NVL(:M_IDKEY_N, M_IDKEY),
                            M_INVKEY = NVL(:M_INVKEY_N, M_INVKEY),
                            M_GOVKEY = NVL(:M_GOVKEY_N, M_GOVKEY),
                            M_CONSUMID = NVL(:M_CONSUMID_N, M_CONSUMID),
                            M_PAYKIND = NVL(:M_PAYKIND_N, M_PAYKIND),
                            M_PAYID = NVL(:M_PAYID_N, M_PAYID),
                            M_TRNID = NVL(:M_TRNID_N, M_TRNID)
                        WHERE 
                            MMCODE = :MMCODE
                       ";

            return DBWork.Connection.Execute(sql, aa0097m, DBWork.Transaction);
        }

        //檢查此院內碼是否存在基本檔
        public bool CheckExistsMMCODE(string MMCODE)
        {
            string sql = @"     SELECT 1
                                FROM MI_MAST MMT
                                WHERE MMT.MMCODE = :MMCODE";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = MMCODE }, DBWork.Transaction) == null);
        }

        //檢查此院內碼物料類別是否為02-衛材
        public bool CheckExistsMMCODEMAT(string MMCODE)
        {
            string sql = @"     SELECT 1
                                FROM MI_MAST MMT
                                WHERE MMT.MMCODE = :MMCODE AND MMT.MAT_CLASS = '02'";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = MMCODE }, DBWork.Transaction) == null);
        }

        //檢查新申購計量單位是否存在於計量單位檔
        public bool CheckExistsPURUN(string M_PURUN)
        {
            string sql = @"     SELECT 1
                                FROM MI_UNITCODE
                                WHERE UNIT_CODE = :M_PURUN ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { M_PURUN = M_PURUN }, DBWork.Transaction) == null);
        }

        //檢查院內碼+新申購計量單位是否存在於轉換率檔 (用途：若存在則修改；若不存在則新增)
        public bool CheckExistsEXCH_RATIO(string MMCODE, string M_PURUN)
        {
            string sql = @"     SELECT 1
                                FROM MI_UNITEXCH
                                WHERE MMCODE = :MMCODE AND UNIT_CODE = :M_PURUN ";
            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = MMCODE, M_PURUN = M_PURUN }, DBWork.Transaction) == null);
        }

        //插入 EXCH_RATIO
        public bool InsertEXCH_RATIO(string MMCODE, string M_PURUN_N, string EXCH_RATIO_N)
        {
            string sql = @" INSERT INTO 
                            MI_UNITEXCH (
                                MMCODE,
                                UNIT_CODE,
                                AGEN_NO,
                                EXCH_RATIO
                            )
                            VALUES (
                                :MMCODE,
                                :M_PURUN_N,
                                (select M_AGENNO from MI_MAST where MMCODE = :MMCODE),
                                :EXCH_RATIO_N
                            )
                        ";

            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = MMCODE, M_PURUN_N = M_PURUN_N, EXCH_RATIO_N = EXCH_RATIO_N }, DBWork.Transaction) == null);
        }

        //更新 EXCH_RATIO
        public bool UpdateEXCH_RATIO(string MMCODE, string M_PURUN, string EXCH_RATIO)
        {
            string sql = @" UPDATE MI_UNITEXCH
                            SET EXCH_RATIO = NVL(:EXCH_RATIO, EXCH_RATIO)
                            WHERE MMCODE = :MMCODE AND UNIT_CODE = :M_PURUN ";

            return !(DBWork.Connection.ExecuteScalar(sql, new { MMCODE = MMCODE, M_PURUN = M_PURUN, EXCH_RATIO = EXCH_RATIO }, DBWork.Transaction) == null);
        }


    }
}