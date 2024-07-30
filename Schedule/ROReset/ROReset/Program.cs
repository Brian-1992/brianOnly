using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROReset
{
    class Program
    {
        static void Main(string[] args)
        {
            ROResetService s = new ROResetService();
            s.Run();
        }
    }
}
