using JCLib.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Controllers.AB;
using WebApp.Repository.AB;

namespace AAS001
{
    public class Apply
    {
        LogController log = new LogController("Apply", "Apply");

        AB0100Controller ab0100Controller = new AB0100Controller();
        public void Run()
        {
            // 呼叫AB0100的Transfer功能
            ab0100Controller.Transfer(new AB0100Controller.AB0100() { isSchedule = "Y" });
            
        }
    }
}
