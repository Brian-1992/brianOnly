using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BES002
{
    class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        static void Main(string[] args)
        {
            string input = args[0];
            //string input = "5MinRun";
            // 4小時執行
            if (input == "4HourRun")
            {
                UpCrdocStatus service = new UpCrdocStatus();
                service.Run();
            }
            // 5分鐘執行
            if (input == "5MinRun")
            {
                Console.WriteLine("執行 內網到外網");
                // 內網到外網
                InternalToInternet service1 = new InternalToInternet();
                service1.Run();

                Console.WriteLine("執行 外網到內網");
                // 外網到內網
                InternetToInternal service2 = new InternetToInternal();
                service2.Run();
            }
        }
    }
}
