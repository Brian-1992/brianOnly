using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace His2ConsumeTransferByDatetimeRange
{
    class Program
    {
        static void Main(string[] args)
        {
            string startDateString = args[0];
            string startTimeString = args[1];
            string endDateString = args[2];
            string endTimeString = args[3];

            string startDate = string.Format("{0} {1}", startDateString, startTimeString);
            string endDate = string.Format("{0} {1}", endDateString, endTimeString);

            Console.WriteLine("起始時間: {0}，結束時間: {1}", startDate, endDate);

            Service service = new Service();
            service.Run(startDate, endDate);
        }
    }
}
