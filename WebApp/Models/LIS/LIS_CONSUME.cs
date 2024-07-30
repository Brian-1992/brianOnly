using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models.LIS
{
    public class LIS_CONSUME
    {
        public string DATA_DATE { get; set; }
        public string DATA_BTIME {get;set;}
        public string DATA_ETIME { get; set; }
        public string CHK_WH_NO { get; set; }
        public string MMCODE { get; set; }
        public string CONSUME_QTY { get; set; }
        public string BASE_UNIT { get; set; }
        public string INSTIME { get; set; }
        public string RDTIME { get; set; }
    }
}