using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class CHK_DETAILTOT : JCLib.Mvc.BaseModel
    {
        public string CHK_NO { get; set; }
        public string MMCODE { get; set; }
        public string MMNAME_C { get; set; }
        public string MMNAME_E { get; set; }
        public string M_PURUN { get; set; }
        public string M_CONTPRICE { get; set; }
        public string WH_NO { get; set; }
        public string STORE_LOC { get; set; }
        public string MAT_CLASS { get; set; }
        public string M_STOREID { get; set; }
        public string STORE_QTY { get; set; }
        public string STORE_QTYC { get; set; }
        public string STORE_QTYM { get; set; }
        public string STORE_QTYS { get; set; }
        public string LAST_QTY { get; set; }
        public string LAST_QTYC { get; set; }
        public string LAST_QTYM { get; set; }
        public string LAST_QTYS { get; set; }
        public string GAP_T { get; set; }
        public string GAP_C { get; set; }
        public string GAP_M { get; set; }
        public string GAP_S { get; set; }
        public string PRO_LOS_QTY { get; set; }
        public string PRO_LOS_AMOUNT { get; set; }
        public string MISS_PER { get; set; }
        public string MISS_PERC { get; set; }
        public string MISS_PERM { get; set; }
        public string MISS_PERS { get; set; }
        public string APL_OUTQTY { get; set; }
        public string CHK_REMARK { get; set; }
        public string CHK_QTY1 { get; set; }
        public string CHK_QTY2 { get; set; }
        public string CHK_QTY3 { get; set; }
        public string STATUS_TOT { get; set; }
        public string CREATE_DATE { get; set; }
        public string CREATE_USER { get; set; }
        public string UPDATE_TIME { get; set; }
        public string UPDATE_USER { get; set; }
        public string UPDATE_IP { get; set; }

        public string BASE_UNIT { get; set; }



        public string CONSUME_QTY { get; set; }     // 消耗量
        public string CONSUME_AMOUNT { get; set; }  // 消耗金額

    }
}