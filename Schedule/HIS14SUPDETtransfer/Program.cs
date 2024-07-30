using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIS14SUPDETtransfer
{
    class Program
    {
        static void Main(string[] args)
        {
            //string input = args[0];
            string hosp_id = JCLib.Util.GetEnvSetting("HOSP_ID"); // 804
            string hosp_table_prefix = JCLib.Util.GetEnvSetting("HOSP_TABLE_PREFIX");

            Service service = new Service();
            service.Run(hosp_id, hosp_table_prefix);

            Console.WriteLine("\n轉檔完成");
            
            Console.ReadLine();
        }
    }
}
