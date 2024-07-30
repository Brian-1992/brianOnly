using System;
using System.Collections.Generic;

namespace WebApp.Models.C
{
    public class CE0040 : JCLib.Mvc.BaseModel
    {
        public string CHK_YM { get; set; }
        public string CHK_NO { get; set; }
        public string CHK_WH_NO { get; set; }
        public string WH_NO { get; set; }
        public string WH_NAME { get; set; }
        public string MAT_CLASS { get; set; }
        public string MAT_CLSNAME { get; set; }
        public string CHK_STATUS { get; set; }
        public string CHK_TOTAL { get; set; }
        public string CHK_NUM { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string STORE_QTYC { get; set; }
        public string CHK_QTY { get; set; }
        public string USE_QTY { get; set; }
        public string INVENTORY { get; set; }
        public string DIFF_AMOUNT { get; set; }
        public string CHK_TIME { get; set; }
        public string CHK_ENDTIME { get; set; }
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }

        //結束全院盤點
        public string CHK_WH_KIND { get; set; }
        public string CHK_WH_GRADE { get; set; }
        public string ALTERED_USE_QTY { get; set; }
        public string USE_QTY_AF_CHK { get; set; }
        public string INV_QTY { get; set; }
        public string SET_YM { get; set; }
    }
}
