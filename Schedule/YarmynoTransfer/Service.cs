using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YarmynoTransfer
{
    public class Service
    {
        public void Run() {
            using (WorkSession session = new WorkSession()) {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    var repo = new Repository(DBWork);

                    // 1. 找到有112-113的資料塞到新table中
                    int i = repo.deleteTemp();
                    Console.WriteLine(string.Format("清空TEMP_YARMYNO_LOG，共 {0} 筆", i));
                    i = repo.InsertTemp();
                    
                    Console.WriteLine(string.Format("找到有112-113的資料塞到新table中，共 {0} 筆", i));

                    // 
                    i = repo.InsertTemp_1();
                    Console.WriteLine(string.Format("找到無112-113有110-111的資料塞到新table中，共 {0} 筆", i));

                    // 2. 逐筆找舊資料 更新舊欄位
                    IEnumerable<TEMP_LOG> temps = repo.GetTemps();
                    i = 0;
                    foreach (TEMP_LOG temp in temps) {
                        MI_MAST_LOG log = repo.GetMiMastLog(temp.MMCODE);
                        if (log == null) {
                            log = new MI_MAST_LOG();
                        }
                        MILMED_JBID_LIST jbid = repo.GetJbid(log.E_YRARMYNO, log.E_ITEMARMYNO);
                        if (jbid == null) {
                            jbid = new MILMED_JBID_LIST();
                        }
                        i += repo.UpdateTemp(temp.MMCODE, log.E_YRARMYNO, log.E_ITEMARMYNO,
                             jbid.ISWILLING, jbid.DISCOUNT_QTY, jbid.DISC_COST_UPRICE);
                    }
                    Console.WriteLine(string.Format("逐筆找舊資料 更新舊欄位，共 {0} 筆", i));

                    //// 3. 更新INREC
                    //i = repo.backupInrec();
                    //Console.WriteLine(string.Format("備份INREC到TEMP_MM_PO_INREC_20230103，共 {0} 筆", i));

                    i = repo.UpdateInrec();
                    Console.WriteLine(string.Format("更新INREC，共 {0} 筆", i));

                    DBWork.Commit();
                    
                }
                catch (Exception ex) {
                    Console.WriteLine(string.Format("error：{0}", ex.Message));
                    Console.WriteLine(string.Format("trace：{0}", ex.StackTrace));
                    DBWork.Rollback();
                }

                Console.ReadLine();
            }
        }
    }
}
