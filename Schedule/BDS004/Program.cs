using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Text.RegularExpressions;

namespace BDS004
{
    static class Program
    {

        static void pre_test()
        {
            List<String> lst = new List<string>();
            lst.Add("這是三軍總醫院民診處『合約藥品』108年9月份E-MAIL訂貨單");
            lst.Add("這是三軍總醫院民診處『零購藥品』108年9月份E-MAIL訂貨單");
            lst.Add("發票抬頭:三軍總醫院民診處，請全部開列108年9月份發票，即日起發票單價請以最小單位開立(如粒或支)，切勿以盒或瓶裝開立");
            lst.Add("108年9月份驗收單請依據108年04月23日衛驗字第5號文通知，履約期限：109年12月31日");
            String sDt = (int.Parse(DateTime.Now.ToString("yyyy")) - 1911) + "年" + int.Parse(DateTime.Now.ToString("MM")).ToString() + "月份";
            Regex r;
            String sNewReplace;
            foreach (String s in lst)
            {
                // 108年9月份E-MAIL訂貨單
                r = new Regex(@"[0-9]{3}年[0-9]+月份E");
                if (r.Match(s).Success)
                {
                    sNewReplace = r.Replace(s, sDt + "E");
                }
                // 108年9月份驗收單
                r = new Regex(@"[0-9]{3}年[0-9]+月份發票");
                if (r.Match(s).Success)
                {
                    sNewReplace = r.Replace(s, sDt + "發票");
                }
                // 108年9月份發票
                r = new Regex(@"[0-9]{3}年[0-9]+月份驗收單");
                if (r.Match(s).Success)
                {
                    sNewReplace = r.Replace(s, sDt + "驗收單");
                }
            }
        }
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        //[STAThread]
        static void Main(string[] args)
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            //pre_test(); 

            Console.WriteLine("開始執行..");
            C_BDS004 c = new C_BDS004(args);
            //c.全部單元測試(args);
            c.run();
            Console.WriteLine("執行完畢(請按任意鍵結束)..");
            // Console.ReadKey();
        }
    } // ec
} // en
