using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class CE0011M : JCLib.Mvc.BaseModel
    {
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT { get; set; }
        public string STORE_QTYC { get; set; }
        public string CHK_QTY { get; set; }
        public string QTY_DIFF { get; set; }
        public string CHK_UID { get; set; }
        public string CHK_UID_NAME { get; set; }
        public string STORE_LOC_NAMES { get; set; }
        public string CHK_WH_NO { get; set; }
        public string WH_NAME { get; set; }
        public string CHK_YM { get; set; }
        public string CHK_LEVEL { get; set; }
        public string CHK_LEVEL_NAME { get; set; }
        public string CHK_KEEPER { get; set; }
        public string CHK_KEEPER_NAME { get; set; }
        public string CHK_PERIOD { get; set; }
        public string CHK_PERIOD_NAME { get; set; }
        public string CHK_STATUS { get; set; }
        public string CHK_STATUS_NAME { get; set; }
        public string CHK_TYPE { get; set; }
        public string CHK_TYPE_NAME { get; set; }
        public string CHK_WH_GRADE { get; set; }
        public string CHK_WH_KIND { get; set; }
        public string WH_KIND_NAME { get; set; }
        public string CHK_NO { get; set; }


        public string CONSUME_QTY { get; set; }
        public string CHK_CLASS { get; set; }
        public string CHK_CLASS_NAME { get; set; }
    }
}