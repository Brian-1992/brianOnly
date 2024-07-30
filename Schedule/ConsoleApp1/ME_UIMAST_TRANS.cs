using JCLib.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Models;

namespace ConsoleApp1
{
    public class ME_UIMAST_TRANS
    {
        LogController logController = new LogController("ME_UIMAST_TRANS", DateTime.Now.ToString("yyyy-MM"), "ME_UIMAST_TRANS");

        public void Run()
        {
            string wh_no = string.Empty;
            using (WorkSession session = new WorkSession())
            {
                var DBWork = session.UnitOfWork;
                DBWork.BeginTransaction();
                try
                {
                    ME_UIMAST_TRANSRepository repo = new ME_UIMAST_TRANSRepository(DBWork);

                    // 取得所有二級庫ME_UIMAST資料
                    logController.AddLogs("取得所有二級庫ME_UIMAST資料");
                    Console.WriteLine("取得所有二級庫ME_UIMAST資料");
                    IEnumerable<ME_UIMAST> uimasts = repo.GetUimasts2();
                    logController.AddLogs(string.Format("共 {0} 筆", uimasts.Count()));
                    Console.WriteLine(string.Format("共 {0} 筆", uimasts.Count()));
                    ME_UIMAST temp = new ME_UIMAST();
                    int i = 0;
                   
                    foreach (ME_UIMAST uimast in uimasts)
                    {
                        if (wh_no != string.Empty && wh_no != uimast.WH_NO) {
                            Console.WriteLine(string.Format("產生{0} txt", wh_no));
                            logController.CreateLogFile(wh_no);
                            Console.WriteLine(string.Format("產生{0} txt完成", wh_no));
                            logController.ClearLogs();
                        }
                        wh_no = uimast.WH_NO;

                        i++;
                        logController.AddLogs(string.Format("{5}. 庫房代碼:{0} 院內碼:{1} 包裝單位:{2} 包裝數量:{3} 包裝申領倍數:{4}",
                            uimast.WH_NO, uimast.MMCODE, uimast.PACK_UNIT, uimast.PACK_QTY, uimast.PACK_TIMES, i.ToString().PadLeft(5, '0')));
                        Console.WriteLine(string.Format("{5}. 庫房代碼:{0} 院內碼:{1} 包裝單位:{2} 包裝數量:{3} 包裝申領倍數:{4}",
                            uimast.WH_NO, uimast.MMCODE, uimast.PACK_UNIT, uimast.PACK_QTY, uimast.PACK_TIMES, i.ToString().PadLeft(5, '0')));
                        logController.AddLogs("     取得藥庫資料");
                        Console.WriteLine("     取得藥庫資料");
                        ME_UIMAST ph1s_uimast = repo.GetPh1sUimast(uimast.MMCODE);
                        if (ph1s_uimast == null) {
                            logController.AddLogs("     藥庫無此院內碼資料，不做修正");
                            Console.WriteLine("     藥庫無此院內碼資料，不做修正");
                            continue;
                        }
                        logController.AddLogs("     比較藥庫資料");
                        Console.WriteLine("     比較藥庫資料");
                        Dictionary<string, string> ph1s_dic = GetDictionary(ph1s_uimast);
                        string name = string.Empty;
                        string value = string.Empty;
                        if (i == 1128) {
                            string t = string.Empty;
                        }
                        for (int j = 1; j < 6; j++)
                        {
                            if (ph1s_dic[string.Format("PACK_QTY{0}", j)] == uimast.PACK_QTY)
                            {
                                if (ph1s_dic[string.Format("PACK_UNIT{0}", j)] == null) {
                                    continue;
                                }
                                name = string.Format("PACK_UNIT{0}", j);
                                value = string.Format("PACK_QTY{0}", j);
                            }
                        }
                        if (name == string.Empty)
                        {
                            logController.AddLogs("     無相同包裝單位資料，不做修正");
                            Console.WriteLine("     無相同包裝單位資料，不做修正");
                            continue;
                        }
                        logController.AddLogs(string.Format("     相同申領包裝轉換量欄位: {0} 藥庫申領包裝單位:{1}", value, ph1s_dic[name]));
                        Console.WriteLine(string.Format("     相同申領包裝轉換量欄位: {0} 藥庫申領包裝單位:{1}", value, ph1s_dic[name]));
                        if (uimast.PACK_UNIT.Trim() == ph1s_dic[name].Trim())
                        {
                            logController.AddLogs("     申領包裝單位與藥庫包裝量相同，不做修正");
                            Console.WriteLine("     申領包裝單位與藥庫包裝量相同，不做修正");
                        }

                        bool isMod0 = (int.Parse(uimast.PACK_QTY) % int.Parse(ph1s_dic[value])) == 0;
                        if (isMod0 == false)
                        {
                            logController.AddLogs("     與藥庫設定包裝量不為倍數，不做修正");
                            Console.WriteLine("     與藥庫設定包裝量不為倍數，不做修正");
                            continue;
                        }

                        int final = int.Parse(uimast.PACK_QTY) * int.Parse(uimast.PACK_TIMES);
                        int baseValue = int.Parse(ph1s_dic[value]);

                        int n_pack_times = final / baseValue;

                        logController.AddLogs(string.Format("     總數量:{0} 藥庫包裝量:{1} 修改後申領倍數:{2}", final, baseValue, n_pack_times));
                        Console.WriteLine(string.Format("     總數量:{0} 藥庫包裝量:{1} 修改後申領倍數:{2}", final, baseValue, n_pack_times));

                        logController.AddLogs("     修改ME_UIMAST");
                        Console.WriteLine("     修改ME_UIMAST");
                        repo.UpdateMeUimast(uimast.WH_NO, uimast.MMCODE, ph1s_dic[name], n_pack_times.ToString());
                        logController.AddLogs("     修改完成");
                        Console.WriteLine("     修改完成");


                    }
                    DBWork.Commit();
                }
                catch (Exception ex)
                {
                    DBWork.Rollback();
                    
                    logController.AddLogs(string.Format("error: {0}", ex.Message));
                    Console.WriteLine(string.Format("error: {0}", ex.Message));
                    logController.AddLogs(string.Format("trace: {0}", ex.StackTrace));
                    logController.CreateLogFile("error");
                    throw;
                }
            }
            logController.CreateLogFile(wh_no);

        }

        private Dictionary<string, string> GetDictionary(ME_UIMAST item)
        {
            var json = JsonConvert.SerializeObject(item);
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }
    }
}
