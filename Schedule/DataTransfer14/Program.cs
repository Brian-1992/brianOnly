using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransfer14
{
    class Program
    {
        static void Main(string[] args)
        {
            //string input = args[0];
            string hosp_id = JCLib.Util.GetEnvSetting("HOSP_ID");
            string hosp_table_prefix = JCLib.Util.GetEnvSetting("HOSP_TABLE_PREFIX");

            Service service = new Service();

            Console.WriteLine(@"操作模式如下，請鍵入模式代碼
輸入１：全部 (資料建立+MI_BASERO_14轉檔)
輸入２：只操作資料建立功能
輸入３：只操作MI_BASERO_14轉檔");
            string workingMode = Console.ReadLine();

            if (workingMode == "1")
            {
                Console.WriteLine("--------開始進行資料建立作業--------");
                service.Run(hosp_id, hosp_table_prefix);

                Console.WriteLine("--------即將開始轉檔MI_BASERO_14--------");
                Console.WriteLine("請輸入起始日期(北投醫院起: 1130227 )");
                string startXfTime = Console.ReadLine();  //用於指定Transfer_MI_BASERO_14撈取的時間
                Console.WriteLine("請輸入結束日期(北投醫院迄: 1130326 )");
                string endXfTime = Console.ReadLine();
                if (startXfTime != "" && startXfTime != "" && startXfTime.Length == 7 && endXfTime.Length == 7)
                {
                    service.RunTransfer_MI_BASERO_14(hosp_id, hosp_table_prefix, startXfTime, endXfTime);
                }
                else
                {
                    Console.WriteLine("輸入格式錯誤! 請透過指令3重新操作Transfer_MI_BASERO_14");
                }

            }
            else if (workingMode == "2")
            {
                Console.WriteLine("--------開始進行資料建立作業--------");
                service.Run(hosp_id, hosp_table_prefix);

            }
            else if (workingMode == "3")
            {
                Console.WriteLine("--------即將開始轉檔MI_BASERO_14--------");
                Console.WriteLine("請輸入起始日期(北投醫院起: 1130227 )");
                string startXfTime = Console.ReadLine();  //用於指定Transfer_MI_BASERO_14撈取的時間
                Console.WriteLine("請輸入結束日期(北投醫院迄: 1130326 )");
                string endXfTime = Console.ReadLine();
                if (startXfTime != "" && startXfTime != "" && startXfTime.Length == 7 && endXfTime.Length == 7)
                {
                    service.RunTransfer_MI_BASERO_14(hosp_id, hosp_table_prefix, startXfTime, endXfTime);
                }
                else
                {
                    Console.WriteLine("輸入格式錯誤! 請透過指令3重新操作Transfer_MI_BASERO_14");
                }
            }
            else
            {
                Console.WriteLine("操作代碼錯誤!");
            }


            Console.WriteLine("\n操作結束");

            Console.ReadLine();
        }
    }
}
