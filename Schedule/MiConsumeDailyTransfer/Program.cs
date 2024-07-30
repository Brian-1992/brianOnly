using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiConsumeDailyTransfer
{
    class Program
    {
        static void Main(string[] args)
        {
            Service service = new Service();
            service.Run();
        }
    }
}
