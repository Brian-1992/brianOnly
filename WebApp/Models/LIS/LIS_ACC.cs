using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class LIS_ACC
    {
        public string DOCNO { get; set; }
        public string PURCHNO { get; set; }
        public string SEQ { get; set; }
        public string MMCODE { get; set; }
        public string LOT_NO { get; set; }
        public string EXP_DATE { get; set; }
        public string APVQTY { get; set; }
        public string BASE_UNIT { get; set; }
        public string INSTIME { get; set; }
        public string RD_TIME { get; set; }
        public string ISACC { get; set; }

        public List<LIS_ACC> Ys { get; set; }
        public List<LIS_ACC> Ns { get; set; }
    }
}