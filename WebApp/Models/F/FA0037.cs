using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models.F
{
    public class FA0037 : JCLib.Mvc.BaseModel
    {

        public string PR_NO { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string AGEN_NO { get; set; }
        public string AGEN_NAMEC { get; set; }
        public string AGEN_TEL { get; set; }
        public string M_PURUN { get; set; }
        public string M_AGENLAB { get; set; }
        public string M_CONTPRICE { get; set; }
        public string BASE_UNIT { get; set; }
        //public string M_CONTID { get; set; }
        public string M_PHCTNCO { get; set; }
        public string M_ENVDT { get; set; }
        public string DISC { get; set; }
        //public string M_STROREID { get; set; }
        public string REQ_QTY_T { get; set; }
        public double TOT { get; set; }

        public string UNIT_SWAP { get; set; }

        public string ISCR { get; set; }
    }
}