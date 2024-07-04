using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Models;

namespace SetEffMiMast
{
    class SetEffMiMastService
    {
        private IUnitOfWork _dbWork = null;
        private SetEffMiMastRepository _repo = null;

        public void Run()
        {
            using (WorkSession session = new WorkSession())
            {
                // init
                _dbWork = session.UnitOfWork;
                _repo = new SetEffMiMastRepository(_dbWork);

                try
                {
                    // 1. 更新 基本單位表
                    _repo.MergeIntoMI_MAST();

                    // 2. 更新 單位轉換率檔
                    _repo.MergeIntoMI_UNITEXCH();

                    // 3. 更新 庫房存量管制檔
                    UpdateMI_WINVCTL();
                }
                catch (Exception e)
                {
                    // logController.AddLogs(string.Format("error：{0}", ex.Message));
                    // logController.CreateLogFile();
                    throw;

                }
            }
        }

        public void UpdateMI_WINVCTL()
        {
            IEnumerable<MI_MAST_HISTORY> HistoryList = _repo.GetMmcodesFromMi_Mast_History();
            IEnumerable<MI_WHMAST> WhMastList = _repo.GetWH_NosFromMi_Whmast();

            try
            {
                _dbWork.BeginTransaction();

                foreach (MI_MAST_HISTORY MI_MAST_HISTORY in HistoryList)
                {
                    foreach (MI_WHMAST MI_WINVCTL in WhMastList)
                    {
                        if ("1" != _repo.GetMI_WINVCTL(MI_MAST_HISTORY.MMCODE, MI_WINVCTL.WH_NO))
                        {
                            _repo.InsertIntoMI_WINVCTL(MI_MAST_HISTORY.MMCODE, MI_WINVCTL.WH_NO);
                        }
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
}
