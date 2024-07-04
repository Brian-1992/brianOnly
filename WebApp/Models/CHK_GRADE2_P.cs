using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class CHK_GRADE2_P : JCLib.Mvc.BaseModel
    {
        public string CHK_YM { get; set; }
        public string MMCODE { get; set; }
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }


        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string BASE_UNIT { get; set; }
        public string M_PURUN { get; set; }
        public string M_CONTPRICE { get; set; }
        public string E_TAKEKIND { get; set; }
        public string E_TAKEKIND_NAME { get; set; }


        public string STORE_QTYC { get; set; }
        public string CHK_QTY { get; set; }
        public string GAP_T { get; set; }
        public string DIFF_AMOUNT { get; set; }
        public string WH_NO { get; set; }
        public string WH_NAME { get; set; }

        public string CHK_QTY1 { get; set; }
        public string CHK_QTY2 { get; set; }
        public string CHK_QTY3 { get; set; }
        public string STATUS_TOT { get; set; }

        public string ITEM_STRING { get; set; }

        public string DIFF_P { get; set; }
        

    }
}