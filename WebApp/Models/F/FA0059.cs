using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class FA0059 : JCLib.Mvc.BaseModel
    {
        public string SEQ { get; set; }
        public string MAT_CLASS { get; set; }
        public string SKEY { get; set; }
        public string MAT_CLSNAME { get; set; }
        public string INV_TYPE { get; set; }

        public string P_AMT { get; set; }
        public string IN_AMT { get; set; }
        public string OUT_AMT { get; set; }
        public string CHECK_P_AMT { get; set; }
        public string CHECK_M_AMT { get; set; }

        public string CHECK_AMT { get; set; }
        public string INV_AMT { get; set; }
        public string REP_TIME { get; set; }


    }
}