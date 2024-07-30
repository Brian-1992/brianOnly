using JCLib.DB;
using JCLib.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ROReset
{
    class ROResetService
    {
        /// <summary>
        /// 需要被更新的欄位 平均消耗 x天
        /// </summary>
        private int[] _dayUseSet = { 10, 14, 90 };
        private IUnitOfWork _dbWork = null;
        private ROResetRepository _repo = null;

        public void Run()
        {
            using (WorkSession session = new WorkSession())
            {
                // init
                _dbWork = session.UnitOfWork;
                _repo = new ROResetRepository(_dbWork);

                try
                {
                    // 1. 2.
                    QueryAndUpdateDayUse();
                    // 3. 4.
                    QueryAndUpdateNowRo();
                    // 6.
                    QueryAndUpdateMonthlyConsumption();
                    // 7. 8.
                    QueryAndUpdateMonthlyAvgUse3();
                    // 9.
                    UpdateAllRoWhtypeQtys();
                }
                catch (Exception e)
                {
                    // logController.AddLogs(string.Format("error：{0}", ex.Message));
                    // logController.CreateLogFile();
                    throw;
                }
            }
        }

        /// <summary>
        /// 取出所有 院內碼及相關庫房級別
        /// 
        /// 並更新，日平均消耗天數為{10, 14, 90}的平均消耗量
        /// </summary>
        private void QueryAndUpdateDayUse()
        {
            IEnumerable<SelectType> res = _repo.GetSelectTypeResult();

            if (res.Count() > 0)
            {
                try
                {
                    _dbWork.BeginTransaction();

                    foreach (SelectType rec in res)
                    {
                        foreach (int iDays in _dayUseSet)
                        {
                            int iConsumption = _repo.GetConsumptionOfLastDays(rec.MMCODE, iDays);
                            _repo.UpdateDayUse(iConsumption, rec.MMCODE, "day_use_" + iDays.ToString());
                        }
                    }

                    _dbWork.Commit();
                }
                catch (Exception e)
                {
                    _dbWork.Rollback();
                    throw e;
                }
            }

        }

        /// <summary>
        /// 取出基準量為 日基準 的院內碼資料
        /// 
        /// 之後，比較3月平均消耗量與日平均消耗10天差異是否在誤差百分比內，
        /// 若超過則以日平均消耗10天更新RO，否則以3月平均消耗量更新 RO。
        /// </summary>
        private void QueryAndUpdateNowRo()
        {
            // 基準量模式(1:日基準 2:月基準 3:自訂基準量)
            IEnumerable<SelectType> res = _repo.GetSelectTypeResultByRoType("1");

            if (res.Count() > 0)
            {
                try
                {
                    _dbWork.BeginTransaction();

                    foreach (SelectType rec in res)
                    {
                        _repo.UpdateNowRo(rec.MMCODE, rec.RO_WHTYPE);
                    }

                    _dbWork.Commit();
                }
                catch (Exception e)
                {
                    _dbWork.Rollback();
                    throw e;
                }
            }

        }

        /// <summary>
        /// 取出基準量為 日基準 的院內碼資料
        /// 
        /// 之後，比較3月平均消耗量與日平均消耗10天差異是否在誤差百分比內，
        /// 若超過則以日平均消耗10天更新RO，否則以3月平均消耗量更新 RO。
        /// </summary>
        private void QueryAndUpdateMonthlyConsumption()
        {
            // 基準量模式(1:日基準 2:月基準 3:自訂基準量)
            IEnumerable<SelectType> res = _repo.GetSelectTypeResultByRoType("1");

            if (res.Count() > 0)
            {
                try
                {
                    _dbWork.BeginTransaction();

                    foreach (SelectType rec in res)
                    {
                        _repo.UpdateMonthlyConsumption(rec.MMCODE, rec.RO_WHTYPE);
                    }

                    _dbWork.Commit();
                }
                catch (Exception e)
                {
                    _dbWork.Rollback();
                    throw e;
                }
            }

        }

        /// <summary>
        /// 更新now_ro資料為3個月平均消耗
        /// 處理藥品資料(藥品未計算本次盤點資料，以醫令扣庫為主)
        /// 處理衛材資料(衛材未計算本次月結資料，以前2~4月為主)
        /// </summary>
        private void QueryAndUpdateMonthlyAvgUse3()
        {
            // 基準量模式(1:日基準 2:月基準 3:自訂基準量)
            IEnumerable<SelectType> res = _repo.GetSelectTypeResultByRoType("2");

            if (res.Count() > 0)
            {
                try
                {
                    _dbWork.BeginTransaction();

                    foreach (SelectType rec in res)
                    {
                        _repo.UpdateMonthlyConsumption(rec.MMCODE, rec.RO_WHTYPE);
                        _repo.UpdateBaseRo2To4Months(rec.MMCODE, rec.RO_WHTYPE);
                    }

                    _dbWork.Commit();
                }
                catch (Exception e)
                {
                    _dbWork.Rollback();
                    throw e;
                }
            }
        }

        /// <summary>
        /// 更新 所有庫房的
        /// 
        /// 護理病房最大請領量, 供應中心最大請領量, 藥局請領最大量, 安全庫存量, 正常庫存量
        /// </summary>
        private void UpdateAllRoWhtypeQtys()
        {
            // 基準量模式 (1:日基準 2:月基準 3:自訂基準量)
            IEnumerable<SelectType> res = _repo.GetSelectTypeResultByRoType("2");

            if (res.Count() > 0)
            {
                try
                {
                    _dbWork.BeginTransaction();

                    _repo.UpdateHealthcareMaterialQtys();
                    _repo.UpdateMedicineQtys();
                    _repo.UpdatePharmacyQtys();
                    _repo.UpdateSupplyCenterQtys();

                    _dbWork.Commit();
                }
                catch (Exception e)
                {
                    _dbWork.Rollback();
                    throw e;
                }
            }
        }

        /// <summary>
        /// 檢查是否為月結後的第二天
        /// </summary>
        /// <returns></returns>
        private bool IsNextDayOfMonthlyClosing()
        {
            return _repo.GetEndOfMonth() == "1";
        }
    }
}
