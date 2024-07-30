using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace AA0112
{
    public class Program
    {
        static void Main(string[] args)
        {
            string input = args[0];

            // 每月24日自動產生科室病房衛材盤點單
            if (input == "Monthly24Ward") {
                AA0112 aa0112 = new AA0112();
                aa0112.Run();
            }
            // 每月1日自動產生病房管制藥日盤單
            if (input == "DailyControllMed") {
                DailyControllMed service = new DailyControllMed();
                service.Run();
            }
            // 每月月結日晚上8點鎖單 (每天執行，若當天為設定之月結日再繼續執行)
            if (input == "MonthlyClose") {
                MonthlyClose service = new MonthlyClose();
                service.Run();
            }
            // 每月月結日晚上9點檢查藥局盤點品項是否存在MI_WHINV (每天執行，若當天為設定之月結日再繼續執行)
            if (input == "PhrWhinvCheck")
            {
                PhrWhinvCheck service = new PhrWhinvCheck();
                service.Run();
            }
            // 檢查是否有新增
            if (input == "PhrG2WhinvAdd")
            {
                PhrG2WhinvAdd service = new PhrG2WhinvAdd();
                service.Run();
            }
            // 檢查國軍醫院盤點開單
            if (input == "Monthly24_14")
            {
                Monthly24_14 service = new Monthly24_14();
                service.Run();
            }
        }
    }
}
