using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models.AB
{
    public class AB0110 : JCLib.Mvc.BaseModel
    {
        public string CRDOCNO { get; set; }
        public string ACKMMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string APPQTY { get; set; }
        public string BASE_UNIT { get; set; }
        public string INQTY { get; set; }
        public string WH_NAME { get; set; }
        public string CR_STATUS_NAME { get; set; }
        public string ISSMALL { get; set; }
        public string USEWHEN { get; set; }
        public string USEWHERE { get; set; }
        public string TEL { get; set; }

        public string CR_D_SEQ { get; set; }
        public string LOT_NO { get; set; }
        public string EXP_DATE { get; set; }
        public string ISUDI { get; set; }

    }
}