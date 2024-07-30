using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAS001
{
    public class Program
    {
        static void Main(string[] args)
        {
            string input = args[0];

            // 盤點轉檔
            if (input == "ChkService")
            {
                Chk service = new Chk();
                service.Run();
            }

            // 申請轉檔
            if (input == "ApplyService")
            {
                Apply service = new Apply();
                service.Run();
            }

            // 入庫轉檔
            if (input == "ApproveService")
            {
                Approve service = new Approve();
                service.Run();
            }
        }
    }
}
