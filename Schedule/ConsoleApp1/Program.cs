using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string input = args[0];
            if (input == "ME_UIMAST_TRANS") {
                ME_UIMAST_TRANS trans = new ME_UIMAST_TRANS();
                trans.Run();
            }
        }
    }
}
