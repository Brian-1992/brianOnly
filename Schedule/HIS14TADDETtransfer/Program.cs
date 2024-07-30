using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIS14TADDETtransfer
{
    class Program
    {
        /// <summary>
        /// args[0] "2019-02-02"
        /// args[1] "2019-03-13"
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            string hosp_id = JCLib.Util.GetEnvSetting("HOSP_ID");
            string startDate = "";
            string endDate = "";

            if (args.Length != 0)
            {
                startDate = args[0];
                endDate = args[1];
            }

            Service service = new Service();
            service.Run(hosp_id, startDate, endDate);

            Console.WriteLine("\n轉檔完成");

        }
    }
}
