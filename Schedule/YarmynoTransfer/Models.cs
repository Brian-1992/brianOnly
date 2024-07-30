using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YarmynoTransfer
{
    public class Models
    {
        
    }

    public class TEMP_LOG
    {
        public string MMCODE { get; set; }
        public string YRARMYNO_N { get; set; }
        public string ITEMARMYNO_N { get; set; }
        public string YRARMYNO_O { get; set; }
        public string ITEMARMYNO_O { get; set; }
    }

    public class MI_MAST_LOG
    {
        public string LOGTIME { get; set; }
        public DateTime LOGTIME_TIME { get; set; }
        public string MMCODE { get; set; }
        public string E_YRARMYNO { get; set; }
        public string E_ITEMARMYNO { get; set; }
    }

    public class MILMED_JBID_LIST {
        public string JBID_STYR { get; set; }
        public string JBID_EDYR { get; set; }
        public string BID_NO { get; set; }
        public string ISWILLING { get; set; }
        public string DISCOUNT_QTY { get; set; }
        public string DISC_COST_UPRICE { get; set; }
    }
}
