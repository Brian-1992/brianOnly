using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public class MM_PR_M : JCLib.Mvc.BaseModel
    {
        public string PR_NO { get; set; }
        public string M_STOREID { get; set; }
        public string MAT_CLASS { get; set; }
        public string PR_DEPT { get; set; }
        public string PR_STATUS { get; set; }
        public DateTime PR_TIME { get; set; }
        public string PR_USER { get; set; }
        public string PR_AMT { get; set; }
        public DateTime CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_IP { get; set; }
        public DateTime UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }

        public string ISFROMDOCM { get; set; }

        // 緊急醫療出貨用
        public string MEMO { get; set; }
        public string IS_CR { get; set; }



    }
}
