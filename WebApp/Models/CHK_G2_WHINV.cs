using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class CHK_G2_WHINV
    {
        public string CHK_NO { get; set; }
        public string WH_NO { get; set; }
        public string MMCODE { get; set; }
        public string STORE_QTY { get; set;}
        public string CHK_PRE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }



        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT { get; set; }

        public string SEQ { get; set; }

        public string MEMO { get; set; }
    }
}