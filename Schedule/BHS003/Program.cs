using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BHS003
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("開始執行..");
            C_BHS003 c_bhs003 = new C_BHS003(args);
            //c_bhs003.全部單元測試(args);
            c_bhs003.run();
            //Console.WriteLine("執行完畢(請按任意鍵結束)..");
            // Console.ReadKey();
        } // 

    } // ec
} // en
