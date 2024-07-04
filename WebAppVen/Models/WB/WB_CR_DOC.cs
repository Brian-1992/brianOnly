using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAppVen.Models
{
    public class WB_CR_DOC : JCLib.Mvc.BaseModel
    {
        public string CRDOCNO { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string APPQTY { get; set; }
        public string BASE_UNIT { get; set; }
        public string TOWH { get; set; }
        public string WH_NAME { get; set; }
        public string REQDATE { get; set; }
        public string CR_UPRICE { get; set; }
        public string APPTIME { get; set; }
        public string AGEN_NO { get; set; }
        public string EMAIL { get; set; }
        public string AGEN_NAMEC { get; set; }
        public string AGEN_TEL { get; set; }
        public string AGEN_BOSS { get; set; }
        public string REPLYTIME { get; set; }
        public string REPLY_STATUS { get; set; }
        public string INQTY { get; set; }
        public string WEXP_ID { get; set; }
        public string LOT_NO { get; set; }
        public string EXP_DATE { get; set; }
        public string IN_STATUS { get; set; }
        public string CREATE_TIME { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }

        public string IN_STATUS_TEXT { get; set; }

        public string PATIENTNAME { get; set; }
        public string CHARTNO { get; set; }

    }
}