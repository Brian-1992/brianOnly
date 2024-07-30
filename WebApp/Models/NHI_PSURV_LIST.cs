using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class NHI_PSURV_LIST : JCLib.Mvc.BaseModel
    {
        public string PS_SEQ { get; set; }
        public string M_NHIKEY { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string E_SPEC { get; set; }
        public string E_UNIT { get; set; }
        public string E_DRUGFORM { get; set; }
        public string AGEN_NAME { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_IP { get; set; }

        public string MMCODE { get; set; }
        public string HIS_E_SPECNUNIT { get; set; } // HIS_規格量及單位
        public string HIS_E_COMPUNIT { get; set; }  // HIS_成份
        public string HIS_BASE_UNIT { get; set; }   // HIS_計量單位
        public string HIS_E_DRUGFORM { get; set; }  // HIS_藥品劑型

        public string RCM_RATIO { get; set; }
        public string SET_RATIO { get; set; }

        public int? Seq { get; set; }
        public NHI_PSURV_LIST() {
            Seq = 0;
        }
    }
}