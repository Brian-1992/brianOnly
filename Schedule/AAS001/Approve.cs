using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Models;
using WebApp.Repository.AB;

namespace AAS001
{
    public class Approve
    {
        LogController log = new LogController("Approve", "Approve");
        IEnumerable<string> approve_flowids = new List<string>() { "3", "4"};
        public void Run() {
            using (WorkSession session = new WorkSession()) {
                var DBWork = session.UnitOfWork;
                try
                {
                    log.AddLogs("開始Approve");
                    DBWork.BeginTransaction();
                    ApproveRepository repo = new ApproveRepository(DBWork);
                    AB0100Repository repoAB100 = new AB0100Repository(DBWork);

                    // 1. 取得撥發資料寫入LIS
                    log.AddLogs("1. 取得撥發資料寫入LIS");
                    // 1.1 取得庫房
                    log.AddLogs(string.Format("1.1 取得庫房"));
                    string towh = repoAB100.GetTowh();
                    log.AddLogs(string.Format("庫房代碼: {0}", towh));
                    // 1.2 取得未入庫申請單
                    IEnumerable<LIS_ACC> accs = repo.GetLisAccs(towh);
                    log.AddLogs(string.Format("1.2 取得未入庫申請單"));
                    // 取得批號校旗
                    log.AddLogs(string.Format("取得批號效期資料"));
                    accs = GetDocexp(accs.ToList(), DBWork);

                    // 1.3 寫入LIS_ACC
                    log.AddLogs(string.Format("寫入LIS_ACC"));
                    int afrs = -1;
                    foreach (LIS_ACC acc in accs)
                    {
                        afrs = repo.InsertLisAcc(acc);
                    }
                    log.AddLogs(string.Format("寫入LIS_ACC成功"));

                    // 2.  點收
                    log.AddLogs(string.Format("2.  點收"));
                    // 2.1 取得LIS_ACC回饋資料
                    log.AddLogs(string.Format("取得LIS_ACC回饋資料"));
                    IEnumerable<LIS_ACC> return_accs = repo.GetLisAcc();
                    // 2.2 以docno將資料區分為Y & N
                    log.AddLogs(string.Format("以docno將資料區分為Y & N"));
                    return_accs = GetLisAccGroup(return_accs);

                    // 2.3 逐筆檢查
                    foreach (LIS_ACC acc in return_accs)
                    {
                        log.AddLogs(string.Format("docno: {0}", acc.DOCNO));
                        // 2.3.1 檢查ME_DOCM.flowid是否為 3或4
                        if (approve_flowids.Contains(repo.GetMedocmFlowid(acc.DOCNO)) == false) {
                            log.AddLogs(string.Format("flowid不為3或4，跳過"));
                            continue;
                        }

                        string rtn;
                        // 2.3.2 沒有N，全部相符，直接呼叫SP更新庫存量
                        if (acc.Ns.Any() == false)
                        {
                            log.AddLogs(string.Format("沒有N，全部相符，直接呼叫SP更新庫存量"));
                            rtn = repo.CallProc(acc.DOCNO, "LIS", "LIS");
                            if (rtn == "Y")
                            {
                                afrs = repo.UpdateDocd(acc.DOCNO);
                            }

                            continue;
                        }

                        // 2.3.3 有N=有不同
                        log.AddLogs(string.Format("有N=有不同"));
                        // 刪除ME_DOCEXP內不符資料
                        log.AddLogs(string.Format("刪除ME_DOCEXP內不符資料"));
                        afrs = repo.DeleteMeDocExp(acc.DOCNO);
                        // 新增N的資料到ME_DOCEXP
                        log.AddLogs(string.Format("新增N的資料到ME_DOCEXP"));
                        afrs = repo.InsertMeDocExp(acc.DOCNO);
                        // 更新ME_DOCD.ACKQTY
                        log.AddLogs(string.Format("ME_DOCD.ACKQTY"));
                        afrs = repo.UpdateMeDocdAchkqty(acc.DOCNO);
                        // 呼叫SP
                        log.AddLogs(string.Format("呼叫SP"));
                        rtn = repo.CallProc(acc.DOCNO, "LIS", "LIS");
                        if (rtn == "Y")
                        {
                            afrs = repo.UpdateDocd(acc.DOCNO);
                        }
                    }


                    DBWork.Commit();
                    log.AddLogs("完成轉檔");
                    log.CreateLogFile();
                }
                catch (Exception e) {
                    DBWork.Rollback();
                    log.CreateLogFile();
                    throw;
                }
            }
        }

        private IEnumerable<LIS_ACC> GetDocexp(List<LIS_ACC> list, UnitOfWork DBWork) {
            List<LIS_ACC> newList = new List<LIS_ACC>();

            ApproveRepository repo = new ApproveRepository(DBWork);

            foreach (LIS_ACC item in list) {
                IEnumerable<ME_DOCEXP> exps = repo.GetDocexps(item.DOCNO, item.MMCODE);
                if (exps.Any() == false) {
                    newList.Add(item);
                    continue;
                }

                foreach (ME_DOCEXP exp in exps) {
                    LIS_ACC temp = new LIS_ACC()
                    {
                        DOCNO = item.DOCNO,
                        PURCHNO = item.PURCHNO,
                        SEQ = item.SEQ,
                        MMCODE = item.MMCODE,
                        LOT_NO = exp.LOT_NO,
                        EXP_DATE = exp.EXP_DATE,
                        APVQTY = exp.APVQTY,
                        BASE_UNIT = item.BASE_UNIT
                    };

                    newList.Add(temp);
                }
            }

            return newList;
        }

        private IEnumerable<LIS_ACC> GetLisAccGroup(IEnumerable<LIS_ACC> accs) {
            var o = from a in accs
                    group a by new
                    {
                        DOCNO = a.DOCNO
                    } into g
                    select new LIS_ACC
                    {
                        DOCNO = g.Key.DOCNO,
                        Ys = g.Where(x => x.ISACC == "Y").Select(x => x).ToList(),
                        Ns = g.Where(x => x.ISACC == "N").Select(x => x).ToList(),
                    };
            List<LIS_ACC> list = o.ToList();

            return list;
        }
    }
}
